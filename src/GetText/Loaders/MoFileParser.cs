using System;
using System.Text;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;

namespace GetText.Loaders
{
    /// <summary>
    /// MO file format parser.
    /// See http://www.gnu.org/software/gettext/manual/html_node/MO-Files.html
    /// </summary>
    public class MoFileParser
    {
        /// <summary>
        /// MO file format magic number.
        /// </summary>
        private const uint MO_FILE_MAGIC = 0x950412de;

        private const ushort MAX_SUPPORTED_VERSION = 1;

        private static readonly char[] linefeed = { '\n', '\r' };
        private static readonly char[] nullValue = { '\0' };

        private struct StringOffsetTable
        {
            public int Length;
            public int Offset;
        }

        /// <summary>
        /// Default encoding for decoding all strings in given MO file.
        /// Must be binary compatible with US-ASCII to be able to read file headers.
        /// </summary>
        /// <remarks>
        /// Default value is UTF-8 as it is compatible with required by specifications US-ASCII.
        /// </remarks>
        public Encoding DefaultEncoding { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whenever the parser can detect file encoding using the Content-Type MIME header.
        /// </summary>
        public bool AutoDetectEncoding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoFileParser"/> class with UTF-8 as default encoding and with enabled automatic encoding detection.
        /// </summary>
        public MoFileParser()
        {
            DefaultEncoding = Encoding.UTF8;
            AutoDetectEncoding = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoFileParser"/> class using given default encoding and given automatic encoding detection option.
        /// </summary>
        public MoFileParser(Encoding defaultEncoding, bool autoDetectEncoding = true)
        {
            DefaultEncoding = defaultEncoding;
            AutoDetectEncoding = autoDetectEncoding;
        }

        /// <summary>
        /// Parses a GNU MO file from the given stream and loads all available data.
        /// </summary>
        /// <remarks>
        ///	http://www.gnu.org/software/gettext/manual/html_node/MO-Files.html
        /// </remarks>
        /// <param name="stream">Stream that contain binary data in the MO file format</param>
        /// <returns>Parsed file data.</returns>
        public MoFile Parse(Stream stream)
        {
#if DEBUG
            Trace.WriteLine("Trying to parse a MO file stream...", "GetText");
#endif

            if (stream == null || stream.Length < 20)
            {
                throw new ArgumentException("Stream can not be null of less than 20 bytes long.");
            }

            bool bigEndian = false;
            using (ReadOnlyStream readOnlyStream = new ReadOnlyStream(stream))
            {

                BinaryReader reader = new BinaryReader(readOnlyStream, Encoding.UTF8, true);
                try
                {
                    uint magicNumber = reader.ReadUInt32();
                    if (magicNumber != MO_FILE_MAGIC)
                    {
                        // System.IO.BinaryReader does not respect machine endianness and always uses LittleEndian
                        // So we need to detect and read BigEendian files by ourselves
                        if (ReverseBytes(magicNumber) == MO_FILE_MAGIC)
                        {
#if DEBUG
                            Trace.WriteLine("BigEndian file detected. Switching readers...", "GetText");
#endif
                            bigEndian = true;
                            ((IDisposable)reader).Dispose();
                            reader = new BigEndianBinaryReader(readOnlyStream, Encoding.UTF8, true);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid stream: can not find MO file magic number.");
                        }
                    }

                    int revision = reader.ReadInt32();
                    MoFile parsedFile = new MoFile(new Version(revision >> 16, revision & 0xffff), DefaultEncoding, bigEndian);

#if DEBUG
                    Trace.WriteLine($"MO File Revision: {parsedFile.FormatRevision.Major}.{parsedFile.FormatRevision.Minor}.", "GetText");
#endif

                    if (parsedFile.FormatRevision.Major > MAX_SUPPORTED_VERSION)
                    {
                        throw new CatalogLoadingException($"Unsupported MO file major revision: {parsedFile.FormatRevision.Major}.");
                    }

                    int stringCount = reader.ReadInt32();
                    int originalTableOffset = reader.ReadInt32();
                    int translationTableOffset = reader.ReadInt32();

                    // We don't support hash tables and system dependent segments.

#if DEBUG
                    Trace.WriteLine($"MO File contains {stringCount} strings.", "GetText");
#endif

                    StringOffsetTable[] originalTable = new StringOffsetTable[stringCount];
                    StringOffsetTable[] translationTable = new StringOffsetTable[stringCount];

#if DEBUG
                    Trace.WriteLine($"Trying to parse strings using encoding \"{parsedFile.Encoding}\"...", "GetText");
#endif

                    reader.BaseStream.Seek(originalTableOffset, SeekOrigin.Begin);
                    for (int i = 0; i < stringCount; i++)
                    {
                        originalTable[i].Length = reader.ReadInt32();
                        originalTable[i].Offset = reader.ReadInt32();
                    }

                    reader.BaseStream.Seek(translationTableOffset, SeekOrigin.Begin);
                    for (int i = 0; i < stringCount; i++)
                    {
                        translationTable[i].Length = reader.ReadInt32();
                        translationTable[i].Offset = reader.ReadInt32();
                    }


                    for (int i = 0; i < stringCount; i++)
                    {
                        string[] originalStrings = ReadStrings(reader, originalTable[i].Offset, originalTable[i].Length, parsedFile.Encoding);
                        string[] translatedStrings = ReadStrings(reader, translationTable[i].Offset, translationTable[i].Length, parsedFile.Encoding);

                        if (originalStrings.Length == 0 || translatedStrings.Length == 0) continue;

                        if (originalStrings[0].Length == 0)
                        {
                            // MO file meta data processing
                            foreach (string headerText in translatedStrings[0].Split(linefeed, StringSplitOptions.RemoveEmptyEntries))
                            {
                                int separatorIndex = headerText.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                                if (separatorIndex > 0)
                                {
                                    string headerName = headerText.Substring(0, separatorIndex);
                                    string headerValue = headerText.Substring(separatorIndex + 1).Trim();
                                    parsedFile.Headers.Add(headerName, headerValue.Trim());
                                }
                            }

                            if (AutoDetectEncoding && parsedFile.Headers.ContainsKey("Content-Type"))
                            {
                                try
                                {
                                    ContentType contentType = new ContentType(parsedFile.Headers["Content-Type"]);
                                    if (!string.IsNullOrEmpty(contentType.CharSet))
                                    {
                                        parsedFile.Encoding = Encoding.GetEncoding(contentType.CharSet);
#if DEBUG
                                        Trace.WriteLine($"File encoding switched to \"{parsedFile.Encoding}\" (\"{contentType.CharSet}\" requested).", "GetText");
#endif
                                    }
                                }
                                catch (Exception exception)
                                {
                                    throw new CatalogLoadingException($"Unable to change parser encoding using the Content-Type header: \"{exception.Message}\".", exception);
                                }
                            }
                        }

                        parsedFile.Translations.Add(originalStrings[0], translatedStrings);
                    }

#if DEBUG
                    Trace.WriteLine("String parsing completed.", "GetText");
#endif
                    return parsedFile;
                }
                finally
                {
                    ((IDisposable)reader).Dispose();
                }
            }
        }

        private static string[] ReadStrings(BinaryReader reader, int offset, int length, Encoding encoding)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            byte[] stringBytes = reader.ReadBytes(length);
            return encoding.GetString(stringBytes, 0, stringBytes.Length).Split(nullValue);
        }

        private static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}
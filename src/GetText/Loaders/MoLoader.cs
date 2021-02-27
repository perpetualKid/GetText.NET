using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Globalization;
using System.IO;

using GetText.Plural;

namespace GetText.Loaders
{
    /// <summary>
    /// A catalog loader that loads data from files in the GNU/Gettext MO file format.
    /// </summary>
    public class MoLoader : ILoader
    {
        private const string MO_FILE_EXT = ".mo";

        private readonly Stream moStream;

        private readonly string filePath;
        private readonly string domain;
        private readonly string localeDir;

        /// <summary>
        /// Gets a current plural generator instance.
        /// </summary>
        public IPluralRuleGenerator PluralRuleGenerator { get; private set; }

        /// <summary>
        /// Gets a MO file format parser instance.
        /// </summary>
        public MoFileParser Parser { get; private set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// that will be located in the localeDir using the domain name and catalog's culture info.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        /// <param name="pluralRuleGenerator"></param>
        /// <param name="parser"></param>
        public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
        {
            this.domain = domain ?? throw new ArgumentNullException(nameof(domain));
            this.localeDir = localeDir ?? throw new ArgumentNullException(nameof(localeDir));
            PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pluralRuleGenerator"></param>
        /// <param name="parser"></param>
        public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
        {
            this.filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified stream.
        /// </summary>
        /// <param name="moStream"></param>
        /// <param name="pluralRuleGenerator"></param>
        /// <param name="parser"></param>
        public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser)
        {
            this.moStream = moStream ?? throw new ArgumentNullException(nameof(moStream));
            PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
            Parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        #endregion

        #region Constructor overloads

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// that will be located in the localeDir using the domain name and catalog's culture info.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        /// <param name="pluralRuleGenerator"></param>
        public MoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator)
            : this(domain, localeDir, pluralRuleGenerator, new MoFileParser())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// that will be located in the localeDir using the domain name and catalog's culture info.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        /// <param name="parser"></param>
        public MoLoader(string domain, string localeDir, MoFileParser parser)
            : this(domain, localeDir, new DefaultPluralRuleGenerator(), parser)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// that will be located in the localeDir using the domain name and catalog's culture info.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        public MoLoader(string domain, string localeDir)
            : this(domain, localeDir, new DefaultPluralRuleGenerator(), new MoFileParser())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pluralRuleGenerator"></param>
        public MoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator)
            : this(filePath, pluralRuleGenerator, new MoFileParser())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="parser"></param>
        public MoLoader(string filePath, MoFileParser parser)
            : this(filePath, new DefaultPluralRuleGenerator(), parser)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified path.
        /// </summary>
        /// <param name="filePath"></param>
        public MoLoader(string filePath)
            : this(filePath, new DefaultPluralRuleGenerator(), new MoFileParser())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified stream.
        /// </summary>
        /// <param name="moStream"></param>
        /// <param name="pluralRuleGenerator"></param>
        public MoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator)
            : this(moStream, pluralRuleGenerator, new MoFileParser())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified stream.
        /// </summary>
        /// <param name="moStream"></param>
        /// <param name="parser"></param>
        public MoLoader(Stream moStream, MoFileParser parser)
            : this(moStream, new DefaultPluralRuleGenerator(), parser)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MoLoader"/> class which will try to load a MO file
        /// from the specified stream.
        /// </summary>
        /// <param name="moStream"></param>
        public MoLoader(Stream moStream)
            : this(moStream, new DefaultPluralRuleGenerator(), new MoFileParser())
        {
        }

        #endregion


        /// <summary>
        /// Loads translations to the specified catalog using catalog's culture info.
        /// </summary>
        /// <param name="catalog">A catalog instance to load translations to.</param>
        public void Load(Catalog catalog)
        {
            if (moStream != null)
            {
                Load(moStream, catalog);
            }
            else if (filePath != null)
            {
                Load(filePath, catalog);
            }
            else
            {
                Load(domain, localeDir, catalog);
            }
        }

        /// <summary>
        /// Loads translations to the specified catalog using catalog's culture info from specified locale directory and specified domain.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        /// <param name="catalog"></param>
        protected virtual void Load(string domain, string localeDir, Catalog catalog)
        {
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            string path = FindTranslationFile(catalog.CultureInfo, domain, localeDir);
            if (path == null)
            {
#if DEBUG
                // Suppress FileNotFound exceptions
                Trace.WriteLine($"Translation file loading fail. Can not find MO file name in locale directory \"{localeDir ?? "\\."}\".", "GetText");
#endif
            }
            else
                Load(path, catalog);
        }

        /// <summary>
        /// Loads translations to the specified catalog from specified MO file path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="catalog"></param>
        protected virtual void Load(string filePath, Catalog catalog)
        {
#if DEBUG
            Trace.WriteLine($"Loading translations from file \"{filePath}\"...", "GetText");
#endif

            using (FileStream stream = File.OpenRead(filePath))
            {
                Load(stream, catalog);
            }
        }

        /// <summary>
        /// Loads translations to the specified catalog from specified MO file stream.
        /// </summary>
        /// <param name="moStream"></param>
        /// <param name="catalog"></param>
        protected virtual void Load(Stream moStream, Catalog catalog)
        {
            MoFile parsedMoFile = Parser.Parse(moStream);

            Load(parsedMoFile, catalog);
        }

        /// <summary>
        /// Loads translations to the specified catalog using specified MO file parser.
        /// </summary>
        /// <param name="parsedMoFile"></param>
        /// <param name="catalog"></param>
        protected virtual void Load(MoFile parsedMoFile, Catalog catalog)
        {
            if (parsedMoFile == null)
                throw new ArgumentNullException(nameof(parsedMoFile));
            if (catalog == null)
                throw new ArgumentNullException(nameof(catalog));

            foreach (KeyValuePair<string, string[]> translation in parsedMoFile.Translations)
            {
                catalog.Translations.Add(translation.Key, translation.Value);
            }

            if (parsedMoFile.Headers.TryGetValue("Plural-Forms", out string pluralForms) && (PluralRuleGenerator is IPluralRuleTextParser generator))
            {
                generator.SetPluralRuleText(pluralForms);
            }
            if (parsedMoFile.Headers.TryGetValue("Language", out string language))
            {
                try
                {
                    CultureInfo cultureInfo = new CultureInfo(language);
                    if (!cultureInfo.Equals(catalog.CultureInfo))
                    {
#if DEBUG
#endif
                        catalog.UpdateCultureInfo(cultureInfo);
                    }
                }
                catch (CultureNotFoundException) 
                {
#if DEBUG
                    Trace.WriteLine($"Could not create CultureInfo from language code {language}.", "GetText");
#endif
                }
            }
            catalog.PluralRule = PluralRuleGenerator.CreateRule(catalog.CultureInfo);
        }

        /// <summary>
        /// Finds corresponding translation file using specified culture info, domain and a locale directory.
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="domain"></param>
        /// <param name="localeDir"></param>
        /// <returns></returns>
        protected virtual string FindTranslationFile(CultureInfo cultureInfo, string domain, string localeDir)
        {
            if (cultureInfo == null)
                throw new ArgumentNullException(nameof(cultureInfo));

            string[] possibleFiles = new[] {
                GetFileName(localeDir, domain, cultureInfo.Name.Replace('-', '_')),
                GetFileName(localeDir, domain, cultureInfo.Name),
                GetFileName(localeDir, domain, cultureInfo.TwoLetterISOLanguageName)
            };

            foreach (string possibleFilePath in possibleFiles)
            {
                if (File.Exists(possibleFilePath))
                {
                    return possibleFilePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Constructs a standard path to the MO translation file using specified path to the locale directory, 
        /// domain and locale's TwoLetterISOLanguageName string.
        /// </summary>
        /// <param name="localeDir"></param>
        /// <param name="domain"></param>
        /// <param name="locale"></param>
        /// <returns></returns>
        protected virtual string GetFileName(string localeDir, string domain, string locale)
        {
            return string.IsNullOrEmpty(localeDir) ? Path.Combine(locale, domain + MO_FILE_EXT) : Path.Combine(localeDir, locale, domain + MO_FILE_EXT);
        }
    }
}
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace GetText.Extractor.CommandLine
{
    internal static class CommandLineOptions
    {
        internal static RootCommand RootCommand => new RootCommand("Extracts strings from C# source code files to creates or updates PO template file");

        internal static Option<FileInfo> SourceOption
        {
            get
            {
                return new Option<FileInfo>(new[] { "-s", "--source" }, TryParseSourceFilePathArgument, true, "Visual Studio Solution file, Project file, or source directory") {
                    Name = "Source",
                };
            }
        }

        internal static Option<FileInfo> OutFile
        {
            get
            {
                return new Option<FileInfo>(new[] { "-t", "--target" }, TryParseDefaultTargetFile, true, "Target PO template file. ") {
                    Name = "Target",
                };
            }
        }

        internal static Option<bool> Merge => new Option<bool>(new[] { "--merge", "-m" }, "Merge with existing file instead of overwrite");

        internal static Option<bool> Verbose => new Option<bool>(new[] { "--verbose", "-v" }, "Verbose output");

        internal static Option<bool> UseUnixPathSeparator => new Option<bool>(new[] { "--unixstyle", "-u" }, "Unix-style Path Separator ('/')");

        #region private validation and parsing
        private static FileInfo TryParseDefaultTargetFile(ArgumentResult argument)
        {
            string token = argument.Tokens.Count switch
            {
                0 => "messages.pot",
                1 => argument.Tokens[0].Value,
                _ => throw new InvalidOperationException("Unexpected token count."),
            };
            try
            {
                token = Path.GetFullPath(token);
                if (File.Exists(token))
                {
                    return new FileInfo(token);
                }
                else if (Directory.Exists(token))
                {
                    return new FileInfo(Path.Combine(token, "messages.pot"));
                }
                else if (Directory.Exists(Path.GetDirectoryName(token)))
                {
                    return new FileInfo(Path.ChangeExtension(token, ".pot"));
                }
                else if (Path.GetExtension(token).Equals(".pot", StringComparison.OrdinalIgnoreCase))
                {
                    //assume this will create the file
                    return new FileInfo(token);
                }
            }
            catch(Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
            {
                argument.ErrorMessage = $"The path for '{token}' is not valid. ({ex.Message})";
                return default;
            }
            argument.ErrorMessage = $"The path for '{token}' could not be found.";
            return default;
        }

        private static FileInfo TryParseSourceFilePathArgument(ArgumentResult argument)
        {
            string token = argument.Tokens.Count switch
            {
                0 => ".",
                1 => argument.Tokens[0].Value,
                _ => throw new InvalidOperationException("Unexpected token count."),
            };
            if (File.Exists(token))
            {
                return new FileInfo(Path.GetFullPath(token));
            }

            if (Directory.Exists(token))
            {
                if (TryFindProjectFile(token, out string path))
                {
                    return new FileInfo(path);
                }
                else
                {
                    return new FileInfo(Path.GetFullPath(token));
                }
            }

            argument.ErrorMessage = $"The file or directory '{token}' could not be found.";
            return default;
        }

        private static bool TryFindProjectFile(string directoryPath, out string result)
        {
            List<string> matches = new List<string>();
            foreach (string candidateItem in Directory.EnumerateFiles(directoryPath))
            {
                if (Path.GetExtension(candidateItem).EndsWith(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(candidateItem);
                }

                if (Path.GetExtension(candidateItem).EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(candidateItem);
                }
            }

            // Prefer solution if both are in the same directory. This helps to avoid some conflicts.
            if (matches.Any(m => m.EndsWith(".sln", StringComparison.OrdinalIgnoreCase)))
            {
                matches.RemoveAll(m => m.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));
            }

            if (matches.Count == 0)
            {
                result = $"No project file or solution file was found in directory '{Path.GetFullPath(directoryPath)}'.";
                return false;
            }
            else if (matches.Count == 1)
            {
                result = matches[0];
                return true;
            }
            else
            {
                result = $"More than one project file or solution file was found in directory '{Path.GetFullPath(directoryPath)}'.";
                return false;
            }
        }
        #endregion
    }
}

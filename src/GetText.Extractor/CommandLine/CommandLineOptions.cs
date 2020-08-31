using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace GetText.Extractor.CommandLine
{
    public class CommandLineResult
    {
        public FileInfo Source { get; private set; }
        public FileInfo Target { get; private set; }
        public bool Merge { get; private set; }
    }


    internal static class CommandLineOptions
    {
        internal static RootCommand RootCommand => new RootCommand("Extracts strings from C# source code files to creates or updates PO template file");

        internal static Option<FileInfo> SourceOption
        {
            get
            {
                Argument<FileInfo> argument = new Argument<FileInfo>(TryParseSourceFilePathArgument, true) {
                    Name = "solution-file or project-file or directory",
                };

                return new Option<FileInfo>(new[] { "-s", "--source" }, "Visual Studio Solution file, Project file, or source directory") {
                    Argument = argument,
                    Name = "Source",
                };
            }
        }

        internal static Option<FileInfo> OutFile
        {
            get
            {
                Argument<FileInfo> argument = new Argument<FileInfo>(TryParseDefaultTargetFile, true) {
                    Name = "target PO template file name (*.pot). Extension may be omitted",
                };

                return new Option<FileInfo>(new[] { "-t", "--target" }, "Target PO template file. ") {
                    Argument = argument,
                    Name = "Target",
                };
            }
        }

        internal static Option<bool> Merge => new Option<bool>(new[] { "--merge", "-m" }, "Merge with existing file instead of overwrite");

        internal static Option<bool> Verbose => new Option<bool>(new[] { "--verbose", "-v" }, "Verbose output");

        #region private validation and parsing
        private static FileInfo TryParseDefaultTargetFile(ArgumentResult argument)
        {
            string token;
            switch (argument.Tokens.Count)
            {
                case 0: token = "messages.pot"; break;
                case 1: token = argument.Tokens[0].Value; break;
                default: throw new InvalidOperationException("Unexpected token count.");
            }

            token = Path.GetFullPath(token);
            if (File.Exists(token) || Directory.Exists(Path.GetDirectoryName(token)))
            {
                FileInfo result = new FileInfo(token);
                if ((result.Attributes | FileAttributes.Directory) == FileAttributes.Directory)
                    result = new FileInfo(Path.Combine(token, "messages.pot"));
                return result;
            }
            argument.ErrorMessage = $"The path for '{token}' could not be found.";
            return default;
        }

        private static FileInfo TryParseSourceFilePathArgument(ArgumentResult argument)
        {
            string token;
            switch (argument.Tokens.Count)
            {
                case 0: token = "."; break;
                case 1: token = argument.Tokens[0].Value; break;
                default: throw new InvalidOperationException("Unexpected token count.");
            }

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

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using NGettext.Plural;

namespace NGettext.Loaders
{
	/// <summary>
	/// A catalog loader that loads data from files in the GNU/Gettext MO file format.
	/// </summary>
	public class MoLoader : ILoader
	{
		private const string LC_MESSAGES = "LC_MESSAGES";
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
			this.PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
			this.Parser = parser ?? throw new ArgumentNullException(nameof(parser));
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
			this.PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
			this.Parser = parser ?? throw new ArgumentNullException(nameof(parser));
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
			this.PluralRuleGenerator = pluralRuleGenerator ?? throw new ArgumentNullException(nameof(pluralRuleGenerator));
			this.Parser = parser ?? throw new ArgumentNullException(nameof(parser));
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
			if (this.moStream != null)
			{
				this.Load(this.moStream, catalog);
			}
			else if (this.filePath != null)
			{
				this.Load(this.filePath, catalog);
			}
			else
			{
				this.Load(this.domain, this.localeDir, catalog);
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

            string path = this.FindTranslationFile(catalog.CultureInfo, domain, localeDir);
			if (path == null)
			{
				throw new FileNotFoundException($"Can not find MO file name in locale directory \"{localeDir}\".");
			}

			this.Load(path, catalog);
		}

		/// <summary>
		/// Loads translations to the specified catalog from specified MO file path.
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="catalog"></param>
		protected virtual void Load(string filePath, Catalog catalog)
		{
#if DEBUG
			Trace.WriteLine($"Loading translations from file \"{filePath}\"...", "NGettext");
#endif
			
			using (var stream = File.OpenRead(filePath))
			{
				this.Load(stream, catalog);
			}
		}

		/// <summary>
		/// Loads translations to the specified catalog from specified MO file stream.
		/// </summary>
		/// <param name="moStream"></param>
		/// <param name="catalog"></param>
		protected virtual void Load(Stream moStream, Catalog catalog)
		{
			var parsedMoFile = this.Parser.Parse(moStream);

			this.Load(parsedMoFile, catalog);
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

			foreach (var translation in parsedMoFile.Translations)
			{
				catalog.Translations.Add(translation.Key, translation.Value);
			}

			if (parsedMoFile.Headers.ContainsKey("Plural-Forms"))
			{
                if (this.PluralRuleGenerator is IPluralRuleTextParser generator)
                {
                    generator.SetPluralRuleText(parsedMoFile.Headers["Plural-Forms"]);
                }
            }
			catalog.PluralRule = this.PluralRuleGenerator.CreateRule(catalog.CultureInfo);
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
				this.GetFileName(localeDir, domain, cultureInfo.Name.Replace('-', '_')),
				this.GetFileName(localeDir, domain, cultureInfo.Name),
				this.GetFileName(localeDir, domain, cultureInfo.TwoLetterISOLanguageName)
			};

			foreach (var possibleFilePath in possibleFiles)
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
			return Path.Combine(localeDir, Path.Combine(locale, Path.Combine(LC_MESSAGES, domain + MO_FILE_EXT)));
		}
	}
}
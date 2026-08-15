using System;
using System.Windows.Markup;

namespace GetText.Wpf
{
    /// <summary>
    /// Provides gettext translation for string literals used directly in XAML.
    /// </summary>
    [MarkupExtensionReturnType(typeof(string))]
    public class GettextExtension : MarkupExtension
    {
        private const char ContextSeparator = '|';

        /// <summary>
        /// Initializes a new instance of the <see cref="GettextExtension"/> class.
        /// </summary>
        public GettextExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GettextExtension"/> class for the specified source text.
        /// </summary>
        /// <param name="text">The source text to translate.</param>
        public GettextExtension(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GettextExtension"/> class for the specified context and source text.
        /// </summary>
        /// <param name="context">The translation context.</param>
        /// <param name="text">The source text to translate.</param>
        public GettextExtension(string context, string text)
        {
            Context = context;
            Text = text;
        }

        /// <summary>
        /// Gets or sets the fallback catalog used when an extension instance does not specify a catalog.
        /// </summary>
        public static ICatalog DefaultCatalog { get; set; }

        /// <summary>
        /// Gets or sets the catalog used to translate this extension instance.
        /// </summary>
        public ICatalog Catalog { get; set; }

        /// <summary>
        /// Gets or sets the translation context.
        /// </summary>
        [ConstructorArgument("context")]
        public string Context { get; set; }

        /// <summary>
        /// Gets or sets the source text to translate.
        /// </summary>
        [ConstructorArgument("text")]
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the plural source text to translate when <see cref="Count"/> is not one.
        /// </summary>
        public string PluralText { get; set; }

        /// <summary>
        /// Gets or sets the value used to choose singular or plural translations.
        /// </summary>
        public long Count { get; set; } = 1;

        /// <summary>
        /// Returns the translated source text for the current XAML service context.
        /// </summary>
        /// <param name="serviceProvider">The XAML service provider.</param>
        /// <returns>The translated string, or the source string when no catalog is available.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(Text))
                return Text;

            string context = Context;
            string text = Text;
            SplitContext(ref context, ref text);

            ICatalog catalog = Catalog ?? DefaultCatalog;
            if (catalog == null)
                return Count == 1 || string.IsNullOrEmpty(PluralText) ? text : PluralText;

            if (string.IsNullOrEmpty(PluralText))
                return string.IsNullOrEmpty(context) ? catalog.GetString(text) : catalog.GetParticularString(context, text);

            return string.IsNullOrEmpty(context)
                ? catalog.GetPluralString(text, PluralText, Count)
                : catalog.GetParticularPluralString(context, text, PluralText, Count);
        }

        private static void SplitContext(ref string context, ref string text)
        {
            if (!string.IsNullOrEmpty(context) || string.IsNullOrEmpty(text))
                return;

            int separatorIndex = text.IndexOf(ContextSeparator);
            if (separatorIndex <= 0 || separatorIndex == text.Length - 1)
                return;

            context = text.Substring(0, separatorIndex);
            text = text.Substring(separatorIndex + 1);
        }
    }
}

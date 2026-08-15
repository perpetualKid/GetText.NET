using System;
using System.Globalization;
using System.Windows.Data;

namespace GetText.Wpf
{
    /// <summary>
    /// Converts bound string values through a gettext catalog.
    /// </summary>
    public class CatalogConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the catalog used by this converter.
        /// </summary>
        public ICatalog Catalog { get; set; }

        /// <summary>
        /// Converts a string value into its translated representation.
        /// </summary>
        /// <param name="value">The source binding value.</param>
        /// <param name="targetType">The binding target type.</param>
        /// <param name="parameter">The binding converter parameter.</param>
        /// <param name="culture">The binding culture.</param>
        /// <returns>The translated string, or the original value when it is not translatable.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is string text) || string.IsNullOrEmpty(text))
                return value;

            return (Catalog ?? GettextExtension.DefaultCatalog)?.GetString(text) ?? text;
        }

        /// <summary>
        /// Returns the value unchanged because gettext conversion is one-way.
        /// </summary>
        /// <param name="value">The target binding value.</param>
        /// <param name="targetType">The binding source type.</param>
        /// <param name="parameter">The binding converter parameter.</param>
        /// <param name="culture">The binding culture.</param>
        /// <returns>The unchanged value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

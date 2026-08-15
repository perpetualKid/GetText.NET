using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

using Samples.Shared;

using GetText;
using GetText.Wpf;

namespace Samples.Hello
{
    public partial class MainWindow : Window
    {
        private const string LocaleDirectory = ".\\..\\..\\..\\..\\..\\Shared\\Locales";

        private readonly ObjectPropertiesStore store = new ObjectPropertiesStore();
        private readonly Dictionary<Languages, string> valueToDescriptionMap = new Dictionary<Languages, string>();
        private readonly string enumDescription;
        private CultureInfo currentCulture;
        private Languages currentLanguage;

        public MainWindow()
        {
            InitializeComponent();
            // Assembly-name pattern: the WinForms and WPF samples share the Samples.Hello assembly
            // name, while CatalogManager<Languages> resolves the Samples.Shared catalog for the title.
            CatalogManager.SetCatalogDomainPattern(CatalogDomainPattern.AssemblyName, null, LocaleDirectory);

            enumDescription = typeof(Languages).GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .Select(x => x.Description)
                .FirstOrDefault();

            foreach (Languages value in (Languages[])Enum.GetValues(typeof(Languages)))
            {
                FieldInfo field = typeof(Languages).GetField(value.ToString());
                valueToDescriptionMap[value] = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .Cast<DescriptionAttribute>()
                    .Select(x => x.Description)
                    .FirstOrDefault();
            }

            OnLocaleChanged(rbEnUs, new RoutedEventArgs());
        }

        private void SetTexts()
        {
            // The WinForms and WPF samples share the Samples.Hello assembly name, so CatalogManager
            // resolves the very same gettext catalog for both UIs.
            ICatalog catalog = CatalogManager.Catalog;
            GettextExtension.DefaultCatalog = catalog;
            Localizer.Localize(this, catalog, store);

            processTextBlock.Text = ProcessDetails.GetProcessIdText();
            singlePluralTextBlock.Text = catalog.GetPluralString($"found {1} similar word", $"found {1} similar words", 1);
            twoPluralTextBlock.Text = catalog.GetPluralString("found {0} similar word", $"found {0} similar words", 2);
            fivePluralTextBlock.Text = catalog.GetPluralString($"found {5} similar word", $"found {5} similar words", 5);
            computersContextTextBlock.Text = string.Format(currentCulture, "{0} ('computers')", catalog.GetParticularString("Computers", "Text encoding"));
            militaryContextTextBlock.Text = $"{catalog.GetParticularString("Military", "Text encoding")} ('military')";
            plainContextTextBlock.Text = $"{catalog.GetString("Text encoding")} (non contextual)";
            Title += " " + CatalogManager<Languages>.Catalog.GetParticularString(enumDescription, valueToDescriptionMap[currentLanguage]);

            string[] encodings =
            {
                catalog.GetString("Text encoding"),
                catalog.GetParticularString("Computers", "Text encoding"),
                catalog.GetParticularString("Military", "Text encoding"),
            };
            translationListView.ItemsSource = encodings;
            translationDataGrid.ItemsSource = encodings;
        }

        private void OnLocaleChanged(object sender, RoutedEventArgs e)
        {
            string locale = "en-US";
            if (sender == rbFrFr)
            {
                locale = "fr-FR";
                currentLanguage = Languages.French;
            }
            else if (sender == rbRuRu)
            {
                locale = "ru-RU";
                currentLanguage = Languages.Russian;
            }
            else
            {
                currentLanguage = Languages.English;
            }

            currentCulture = new CultureInfo(locale);
            Thread.CurrentThread.CurrentCulture = currentCulture;
            Thread.CurrentThread.CurrentUICulture = currentCulture;
            Localizer.Revert(this, store);
            CatalogManager.Reset();
            SetTexts();
        }
    }
}

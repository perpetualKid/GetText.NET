
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using Examples.OtherLibrary;

using GetText;
using GetText.WindowsForms;

namespace Examples.HelloForms
{
    public partial class Form1 : Form
    {
		private readonly ObjectPropertiesStore store = new ObjectPropertiesStore();
		private CultureInfo currentCulture;
        private Languages currentLanguage;

        private readonly Dictionary<Languages, string> valueToDescriptionMap = new Dictionary<Languages, string>();
        private readonly string enumDescription;

        public Form1()
        {
            InitializeComponent();
            CatalogManager.SetCatalogDomainPattern(CatalogDomainPattern.AssemblyName, null, ".\\..\\..\\..\\..\\Locales");


            enumDescription = typeof(Languages).GetCustomAttributes(typeof(DescriptionAttribute), false).
                Cast<DescriptionAttribute>().
                Select(x => x.Description).
                FirstOrDefault();

            foreach (Languages value in ((Languages[])Enum.GetValues(typeof(Languages))))
            {
                FieldInfo field = typeof(Languages).GetField(value.ToString());
                valueToDescriptionMap[value] = field.GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .Cast<DescriptionAttribute>()
                            .Select(x => x.Description)
                            .FirstOrDefault();
            }

            OnLocaleChanged(this, new EventArgs());
        }

        private void SetTexts()
		{
            ICatalog catalog = CatalogManager.Catalog;//new Catalog("messages");
			Localizer.Localize(this, catalog, store);
            // We need pass 'store' argument only to be able revert original text and switch languages on fly
            // Common use case doesn't required it: Localizer.Localize(this, catalog);

            // Manually formatted strings
            //label2.Text = catalog.GetString("This program is running as process number\r \"{0}\".",
            //                                   System.Diagnostics.Process.GetCurrentProcess().Id);
            label2.Text = ProcessDetails.GetProcessIdText();
            label3.Text = catalog.GetPluralString($"found {1} similar word", $"found {1} similar words", 1);
//            label4.Text = catalog.GetPluralString($"found {2} similar word", $"found {2} similar words", 2);
            label4.Text = catalog.GetPluralString("found {0} similar word", $"found {0} similar words", 2);
            label5.Text = catalog.GetPluralString($"found {5} similar word", $"found {5} similar words", 5);
            label6.Text = string.Format(currentCulture, "{0} ('computers')", catalog.GetParticularString("Computers", "Text encoding"));
            label7.Text = $"{catalog.GetParticularString("Military", "Text encoding")} ('military')";
            label8.Text = $"{catalog.GetString("Text encoding")} (non contextual)";

            Text += " " + CatalogManager<Languages>.Catalog.GetParticularString(enumDescription, valueToDescriptionMap[currentLanguage]);
		}

		private void OnLocaleChanged(object sender, EventArgs e)
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
			System.Threading.Thread.CurrentThread.CurrentUICulture = currentCulture;
			Localizer.Revert(this, store);
            CatalogManager.Reset();
			SetTexts();
		}

	}
}

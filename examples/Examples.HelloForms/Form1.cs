
using System;
using System.Globalization;
using System.Windows.Forms;

using GetText;
using GetText.WindowsForms;

namespace Examples.HelloForms
{
    public partial class Form1 : Form
    {
		private ObjectPropertiesStore store = new ObjectPropertiesStore();
		private CultureInfo currentCulture;

		public Form1()
        {
            InitializeComponent();
			OnLocaleChanged(this, new EventArgs());
		}

		private void SetTexts()
		{
			ICatalog catalog = new Catalog("messages");
			Localizer.Localize(this, catalog, store);
            // We need pass 'store' argument only to be able revert original text and switch languages on fly
            // Common use case doesn't required it: Localizer.Localize(this, catalog);

            // Manually formatted strings
            //label2.Text = catalog.GetString("This program is running as process number\r \"{0}\".",
            //                                   System.Diagnostics.Process.GetCurrentProcess().Id);
#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
            label2.Text = catalog.GetString($"This program is running as process number \"{System.Diagnostics.Process.GetCurrentProcess().Id}\".");
            label3.Text = catalog.GetPluralString($"found {1} similar word", $"found {1} similar words", 1);
//            label4.Text = catalog.GetPluralString($"found {2} similar word", $"found {2} similar words", 2);
            label4.Text = catalog.GetPluralString("found {0} similar word", $"found {0} similar words", 2);
            label5.Text = catalog.GetPluralString($"found {5} similar word", $"found {5} similar words", 5);
            label6.Text = string.Format(currentCulture, "{0} ('computers')", catalog.GetParticularString("Computers", "Text encoding"));
            label7.Text = $"{catalog.GetParticularString("Military", "Text encoding")} ('military')";
            label8.Text = $"{catalog.GetString("Text encoding")} (non contextual)";
#pragma warning restore HAA0601 // Value type to reference type conversion causing boxing allocation
		}

		private void OnLocaleChanged(object sender, EventArgs e)
		{
			string locale = "en-US";
			if (sender == rbFrFr)
			{
				locale = "fr-FR";
			}
			else if (sender == rbRuRu)
			{
				locale = "ru-RU";
			}
			currentCulture = new CultureInfo(locale);
			System.Threading.Thread.CurrentThread.CurrentUICulture = currentCulture;
			Localizer.Revert(this, store);
			SetTexts();
		}

	}
}

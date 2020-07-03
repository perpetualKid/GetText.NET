
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

		public Form1()
        {
            InitializeComponent();
			rbEnUs.PerformClick();
		}

		private void SetTexts()
		{
			ICatalog catalog = new Catalog("messages", "./locales");
			// If satellite assemblies have another base name use GettextResourceManager("Examples.HelloForms.Messages") constructor
			// If you call from another assembly, use GettextResourceManager(anotherAssembly) constructor
			Localizer.Localize(this, catalog, store);
			// We need pass 'store' argument only to be able revert original text and switch languages on fly
			// Common use case doesn't required it: Localizer.Localize(this, catalog);

			// Manually formatted strings
			label2.Text = catalog.GetString("This program is running as process number \"{0}\".",
											   System.Diagnostics.Process.GetCurrentProcess().Id);
			label3.Text = string.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 1),
				1);
			label4.Text = string.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 2),
				2);
			label5.Text = string.Format(
				catalog.GetPluralString("found {0} similar word", "found {0} similar words", 5),
				5);
			label6.Text = string.Format("{0} ('computers')", catalog.GetParticularString("Computers", "Text encoding"));
			label7.Text = string.Format("{0} ('military')", catalog.GetParticularString("Military", "Text encoding"));
			label8.Text = string.Format("{0} (non contextual)", catalog.GetString("Text encoding"));
			//label8.Text = string.Format("{0} (non contextual)", catalog.GetString("Text not found"));
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
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
			Localizer.Revert(this, store);
			SetTexts();
		}

	}
}

using System.Globalization;
using System.IO;
using Xunit;

namespace GetText.Tests
{
	public class CatalogFileLoadingTest
	{
		private readonly string localesDir;

		public CatalogFileLoadingTest()
		{
			this.localesDir = Path.Combine(Directory.GetCurrentDirectory(), Path.Combine("TestResources", "locales"));
		}

		[Fact]
		public void TestEmpty()
		{
			var t = new Catalog();

			Assert.Empty(t.Translations);
			Assert.Equal(CultureInfo.CurrentUICulture, t.CultureInfo);

			t = new Catalog(new CultureInfo("fr"));
			Assert.Equal(new CultureInfo("fr"), t.CultureInfo);
		}

		[Fact]
		public void TestStream()
		{
			using (var stream = File.OpenRead(Path.Combine(this.localesDir, Path.Combine("ru_RU", "Test.mo"))))
			{
				var t = new Catalog(stream, new CultureInfo("ru-RU"));
				TestLoadedTranslation(t);
			}
		}

		[Fact]
		public void TestLocaleDir()
		{
			var t = new Catalog("Test", this.localesDir, new CultureInfo("ru-RU"));
			TestLoadedTranslation(t);
		}

        private static void TestLoadedTranslation(ICatalog t)
		{
			Assert.Equal("тест", t.GetString("test"));
			Assert.Equal("тест2", t.GetString("test2"));
			Assert.Equal("1 минута", t.GetPluralString("{0} minute", "{0} minutes", 1, 1));
			Assert.Equal("5 минут", t.GetPluralString("{0} minute", "{0} minutes", 5, 5));

			Assert.Equal("тест3контекст1", t.GetParticularString("context1", "test3"));
			Assert.Equal("тест3контекст2", t.GetParticularString("context2", "test3"));
		}

	}
}
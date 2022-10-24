using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GetText.Extractor.Engine;
using GetText.Extractor.Template;

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetText.Extractor.Tests.Engine
{
    [TestClass]
    public class ParserTests
    {
        private string outputFile;
        CatalogTemplate catalog;

        [TestInitialize]
        public async Task Initialize()
        {
            outputFile = Path.GetTempFileName();

            catalog = new CatalogTemplate(outputFile);

            SyntaxTreeParser parser = new SyntaxTreeParser(catalog, new List<FileInfo>() { new FileInfo(".\\..\\..\\..\\..\\Sample\\ExtractorTestSample\\ExtractorTestSample.csproj") }, false, false, new Aliases());
            await parser.Parse().ConfigureAwait(false);
        }

        [TestCleanup]
        public void Clenaup()
        {
            //no cleanup needed when catalog is not saved 
        }

        [TestMethod]
        public void InputLineNumberCheck()
        {
            ExtractorTestSample.Program.Main(null);
        }

        [TestMethod]
        public void LiteralStringTest() => TestValidity(22, "Simple literal string");

        [TestMethod]
        public void LiteralStringWithParamTest() => TestValidity(23, "Simple literal string with param {0}");

        [TestMethod]
        public void InterpolatedStringNoParamTest() => TestValidity(24, "Simple interpolated string");

        [TestMethod]
        public void InterpolatedStringOneParamTest() => TestValidity(25, "Simple {0} interpolated string");

        [TestMethod]
        public void InterpolatedStringTwoParamTest() => TestValidity(26, "{0} and {1} params interpolated string");

        [TestMethod]
        public void InterpolatedStringNestedLiteralStringTest() => TestValidity(27, "Nested interpolated string {0} literal param");

        [TestMethod]
        public void InterpolatedStringNestedTranslatedStringTest()
        {
            TestValidity(28, "0 - Nested interpolated string {0} param");
            TestValidity(28, "1 - Literal string", 1);
        }

        [TestMethod]
        public void InterpolatedStringTwoNestedTranslatedStringTest()
        {
            TestValidity(29, "0 - Another nested interpolated string with {0} and {1} param");
            TestValidity(29, "1 - one literal string", 1);
            TestValidity(29, "2 - another literal string", 2);
        }

        [TestMethod]
        public void InterpolatedStringTripleNestedTranslatedStringTest()
        {
            TestValidity(30, "0 - Triple nested {0}");
            TestValidity(30, "1 - double nested {0}", 1);
            TestValidity(30, "2 - single nested {0}", 2);
            TestValidity(30, "3 - Inner literal string", 3);
        }

        [TestMethod]
        public void SimpleConcatenatedStringTest() => TestValidity(31, "Simple concatenation");

        [TestMethod]
        public void ConcatenatedStringTest() => TestValidity(32, "first part and another part");

        [TestMethod]
        public void Issue44Test() => TestValidity(35, "{0}login {1} {2} - Logs in using your username and password.");

        private void TestValidity(int line, string expected, int resultNumber = 0)
        {
            CatalogEntry entry = catalog.entries.Values.Where((entry) => entry.References.Where(reference => reference.EndsWith($":{line}")).Any()).OrderBy(entry => entry.MessageId).Skip(resultNumber).FirstOrDefault();
            Assert.IsNotNull(entry);
            Assert.AreEqual(expected, entry.MessageId);
        }
    }
}

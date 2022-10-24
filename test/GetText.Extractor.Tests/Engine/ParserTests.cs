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
        public void LiteralStringTest() => TestValidity(16, "Simple literal string");

        [TestMethod]
        public void LiteralStringWithParamTest() => TestValidity(17, "Simple literal string with param {0}");

        [TestMethod]
        public void InterpolatedStringNoParamTest() => TestValidity(18, "Simple interpolated string");

        [TestMethod]
        public void InterpolatedStringOneParamTest() => TestValidity(19, "Simple {0} interpolated string");

        [TestMethod]
        public void InterpolatedStringTwoParamTest() => TestValidity(20, "{0} and {1} params interpolated string");

        [TestMethod]
        public void InterpolatedStringNestedLiteralStringTest() => TestValidity(21, "Nested interpolated string {0} literal param");

        [TestMethod]
        public void InterpolatedStringNestedTranslatedStringTest()
        {
            TestValidity(22, "0 - Nested interpolated string {0} param");
            TestValidity(22, "1 - Literal string", 1);
        }

        [TestMethod]
        public void InterpolatedStringTwoNestedTranslatedStringTest()
        {
            TestValidity(23, "0 - Another nested interpolated string with {0} and {1} param");
            TestValidity(23, "1 - one literal string", 1);
            TestValidity(23, "2 - another literal string", 2);
        }

        [TestMethod]
        public void InterpolatedStringTripleNestedTranslatedStringTest()
        {
            TestValidity(24, "0 - Triple nested {0}");
            TestValidity(24, "1 - double nested {0}", 1);
            TestValidity(24, "2 - single nested {0}", 2);
            TestValidity(24, "3 - Inner literal string", 3);
        }

        [TestMethod]
        public void SimpleConcatenatedStringTest() => TestValidity(25, "Simple concatenation");

        [TestMethod]
        public void ConcatenatedStringTest() => TestValidity(26, "first part and another part");

        [TestMethod]
        public void Issue44Test() => TestValidity(29, "{0}login {1} {2} - Logs in using your username and password.");

        private void TestValidity(int line, string expected, int resultNumber = 0)
        {
            CatalogEntry entry = catalog.entries.Values.Where((entry) => entry.References.Where(reference => reference.EndsWith($":{line}")).Any()).OrderBy(entry => entry.MessageId).Skip(resultNumber).FirstOrDefault();
            Assert.IsNotNull(entry);
            Assert.AreEqual(expected, entry.MessageId);
        }
    }
}

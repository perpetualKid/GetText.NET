using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GetText.Extractor.Template;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetText.Extractor.Tests.Template
{
    [TestClass]
    public class CatalogEntryTests
    {
        [TestMethod]
        public void CatalogEntryCtorTest()
        {
            CatalogEntry entry = new CatalogEntry("MessageId");
            Assert.IsFalse(entry.HasPlural);
        }

        [TestMethod]
        public void CatalogEntryWrapLongLineTest()
        {
            CatalogEntry entry = new CatalogEntry("Restoring wrong weather type : trying to restore dynamic weather but save contains user controlled weather");

            string test = entry.ToString();
        }
    }
}

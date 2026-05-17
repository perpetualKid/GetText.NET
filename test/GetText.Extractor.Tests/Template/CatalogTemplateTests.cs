using System.IO;
using System.Linq;
using System.Threading.Tasks;

using GetText.Extractor.Template;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GetText.Extractor.Tests.Template
{
    [TestClass]
    public class CatalogTemplateTests
    {
        [TestMethod]
        public void DuplicateReferenceIsAddedOnlyOnce()
        {
            CatalogTemplate catalog = new CatalogTemplate(Path.GetTempFileName());

            // Add the identical reference 2 times
            catalog.AddOrUpdateEntry(null, "Shared text", "File1.cs:10", false);
            catalog.AddOrUpdateEntry(null, "Shared text", "File1.cs:10", false);

            CatalogEntry entry = catalog.entries.Single().Value;

            // Only a single reference is expected
            Assert.AreEqual(1, entry.References.Count);
            Assert.AreEqual("File1.cs:10", entry.References.Single());
        }

        [TestMethod]
        public void DuplicatePluralReferenceIsAddedOnlyOnce()
        {
            CatalogTemplate catalog = new CatalogTemplate(Path.GetTempFileName());

            // Add the identical reference 2 times
            catalog.AddOrUpdateEntry(null, "One file", "Many files", "File1.cs:10", false);
            catalog.AddOrUpdateEntry(null, "One file", "Many files", "File1.cs:10", false);

            CatalogEntry entry = catalog.entries.Single().Value;

            // Only a single reference is expected
            Assert.AreEqual(1, entry.References.Count);
            Assert.AreEqual("File1.cs:10", entry.References.Single());
        }
        [TestMethod]
        public async Task ConcurrentReferenceUpdatesKeepAllUniqueReferences()
        {
            CatalogTemplate catalog = new CatalogTemplate(Path.GetTempFileName());
            string[] references = Enumerable.Range(1, 256).Select(i => $"File{i}.cs:10").ToArray();

            await Parallel.ForEachAsync(references, (reference, token) =>
            {
                catalog.AddOrUpdateEntry(null, "Shared text", reference, false);
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);

            CatalogEntry entry = catalog.entries.Single().Value;
            Assert.AreEqual(references.Length, entry.References.Count);
            CollectionAssert.AreEquivalent(references, entry.References);
        }
    }
}

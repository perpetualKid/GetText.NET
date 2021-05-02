using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace GetText
{
    /// <summary>
    /// Determines the pattern to be used when trying to load a catalog
    /// </summary>
    public enum CatalogDomainPattern
    {
        /// <summary>
        /// Catalog domain is derived from Assembly name, so the catalog name should be same as assembly file name
        /// ie. for GetText.Net.dll the catalog would be loaded from GetText.Net.mo
        /// </summary>
        AssemblyName,

        /// <summary>
        /// Catalog domain name is a constant value, such as 'message', so it will load a catalog 'messages.mo'
        /// Only a single catalog is supported!
        /// </summary>
        Static,

        /// <summary>
        /// Catalog domain name is build from given pattern.
        /// Valid pattern arguments are
        /// {AssemblyName}
        /// {CultureName} (in CultureInfo.Name format "languagecode2-country/regioncode2")
        /// </summary>
        FormatPattern,
    }

    /// <summary>
    /// CatalogManager is a static holder type to cache access to catalog.
    /// While it can work for a single catalog as well, a typical scenario would be when each assembly has it's own catalog, 
    /// such as multiple libraries and executables in larger solutions.
    /// Catalog name will be derived from assembly name, or some pattern (as defined by format string)
    /// </summary>
    public class CatalogManager
    {
        private protected static readonly Dictionary<string, Catalog> catalogs = new Dictionary<string, Catalog>();
        private protected static CatalogDomainPattern catalogDomainPattern;
        private protected static string pattern;
        private protected static string folder;

        public static Catalog Catalog
        {
            get
            {
                string domain = null;
                Catalog catalog;
                switch (catalogDomainPattern)
                {
                    case CatalogDomainPattern.AssemblyName:
                        domain = Assembly.GetCallingAssembly().GetName().Name;
                        break;
                    case CatalogDomainPattern.Static:
                        domain = pattern;
                        break;
                    case CatalogDomainPattern.FormatPattern:
                        domain = pattern.Replace("{AssemblyName}", Assembly.GetCallingAssembly().GetName().Name).Replace("{CultureName}", CultureInfo.CurrentCulture.Name);
                        break;
                }
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    if (!catalogs.TryGetValue(domain, out catalog))
                    {
                        catalog = string.IsNullOrEmpty(folder) ? new Catalog(domain) : new Catalog(domain, folder);
                        catalogs.Add(domain, catalog);
                    }
                }
                else
                {
                    catalog = new Catalog();
                    catalogs.Add(domain, catalog);
                }
                return catalog;
            }
        }

        /// <summary>
        /// Remove all existing catalogs, i.e. when switching languages
        /// </summary>
        public static void Reset()
        {
            catalogs.Clear();
        }

        /// <summary>
        /// Initialization - set the pattern type, the name/pattern, and optionally a folder path
        /// </summary>
        public static void SetCatalogDomainPattern(CatalogDomainPattern patternType, string pattern, string folder = null)
        {
            catalogDomainPattern = patternType;
            CatalogManager.pattern = pattern;
            CatalogManager.folder = folder;
        }
    }

    public class CatalogManager<T> : CatalogManager where T : Enum
    {
        public static new Catalog Catalog
        {
            get
            {
                string domain = null;
                Catalog catalog;
                switch (catalogDomainPattern)
                {
                    case CatalogDomainPattern.AssemblyName:
                        domain = typeof(T).Assembly.GetName().Name;
                        break;
                    case CatalogDomainPattern.Static:
                        domain = pattern;
                        break;
                    case CatalogDomainPattern.FormatPattern:
                        domain = pattern.Replace("{AssemblyName}", typeof(T).Assembly.GetName().Name).Replace("{CultureName}", CultureInfo.CurrentCulture.Name);
                        break;
                }
                if (!string.IsNullOrWhiteSpace(domain))
                {
                    if (!catalogs.TryGetValue(domain, out catalog))
                    {
                        catalog = string.IsNullOrEmpty(folder) ? new Catalog(domain) : new Catalog(domain, folder);
                        catalogs.Add(domain, catalog);
                    }
                }
                else
                {
                    catalog = new Catalog();
                    catalogs.Add(domain, catalog);
                }
                return catalog;
            }
        }

    }

}

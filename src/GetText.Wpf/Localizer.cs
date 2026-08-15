using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace GetText.Wpf
{
    public class Localizer
    {
        private static readonly string[] StringProperties = new[] { "Text", "Title" };
        private static readonly string[] ObjectTextProperties = new[] { "Content", "Header", "ToolTip" };

        private readonly DependencyObject root;

        public ICatalog Catalog { get; private set; }
        public ObjectPropertiesStore OriginalTextStore { get; private set; }

        public Localizer(DependencyObject rootElement, string resourceBaseName)
            : this(rootElement, new Catalog(resourceBaseName), null)
        {
        }

        public Localizer(DependencyObject rootElement, string resourceBaseName, ObjectPropertiesStore originalTextStore)
            : this(rootElement, new Catalog(resourceBaseName), originalTextStore)
        {
        }

        public Localizer(DependencyObject rootElement, ICatalog catalog)
            : this(rootElement, catalog, null)
        {
        }

        public Localizer(DependencyObject rootElement, ICatalog catalog, ObjectPropertiesStore originalTextStore)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            OriginalTextStore = originalTextStore;
            root = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
        }

        public static void Localize(DependencyObject element, ICatalog catalog)
        {
            Localize(element, catalog, null);
        }

        public static void Localize(DependencyObject element, ICatalog catalog, ObjectPropertiesStore originalTextStore)
        {
            if (catalog == null)
                return;

            Localizer localizer = new Localizer(element, catalog, originalTextStore);
            localizer.Localize();
        }

        public static void Revert(DependencyObject element, ObjectPropertiesStore originalTextStore)
        {
            Localizer localizer = new Localizer(element, new Catalog(), originalTextStore);
            localizer.Revert();
        }

        public virtual void Localize()
        {
            IterateElements(root, false, new HashSet<DependencyObject>());
        }

        public virtual void Revert()
        {
            IterateElements(root, true, new HashSet<DependencyObject>());
        }

        protected virtual void HandleElement(DependencyObject element, bool revert)
        {
            switch (element)
            {
                case DataGrid dataGrid:
                    foreach (DataGridColumn column in dataGrid.Columns)
                    {
                        if (revert)
                            RevertProperty(column, "Header");
                        else
                            LocalizeProperty(column, "Header");
                    }
                    break;
                case ListView listView when listView.View is GridView gridView:
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (revert)
                            RevertProperty(column, "Header");
                        else
                            LocalizeProperty(column, "Header");
                    }
                    break;
            }

            foreach (string propertyName in StringProperties)
            {
                if (revert)
                    RevertProperty(element, propertyName);
                else
                    LocalizeProperty(element, propertyName);
            }

            foreach (string propertyName in ObjectTextProperties)
            {
                if (revert)
                    RevertProperty(element, propertyName);
                else
                    LocalizeProperty(element, propertyName);
            }
        }

        private void IterateElements(DependencyObject element, bool revert, HashSet<DependencyObject> visited)
        {
            if (element == null || !visited.Add(element))
                return;

            foreach (object child in LogicalTreeHelper.GetChildren(element))
            {
                if (child is DependencyObject dependencyObject)
                    IterateElements(dependencyObject, revert, visited);
            }

            int childrenCount = element is Visual || element is Visual3D ? VisualTreeHelper.GetChildrenCount(element) : 0;
            for (int index = 0; index < childrenCount; index++)
            {
                IterateElements(VisualTreeHelper.GetChild(element, index), revert, visited);
            }

            HandleElement(element, revert);
        }

        private void LocalizeProperty(object source, string propertyName)
        {
            if (IsDataBound(source, propertyName))
                return;

            PropertyInfo pi = source.GetType().GetProperty(propertyName);
            if (pi == null || !pi.CanRead || !pi.CanWrite)
                return;

            object value = pi.GetValue(source, null);
            if (!(value is string text) || string.IsNullOrEmpty(text))
                return;

            StoreIfOriginal(source, propertyName, value);
            pi.SetValue(source, Catalog.GetString(text), null);
        }

        private void RevertProperty(object source, string propertyName)
        {
            if (OriginalTextStore == null || IsDataBound(source, propertyName))
                return;

            object originalValue = OriginalTextStore.GetState(source, propertyName);
            if (originalValue == null)
                return;

            PropertyInfo pi = source.GetType().GetProperty(propertyName);
            if (pi != null && pi.CanWrite)
                pi.SetValue(source, originalValue, null);
        }

        private void StoreIfOriginal(object source, string propertyName, object value)
        {
            if (OriginalTextStore != null && OriginalTextStore.GetState(source, propertyName) == null)
                OriginalTextStore.SetState(source, propertyName, value);
        }

        private static bool IsDataBound(object source, string propertyName)
        {
            if (!(source is DependencyObject dependencyObject))
                return false;

            DependencyProperty dependencyProperty = GetDependencyProperty(source.GetType(), propertyName);
            return dependencyProperty != null && BindingOperations.GetBindingBase(dependencyObject, dependencyProperty) != null;
        }

        private static DependencyProperty GetDependencyProperty(Type type, string propertyName)
        {
            FieldInfo field = type.GetField(propertyName + "Property", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return field?.GetValue(null) as DependencyProperty;
        }
    }
}

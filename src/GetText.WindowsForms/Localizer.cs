using System;
using System.ComponentModel;
#if DEBUG
using System.Diagnostics;
#endif
using System.Reflection;
using System.Windows.Forms;

namespace GetText.WindowsForms
{
    public class Localizer
    {
        public delegate void OnIterateControl(Control control);

        public ICatalog Catalog { get; private set; }
        public ObjectPropertiesStore OriginalTextStore { get; private set; }
        public ToolTipControls ToolTips { get; } = new ToolTipControls();
        protected readonly Control root;

        #region Constructors
        public Localizer(Control rootControl, string resourceBaseName)
            : this(rootControl, new Catalog(resourceBaseName), null)
        {
        }

        public Localizer(Control rootControl, string resourceBaseName, ObjectPropertiesStore originalTextStore)
            : this(rootControl, new Catalog(resourceBaseName), originalTextStore)
        {
        }

        public Localizer(Control rootControl, ICatalog catalog)
            : this(rootControl, catalog, null)
        {
        }

        public Localizer(Control rootControl, ICatalog catalog, ObjectPropertiesStore originalTextStore)
        {
            Catalog = catalog;
            OriginalTextStore = originalTextStore;
            root = rootControl ?? throw new ArgumentNullException(nameof(rootControl));

            // Access to form components
            // Try access by container
            IterateControls(root,
                           delegate (Control control)
                           {
                               InitFromContainer(control.Container);
                           });
            // Access by private member
            for (Control c = root; c != null; c = c.Parent)
            {
                if (c is Form || c is UserControl)
                {
                    FieldInfo fi = c.GetType().GetField("components", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fi != null)
                    {
                        InitFromContainer((IContainer)fi.GetValue(c));
                    }
                }
            }
        }

        protected virtual void InitFromContainer(IContainer container)
        {
            if (container?.Components == null)
                return;
            foreach (Component component in container.Components)
            {
                if (component is ToolTip)
                {
                    if (!ToolTips.Contains(component as ToolTip))
                        ToolTips.Add(component as ToolTip);
                }
            }
        }
        #endregion

        #region Public interface
        public static void Localize(Control control, ICatalog catalog)
        {
            Localize(control, catalog, null);
        }

        public static void Localize(Control control, ICatalog catalog, ObjectPropertiesStore originalTextStore)
        {
            if (catalog == null)
                return;
            Localizer loc = new Localizer(control, catalog, originalTextStore);
            loc.Localize();
        }

        public static void Revert(Control control, ObjectPropertiesStore originalTextStore)
        {
            Localizer loc = new Localizer(control, new Catalog(), originalTextStore);//TODO new GettextResourceManager(), originalTextStore);
            loc.Revert();
        }

        public void Localize()
        {
            IterateControls(root, IterateMode.Localize);
        }

        public void Revert()
        {
            IterateControls(root, IterateMode.Revert);
        }

        #endregion

        #region Handlers for different types
        protected enum IterateMode
        {
            Localize,
            Revert
        }

        private void IterateControlHandler(LocalizableObjectAdapter adapter, IterateMode mode)
        {
            switch (mode)
            {
                case IterateMode.Localize:
#if DEBUG
                    Debug.WriteLine($"Localizing '{adapter}'");
#endif
                    adapter.Localize(Catalog);
                    break;
                case IterateMode.Revert:
#if DEBUG
                    Debug.WriteLine($"Reverting '{adapter}'");
#endif
                    adapter.Revert();
                    break;
            }
        }

        #endregion

        protected virtual void IterateControls(Control control, OnIterateControl onIterateControl)
        {
            foreach (Control child in control.Controls)
            {
                IterateControls(child, onIterateControl);
            }

            onIterateControl?.Invoke(control);
        }


        protected virtual void IterateControls(Control control, IterateMode mode)
        {
            foreach (Control child in control.Controls)
            {
                IterateControls(child, mode);
            }
            HandleControl(control, mode);
        }

        protected virtual void HandleControl(Control control, IterateMode mode)
        {
            switch (control)
            {
                case DataGridView gridView:
                    foreach (DataGridViewColumn col in gridView.Columns)
                    {
                        IterateControlHandler(new LocalizableObjectAdapter(col, OriginalTextStore, ToolTips), mode);
                    }
                    break;
                case ListView listView:
                    foreach (ColumnHeader header in listView.Columns)
                    {
                        IterateControlHandler(new LocalizableObjectAdapter(header, OriginalTextStore, ToolTips), mode);
                    }
                    break;
                case ToolStrip toolStrip:
                    foreach (ToolStripItem item in toolStrip.Items)
                    {
                        IterateToolStripItems(item, mode);
                    }
                    break;
            }
            IterateControlHandler(new LocalizableObjectAdapter(control, OriginalTextStore, ToolTips), mode);
        }

        protected virtual void IterateToolStripItems(ToolStripItem item, IterateMode mode)
        {
            if (item is ToolStripDropDownItem toolStripDropDownItem)
            {
                foreach (ToolStripItem subitem in toolStripDropDownItem.DropDownItems)
                {
                    IterateToolStripItems(subitem, mode);
                }
            }
            IterateControlHandler(new LocalizableObjectAdapter(item, OriginalTextStore, ToolTips), mode);
        }
    }
}

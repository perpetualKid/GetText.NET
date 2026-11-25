using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;

using GetText.Wpf;

using Xunit;

namespace GetText.Wpf.Tests
{
    public class LocalizerTests
    {
        [Fact]
        public void RealizedMenuPreservesTextAndAccessKeysAcrossLanguageSwitches()
        {
            OnStaThread(() =>
            {
                MenuItem item = new MenuItem { Header = "_File" };
                Menu menu = new Menu();
                menu.Items.Add(item);
                WithWindow(menu, window =>
                {
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    RecordingCatalog catalog = new RecordingCatalog();
                    AccessText initialText = FindVisual<AccessText>(item);
                    AccessKeyPressedEventArgs keyEvent = new AccessKeyPressedEventArgs("F");
                    initialText.RaiseEvent(keyEvent);
                    Assert.Same(item, keyEvent.Target);
                    object scope = keyEvent.Scope ?? PresentationSource.FromVisual(window);
                    Assert.Equal("_File", initialText.Text);
                    Assert.True(AccessKeyManager.IsKeyRegistered(scope, "F"));

                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        string translated = cycle == 1 ? "[_Dossier !!!]" : "[_F\u00ef\u2113\u00e8 !!!]";
                        char accessKey = cycle == 1 ? 'D' : 'F';
                        catalog.Translations["_File"] = new[] { translated };
                        catalog.Inputs.Clear();
                        Localizer.Localize(window, catalog, store);
                        FlushLayout(window);
                        AccessText accessText = FindVisual<AccessText>(item);
                        Assert.Equal(translated, item.Header);
                        Assert.Equal(item.Header, accessText.Text);
                        Assert.Equal(accessKey, accessText.AccessKey);
                        Assert.Equal(new[] { "_File" }, catalog.Inputs);
                        Assert.True(AccessKeyManager.IsKeyRegistered(scope, accessKey.ToString()));
                        if (accessKey == 'D')
                            Assert.False(AccessKeyManager.IsKeyRegistered(scope, "F"));

                        Localizer.Revert(window, store);
                        FlushLayout(window);
                        accessText = FindVisual<AccessText>(item);
                        Assert.Equal("_File", item.Header);
                        Assert.Equal("_File", accessText.Text);
                        Assert.Equal('F', accessText.AccessKey);
                        Assert.True(AccessKeyManager.IsKeyRegistered(scope, "F"));
                        Assert.False(AccessKeyManager.IsKeyRegistered(scope, "D"));
                    }
                });
            });
        }

        [Fact]
        public void RealizedStringContentIsTranslatedOnlyAtItsOwner()
        {
            OnStaThread(() =>
            {
                Button button = new Button { Content = "Continue" };
                WithWindow(button, window =>
                {
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("Continue", new[] { "Proceed" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    TextBlock text = FindVisual<TextBlock>(button);
                    Assert.Equal("Continue", text.Text);
                    Localizer.Localize(window, catalog, store);
                    FlushLayout(window);
                    Assert.Equal("Proceed", button.Content);
                    Assert.Equal("Proceed", FindVisual<TextBlock>(button).Text);
                    Assert.Equal(new[] { "Continue" }, catalog.Inputs);
                    Localizer.Revert(window, store);
                    FlushLayout(window);
                    Assert.Equal("Continue", button.Content);
                    Assert.Equal("Continue", FindVisual<TextBlock>(button).Text);
                });
            });
        }

        [Fact]
        public void BoundHeadersAndTextStayBoundIncludingTheirGeneratedInlines()
        {
            OnStaThread(() =>
            {
                TextBlock source = new TextBlock { Text = "_File" };
                MenuItem item = new MenuItem();
                Binding headerBinding = new Binding("Text") { Source = source };
                item.SetBinding(MenuItem.HeaderProperty, headerBinding);
                Menu menu = new Menu();
                menu.Items.Add(item);
                TextBlock text = new TextBlock();
                Binding textBinding = new Binding("Text") { Source = source };
                text.SetBinding(TextBlock.TextProperty, textBinding);
                // Materialize the Run generated by TextBlock.Text before traversal.
                Assert.Single(text.Inlines);
                StackPanel panel = new StackPanel();
                panel.Children.Add(menu);
                panel.Children.Add(text);
                WithWindow(panel, window =>
                {
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("_File", new[] { "_Translated" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, catalog, store);
                    Localizer.Revert(window, store);
                    FlushLayout(window);
                    Assert.Empty(catalog.Inputs);
                    Assert.Same(headerBinding, BindingOperations.GetBinding(item, MenuItem.HeaderProperty));
                    Assert.Same(textBinding, BindingOperations.GetBinding(text, TextBlock.TextProperty));
                    Assert.Equal("_File", FindVisual<AccessText>(item).Text);
                    Assert.Equal("_File", text.Text);
                    source.Text = "_Edit";
                    FlushLayout(window);
                    Assert.Equal("_Edit", item.Header);
                    Assert.Equal("_Edit", FindVisual<AccessText>(item).Text);
                    Assert.Equal("_Edit", text.Text);
                });
            });
        }

        [Fact]
        public void AuthoredInlinesRetainFormattingAndBindingsAcrossLanguageSwitches()
        {
            OnStaThread(() =>
            {
                Run first = new Run("Hello");
                Run second = new Run(" world");
                Bold bold = new Bold(first);
                TextBlock source = new TextBlock { Text = " bound" };
                Run bound = new Run();
                Binding binding = new Binding("Text") { Source = source };
                bound.SetBinding(Run.TextProperty, binding);
                TextBlock text = new TextBlock();
                text.Inlines.Add(bold);
                text.Inlines.Add(second);
                text.Inlines.Add(bound);
                WithWindow(text, window =>
                {
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("Hello", new[] { "Bonjour" });
                    catalog.Translations.Add(" world", new[] { " monde" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        catalog.Inputs.Clear();
                        Localizer.Localize(window, catalog, store);
                        Assert.Equal("Bonjour monde bound", text.Text);
                        Assert.Equal(new[] { "Hello", " world" }, catalog.Inputs);
                        Assert.Equal(new Inline[] { bold, second, bound }, text.Inlines);
                        Assert.Same(bold, first.Parent);
                        Assert.Same(binding, BindingOperations.GetBinding(bound, Run.TextProperty));
                        Localizer.Revert(window, store);
                        Assert.Equal("Hello world bound", text.Text);
                        Assert.Equal(new Inline[] { bold, second, bound }, text.Inlines);
                        Assert.Equal("Hello", first.Text);
                        Assert.Equal(" world", second.Text);
                    }
                });
            });
        }

        [Fact]
        public void AuthoredAccessTextRemainsLocalizableWithoutOverwritingTemplateValues()
        {
            OnStaThread(() =>
            {
                AccessText accessText = new AccessText { Text = "_Open" };
                Button button = new Button { Content = "Continue" };
                button.Template = (ControlTemplate)XamlReader.Parse(
                    "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='Button'>" +
                    "<StackPanel><TextBlock Text='{TemplateBinding Content}'/><TextBlock Text='Template label'/></StackPanel>" +
                    "</ControlTemplate>");
                StackPanel panel = new StackPanel();
                panel.Children.Add(accessText);
                panel.Children.Add(button);
                WithWindow(panel, window =>
                {
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("_Open", new[] { "_Load" });
                    catalog.Translations.Add("Continue", new[] { "Proceed" });
                    catalog.Translations.Add("Template label", new[] { "Template translation" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, catalog, store);
                    FlushLayout(window);
                    Assert.Equal("_Load", accessText.Text);
                    Assert.Equal('L', accessText.AccessKey);
                    Assert.Equal(new[] { "Proceed", "Template label" }, Visuals<TextBlock>(button).Select(value => value.Text));
                    Assert.Equal(new[] { "_Open", "Continue" }, catalog.Inputs);
                    Localizer.Revert(window, store);
                    FlushLayout(window);
                    Assert.Equal("_Open", accessText.Text);
                    Assert.Equal('O', accessText.AccessKey);
                    Assert.Equal(new[] { "Continue", "Template label" }, Visuals<TextBlock>(button).Select(value => value.Text));
                });
            });
        }

        [Fact]
        public void TraversalSnapshotsLogicalAndVisualChildrenBeforeWrites()
        {
            OnStaThread(() =>
            {
                StackPanel panel = new StackPanel();
                TextBlock first = new TextBlock { Text = "First" };
                TextBlock second = new TextBlock { Text = "Second" };
                panel.Children.Add(first);
                panel.Children.Add(second);
                WithWindow(panel, window =>
                {
                    RemovingLocalizer localizer = new RemovingLocalizer(panel, first, second);
                    localizer.Localize();
                    Assert.Equal(new DependencyObject[] { first, second, panel }, localizer.Handled);
                    panel.Children.Add(second);
                    localizer.Handled.Clear();
                    localizer.Revert();
                    Assert.Equal(new DependencyObject[] { first, second, panel }, localizer.Handled);
                });
            });
        }

        private static void OnStaThread(Action action)
        {
            ExceptionDispatchInfo failure = null;
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    failure = ExceptionDispatchInfo.Capture(exception);
                }
                finally
                {
                    Dispatcher.CurrentDispatcher.InvokeShutdown();
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            failure?.Throw();
        }

        private static void WithWindow(UIElement content, Action<Window> action)
        {
            Window window = new Window { Content = content, Width = 400, Height = 200, ShowInTaskbar = false };
            try
            {
                window.Show();
                FlushLayout(window);
                Assert.True(window.IsVisible);
                action(window);
            }
            finally
            {
                window.Close();
            }
        }

        private static void FlushLayout(Window window)
        {
            window.UpdateLayout();
            window.Dispatcher.Invoke(DispatcherPriority.ContextIdle, new Action(() => { }));
            window.UpdateLayout();
        }

        private static T FindVisual<T>(DependencyObject root) where T : DependencyObject
        {
            return Assert.Single(Visuals<T>(root));
        }

        private static IEnumerable<T> Visuals<T>(DependencyObject root) where T : DependencyObject
        {
            for (int index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, index);
                if (child is T match)
                    yield return match;
                foreach (T descendant in Visuals<T>(child))
                    yield return descendant;
            }
        }

        private sealed class RecordingCatalog : Catalog
        {
            public List<string> Inputs { get; } = new List<string>();

            public override string GetString(FormattableStringAdapter text)
            {
                Inputs.Add(text.String);
                return base.GetString(text);
            }
        }

        private sealed class RemovingLocalizer : Localizer
        {
            private readonly StackPanel panel;
            private readonly DependencyObject first;
            private readonly UIElement second;

            public List<DependencyObject> Handled { get; } = new List<DependencyObject>();

            public RemovingLocalizer(StackPanel panel, DependencyObject first, UIElement second)
                : base(panel, new Catalog())
            {
                this.panel = panel;
                this.first = first;
                this.second = second;
            }

            protected override void HandleElement(DependencyObject element, bool revert)
            {
                Handled.Add(element);
                if (element == first)
                    panel.Children.Remove(second);
            }
        }
    }
}
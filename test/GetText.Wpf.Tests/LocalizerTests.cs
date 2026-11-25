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

        [Theory]
        [InlineData(false, BindingMode.OneWay, false)]
        [InlineData(false, BindingMode.TwoWay, false)]
        [InlineData(false, BindingMode.OneWay, true)]
        [InlineData(false, BindingMode.TwoWay, true)]
        [InlineData(true, BindingMode.OneWay, false)]
        [InlineData(true, BindingMode.TwoWay, true)]
        public void ComboBoxSelectionSurvivesLocalizationAndReversion(bool editable, BindingMode mode, bool bindValue)
        {
            OnStaThread(() =>
            {
                LanguageOption first = new LanguageOption("", "System default");
                LanguageOption second = new LanguageOption("qps-ploc", "Pseudo language");
                LanguageOption[] options = new[] { first, second };
                TextBlock source = new TextBlock { Tag = bindValue ? (object)first.Code : first };
                ComboBox picker = new ComboBox
                {
                    IsEditable = editable,
                    DisplayMemberPath = "DisplayName",
                    SelectedValuePath = "Code",
                    ToolTip = "Select language"
                };
                Binding itemsBinding = new Binding { Source = options };
                Binding selectionBinding = new Binding("Tag") { Source = source, Mode = mode };
                DependencyProperty selectionProperty = bindValue ? ComboBox.SelectedValueProperty : ComboBox.SelectedItemProperty;
                picker.SetBinding(ItemsControl.ItemsSourceProperty, itemsBinding);
                picker.SetBinding(selectionProperty, selectionBinding);
                TextBlock label = new TextBlock { Text = "Language" };
                StackPanel panel = new StackPanel();
                panel.Children.Add(label);
                panel.Children.Add(picker);
                WithWindow(panel, window =>
                {
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, new Catalog(), store);
                    // Old package versions may already have captured selection-derived text.
                    store.SetState(picker, "Text", first.DisplayName);
                    List<string> requests = new List<string>();
                    picker.SelectionChanged += (sender, args) =>
                    {
                        foreach (LanguageOption added in args.AddedItems)
                            requests.Add(added.Code);
                    };
                    source.Tag = bindValue ? (object)second.Code : second;
                    FlushLayout(window);
                    Assert.Equal(new[] { second.Code }, requests);
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("Language", new[] { "[Language]" });
                    catalog.Translations.Add("Select language", new[] { "[Select language]" });
                    catalog.Translations.Add(first.DisplayName, new[] { "[System default]" });
                    catalog.Translations.Add(second.DisplayName, new[] { "[Pseudo language]" });

                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        Localizer.Revert(window, store);
                        FlushLayout(window);
                        Assert.Same(second, picker.SelectedItem);
                        Assert.Equal(second.Code, picker.SelectedValue);
                        Assert.Equal(second.DisplayName, picker.Text);
                        Assert.Equal("Language", label.Text);
                        Assert.Equal("Select language", picker.ToolTip);
                        Localizer.Localize(window, catalog, store);
                        FlushLayout(window);
                        Assert.Same(second, picker.SelectedItem);
                        Assert.Equal(second.Code, picker.SelectedValue);
                        Assert.Equal(second.DisplayName, picker.Text);
                        Assert.Equal("[Language]", label.Text);
                        Assert.Equal("[Select language]", picker.ToolTip);
                        Assert.Same(selectionBinding, BindingOperations.GetBinding(picker, selectionProperty));
                        Assert.Same(itemsBinding, BindingOperations.GetBinding(picker, ItemsControl.ItemsSourceProperty));
                        Assert.Equal(bindValue ? (object)second.Code : second, source.Tag);
                        Assert.Equal(new[] { second.Code }, requests);
                        Assert.DoesNotContain(first.DisplayName, catalog.Inputs);
                        Assert.DoesNotContain(second.DisplayName, catalog.Inputs);
                        if (editable)
                            Assert.Equal(second.DisplayName, FindVisual<TextBox>(picker).Text);
                        else
                            Assert.Contains(Visuals<TextBlock>(picker), text => text.Text == second.DisplayName);
                    }
                });
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void LocalizingComboBoxDoesNotTranslateSelectedText(bool editable)
        {
            OnStaThread(() =>
            {
                ComboBox picker = new ComboBox { IsEditable = editable };
                picker.Items.Add("System default");
                picker.Items.Add("Pseudo language");
                picker.SelectedIndex = 1;
                WithWindow(picker, window =>
                {
                    int changes = 0;
                    picker.SelectionChanged += (sender, args) => changes++;
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("Pseudo language", new[] { "System default" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, catalog, store);
                    FlushLayout(window);
                    Assert.Equal(1, picker.SelectedIndex);
                    Assert.Equal("Pseudo language", picker.SelectedItem);
                    Assert.Equal("Pseudo language", picker.Text);
                    Assert.Equal(0, changes);
                    Assert.Empty(catalog.Inputs);
                    Assert.Null(store.GetState(picker, "Text"));
                });
            });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void EditableComboBoxPreservesFreeText(bool textBound)
        {
            OnStaThread(() =>
            {
                ComboBox picker = new ComboBox { IsEditable = true, ToolTip = "Select language" };
                picker.Items.Add("System default");
                picker.Items.Add("Pseudo language");
                TextBlock source = new TextBlock { Text = "Original input" };
                Binding binding = new Binding("Text") { Source = source, Mode = BindingMode.TwoWay };
                if (textBound)
                    picker.SetBinding(ComboBox.TextProperty, binding);
                else
                    picker.Text = source.Text;
                WithWindow(picker, window =>
                {
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, new Catalog(), store);
                    store.SetState(picker, "Text", "Original input");
                    TextBox editor = FindVisual<TextBox>(picker);
                    editor.Text = "User input";
                    FlushLayout(window);
                    Assert.Equal("User input", picker.Text);
                    Assert.Null(picker.SelectedItem);
                    int changes = 0;
                    picker.SelectionChanged += (sender, args) => changes++;
                    RecordingCatalog catalog = new RecordingCatalog();
                    catalog.Translations.Add("User input", new[] { "Translated input" });
                    catalog.Translations.Add("Select language", new[] { "[Select language]" });
                    for (int cycle = 0; cycle < 3; cycle++)
                    {
                        Localizer.Revert(window, store);
                        Assert.Equal("User input", picker.Text);
                        Localizer.Localize(window, catalog, store);
                        FlushLayout(window);
                        Assert.Equal("User input", picker.Text);
                        Assert.Equal("User input", editor.Text);
                        Assert.Null(picker.SelectedItem);
                        Assert.Equal(-1, picker.SelectedIndex);
                        Assert.Equal(0, changes);
                        Assert.DoesNotContain("User input", catalog.Inputs);
                        Assert.Equal("[Select language]", picker.ToolTip);
                        if (textBound)
                        {
                            Assert.Same(binding, BindingOperations.GetBinding(picker, ComboBox.TextProperty));
                            Assert.Equal("User input", source.Text);
                        }
                    }
                });
            });
        }

        [Fact]
        public void EditableComboBoxProtectionDoesNotExcludeOrdinaryTextBoxes()
        {
            OnStaThread(() =>
            {
                TextBox text = new TextBox { Name = "PART_EditableTextBox", Text = "Static caption" };
                WithWindow(text, window =>
                {
                    Catalog catalog = new Catalog();
                    catalog.Translations.Add("Static caption", new[] { "Translated caption" });
                    ObjectPropertiesStore store = new ObjectPropertiesStore();
                    Localizer.Localize(window, catalog, store);
                    Assert.Equal("Translated caption", text.Text);
                    Assert.Equal("Static caption", store.GetState(text, "Text"));
                    Localizer.Revert(window, store);
                    Assert.Equal("Static caption", text.Text);
                });
            });
        }

        private sealed class LanguageOption
        {
            public string Code { get; }
            public string DisplayName { get; }

            public LanguageOption(string code, string displayName)
            {
                Code = code;
                DisplayName = displayName;
            }
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
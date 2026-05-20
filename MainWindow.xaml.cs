using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using smartFinder.Models;

namespace smartFinder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Register custom React syntax highlighting from React-Mode.xshd
        try
        {
            var resourceInfo = Application.GetResourceStream(new Uri("pack://application:,,,/React-Mode.xshd"));
            if (resourceInfo != null)
            {
                using (var stream = resourceInfo.Stream)
                using (var reader = new System.Xml.XmlTextReader(stream))
                {
                    var customDefinition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
                    ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.RegisterHighlighting("React", new[] { ".js", ".jsx", ".ts", ".tsx" }, customDefinition);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load React syntax highlighting: {ex.Message}");
        }

        var viewModel = DataContext as ViewModels.MainViewModel;
        if (viewModel != null)
        {
            // Restore window size and position safely
            if (viewModel.WindowWidth > 100) this.Width = viewModel.WindowWidth;
            if (viewModel.WindowHeight > 100) this.Height = viewModel.WindowHeight;

            // Ensure the window position is on screen to prevent off-screen invisible starts
            if (viewModel.WindowLeft >= 0 && viewModel.WindowLeft < SystemParameters.VirtualScreenWidth - 100)
                this.Left = viewModel.WindowLeft;
            if (viewModel.WindowTop >= 0 && viewModel.WindowTop < SystemParameters.VirtualScreenHeight - 100)
                this.Top = viewModel.WindowTop;

            if (Enum.TryParse(viewModel.WindowState, out WindowState state))
            {
                this.WindowState = state;
            }

            // Theme change event subscription
            viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ViewModels.MainViewModel.ThemeCategory) ||
                    e.PropertyName == nameof(ViewModels.MainViewModel.SelectedSubTheme))
                {
                    ApplyDetailedTheme(viewModel.ThemeCategory, viewModel.SelectedSubTheme);
                }
            };

            // Apply loaded theme initial state
            ApplyDetailedTheme(viewModel.ThemeCategory, viewModel.SelectedSubTheme);
        }
        else
        {
            ApplyDetailedTheme("Light", "Default Light");
        }

        // Enable line highlighting
        textEditor.Options.HighlightCurrentLine = true;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        var viewModel = DataContext as ViewModels.MainViewModel;
        if (viewModel != null)
        {
            // Save normal size and position bounds, not maximized bounds
            if (this.WindowState == WindowState.Normal)
            {
                viewModel.WindowWidth = this.Width;
                viewModel.WindowHeight = this.Height;
                viewModel.WindowLeft = this.Left;
                viewModel.WindowTop = this.Top;
            }
            viewModel.WindowState = this.WindowState.ToString();

            // Save settings to json file via command
            if (viewModel.SaveSettingsCommand.CanExecute(null))
            {
                viewModel.SaveSettingsCommand.Execute(null);
            }
        }
        base.OnClosing(e);
    }

    private void ApplyDetailedTheme(string category, string subTheme)
    {
        Color windowBg, panelBg, textFg, borderBrush, altRowBg, statusBg, editorBg, editorFg, lineNumbersFg, currentLineBg;

        bool isDark = category == "Dark";

        if (isDark)
        {
            switch (subTheme)
            {
                case "SmartFinder Dark":
                    windowBg = Color.FromRgb(15, 23, 42); // Deep Slate Slate-900 (#0f172a)
                    panelBg = Color.FromRgb(30, 41, 59);  // Slate Slate-800 (#1e293b)
                    textFg = Color.FromRgb(241, 245, 249); // Slate Slate-100 (#f1f5f9)
                    borderBrush = Color.FromRgb(51, 65, 85); // Slate Slate-700 (#334155)
                    altRowBg = Color.FromRgb(22, 32, 51); // Slightly darker Slate (#162033)
                    statusBg = Color.FromRgb(30, 58, 138); // Blue Blue-900 Accent (#1e3a8a)
                    editorBg = Color.FromRgb(15, 23, 42); // Deep Slate Slate-900
                    editorFg = Color.FromRgb(248, 250, 252); // Slate Slate-50 (#f8fafc)
                    lineNumbersFg = Color.FromRgb(100, 116, 139); // Slate-500 (#64748b)
                    currentLineBg = Color.FromArgb(40, 255, 255, 255);
                    break;

                case "Monokai":
                    windowBg = Color.FromRgb(30, 31, 28);
                    panelBg = Color.FromRgb(39, 40, 34);
                    textFg = Color.FromRgb(248, 248, 242);
                    borderBrush = Color.FromRgb(62, 62, 56);
                    altRowBg = Color.FromRgb(34, 35, 30);
                    statusBg = Color.FromRgb(24, 25, 22);
                    editorBg = Color.FromRgb(39, 40, 34);
                    editorFg = Color.FromRgb(248, 248, 242);
                    lineNumbersFg = Color.FromRgb(117, 113, 94);
                    currentLineBg = Color.FromArgb(40, 248, 248, 242);
                    break;

                case "Obsidian":
                    windowBg = Color.FromRgb(34, 40, 42);
                    panelBg = Color.FromRgb(41, 49, 52);
                    textFg = Color.FromRgb(224, 226, 228);
                    borderBrush = Color.FromRgb(58, 70, 74);
                    altRowBg = Color.FromRgb(37, 44, 47);
                    statusBg = Color.FromRgb(26, 31, 33);
                    editorBg = Color.FromRgb(41, 49, 52);
                    editorFg = Color.FromRgb(224, 226, 228);
                    lineNumbersFg = Color.FromRgb(102, 116, 123);
                    currentLineBg = Color.FromArgb(40, 255, 255, 255);
                    break;

                case "Ruby Blue":
                    windowBg = Color.FromRgb(12, 28, 43);
                    panelBg = Color.FromRgb(17, 36, 53);
                    textFg = Color.FromRgb(255, 255, 255);
                    borderBrush = Color.FromRgb(29, 61, 90);
                    altRowBg = Color.FromRgb(14, 32, 48);
                    statusBg = Color.FromRgb(8, 20, 31);
                    editorBg = Color.FromRgb(17, 36, 53);
                    editorFg = Color.FromRgb(255, 255, 255);
                    lineNumbersFg = Color.FromRgb(141, 176, 211);
                    currentLineBg = Color.FromArgb(35, 255, 255, 255);
                    break;

                case "Twilight":
                    windowBg = Color.FromRgb(15, 15, 15);
                    panelBg = Color.FromRgb(20, 20, 20);
                    textFg = Color.FromRgb(248, 248, 248);
                    borderBrush = Color.FromRgb(40, 40, 40);
                    altRowBg = Color.FromRgb(17, 17, 17);
                    statusBg = Color.FromRgb(10, 10, 10);
                    editorBg = Color.FromRgb(20, 20, 20);
                    editorFg = Color.FromRgb(248, 248, 248);
                    lineNumbersFg = Color.FromRgb(120, 120, 120);
                    currentLineBg = Color.FromArgb(30, 255, 255, 255);
                    break;

                case "Choco":
                    windowBg = Color.FromRgb(21, 12, 9);
                    panelBg = Color.FromRgb(26, 15, 11);
                    textFg = Color.FromRgb(195, 190, 152);
                    borderBrush = Color.FromRgb(45, 28, 21);
                    altRowBg = Color.FromRgb(23, 13, 10);
                    statusBg = Color.FromRgb(16, 9, 7);
                    editorBg = Color.FromRgb(26, 15, 11);
                    editorFg = Color.FromRgb(195, 190, 152);
                    lineNumbersFg = Color.FromRgb(130, 115, 95);
                    currentLineBg = Color.FromArgb(30, 255, 255, 255);
                    break;

                case "Default Dark":
                default:
                    windowBg = Color.FromRgb(18, 18, 18);
                    panelBg = Color.FromRgb(30, 30, 30);
                    textFg = Color.FromRgb(224, 224, 224);
                    borderBrush = Color.FromRgb(51, 51, 51);
                    altRowBg = Color.FromRgb(38, 38, 38);
                    statusBg = Color.FromRgb(26, 26, 26);
                    editorBg = Color.FromRgb(30, 30, 30);
                    editorFg = Color.FromRgb(212, 212, 212);
                    lineNumbersFg = Color.FromRgb(90, 90, 90);
                    currentLineBg = Color.FromArgb(40, 255, 255, 255);
                    break;
            }
        }
        else
        {
            switch (subTheme)
            {
                case "SmartFinder Light":
                    windowBg = Color.FromRgb(242, 243, 255); // Soft periwinkle-blue background (#f2f3ff)
                    panelBg = Color.FromRgb(255, 255, 255);  // Pure White for clean container cards (#ffffff)
                    textFg = Color.FromRgb(7, 18, 54);       // Deep midnight-blue/cobalt navy for AAA readability (#071236)
                    borderBrush = Color.FromRgb(203, 211, 255); // Soft light periwinkle border (#cbd3ff)
                    altRowBg = Color.FromRgb(250, 248, 255); // Faint periwinkle-white row tint (#faf8ff)
                    statusBg = Color.FromRgb(226, 231, 255); // Rich periwinkle highlight background (#e2e7ff)
                    editorBg = Color.FromRgb(255, 255, 255);
                    editorFg = Color.FromRgb(7, 18, 54);
                    lineNumbersFg = Color.FromRgb(100, 106, 195); // Periwinkle-aligned line numbers (#646ac3)
                    currentLineBg = Color.FromRgb(242, 243, 255);
                    break;

                case "Default Light":
                default:
                    windowBg = Color.FromRgb(245, 245, 245);
                    panelBg = Color.FromRgb(255, 255, 255);
                    textFg = Color.FromRgb(17, 17, 17);
                    borderBrush = Color.FromRgb(204, 204, 204);
                    altRowBg = Color.FromRgb(249, 249, 249);
                    statusBg = Color.FromRgb(234, 234, 234);
                    editorBg = Color.FromRgb(255, 255, 255);
                    editorFg = Color.FromRgb(0, 0, 0);
                    lineNumbersFg = Color.FromRgb(160, 160, 160);
                    currentLineBg = Color.FromRgb(255, 253, 215);
                    break;
            }
        }

        // Update MainWindow instance resources
        Resources["WindowBg"] = new SolidColorBrush(windowBg);
        Resources["PanelBg"] = new SolidColorBrush(panelBg);
        Resources["TextFg"] = new SolidColorBrush(textFg);
        Resources["BorderColor"] = new SolidColorBrush(borderBrush);
        Resources["AltRowBg"] = new SolidColorBrush(altRowBg);
        Resources["StatusBg"] = new SolidColorBrush(statusBg);

        // Update Global Application resources for child windows (like SettingsWindow)
        if (Application.Current != null)
        {
            Application.Current.Resources["WindowBg"] = new SolidColorBrush(windowBg);
            Application.Current.Resources["PanelBg"] = new SolidColorBrush(panelBg);
            Application.Current.Resources["TextFg"] = new SolidColorBrush(textFg);
            Application.Current.Resources["BorderColor"] = new SolidColorBrush(borderBrush);
            Application.Current.Resources["AltRowBg"] = new SolidColorBrush(altRowBg);
            Application.Current.Resources["StatusBg"] = new SolidColorBrush(statusBg);
        }

        // Update AvalonEdit theme colors
        textEditor.Background = new SolidColorBrush(editorBg);
        textEditor.Foreground = new SolidColorBrush(editorFg);
        textEditor.LineNumbersForeground = new SolidColorBrush(lineNumbersFg);
        
        // Update line highlight color depending on dark/light
        textEditor.TextArea.TextView.CurrentLineBackground = new SolidColorBrush(currentLineBg);

        // Adjust syntax highlighting colors dynamically for the new theme
        if (textEditor.SyntaxHighlighting != null)
        {
            AdjustHighlightingColors(textEditor.SyntaxHighlighting, isDark);
        }
    }

    private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        if (dataGrid.SelectedItem is SearchResult result)
        {
            LoadPreviewFile(result.FullPath, result.LineNo, result.MatchedTerms);
        }
        else
        {
            textEditor.Text = string.Empty;
            textEditor.SyntaxHighlighting = null;
        }
    }

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        var dataGrid = (DataGrid)sender;
        if (dataGrid.SelectedItem is SearchResult selectedResult)
        {
            var viewModel = DataContext as ViewModels.MainViewModel;
            if (viewModel != null && viewModel.OpenInExternalEditorCommand.CanExecute(selectedResult))
            {
                viewModel.OpenInExternalEditorCommand.Execute(selectedResult);
            }
        }
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
            DataContext = this.DataContext
        };
        settingsWindow.ShowDialog();

        // Save settings immediately after the settings dialog is closed
        var viewModel = DataContext as ViewModels.MainViewModel;
        if (viewModel != null && viewModel.SaveSettingsCommand.CanExecute(null))
        {
            viewModel.SaveSettingsCommand.Execute(null);
        }
    }

    private void LoadPreviewFile(string filePath, int targetLineNo = 1, string matchedTerms = "")
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                textEditor.Text = string.Empty;
                textEditor.SyntaxHighlighting = null;
                filePathTextBox.Text = "선택된 파일 없음";
                return;
            }

            filePathTextBox.Text = filePath;

            // Read file content safely, handling locks gracefully
            string content;
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            }

            textEditor.Text = content;

            // Move caret and scroll to matched line
            if (targetLineNo <= textEditor.Document.LineCount && targetLineNo > 0)
            {
                var line = textEditor.Document.GetLineByNumber(targetLineNo);
                textEditor.CaretOffset = line.Offset;
                textEditor.ScrollToLine(targetLineNo);
            }

            // Configure syntax highlighting based on file extension automatically
            string ext = Path.GetExtension(filePath);
            if (!string.IsNullOrEmpty(ext))
            {
                // First try direct extension matching via AvalonEdit's built-in manager
                var definition = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinitionByExtension(ext.ToLower());
                
                // Fallback for custom extensions that might not be registered by default
                if (definition == null)
                {
                    string fallbackName = ext.ToLower() switch
                    {
                        ".csproj" or ".config" or ".props" or ".targets" => "XML",
                        ".ts" or ".json" => "JavaScript",
                        _ => null
                    };

                    if (fallbackName != null)
                    {
                        definition = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition(fallbackName);
                    }
                }

                if (definition != null)
                {
                    var viewModel = DataContext as ViewModels.MainViewModel;
                    bool isDarkTheme = viewModel?.ThemeCategory == "Dark";
                    AdjustHighlightingColors(definition, isDarkTheme);
                }

                textEditor.SyntaxHighlighting = definition;
            }
            else
            {
                textEditor.SyntaxHighlighting = null; // Plain text
            }
        }
        catch (Exception ex)
        {
            textEditor.Text = $"파일을 읽을 수 없습니다: {ex.Message}";
            textEditor.SyntaxHighlighting = null;
        }
    }

    private void AdjustHighlightingColors(ICSharpCode.AvalonEdit.Highlighting.IHighlightingDefinition definition, bool isDark)
    {
        if (definition == null) return;

        foreach (var color in definition.NamedHighlightingColors)
        {
            string name = color.Name.ToLowerInvariant();
            Color? newColor = null;

            if (isDark)
            {
                if (name.Contains("comment"))
                {
                    newColor = Color.FromRgb(122, 147, 122); // soft grayish green
                }
                else if (name.Contains("string") || name.Contains("char") || name.Contains("attributevalue"))
                {
                    newColor = Color.FromRgb(206, 145, 120); // warm orange/salmon
                }
                else if (name.Contains("keyword") || name.Contains("visibility") || name.Contains("preprocessor") || name.Contains("tag"))
                {
                    newColor = Color.FromRgb(86, 156, 214); // neon/sky blue
                }
                else if (name.Contains("type") || name.Contains("class") || name.Contains("interface"))
                {
                    newColor = Color.FromRgb(78, 201, 176); // neon teal
                }
                else if (name.Contains("method") || name.Contains("function"))
                {
                    newColor = Color.FromRgb(220, 220, 170); // soft yellow/gold
                }
                else if (name.Contains("number") || name.Contains("digit") || name.Contains("value"))
                {
                    newColor = Color.FromRgb(181, 206, 168); // light green
                }
                else if (name.Contains("attribute") || name.Contains("attributename"))
                {
                    newColor = Color.FromRgb(156, 220, 254); // light blue/cyan
                }
                else if (name.Contains("hook"))
                {
                    newColor = Color.FromRgb(197, 134, 192); // neon violet/pink hooks
                }
                else
                {
                    // For general punctuation, operators, etc., make it light grey
                    newColor = Color.FromRgb(212, 212, 212);
                }
            }
            else
            {
                // Light theme defaults
                if (name.Contains("comment"))
                {
                    newColor = Color.FromRgb(0, 128, 0); // classic green
                }
                else if (name.Contains("string") || name.Contains("char") || name.Contains("attributevalue"))
                {
                    newColor = Color.FromRgb(163, 21, 21); // classic dark red
                }
                else if (name.Contains("keyword") || name.Contains("visibility") || name.Contains("tag"))
                {
                    newColor = Color.FromRgb(0, 0, 255); // classic blue
                }
                else if (name.Contains("type") || name.Contains("class") || name.Contains("interface"))
                {
                    newColor = Color.FromRgb(38, 139, 210); // slate teal
                }
                else if (name.Contains("method") || name.Contains("function"))
                {
                    newColor = Color.FromRgb(121, 94, 38); // brownish yellow
                }
                else if (name.Contains("number") || name.Contains("digit") || name.Contains("value"))
                {
                    newColor = Color.FromRgb(9, 134, 115); // dark cyan-green
                }
                else if (name.Contains("attribute") || name.Contains("attributename"))
                {
                    newColor = Color.FromRgb(255, 0, 0); // red attribute names
                }
                else if (name.Contains("preprocessor"))
                {
                    newColor = Color.FromRgb(163, 73, 164); // purple
                }
                else if (name.Contains("hook"))
                {
                    newColor = Color.FromRgb(163, 21, 163); // rich classic purple hooks
                }
                else
                {
                    newColor = Color.FromRgb(0, 0, 0); // black
                }
            }

            if (newColor.HasValue)
            {
                color.Foreground = new ICSharpCode.AvalonEdit.Highlighting.SimpleHighlightingBrush(newColor.Value);
            }
        }
    }

        private void CopyPathButton_Click(object sender, RoutedEventArgs e)
        {
            string path = filePathTextBox.Text;
            if (!string.IsNullOrEmpty(path) && path != "선택된 파일 없음")
            {
                try
                {
                    Clipboard.SetText(path);
                    MessageBox.Show("파일 전체 경로가 클립보드에 복사되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"경로 복사 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PopupResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var border = FindParent<Border>(thumb);
                if (border != null)
                {
                    double currentWidth = double.IsNaN(border.Width) ? border.ActualWidth : border.Width;
                    double currentHeight = double.IsNaN(border.Height) ? border.ActualHeight : border.Height;

                    double newWidth = Math.Max(180, currentWidth + e.HorizontalChange);
                    double newHeight = Math.Max(180, currentHeight + e.VerticalChange);

                    border.Width = newWidth;
                    border.Height = newHeight;

                    // Force the parent Popup to recalculate its window position and size.
                    var popup = LogicalTreeHelper.GetParent(border) as Popup;
                    if (popup == null && border.Parent is Popup p)
                    {
                        popup = p;
                    }
                    if (popup != null)
                    {
                        var updateMethod = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (updateMethod != null)
                        {
                            updateMethod.Invoke(popup, null);
                        }
                        else
                        {
                            var offset = popup.HorizontalOffset;
                            popup.HorizontalOffset = offset + 0.1;
                            popup.HorizontalOffset = offset;
                        }
                    }
                }
            }
        }

        private T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            
            return FindParent<T>(parentObject);
        }
    }
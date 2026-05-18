using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ClosedXML.Excel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using smartFinder.Models;
using smartFinder.Services;

namespace smartFinder.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly FileSearchService _searchService = new();
        private CancellationTokenSource _cancellationTokenSource;

        private static readonly string SettingsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartFinder"
        );
        private static readonly string SettingsFilePath = Path.Combine(SettingsDir, "settings.json");

        private class AppSettings
        {
            public string FilePattern { get; set; } = "*.*";
            public string SearchDirectory { get; set; } = @"C:\workproject";
            public bool IncludeSubDirectories { get; set; } = true;
            public bool UseRegex { get; set; } = false;
            public bool MatchCase { get; set; } = false;
            public bool UseAndOperator { get; set; } = false;
            public bool KeepResults { get; set; } = false;
            public string EditorPath { get; set; } = @"C:\Windows\notepad.exe";
            public string EditorArgs { get; set; } = "\"%path%\\%file_name%\" -n%line_no%";
            
            // Window position and sizes
            public double WindowWidth { get; set; } = 1200;
            public double WindowHeight { get; set; } = 700;
            public double WindowLeft { get; set; } = 100;
            public double WindowTop { get; set; } = 100;
            public string WindowState { get; set; } = "Normal";
            
            // Theme settings
            public string ThemeCategory { get; set; } = "Light";
            public string SelectedSubTheme { get; set; } = "SmartFinder Light";
        }

        [ObservableProperty]
        private string searchDirectory = @"C:\workproject";

        [ObservableProperty]
        private string filePattern = "*.*";

        [ObservableProperty]
        private string searchText = "";

        [ObservableProperty]
        private bool includeSubDirectories = true;

        [ObservableProperty]
        private bool useRegex = false;

        [ObservableProperty]
        private bool matchCase = false;

        [ObservableProperty]
        private bool useAndOperator = false;

        [ObservableProperty]
        private bool keepResults = false;

        [ObservableProperty]
        private string editorPath = @"C:\Windows\notepad.exe";

        [ObservableProperty]
        private string editorArgs = "\"%FILE%\"";

        [ObservableProperty]
        private bool isSearching;

        // Window size and location bindings
        [ObservableProperty]
        private double windowWidth = 1200;

        [ObservableProperty]
        private double windowHeight = 700;

        [ObservableProperty]
        private double windowLeft = 100;

        [ObservableProperty]
        private double windowTop = 100;

        [ObservableProperty]
        private string windowState = "Normal";

        // Theme setting binding
        [ObservableProperty]
        private string themeCategory = "Light";

        [ObservableProperty]
        private string selectedSubTheme = "SmartFinder Light";

        public ObservableCollection<string> AvailableSubThemes { get; } = new();

        partial void OnThemeCategoryChanged(string value)
        {
            UpdateAvailableSubThemes(value);
        }

        private void UpdateAvailableSubThemes(string category)
        {
            AvailableSubThemes.Clear();
            if (category == "Dark")
            {
                AvailableSubThemes.Add("SmartFinder Dark");
                AvailableSubThemes.Add("Default Dark");
                AvailableSubThemes.Add("Monokai");
                AvailableSubThemes.Add("Obsidian");
                AvailableSubThemes.Add("Ruby Blue");
                AvailableSubThemes.Add("Twilight");
                AvailableSubThemes.Add("Choco");

                if (string.IsNullOrEmpty(SelectedSubTheme) || !AvailableSubThemes.Contains(SelectedSubTheme))
                {
                    SelectedSubTheme = "SmartFinder Dark";
                }
            }
            else
            {
                AvailableSubThemes.Add("SmartFinder Light");
                AvailableSubThemes.Add("Default Light");

                if (string.IsNullOrEmpty(SelectedSubTheme) || !AvailableSubThemes.Contains(SelectedSubTheme))
                {
                    SelectedSubTheme = "SmartFinder Light";
                }
            }
        }


        [ObservableProperty]
        private string statusMessage = "Ready";

        public ObservableCollection<SearchTab> Tabs { get; } = new();

        [ObservableProperty]
        private SearchTab selectedTab;

        public MainViewModel()
        {
            LoadSettings();
            UpdateAvailableSubThemes(ThemeCategory);
            EnsureDefaultTab();
        }

        private void EnsureDefaultTab()
        {
            if (Tabs.Count == 0)
            {
                var defaultTab = new SearchTab("검색 1");
                Tabs.Add(defaultTab);
                SelectedTab = defaultTab;
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        FilePattern = settings.FilePattern ?? "*.*";
                        SearchDirectory = settings.SearchDirectory ?? @"C:\workproject";
                        IncludeSubDirectories = settings.IncludeSubDirectories;
                        UseRegex = settings.UseRegex;
                        MatchCase = settings.MatchCase;
                        UseAndOperator = settings.UseAndOperator;
                        KeepResults = settings.KeepResults;
                        EditorPath = settings.EditorPath ?? @"C:\Windows\notepad.exe";
                        EditorArgs = settings.EditorArgs ?? "\"%path%\\%file_name%\" -n%line_no%";
                        
                        // Load Window position & size
                        WindowWidth = settings.WindowWidth > 100 ? settings.WindowWidth : 1200;
                        WindowHeight = settings.WindowHeight > 100 ? settings.WindowHeight : 700;
                        WindowLeft = settings.WindowLeft;
                        WindowTop = settings.WindowTop;
                        WindowState = settings.WindowState ?? "Normal";
                        
                        // Load Theme setting
                        ThemeCategory = settings.ThemeCategory ?? "Light";
                        SelectedSubTheme = settings.SelectedSubTheme ?? (ThemeCategory == "Dark" ? "SmartFinder Dark" : "SmartFinder Light");
                    }
                }
            }
            catch (Exception)
            {
                // Ignore load errors
            }
        }

        [RelayCommand]
        public void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(SettingsDir))
                {
                    Directory.CreateDirectory(SettingsDir);
                }

                var settings = new AppSettings
                {
                    FilePattern = FilePattern,
                    SearchDirectory = SearchDirectory,
                    IncludeSubDirectories = IncludeSubDirectories,
                    UseRegex = UseRegex,
                    MatchCase = MatchCase,
                    UseAndOperator = UseAndOperator,
                    KeepResults = KeepResults,
                    EditorPath = EditorPath,
                    EditorArgs = EditorArgs,
                    
                    // Save Window position & size
                    WindowWidth = WindowWidth,
                    WindowHeight = WindowHeight,
                    WindowLeft = WindowLeft,
                    WindowTop = WindowTop,
                    WindowState = WindowState,
                    
                    // Save Theme settings
                    ThemeCategory = ThemeCategory,
                    SelectedSubTheme = SelectedSubTheme
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // Ignore save errors
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        private async Task SearchAsync()
        {
            if (IsSearching)
            {
                _cancellationTokenSource?.Cancel();
                return;
            }

            SaveSettings();

            SearchTab activeTab;
            if (KeepResults)
            {
                int nextNum = 1;
                if (Tabs.Count > 0)
                {
                    var numbers = Tabs
                        .Select(t => t.Header)
                        .Where(h => h != null && h.StartsWith("검색 "))
                        .Select(h => {
                            string numPart = h.Substring(3).Trim();
                            return int.TryParse(numPart, out int n) ? n : 0;
                        })
                        .ToList();
                    if (numbers.Any())
                    {
                        nextNum = numbers.Max() + 1;
                    }
                    else
                    {
                        nextNum = Tabs.Count + 1;
                    }
                }
                activeTab = new SearchTab($"검색 {nextNum}");
                Tabs.Add(activeTab);
                SelectedTab = activeTab;
            }
            else
            {
                var targetTab = SelectedTab ?? Tabs.FirstOrDefault();
                if (targetTab != null)
                {
                    activeTab = targetTab;
                    activeTab.Results.Clear();
                    if (string.IsNullOrEmpty(activeTab.Header) || !activeTab.Header.StartsWith("검색 "))
                    {
                        activeTab.Header = "검색 1";
                    }
                    SelectedTab = activeTab;
                }
                else
                {
                    activeTab = new SearchTab("검색 1");
                    Tabs.Add(activeTab);
                    SelectedTab = activeTab;
                }
            }

            IsSearching = true;
            StatusMessage = "Searching...";
            _cancellationTokenSource = new CancellationTokenSource();

            var criteria = new FileSearchCriteria
            {
                Directory = SearchDirectory,
                FilePattern = FilePattern,
                SearchText = SearchText,
                IncludeSubDirectories = IncludeSubDirectories,
                UseRegex = UseRegex,
                MatchCase = MatchCase,
                UseAndOperator = UseAndOperator
            };

            try
            {
                await _searchService.SearchAsync(criteria, _cancellationTokenSource.Token, result =>
                {
                    Application.Current.Dispatcher.Invoke(() => activeTab.Results.Add(result));
                });

                // Sort results after search completion by Directory -> FileName -> LineNo -> MatchedTerms
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var sortedResults = activeTab.Results
                        .OrderBy(r => r.Directory)
                        .ThenBy(r => r.FileName)
                        .ThenBy(r => r.LineNo)
                        .ThenBy(r => r.MatchedTerms)
                        .ToList();

                    activeTab.Results.Clear();
                    foreach (var result in sortedResults)
                    {
                        activeTab.Results.Add(result);
                    }
                });

                StatusMessage = $"Found {activeTab.Results.Count} files.";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Search canceled.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsSearching = false;
            }
        }

        [RelayCommand]
        private void SelectDirectory()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "검색 폴더 선택",
                InitialDirectory = Directory.Exists(SearchDirectory) ? SearchDirectory : @"C:\workproject"
            };

            if (dialog.ShowDialog() == true)
            {
                SearchDirectory = dialog.FolderName;
            }
        }

        [RelayCommand]
        private void BrowseEditor()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "전용 에디터 실행 파일 선택",
                Filter = "실행 파일 (*.exe)|*.exe|모든 파일 (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
            {
                EditorPath = dialog.FileName;
            }
        }

        [RelayCommand]
        private void DeleteTab(SearchTab tab)
        {
            if (tab == null) return;
            
            int index = Tabs.IndexOf(tab);
            Tabs.Remove(tab);
            
            if (SelectedTab == tab || SelectedTab == null)
            {
                if (Tabs.Count > 0)
                {
                    int newIndex = Math.Min(index, Tabs.Count - 1);
                    SelectedTab = Tabs[newIndex];
                }
            }
            EnsureDefaultTab();
        }

        [RelayCommand]
        private void OpenInExternalEditor(SearchResult result)
        {
            if (result == null) return;

            if (string.IsNullOrWhiteSpace(EditorPath) || !File.Exists(EditorPath))
            {
                // Fallback to default system handler if editor doesn't exist
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = result.FullPath,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"파일을 열 수 없습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            try
            {
                string argsPattern = string.IsNullOrWhiteSpace(EditorArgs) ? "\"%path%\\%file_name%\" -n%line_no%" : EditorArgs;
                string args = argsPattern
                    .Replace("%path%", result.Directory)
                    .Replace("%PATH%", result.Directory)
                    .Replace("%file_name%", result.FileName)
                    .Replace("%FILE_NAME%", result.FileName)
                    .Replace("%line_no%", result.LineNo.ToString())
                    .Replace("%LINE_NO%", result.LineNo.ToString())
                    .Replace("%FILE%", $"\"{result.FullPath}\"")
                    .Replace("%LINE%", result.LineNo.ToString())
                    .Replace("%line%", result.LineNo.ToString())
                    .Replace("LINE", result.LineNo.ToString())
                    .Replace("line", result.LineNo.ToString());

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = EditorPath,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                System.Diagnostics.Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"전용 에디터 실행 실패: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExcelDown()
        {
            try
            {
                if (SelectedTab == null || SelectedTab.Results.Count == 0)
                {
                    MessageBox.Show("다운로드할 검색 결과가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(defaultPath))
                {
                    defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "검색 결과 엑셀(*.xlsx) 내보내기",
                    Filter = "Excel 통합 문서 (*.xlsx)|*.xlsx|CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
                    FileName = $"{SelectedTab.Header.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                    InitialDirectory = defaultPath
                };

                if (dialog.ShowDialog(Application.Current.MainWindow) == true)
                {
                    string ext = Path.GetExtension(dialog.FileName).ToLower();
                    if (ext == ".csv")
                    {
                        ExportAsCsv(dialog.FileName);
                    }
                    else
                    {
                        ExportAsXlsx(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"내보내기 실행 중 예외가 발생했습니다:\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportAsXlsx(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("검색 결과");

                    var headers = new[] { "검색어", "Path", "File Name", "Ext", "Line", "Line Text", "Note" };
                    for (int col = 1; col <= headers.Length; col++)
                    {
                        var cell = worksheet.Cell(1, col);
                        cell.Value = headers[col - 1];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    int row = 2;
                    foreach (var result in SelectedTab.Results)
                    {
                        worksheet.Cell(row, 1).Value = result.MatchedTerms ?? "";
                        worksheet.Cell(row, 2).Value = result.Directory ?? "";
                        worksheet.Cell(row, 3).Value = result.FileName ?? "";
                        worksheet.Cell(row, 4).Value = result.Extension ?? "";
                        worksheet.Cell(row, 5).Value = result.LineNo;
                        worksheet.Cell(row, 6).Value = result.LineText ?? "";
                        worksheet.Cell(row, 7).Value = result.Note ?? "";
                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }

                MessageBox.Show("엑셀 파일이 성공적으로 다운로드되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportAsCsv(string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true))) // UTF8 with BOM for Excel compatibility!
                {
                    writer.WriteLine("검색어,Path,File Name,Ext,Line,Line Text,Note");

                    foreach (var result in SelectedTab.Results)
                    {
                        var row = new[]
                        {
                            EscapeCsvField(result.MatchedTerms),
                            EscapeCsvField(result.Directory),
                            EscapeCsvField(result.FileName),
                            EscapeCsvField(result.Extension),
                            result.LineNo.ToString(),
                            EscapeCsvField(result.LineText),
                            EscapeCsvField(result.Note)
                        };
                        writer.WriteLine(string.Join(",", row));
                    }
                }

                MessageBox.Show("CSV 파일이 성공적으로 다운로드되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CSV 파일 저장 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void ExcelUpload()
        {
            try
            {
                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(defaultPath))
                {
                    defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                }

                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "검색 결과 엑셀/CSV 업로드",
                    Filter = "엑셀/CSV 파일 (*.xlsx;*.csv)|*.xlsx;*.csv|Excel 통합 문서 (*.xlsx)|*.xlsx|CSV 파일 (*.csv)|*.csv|모든 파일 (*.*)|*.*",
                    InitialDirectory = defaultPath
                };

                if (dialog.ShowDialog(Application.Current.MainWindow) == true)
                {
                    string ext = Path.GetExtension(dialog.FileName).ToLower();
                    if (ext == ".csv")
                    {
                        ImportFromCsv(dialog.FileName);
                    }
                    else
                    {
                        ImportFromXlsx(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"업로드 실행 중 예외가 발생했습니다:\n{ex.Message}\n\n상세 정보:\n{ex.StackTrace}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromXlsx(string filePath)
        {
            try
            {
                var newTab = new SearchTab("업로드 결과");
                int lineCount = 0;

                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        MessageBox.Show("엑셀 파일에 시트가 존재하지 않습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var firstRow = worksheet.Row(1);
                    var headers = new List<string>();
                    int lastCol = worksheet.LastColumnUsed()?.ColumnNumber() ?? 6;
                    for (int col = 1; col <= lastCol; col++)
                    {
                        headers.Add(firstRow.Cell(col).GetString().Trim());
                    }

                    int termIdx = headers.FindIndex(h => h.Equals("검색어", StringComparison.OrdinalIgnoreCase));
                    int pathIdx = headers.FindIndex(h => h.Equals("Path", StringComparison.OrdinalIgnoreCase));
                    int fileIdx = headers.FindIndex(h => h.Equals("File Name", StringComparison.OrdinalIgnoreCase));
                    int extIdx = headers.FindIndex(h => h.Equals("Ext", StringComparison.OrdinalIgnoreCase));
                    int lineNoIdx = headers.FindIndex(h => h.Equals("Line", StringComparison.OrdinalIgnoreCase) || h.Equals("Line No", StringComparison.OrdinalIgnoreCase) || h.Equals("라인", StringComparison.OrdinalIgnoreCase));
                    int lineTextIdx = headers.FindIndex(h => h.Equals("Line Text", StringComparison.OrdinalIgnoreCase) || h.Equals("라인 텍스트", StringComparison.OrdinalIgnoreCase));
                    int noteIdx = headers.FindIndex(h => h.Equals("Note", StringComparison.OrdinalIgnoreCase));

                    if (termIdx == -1) termIdx = 0;
                    if (pathIdx == -1) pathIdx = 1;
                    if (fileIdx == -1) fileIdx = 2;
                    if (extIdx == -1) extIdx = 3;
                    if (lineNoIdx == -1) lineNoIdx = 4;
                    if (lineTextIdx == -1) lineTextIdx = 5;
                    if (noteIdx == -1) noteIdx = 6;

                    int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 0;
                    for (int row = 2; row <= lastRow; row++)
                    {
                        var excelRow = worksheet.Row(row);
                        
                        string matchedTerms = excelRow.Cell(termIdx + 1).GetString();
                        string directory = excelRow.Cell(pathIdx + 1).GetString();
                        string fileName = excelRow.Cell(fileIdx + 1).GetString();
                        string ext = excelRow.Cell(extIdx + 1).GetString();
                        
                        int lineNo = 1;
                        string lineNoStr = excelRow.Cell(lineNoIdx + 1).GetString();
                        if (int.TryParse(lineNoStr, out int parsedLineNo))
                        {
                            lineNo = parsedLineNo;
                        }

                        string lineText = excelRow.Cell(lineTextIdx + 1).GetString();
                        string note = excelRow.Cell(noteIdx + 1).GetString();

                        string fullPath = Path.Combine(directory, fileName);
                        
                        var result = new SearchResult(fullPath)
                        {
                            MatchedTerms = matchedTerms,
                            Directory = directory,
                            FileName = fileName,
                            Extension = ext,
                            LineNo = lineNo,
                            LineText = lineText,
                            Note = note
                        };
                        
                        newTab.Results.Add(result);
                        lineCount++;
                    }
                }

                if (lineCount > 0)
                {
                    Tabs.Add(newTab);
                    SelectedTab = newTab;
                    MessageBox.Show($"성공적으로 {lineCount}개의 검색 결과를 로드했습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("불러올 데이터가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"엑셀 업로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportFromCsv(string filePath)
        {
            try
            {
                var newTab = new SearchTab("업로드 결과");
                int lineCount = 0;

                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string? headerLine = reader.ReadLine();
                    if (headerLine == null)
                    {
                        MessageBox.Show("파일이 비어 있습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var headers = ParseCsvLine(headerLine);
                    int termIdx = headers.FindIndex(h => h.Trim().Equals("검색어", StringComparison.OrdinalIgnoreCase));
                    int pathIdx = headers.FindIndex(h => h.Trim().Equals("Path", StringComparison.OrdinalIgnoreCase));
                    int fileIdx = headers.FindIndex(h => h.Trim().Equals("File Name", StringComparison.OrdinalIgnoreCase));
                    int extIdx = headers.FindIndex(h => h.Trim().Equals("Ext", StringComparison.OrdinalIgnoreCase));
                    int lineNoIdx = headers.FindIndex(h => h.Trim().Equals("Line", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("Line No", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("라인", StringComparison.OrdinalIgnoreCase));
                    int lineTextIdx = headers.FindIndex(h => h.Trim().Equals("Line Text", StringComparison.OrdinalIgnoreCase) || h.Trim().Equals("라인 텍스트", StringComparison.OrdinalIgnoreCase));
                    int noteIdx = headers.FindIndex(h => h.Trim().Equals("Note", StringComparison.OrdinalIgnoreCase));

                    if (termIdx == -1) termIdx = 0;
                    if (pathIdx == -1) pathIdx = 1;
                    if (fileIdx == -1) fileIdx = 2;
                    if (extIdx == -1) extIdx = 3;
                    if (lineNoIdx == -1) lineNoIdx = 4;
                    if (lineTextIdx == -1) lineTextIdx = 5;
                    if (noteIdx == -1) noteIdx = 6;

                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var fields = ParseCsvLine(line);
                        if (fields.Count == 0) continue;

                        string matchedTerms = fields.Count > termIdx ? fields[termIdx] : "";
                        string directory = fields.Count > pathIdx ? fields[pathIdx] : "";
                        string fileName = fields.Count > fileIdx ? fields[fileIdx] : "";
                        string ext = fields.Count > extIdx ? fields[extIdx] : "";
                        
                        int lineNo = 1;
                        if (fields.Count > lineNoIdx && int.TryParse(fields[lineNoIdx], out int parsedLineNo))
                        {
                            lineNo = parsedLineNo;
                        }

                        string lineText = (fields.Count > lineTextIdx && lineTextIdx != -1) ? fields[lineTextIdx] : "";
                        string note = fields.Count > noteIdx ? fields[noteIdx] : "";

                        string fullPath = Path.Combine(directory, fileName);
                        
                        var result = new SearchResult(fullPath)
                        {
                            MatchedTerms = matchedTerms,
                            Directory = directory,
                            FileName = fileName,
                            Extension = ext,
                            LineNo = lineNo,
                            LineText = lineText,
                            Note = note
                        };
                        
                        newTab.Results.Add(result);
                        lineCount++;
                    }
                }

                if (lineCount > 0)
                {
                    Tabs.Add(newTab);
                    SelectedTab = newTab;
                    MessageBox.Show($"성공적으로 {lineCount}개의 검색 결과를 로드했습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("불러올 데이터가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"파일 업로드 중 오류가 발생했습니다: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var currentToken = new StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentToken.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentToken.ToString());
                    currentToken.Clear();
                }
                else
                {
                    currentToken.Append(c);
                }
            }
            result.Add(currentToken.ToString());
            return result;
        }

        private string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }
    }
}

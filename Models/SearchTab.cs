using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace smartFinder.Models
{
    public class FilterItem : ObservableObject
    {
        private bool _isChecked = true;
        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        
        public string Value { get; set; } = "";
        public string DisplayText => string.IsNullOrEmpty(Value) ? "(Blank)" : Value;
    }

    public partial class SearchTab : ObservableObject
    {
        [ObservableProperty]
        private string header;

        public ObservableCollection<SearchResult> Results { get; } = new();


        // Excel-like filter selection lists
        public HashSet<string> SelectedMatchedTermsFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedDirectoryFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedFileNameFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedExtensionFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedLineNoFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedLineTextFilters { get; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> SelectedNoteFilters { get; } = new(StringComparer.OrdinalIgnoreCase);

        // Checked item collections for Popups
        public ObservableCollection<FilterItem> MatchedTermsFilterItems { get; } = new();
        public ObservableCollection<FilterItem> DirectoryFilterItems { get; } = new();
        public ObservableCollection<FilterItem> FileNameFilterItems { get; } = new();
        public ObservableCollection<FilterItem> ExtensionFilterItems { get; } = new();
        public ObservableCollection<FilterItem> LineNoFilterItems { get; } = new();
        public ObservableCollection<FilterItem> LineTextFilterItems { get; } = new();
        public ObservableCollection<FilterItem> NoteFilterItems { get; } = new();

        // Popup search text filters
        [ObservableProperty]
        private string matchedTermsSearchText = "";
        [ObservableProperty]
        private string directorySearchText = "";
        [ObservableProperty]
        private string fileNameSearchText = "";
        [ObservableProperty]
        private string extensionSearchText = "";
        [ObservableProperty]
        private string lineNoSearchText = "";
        [ObservableProperty]
        private string lineTextSearchText = "";
        [ObservableProperty]
        private string noteSearchText = "";

        // Popup open states
        [ObservableProperty]
        private bool isMatchedTermsFilterOpen;
        [ObservableProperty]
        private bool isDirectoryFilterOpen;
        [ObservableProperty]
        private bool isFileNameFilterOpen;
        [ObservableProperty]
        private bool isExtensionFilterOpen;
        [ObservableProperty]
        private bool isLineNoFilterOpen;
        [ObservableProperty]
        private bool isLineTextFilterOpen;
        [ObservableProperty]
        private bool isNoteFilterOpen;

        // IsFiltered status flags
        public bool IsMatchedTermsFiltered => SelectedMatchedTermsFilters.Count > 0;
        public bool IsDirectoryFiltered => SelectedDirectoryFilters.Count > 0;
        public bool IsFileNameFiltered => SelectedFileNameFilters.Count > 0;
        public bool IsExtensionFiltered => SelectedExtensionFilters.Count > 0;
        public bool IsLineNoFiltered => SelectedLineNoFilters.Count > 0;
        public bool IsLineTextFiltered => SelectedLineTextFilters.Count > 0;
        public bool IsNoteFiltered => SelectedNoteFilters.Count > 0;

        // ICollectionViews for internal popup listbox searching
        public ICollectionView MatchedTermsFilterItemsView => CollectionViewSource.GetDefaultView(MatchedTermsFilterItems);
        public ICollectionView DirectoryFilterItemsView => CollectionViewSource.GetDefaultView(DirectoryFilterItems);
        public ICollectionView FileNameFilterItemsView => CollectionViewSource.GetDefaultView(FileNameFilterItems);
        public ICollectionView ExtensionFilterItemsView => CollectionViewSource.GetDefaultView(ExtensionFilterItems);
        public ICollectionView LineNoFilterItemsView => CollectionViewSource.GetDefaultView(LineNoFilterItems);
        public ICollectionView LineTextFilterItemsView => CollectionViewSource.GetDefaultView(LineTextFilterItems);
        public ICollectionView NoteFilterItemsView => CollectionViewSource.GetDefaultView(NoteFilterItems);

        private ICollectionView? _resultsView;
        public ICollectionView ResultsView
        {
            get
            {
                if (_resultsView == null)
                {
                    _resultsView = CollectionViewSource.GetDefaultView(Results);
                    _resultsView.Filter = FilterCallback;
                }
                return _resultsView;
            }
        }

        public SearchTab(string header)
        {
            Header = header;

            // Wire up default popup list search filters
            MatchedTermsFilterItemsView.Filter = item => string.IsNullOrEmpty(MatchedTermsSearchText) || (item is FilterItem fi && fi.Value.Contains(MatchedTermsSearchText, StringComparison.OrdinalIgnoreCase));
            DirectoryFilterItemsView.Filter = item => string.IsNullOrEmpty(DirectorySearchText) || (item is FilterItem fi && fi.Value.Contains(DirectorySearchText, StringComparison.OrdinalIgnoreCase));
            FileNameFilterItemsView.Filter = item => string.IsNullOrEmpty(FileNameSearchText) || (item is FilterItem fi && fi.Value.Contains(FileNameSearchText, StringComparison.OrdinalIgnoreCase));
            ExtensionFilterItemsView.Filter = item => string.IsNullOrEmpty(ExtensionSearchText) || (item is FilterItem fi && fi.Value.Contains(ExtensionSearchText, StringComparison.OrdinalIgnoreCase));
            LineNoFilterItemsView.Filter = item => string.IsNullOrEmpty(LineNoSearchText) || (item is FilterItem fi && fi.Value.Contains(LineNoSearchText, StringComparison.OrdinalIgnoreCase));
            LineTextFilterItemsView.Filter = item => string.IsNullOrEmpty(LineTextSearchText) || (item is FilterItem fi && fi.Value.Contains(LineTextSearchText, StringComparison.OrdinalIgnoreCase));
            NoteFilterItemsView.Filter = item => string.IsNullOrEmpty(NoteSearchText) || (item is FilterItem fi && fi.Value.Contains(NoteSearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Trigger updates when popup search text changes
        partial void OnMatchedTermsSearchTextChanged(string value) => MatchedTermsFilterItemsView.Refresh();
        partial void OnDirectorySearchTextChanged(string value) => DirectoryFilterItemsView.Refresh();
        partial void OnFileNameSearchTextChanged(string value) => FileNameFilterItemsView.Refresh();
        partial void OnExtensionSearchTextChanged(string value) => ExtensionFilterItemsView.Refresh();
        partial void OnLineNoSearchTextChanged(string value) => LineNoFilterItemsView.Refresh();
        partial void OnLineTextSearchTextChanged(string value) => LineTextFilterItemsView.Refresh();
        partial void OnNoteSearchTextChanged(string value) => NoteFilterItemsView.Refresh();

        // Populate unique lists when popups are opened
        partial void OnIsMatchedTermsFilterOpenChanged(bool value) { if (value) PopulateFilterItems("MatchedTerms"); }
        partial void OnIsDirectoryFilterOpenChanged(bool value) { if (value) PopulateFilterItems("Directory"); }
        partial void OnIsFileNameFilterOpenChanged(bool value) { if (value) PopulateFilterItems("FileName"); }
        partial void OnIsExtensionFilterOpenChanged(bool value) { if (value) PopulateFilterItems("Extension"); }
        partial void OnIsLineNoFilterOpenChanged(bool value) { if (value) PopulateFilterItems("LineNo"); }
        partial void OnIsLineTextFilterOpenChanged(bool value) { if (value) PopulateFilterItems("LineText"); }
        partial void OnIsNoteFilterOpenChanged(bool value) { if (value) PopulateFilterItems("Note"); }

        private void PopulateFilterItems(string column)
        {
            var uniqueValues = new List<string>();
            switch (column)
            {
                case "MatchedTerms":
                    uniqueValues = Results.Select(r => r.MatchedTerms ?? "").Distinct().OrderBy(s => s).ToList();
                    MatchedTermsSearchText = "";
                    break;
                case "Directory":
                    uniqueValues = Results.Select(r => r.Directory ?? "").Distinct().OrderBy(s => s).ToList();
                    DirectorySearchText = "";
                    break;
                case "FileName":
                    uniqueValues = Results.Select(r => r.FileName ?? "").Distinct().OrderBy(s => s).ToList();
                    FileNameSearchText = "";
                    break;
                case "Extension":
                    uniqueValues = Results.Select(r => r.Extension ?? "").Distinct().OrderBy(s => s).ToList();
                    ExtensionSearchText = "";
                    break;
                case "LineNo":
                    uniqueValues = Results.Select(r => r.LineNo.ToString()).Distinct().OrderBy(s => int.TryParse(s, out int val) ? val : 0).ToList();
                    LineNoSearchText = "";
                    break;
                case "LineText":
                    uniqueValues = Results.Select(r => r.LineText ?? "").Distinct().OrderBy(s => s).Take(1500).ToList();
                    LineTextSearchText = "";
                    break;
                case "Note":
                    uniqueValues = Results.Select(r => r.Note ?? "").Distinct().OrderBy(s => s).ToList();
                    NoteSearchText = "";
                    break;
            }

            ObservableCollection<FilterItem> targetCollection = column switch
            {
                "MatchedTerms" => MatchedTermsFilterItems,
                "Directory" => DirectoryFilterItems,
                "FileName" => FileNameFilterItems,
                "Extension" => ExtensionFilterItems,
                "LineNo" => LineNoFilterItems,
                "LineText" => LineTextFilterItems,
                "Note" => NoteFilterItems,
                _ => throw new ArgumentException()
            };

            HashSet<string> activeFilters = column switch
            {
                "MatchedTerms" => SelectedMatchedTermsFilters,
                "Directory" => SelectedDirectoryFilters,
                "FileName" => SelectedFileNameFilters,
                "Extension" => SelectedExtensionFilters,
                "LineNo" => SelectedLineNoFilters,
                "LineText" => SelectedLineTextFilters,
                "Note" => SelectedNoteFilters,
                _ => throw new ArgumentException()
            };

            targetCollection.Clear();
            foreach (var val in uniqueValues)
            {
                bool isChecked = activeFilters.Count == 0 || activeFilters.Contains(val);
                targetCollection.Add(new FilterItem { Value = val, IsChecked = isChecked });
            }
        }

        [RelayCommand]
        private void ApplyColumnFilter(string column)
        {
            ObservableCollection<FilterItem> items = column switch
            {
                "MatchedTerms" => MatchedTermsFilterItems,
                "Directory" => DirectoryFilterItems,
                "FileName" => FileNameFilterItems,
                "Extension" => ExtensionFilterItems,
                "LineNo" => LineNoFilterItems,
                "LineText" => LineTextFilterItems,
                "Note" => NoteFilterItems,
                _ => throw new ArgumentException()
            };

            HashSet<string> activeFilters = column switch
            {
                "MatchedTerms" => SelectedMatchedTermsFilters,
                "Directory" => SelectedDirectoryFilters,
                "FileName" => SelectedFileNameFilters,
                "Extension" => SelectedExtensionFilters,
                "LineNo" => SelectedLineNoFilters,
                "LineText" => SelectedLineTextFilters,
                "Note" => SelectedNoteFilters,
                _ => throw new ArgumentException()
            };

            activeFilters.Clear();
            
            bool allChecked = items.All(i => i.IsChecked);
            if (!allChecked)
            {
                foreach (var item in items)
                {
                    if (item.IsChecked)
                    {
                        activeFilters.Add(item.Value);
                    }
                }
                
                if (activeFilters.Count == 0)
                {
                    activeFilters.Add("__NO_MATCH_FILTER_VALUE__");
                }
            }

            ResultsView.Refresh();

            // Notify filtered flag changed
            switch (column)
            {
                case "MatchedTerms":
                    OnPropertyChanged(nameof(IsMatchedTermsFiltered));
                    IsMatchedTermsFilterOpen = false;
                    break;
                case "Directory":
                    OnPropertyChanged(nameof(IsDirectoryFiltered));
                    IsDirectoryFilterOpen = false;
                    break;
                case "FileName":
                    OnPropertyChanged(nameof(IsFileNameFiltered));
                    IsFileNameFilterOpen = false;
                    break;
                case "Extension":
                    OnPropertyChanged(nameof(IsExtensionFiltered));
                    IsExtensionFilterOpen = false;
                    break;
                case "LineNo":
                    OnPropertyChanged(nameof(IsLineNoFiltered));
                    IsLineNoFilterOpen = false;
                    break;
                case "LineText":
                    OnPropertyChanged(nameof(IsLineTextFiltered));
                    IsLineTextFilterOpen = false;
                    break;
                case "Note":
                    OnPropertyChanged(nameof(IsNoteFiltered));
                    IsNoteFilterOpen = false;
                    break;
            }
        }

        [RelayCommand]
        private void ClearColumnFilter(string column)
        {
            HashSet<string> activeFilters = column switch
            {
                "MatchedTerms" => SelectedMatchedTermsFilters,
                "Directory" => SelectedDirectoryFilters,
                "FileName" => SelectedFileNameFilters,
                "Extension" => SelectedExtensionFilters,
                "LineNo" => SelectedLineNoFilters,
                "LineText" => SelectedLineTextFilters,
                "Note" => SelectedNoteFilters,
                _ => throw new ArgumentException()
            };

            activeFilters.Clear();
            ResultsView.Refresh();

            switch (column)
            {
                case "MatchedTerms":
                    OnPropertyChanged(nameof(IsMatchedTermsFiltered));
                    IsMatchedTermsFilterOpen = false;
                    break;
                case "Directory":
                    OnPropertyChanged(nameof(IsDirectoryFiltered));
                    IsDirectoryFilterOpen = false;
                    break;
                case "FileName":
                    OnPropertyChanged(nameof(IsFileNameFiltered));
                    IsFileNameFilterOpen = false;
                    break;
                case "Extension":
                    OnPropertyChanged(nameof(IsExtensionFiltered));
                    IsExtensionFilterOpen = false;
                    break;
                case "LineNo":
                    OnPropertyChanged(nameof(IsLineNoFiltered));
                    IsLineNoFilterOpen = false;
                    break;
                case "LineText":
                    OnPropertyChanged(nameof(IsLineTextFiltered));
                    IsLineTextFilterOpen = false;
                    break;
                case "Note":
                    OnPropertyChanged(nameof(IsNoteFiltered));
                    IsNoteFilterOpen = false;
                    break;
            }
        }

        [RelayCommand]
        private void ToggleSelectAll(string column)
        {
            ObservableCollection<FilterItem> items = column switch
            {
                "MatchedTerms" => MatchedTermsFilterItems,
                "Directory" => DirectoryFilterItems,
                "FileName" => FileNameFilterItems,
                "Extension" => ExtensionFilterItems,
                "LineNo" => LineNoFilterItems,
                "LineText" => LineTextFilterItems,
                "Note" => NoteFilterItems,
                _ => throw new ArgumentException()
            };

            if (items.Count == 0) return;
            
            bool allChecked = items.All(i => i.IsChecked);
            foreach (var item in items)
            {
                item.IsChecked = !allChecked;
            }
        }

        private bool FilterCallback(object item)
        {
            if (item is not SearchResult res) return false;

            // 2. Excel Checked Column Filters
            if (SelectedMatchedTermsFilters.Count > 0 && !SelectedMatchedTermsFilters.Contains(res.MatchedTerms ?? ""))
                return false;

            if (SelectedDirectoryFilters.Count > 0 && !SelectedDirectoryFilters.Contains(res.Directory ?? ""))
                return false;

            if (SelectedFileNameFilters.Count > 0 && !SelectedFileNameFilters.Contains(res.FileName ?? ""))
                return false;

            if (SelectedExtensionFilters.Count > 0 && !SelectedExtensionFilters.Contains(res.Extension ?? ""))
                return false;

            if (SelectedLineNoFilters.Count > 0 && !SelectedLineNoFilters.Contains(res.LineNo.ToString()))
                return false;

            if (SelectedLineTextFilters.Count > 0 && !SelectedLineTextFilters.Contains(res.LineText ?? ""))
                return false;

            if (SelectedNoteFilters.Count > 0 && !SelectedNoteFilters.Contains(res.Note ?? ""))
                return false;

            return true;
        }
    }
}

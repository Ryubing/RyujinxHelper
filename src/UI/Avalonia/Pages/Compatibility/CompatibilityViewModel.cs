using CommunityToolkit.Mvvm.ComponentModel;
using Gommon;
using RyuBot.Entities;
using RyuBot.Services;

namespace RyuBot.UI.Avalonia.Pages;

public partial class CompatibilityViewModel : ObservableObject
{
    [ObservableProperty] private CompatibilityCsv _csv = RyujinxBot.Services.Get<CompatibilityCsvService>().Csv;

    [ObservableProperty] private List<CompatibilityEntry> _currentEntries = [];

    public CompatibilityViewModel() => SetEntries(_csv.Entries);

    public void Search(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            SetEntries(Csv.Entries);
            return;
        }

        SetEntries(Csv.Entries.Where(x =>
            x.GameName.ContainsIgnoreCase(searchTerm)
            || x.TitleId.Check(tid => tid.ContainsIgnoreCase(searchTerm))));
    }

    private void SetEntries(IEnumerable<CompatibilityEntry> entries)
    {
#pragma warning disable MVVMTK0034
        _currentEntries = entries.Where(it => !it.Status.IsNullOrEmpty())
            .OrderBy(it => it.GameName).ToList();
        _currentEntries.Sort((e1, e2) => string.CompareOrdinal(e1.GameName, e2.GameName));
#pragma warning restore MVVMTK0034
        OnPropertyChanged(nameof(CurrentEntries));
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using Gommon;
using RyuBot.Entities;
using RyuBot.Services;

namespace RyuBot.UI.Avalonia.Pages;

public partial class CompatibilityViewModel : ObservableObject
{
    [ObservableProperty] private CompatibilityCsv _csv = RyujinxBot.Services.Get<CompatibilityCsvService>().Csv;

    [ObservableProperty] private IEnumerable<CompatibilityEntry> _currentEntries;

    public CompatibilityViewModel()
    {
        _currentEntries = _csv.Entries;
    }

    public void Search(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            CurrentEntries = Csv.Entries;
            return;
        }

        CurrentEntries = Csv.Entries.Where(x =>
            x.GameName.ContainsIgnoreCase(searchTerm)
            || x.TitleId.Check(tid => tid.ContainsIgnoreCase(searchTerm)));
    }
}
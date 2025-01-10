using CommunityToolkit.Mvvm.ComponentModel;
using Gommon;
using RyuBot.Entities;
using RyuBot.Services;

namespace RyuBot.UI.Avalonia.Pages;

public partial class CompatibilityViewModel : ObservableObject
{
    private readonly CompatibilityCsv _csv = RyujinxBot.Services.Get<CompatibilityCsvService>().Csv;

    [ObservableProperty] private IEnumerable<CompatibilityEntry> _currentEntries = [];

    public CompatibilityViewModel() => CurrentEntries = _csv.Entries;

    public void Search(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            CurrentEntries = _csv.Entries;
            return;
        }

        CurrentEntries = _csv.Entries.Where(x =>
            x.GameName.ContainsIgnoreCase(searchTerm)
            || x.TitleId.Check(tid => tid.ContainsIgnoreCase(searchTerm)));
    }
}
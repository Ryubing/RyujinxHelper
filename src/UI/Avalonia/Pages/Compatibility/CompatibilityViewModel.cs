using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using Gommon;
using RyuBot.Entities;
using RyuBot.Services;

namespace RyuBot.UI.Avalonia.Pages;

public partial class CompatibilityViewModel : ObservableObject
{
    [ObservableProperty] private CompatibilityCsv _csv = RyujinxBot.Services.Get<CompatibilityCsvService>().Csv;

    [ObservableProperty] private AvaloniaList<CompatibilityEntry> _currentEntries;

    public CompatibilityViewModel()
    {
        _currentEntries = new AvaloniaList<CompatibilityEntry>(_csv.Entries);
    }

    public void Search(string? searchTerm)
    {
        if (string.IsNullOrEmpty(searchTerm))
        {
            CurrentEntries = new AvaloniaList<CompatibilityEntry>(Csv.Entries);
            return;
        }
        
        CurrentEntries = new AvaloniaList<CompatibilityEntry>(
            Csv.Entries.Where(x => 
                x.GameName.ContainsIgnoreCase(searchTerm)
                || x.TitleId.Check(tid => tid.ContainsIgnoreCase(searchTerm))));
    }
}
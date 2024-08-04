using System.Collections.ObjectModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace Volte.UI.Helpers;

public partial class PageManager : ObservableObject
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<PageManager> _shared = new(() => new PageManager());
    public static PageManager Shared => _shared.Value;

    [ObservableProperty]
    private PageData? _current = null;

    private readonly Dictionary<Page, (int Index, bool IsFooter)> _lookup = [];
    public ObservableCollection<PageData> Pages { get; } = [];
    public ObservableCollection<PageData> FooterPages { get; } = [];

    public PageData this[Page page] {
        get {
            var (index, isFooter) = _lookup[page];
            return (isFooter ? FooterPages : Pages)[index];
        }
    }

    public void Register(Page page, string title, object? content, Symbol icon, string? description = null, bool isDefault = false, bool isFooter = false)
    {
        var source = isFooter ? FooterPages : Pages;
        _lookup[page] = (source.Count, isFooter);

        source.Add(new PageData {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });

        if (isDefault) {
            Focus(page);
        }
    }

    public void Focus(Page page)
    {
        Current = this[page];
    }

    public T Get<T>(Page page) where T : ObservableObject
    {
        var (index, isFooter) = _lookup[page];
        if ((isFooter ? FooterPages : Pages)[index].Content is UserControl { DataContext: T value }) {
            return value;
        }

        throw new InvalidOperationException(
            $"Invalid ViewModel type for '{page}'");
    }
}

public enum Page
{
    Home
}

public class PageData
{
    public required string Title { get; set; }
    public object? Content { get; set; }
    public required Symbol Icon { get; set; }
    public string? Description { get; set; }
}
using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Discord.Interactions;
using FluentAvalonia.UI.Controls;
using Gommon;

namespace Volte.UI.Helpers;

public partial class PageManager : ObservableObject
{
    // ReSharper disable once InconsistentNaming
    private static readonly Lazy<PageManager> _shared = new(() => new PageManager());
    public static PageManager Shared => _shared.Value;

    [ObservableProperty] private Page? _current;

    private readonly Dictionary<PageType, (int Index, bool IsFooter)> _lookup = [];
    public ObservableCollection<Page> Pages { get; } = [];
    public ObservableCollection<Page> FooterPages { get; } = [];

    public Page this[PageType pageType]
    {
        get
        {
            var (index, isFooter) = _lookup[pageType];
            return (isFooter ? FooterPages : Pages)[index];
        }
    }

    public void Register(PageType pageType, string title, object? content, Symbol icon, string? description = null,
        bool isDefault = false, bool isFooter = false)
    {
        var source = isFooter ? FooterPages : Pages;
        _lookup[pageType] = (source.Count, isFooter);

        source.Add(new Page
        {
            Title = title,
            Content = content,
            Description = description,
            Icon = icon
        });

        if (isDefault)
            Focus(pageType);
    }

    public void Focus(PageType pageType)
    {
        Current = this[pageType];
    }


    public T Get<T>(PageType pageType) where T : ObservableObject
    {
        var (index, isFooter) = _lookup[pageType];
        if ((isFooter ? FooterPages : Pages)[index].Content is not UserControl { DataContext: T value })
            throw new InvalidOperationException(
                $"Invalid ViewModel type for '{pageType}'");

        return value;
    }

    #region Attributes

    public void Init(Assembly? assembly = null)
    {
        GetPageTypes(assembly)
            .OrderByDescending(x => x.Attribute.PageType)
            .ForEach(page =>
            {
                var instance = Activator.CreateInstance(page.Type);
                if (instance != null)
                    Register(
                        pageType: page.Attribute.PageType,
                        title: page.Attribute.Title,
                        content: instance,
                        icon: page.Attribute.Icon,
                        description: page.Attribute.Description,
                        isDefault: page.Attribute.IsDefault,
                        isFooter: page.Attribute.IsFooter
                    );
            });
    }

    private static IEnumerable<(Type Type, UiPageAttribute Attribute)> GetPageTypes(Assembly? assembly = null)
        => (assembly ?? Assembly.GetExecutingAssembly()).GetTypes()
            .Where(x => !x.HasAttribute<DontAutoRegisterAttribute>()) 
            // this is a Discord.Interactions attribute but its just a marker and thats what i wanted to add here
            .Where(x => x.HasAttribute<UiPageAttribute>())
            .Select(x => (x, x.GetCustomAttribute<UiPageAttribute>()!));

    #endregion
}

public enum PageType : byte
{
    Logs = 0
}

public class Page
{
    public required string Title { get; set; }
    public required object? Content { get; set; }
    public required Symbol Icon { get; set; }
    public required string? Description { get; set; }
}

[AttributeUsage(AttributeTargets.Class)]
public class UiPageAttribute : Attribute
{
    public UiPageAttribute(PageType pageType, string description, Symbol icon, string? title = null,
        bool isDefault = false, bool isFooter = false)
    {
        PageType = pageType;
        Title = title ?? Enum.GetName(pageType)!;
        Description = description;
        Icon = icon;
        IsDefault = isDefault;
        IsFooter = isFooter;
    }

    public string Title { get; init; }
    public string Description { get; init; }
    public PageType PageType { get; init; }
    public Symbol Icon { get; init; }
    public bool IsDefault { get; init; }
    public bool IsFooter { get; init; }
}
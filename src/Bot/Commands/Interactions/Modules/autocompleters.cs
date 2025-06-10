using Discord.Interactions;

namespace RyuBot.Commands.Interactions.Modules;

public class ReleaseChannelAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        if (context.Channel is INestedChannel { CategoryId: 1372043401540665376 })
            return Task.FromResult(AutocompletionResult.FromSuccess(
                [ 
                    new("Kenji-NX Desktop", GitHubService.KenjinxReleases),
                    new("Kenji-NX Android", GitHubService.KenjinxAndroidReleases)
                ]
            ));
        
        return Task.FromResult(AutocompletionResult.FromSuccess(
            [
                new("Stable", "Stable"),
                new("Canary", "Canary")
            ]
        ));
    }
}

public class AuthorizedReleaseChannelAutocompleter : AutocompleteHandler
{
    public static bool CanMirror(ulong id, string providedValue) =>
        UsersToRepos.TryGetValue(id, out var results) && results.Any(it => providedValue.Equals(it.Value));
    
    private static readonly Dictionary<ulong, AutocompleteResult[]> UsersToRepos = new()
    {
        {
            RequireProjectMaintainerPreconditionAttribute.GreemDev, 
            [
                new("Ryubing Stable", "Stable"),
                new("Ryubing Canary", "Canary")
            ]
        },
        {
            RequireProjectMaintainerPreconditionAttribute.Keaton,
            [
                new("Kenji-NX/Releases", GitHubService.KenjinxReleases),
                new("Kenji-NX Android", GitHubService.KenjinxAndroidReleases)
            ]
        }
    };
    
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services) =>
        Task.FromResult(
            UsersToRepos.TryGetValue(context.User.Id, out var results)
                ? AutocompletionResult.FromSuccess(results)
                : AutocompletionResult.FromError(InteractionCommandError.UnmetPrecondition, "Unauthorized user."));
}

public class TargetProjectAutocompleter : AutocompleteHandler
{
    public static bool CanTarget(ulong id, string targetProject) =>
        UsersToProjects.TryGetValue(id, out var results) && results.Any(it => targetProject.Equals(it.Value));
    
    private static readonly Dictionary<ulong, AutocompleteResult[]> UsersToProjects = new()
    {
        {
            RequireProjectMaintainerPreconditionAttribute.GreemDev, 
            [
                new("Ryubing", "ryubing/ryujinx")
            ]
        },
        {
            RequireProjectMaintainerPreconditionAttribute.Keaton,
            [
                new("Kenji-NX", "kenji-nx/ryujinx")
                //new("Kenji-NX Android", "kenji-nx/ryujinx")
            ]
        }
    };

    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services) =>
        Task.FromResult(
            UsersToProjects.TryGetValue(context.User.Id, out var results)
                ? AutocompletionResult.FromSuccess(results)
                : AutocompletionResult.FromError(InteractionCommandError.UnmetPrecondition, "Unauthorized user."));
}

public class GameCompatibilityNameAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        foreach (var option in autocompleteInteraction.Data.Options)
        {
            var userValue = option.Value?.ToString();

            if (!option.Focused || string.Empty.Equals(userValue)) continue;

            var results = services.Get<CompatibilityCsvService>()
                .SearchEntries(userValue).Take(25).ToArray();

            if (results.Length > 0)
                return Task.FromResult(AutocompletionResult.FromSuccess(
                    results.Select(it => new AutocompleteResult(it.GameName.Truncate(100), it.FormattedTitleId))
                ));
            
            return Task.FromResult(AutocompletionResult.FromSuccess());
        }

        return Task.FromResult(AutocompletionResult.FromSuccess(
            services.Get<CompatibilityCsvService>().Csv.Entries
                .Take(25)
                .Select(it => new AutocompleteResult(it.GameName.Truncate(100), it.FormattedTitleId))
        ));
    }
}

public class WikiPageAutocompleter : AutocompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {
        foreach (var option in autocompleteInteraction.Data.Options)
        {
            var userValue = option.Value?.ToString();

            if (!option.Focused || string.Empty.Equals(userValue)) continue;

            var results = services.Get<GitLabService>()
                .SearchWikiPages(userValue).Take(5).ToArray();

            if (results.Length > 0)
                return Task.FromResult(AutocompletionResult.FromSuccess(
                    results.Select(it => new AutocompleteResult(it.Title, it.Slug))
                ));
        }

        return Task.FromResult(AutocompletionResult.FromSuccess(
            services.Get<GitLabService>().GetWikiPages().Select(it => new AutocompleteResult(it.Title, it.Slug))
        ));
    }
}
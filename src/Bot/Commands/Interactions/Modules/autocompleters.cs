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
        return Task.FromResult(AutocompletionResult.FromSuccess(
            [
                new("Stable", "Stable"),
                new("Canary", "Canary")
            ]
        ));
    }
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
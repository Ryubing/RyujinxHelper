using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Gommon;
using Humanizer;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Volte.Core;
using Volte.Core.Entities;
using Volte.Core.Helpers;

namespace Volte.Services
{
    public class AddonService : IVolteService
    {
        public static FilePath AddonsDir = new("addons", true);
        
        private readonly IServiceProvider _provider;
        private bool _isInitialized;
        public Dictionary<VolteAddon, ScriptState> LoadedAddons { get; }
        internal HashSet<ScriptState> AddonResults { get; }

        public AddonService(IServiceProvider serviceProvider)
        {
            _isInitialized = false;
            _provider = serviceProvider;
            LoadedAddons = new Dictionary<VolteAddon, ScriptState>();
            AddonResults = [];
        }

        private static IEnumerable<VolteAddon> GetAvailableAddons()
        {
            foreach (var dir in AddonsDir.GetSubdirectories())
            {
                if (TryGetAddonContent(dir, out var meta, out var code))
                    yield return new VolteAddon
                    {
                        Meta = meta,
                        Script = code
                    };
                
                if (meta != null && code is null)
                    Logger.Error(LogSource.Service,
                        $"Attempted to load addon {meta.Name} but there were no C# source files in its directory. These are necessary as an addon with no logic does nothing.");
            }
        }

        public async Task InitAsync()
        {
            var sw = Stopwatch.StartNew();
            if (_isInitialized || !AddonsDir.ExistsAsDirectory) return; //don't auto-create a directory; if someone wants to use addons they need to make it themselves.
            if (AddonsDir.GetSubdirectories().Count < 1)
            {
                Logger.Info(LogSource.Service, "No addons are in the addons directory; skipping initialization.");
                return;
            }

            foreach (var addon in GetAvailableAddons())
            {
                try
                {
                    LoadedAddons.Add(addon, await CSharpScript.RunAsync(addon.Script, EvalHelper.Options, new AddonEnvironment(_provider)));
                }
                catch (Exception e)
                {
                    Logger.Error(LogSource.Service, $"Addon {addon.Meta.Name}'s script produced an error.", e);
                }
            }
            sw.Stop();
            Logger.Info(LogSource.Service, $"{"addon".ToQuantity(LoadedAddons.Count)} loaded in {sw.Elapsed.Humanize(2)}.");
            _isInitialized = true;
        }

        private static bool TryGetAddonContent(FilePath addonDir, out VolteAddonMeta meta, out string code)
        {
            meta = null;
            code = null;
            
            foreach (var file in addonDir.GetFiles())
            {
                if (file.Extension is "json")
                {
                    try
                    {
                        meta = JsonSerializer.Deserialize<VolteAddonMeta>(file.ReadAllText(), Config.JsonOptions);
                        
                        if (meta.Name.EqualsIgnoreCase("list"))
                            throw new InvalidOperationException(
                                $"Addon with name {meta.Name} is being ignored because it is using a reserved name. Please change the name or remove the addon.");

                    }
                    catch (JsonException e)
                    {
                        Logger.Error(LogSource.Service, $"Addon meta file '{file}' had invalid JSON contents.", e);
                    }
                    catch (InvalidOperationException e)
                    {
                        meta = null;
                        Logger.Error(LogSource.Service, e.Message);
                    }
                }

                if (file.Extension is "cs")
                    code = file.ReadAllText();
            }

            return meta != null && code != null;
        }
    }
}
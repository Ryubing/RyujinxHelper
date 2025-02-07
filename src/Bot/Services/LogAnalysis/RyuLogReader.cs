using System.Text.RegularExpressions;

namespace RyuBot.Services;

public class RyuLogReader
{
    public LogAnalysis Log;

    double ConvertGiBtoMiB(double GiB)
    {
        return Math.Round(GiB * 1024);
    }

    static bool IsHomebrew(string logFile)
    {
        Match m = Regex.Match(logFile, "Load.*Application: Loading as [Hh]omebrew");
        if (m.Success) return true;
        if (!m.Success) return false;
        else return false;
    }

    static bool IsUsingMetal(string logFile)
    {
        Match m = Regex.Match(logFile, "Gpu : Backend \\(Metal\\): Metal");
        if (m.Success) return true;
        if (!m.Success) return false;
        else return false;
    }

    void GetAppInfo()
    {
        MatchCollection gameNameMatch = Regex.Matches(Log.RawLogContent,
            @"Loader [A-Za-z]*: Application Loaded:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (!gameNameMatch.None())
        {
            string gameName = gameNameMatch[-1].ToString().Trim();

            string appID;
            Match appIDMatch = Regex.Match(gameName, @".* \[([a-zA-Z0-9]*)\]");
            if (appIDMatch.Success)
            {
                appID = appIDMatch.Groups[1].Value.Trim().ToUpper();
            }
            else
            {
                appID = "Unknown";
            }

            MatchCollection bidsMatchAll = Regex.Matches(Log.RawLogContent,
                @"Build ids found for (?:title|application) ([a-zA-Z0-9]*):[\n\r]*((?:\s+.*[\n\r]+)+)");
            if (!bidsMatchAll.None() && bidsMatchAll.Count > 0)
            {
                // this whole thing might not work properly
                string bidsMatch = bidsMatchAll[-1].ToString();
                string appIDFromBids;
                string BuildIDs;

                if (bidsMatch[0].ToString() != null)
                {
                    appIDFromBids = bidsMatch[0].ToString().Trim().ToUpper();
                }
                else
                {
                    appIDFromBids = "Unknown";
                }

                if (bidsMatch[1].ToString() != null)
                {
                    // this might not work
                    BuildIDs = bidsMatch[1].ToString().Trim().ToUpper();
                }
                else
                {
                    BuildIDs = "Unknown";
                }

                Log.Game.Name = gameName;
                Log.Game.AppId = appID;
                Log.Game.AppIdBids = appIDFromBids;
                Log.Game.BuildIDs = BuildIDs;
            }
        }
    }

    static bool ContainsError(string[] searchTerm, List<string> errors)
    {
        foreach (string term in searchTerm)
        {
            foreach (string errorLines in errors)
            {
                string line = errorLines.JoinToString("\n");
                if (term.Contains(line))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void GetErrors()
    {
        List<string> errors = new List<string>();
        List<string> currentErrorsLines = new List<string>();
        bool errorLine = false;

        foreach (string line in Log.RawLogContent.Split("\n"))
        {
            if (line.IsNullOrWhitespace())
            {
                continue;
            }

            if (line.Contains("|E|"))
            {
                currentErrorsLines = [line];
                errors.AddRange(currentErrorsLines);
                errorLine = true;
            }
            else if (errorLine && line[0].ToString() == "")
            {
                currentErrorsLines.AddRange(line);
            }
        }

        if (currentErrorsLines.Count > 0)
        {
            errors.AddRange(currentErrorsLines);
        }

        Log.LogErrors = errors;
    }

    string[] _sizes = ["KB", "KiB", "MB", "MiB", "GB", "GiB"];

    void GetHardwareInfo()
    {
        // CPU
        Match cpuMatch = Regex.Match(Log.RawLogContent, @"CPU:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (cpuMatch.Success)
        {
            Log.Hardware.Cpu = cpuMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            Log.Hardware.Cpu = "Unknown";
        }

        // RAM
        Match ramMatch = Regex.Match(Log.RawLogContent,
            @$"RAM: Total ([\d.]+) ({_sizes}) ; Available ([\d.]+) ({_sizes})", RegexOptions.Multiline);
        if (ramMatch.Success)
        {
            double ramAvailable = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[3].Value));
            double ramTotal = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[1].Value));
            Log.Hardware.Ram = $"{ramAvailable:.0f}/{ramTotal:.0f} MiB";
        }
        else
        {
            Log.Hardware.Ram = "Unknown";
        }

        // Operating System (OS)
        Match osMatch = Regex.Match(Log.RawLogContent, @"Operating System:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (osMatch.Success)
        {
            Log.Hardware.Os = osMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            Log.Hardware.Os = "Unknown";
        }

        // GPU
        Match gpuMatch = Regex.Match(Log.RawLogContent, @"PrintGpuInformation:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (gpuMatch.Success)
        {
            Log.Hardware.Gpu = gpuMatch.Groups[1].Value.TrimEnd();
            // If android logs starts showing up, we can detect android GPUs here
        }
        else
        {
            Log.Hardware.Gpu = "Unknown";
        }
    }

    void GetEmuInfo()
    {
        // Ryujinx Version check
        foreach (string line in Log.RawLogContent.Split("\n"))
        {
            // Greem's Stable build
            if (LogAnalysisPatterns.StableVersion.IsMatch(line))
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.Stable, line.Split("\n")[-1].Trim());
            }
            // Greem's Canary build
            else if (LogAnalysisPatterns.CanaryVersion.IsMatch(line))
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.Canary, line.Split("\n")[-1].Trim());
            }
            // PR build
            else if (LogAnalysisPatterns.PrVersion.IsMatch(line) || LogAnalysisPatterns.OriginalPrVersion.IsMatch(line))
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.Pr, line.Split("\n")[-1].Trim());
            }
            // Original Project build
            else if (LogAnalysisPatterns.OriginalProjectVersion.IsMatch(line))
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.OriginalProject, line.Split("\n")[-1].Trim());
            }
            // Original Project LDN build
            else if (LogAnalysisPatterns.OriginalProjectLdnVersion.IsMatch(line))
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.OriginalProjectLdn, line.Split("\n")[-1].Trim());
            }
            // Custom build
            else
            {
                Log.EmulatorInfo.Version = (RyujinxVersion.Custom, line.Split("\n")[-1].Trim());
            }
        }

        // Logs Enabled ?
        Match logsMatch = Regex.Match(Log.RawLogContent, @"Logs Enabled:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (logsMatch.Success)
        {
            Log.EmulatorInfo.EnabledLogs = logsMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            Log.EmulatorInfo.EnabledLogs = "Unknown";
        }

        // Firmware
        Match firmwareMatch = Regex.Match(Log.RawLogContent, @"Firmware Version:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (firmwareMatch.Success)
        {
            Log.EmulatorInfo.Firmware = firmwareMatch.Groups[-1].Value.Trim();
        }
        else
        {
            Log.EmulatorInfo.Firmware = "Unknown";
        }

    }

    void GetSettings()
    {

        MatchCollection settingsMatch = Regex.Matches(Log.RawLogContent, @"LogValueChange:\s([^;\n\r]*)",
            RegexOptions.Multiline);

        foreach (string line in settingsMatch)
        {
            switch (line)
            {
                // Resolution Scale
                case "ResScaleCustom set to:":
                case "ResScale set to:":
                    Log.Settings.ResScale = settingsMatch[0].Groups[2].Value.Trim();
                    switch (Log.Settings.ResScale)
                    {
                        case "1":
                            Log.Settings.ResScale = "Native (720p/1080p)";
                            break;
                        case "2":
                            Log.Settings.ResScale = "2x (1440p/2060p(4K))";
                            break;
                        case "3":
                            Log.Settings.ResScale = "3x (2160p(4K)/3240p)";
                            break;
                        case "4":
                            Log.Settings.ResScale = "4x (3240p/4320p(8K))";
                            break;
                        case "-1":
                            Log.Settings.ResScale = "Custom";
                            break;
                    }

                    break;
                // Anisotropic Filtering
                case "MaxAnisotropy set to:":
                    Log.Settings.AnisotropicFiltering = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Aspect Ratio
                case "AspectRatio set to:":
                    Log.Settings.AspectRatio = settingsMatch[0].Groups[2].Value.Trim();
                    switch (Log.Settings.AspectRatio)
                    {
                        case "Fixed16x9":
                            Log.Settings.AspectRatio = "16:9";
                            break;
                        // TODO: add more aspect ratios
                    }

                    break;
                // Graphics Backend
                case "GraphicsBackend set to:":
                    Log.Settings.GraphicsBackend = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Custom VSync Interval
                case "CustomVSyncInterval set to:":
                    Log.Settings.CustomVSyncInterval = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Shader cache
                case "EnableShaderCache set to: True":
                    Log.Settings.ShaderCache = true;
                    break;
                case "EnableShaderCache set to: False":
                    Log.Settings.ShaderCache = false;
                    break;
                // Docked or Handheld
                case "EnableDockedMode set to: True":
                    Log.Settings.Docked = true;
                    break;
                case "EnableDockedMode set to: False":
                    Log.Settings.Docked = false;
                    break;
                // PPTC Cache
                case "EnablePtc set to: True":
                    Log.Settings.Pptc = true;
                    break;
                case "EnablePtc set to: False":
                    Log.Settings.Pptc = false;
                    break;
                // FS Integrity check
                case "EnableFsIntegrityChecks set to: True":
                    Log.Settings.FsIntegrityChecks = true;
                    break;
                case "EnableFsIntegrityChecks set to: False":
                    Log.Settings.FsIntegrityChecks = false;
                    break;
                // Audio Backend
                case "AudioBackend set to:":
                    Log.Settings.AudioBackend = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Memory Manager Mode
                case "MemoryManagerMode set to:":
                    Log.Settings.MemoryManager = settingsMatch[0].Groups[2].Value.Trim();
                    switch (Log.Settings.MemoryManager)
                    {
                        case "HostMappedUnsafe":
                            Log.Settings.MemoryManager = "Unsafe";
                            break;
                        // TODO: Add more memory manager modes
                    }

                    break;
                // Hypervisor
                case "UseHypervisor set to:":
                    Log.Settings.Hypervisor = settingsMatch[0].Groups[2].Value.Trim();
                    // If the OS is windows or linux, set hypervisor to 'N/A' because it's only on macos
                    if (Log.Hardware.Os.ToLower() == "windows" || Log.Hardware.Os.ToLower() == "linux")
                    {
                        Log.Settings.Hypervisor = "N/A";
                    }

                    break;
                // Ldn Mode
                case "MultiplayerMode set to:":
                    Log.Settings.MultiplayerMode = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // This is just in case EVERYTHING fails
                default:
                    Log.Settings.ResScale = "Failed";
                    Log.Settings.AnisotropicFiltering = "Failed";
                    Log.Settings.AspectRatio = "Failed";
                    Log.Settings.GraphicsBackend = "Failed";
                    Log.Settings.CustomVSyncInterval = "Failed";
                    Log.Settings.ShaderCache = false;
                    Log.Settings.Docked = false;
                    Log.Settings.Pptc = false;
                    Log.Settings.FsIntegrityChecks = false;
                    Log.Settings.AudioBackend = "Failed";
                    Log.Settings.MemoryManager = "Failed";
                    Log.Settings.Hypervisor = "Failed";
                    break;
            }
        }
    }

    void GetMods()
    {
        MatchCollection modsMatch = Regex.Matches(Log.RawLogContent,
            "Found\\s(enabled)?\\s?mod\\s\\'(.+?)\\'\\s(\\[.+?\\])");
        Log.Game.Mods += modsMatch;
    }

    void GetCheats()
    {
        MatchCollection cheatsMatch = Regex.Matches(Log.RawLogContent,
            @"Installing cheat\s'(.+)'(?!\s\d{2}:\d{2}:\d{2}\.\d{3}\s\|E\|\sTamperMachine\sCompile)");
        Log.Game.Cheats += cheatsMatch;
    }

    void GetAppName()
    {
        Match appNameMatch = Regex.Match(Log.RawLogContent,
            @"Loader [A-Za-z]*: Application Loaded:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        Log.Game.Name += appNameMatch;
    }
    
    
}    
    

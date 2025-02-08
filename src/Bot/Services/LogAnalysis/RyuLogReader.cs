using System.Text.RegularExpressions;

namespace RyuBot.Services;

public class RyuLogReader
{
    private LogAnalysis _log;
    private Notes _notes;
    private FatalErrors _fatalErrors;

    double ConvertGiBtoMiB(double GiB)
    {
        return Math.Round(GiB * 1024);
    }

    static bool IsHomebrew(string logFile)
    {
        Match m = Regex.Match(logFile, "Load.*Application: Loading as [Hh]omebrew");
        return m.Success;
    }

    static bool IsUsingMetal(string logFile)
    {
        Match m = Regex.Match(logFile, "Gpu : Backend \\(Metal\\): Metal");
        return m.Success;
    }

    static bool IsDefaultUserProfile(string logFile)
    {
        Match m = Regex.Match(logFile, "UserId: 00000000000000010000000000000000");
        return m.Success;
    }

    (RyujinxVersion VersionType, string VersionString) GetEmuVersion()
    {
        // Ryujinx Version check
        foreach (string line in _log.RawLogContent.Split("\n"))
        {
            // Greem's Stable build
            if (LogAnalysisPatterns.StableVersion.IsMatch(line))
            {
                return (RyujinxVersion.Stable, line[-1].ToString().Trim());
            }
            
            // Greem's Canary build
            if (LogAnalysisPatterns.CanaryVersion.IsMatch(line))
            {
                return (RyujinxVersion.Canary, line[-1].ToString().Trim());
            }
            
            // PR build
            if (LogAnalysisPatterns.PrVersion.IsMatch(line) 
                     || LogAnalysisPatterns.OriginalPrVersion.IsMatch(line))
            {
                return (RyujinxVersion.Pr, line[-1].ToString().Trim());
            }
            
            // Original Project build
            if (LogAnalysisPatterns.OriginalProjectVersion.IsMatch(line))
            {
                return (RyujinxVersion.OriginalProject, line[-1].ToString().Trim());
            }
            
            // Original Project LDN build
            if (LogAnalysisPatterns.OriginalProjectLdnVersion.IsMatch(line))
            {
                return (RyujinxVersion.OriginalProjectLdn, line[-1].ToString().Trim());
            }
            
            if (LogAnalysisPatterns.MirrorVersion.IsMatch(line))
            {
                return (RyujinxVersion.Mirror, line[-1].ToString().Trim());
            }
        }
        
        return (RyujinxVersion.Custom, "1.0.0-dirty");
    }

    void GetAppInfo()
    {
        MatchCollection gameNameMatch = Regex.Matches(_log.RawLogContent,
            @"Loader [A-Za-z]*: Application Loaded:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (!gameNameMatch.None())
        {
            string gameName = gameNameMatch[-1].ToString().Trim();

            string appId;
            Match appIdMatch = Regex.Match(gameName, @".* \[([a-zA-Z0-9]*)\]");
            if (appIdMatch.Success)
            {
                appId = appIdMatch.Groups[1].Value.Trim().ToUpper();
            }
            else
            {
                appId = "Unknown";
            }

            MatchCollection bidsMatchAll = Regex.Matches(_log.RawLogContent,
                @"Build ids found for (?:title|application) ([a-zA-Z0-9]*):[\n\r]*((?:\s+.*[\n\r]+)+)");
            if (!bidsMatchAll.None() && bidsMatchAll.Count > 0)
            {
                // this whole thing might not work properly
                string bidsMatch = bidsMatchAll[-1].ToString();
                string appIdFromBids;
                string buildIDs;

                if (bidsMatch[0].ToString() != "")
                {
                    appIdFromBids = bidsMatch[0].ToString().Trim().ToUpper();
                }
                else
                {
                    appIdFromBids = "Unknown";
                }

                if (bidsMatch[1].ToString() != "")
                {
                    // this might not work
                    buildIDs = bidsMatch[1].ToString().Trim().ToUpper();
                }
                else
                {
                    buildIDs = "Unknown";
                }

                _log.Game.Name = gameName;
                _log.Game.AppId = appId;
                _log.Game.AppIdBids = appIdFromBids;
                _log.Game.BuildIDs = buildIDs;
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

        foreach (string line in _log.RawLogContent.Split("\n"))
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

        _log.Errors = errors;
    }

    readonly string[] _sizes = ["KB", "KiB", "MB", "MiB", "GB", "GiB"];

    void GetHardwareInfo()
    {
        // CPU
        Match cpuMatch = Regex.Match(_log.RawLogContent, @"CPU:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (cpuMatch.Success)
        {
            _log.Hardware.Cpu = cpuMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            _log.Hardware.Cpu = "Unknown";
        }

        // RAM
        Match ramMatch = Regex.Match(_log.RawLogContent,
            @$"RAM: Total ([\d.]+) ({_sizes}) ; Available ([\d.]+) ({_sizes})", RegexOptions.Multiline);
        if (ramMatch.Success)
        {
            double ramAvailable = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[3].Value));
            double ramTotal = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[1].Value));
            _log.Hardware.Ram = $"{ramAvailable:.0f}/{ramTotal:.0f} MiB";
            _log.Hardware.RamAvailable = ramAvailable.ToString("0.0");
        }
        else
        {
            _log.Hardware.Ram = "Unknown";
            _log.Hardware.RamAvailable = "Unknown";
        }

        // Operating System (OS)
        Match osMatch = Regex.Match(_log.RawLogContent, @"Operating System:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (osMatch.Success)
        {
            _log.Hardware.Os = osMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            _log.Hardware.Os = "Unknown";
        }

        // GPU
        Match gpuMatch = Regex.Match(_log.RawLogContent, @"PrintGpuInformation:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (gpuMatch.Success)
        {
            _log.Hardware.Gpu = gpuMatch.Groups[1].Value.TrimEnd();
            // If android logs starts showing up, we can detect android GPUs here
        }
        else
        {
            _log.Hardware.Gpu = "Unknown";
        }
    }

    void GetEmuInfo()
    {
        _log.Emulator.Version = GetEmuVersion();

        // Logs Enabled ?
        Match logsMatch = Regex.Match(_log.RawLogContent, @"Logs Enabled:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (logsMatch.Success)
        {
            _log.Emulator.EnabledLogs = logsMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            _log.Emulator.EnabledLogs = "Unknown";
        }

        // Firmware
        Match firmwareMatch = Regex.Match(_log.RawLogContent, @"Firmware Version:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (firmwareMatch.Success)
        {
            _log.Emulator.Firmware = firmwareMatch.Groups[-1].Value.Trim();
        }
        else
        {
            _log.Emulator.Firmware = "Unknown";
        }

    }

    void GetSettings()
    {
        MatchCollection settingsMatch = Regex.Matches(_log.RawLogContent, @"LogValueChange:\s([^;\n\r]*)",
            RegexOptions.Multiline);

        foreach (string line in settingsMatch)
        {
            switch (line)
            {
                // Resolution Scale
                case "ResScaleCustom set to:":
                case "ResScale set to:":
                    _log.Settings.ResScale = settingsMatch[0].Groups[2].Value.Trim();
                    switch (_log.Settings.ResScale)
                    {
                        case "1":
                            _log.Settings.ResScale = "Native (720p/1080p)";
                            break;
                        case "2":
                            _log.Settings.ResScale = "2x (1440p/2060p(4K))";
                            break;
                        case "3":
                            _log.Settings.ResScale = "3x (2160p(4K)/3240p)";
                            break;
                        case "4":
                            _log.Settings.ResScale = "4x (3240p/4320p(8K))";
                            break;
                        case "-1":
                            _log.Settings.ResScale = "Custom";
                            break;
                    }

                    break;
                // Anisotropic Filtering
                case "MaxAnisotropy set to:":
                    _log.Settings.AnisotropicFiltering = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Aspect Ratio
                case "AspectRatio set to:":
                    _log.Settings.AspectRatio = settingsMatch[0].Groups[2].Value.Trim();
                    switch (_log.Settings.AspectRatio)
                    {
                        case "Fixed16x9":
                            _log.Settings.AspectRatio = "16:9";
                            break;
                        // TODO: add more aspect ratios
                    }

                    break;
                // Graphics Backend
                case "GraphicsBackend set to:":
                    _log.Settings.GraphicsBackend = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Custom VSync Interval
                case "CustomVSyncInterval set to:":
                    string a = settingsMatch[0].Groups[2].Value.Trim();
                    if (a == "False")
                    {
                        _log.Settings.CustomVSyncInterval = false;
                    }
                    else if (a == "True")
                    {
                        _log.Settings.CustomVSyncInterval = true;
                    }
                    break;
                // Shader cache
                case "EnableShaderCache set to: True":
                    _log.Settings.ShaderCache = true;
                    break;
                case "EnableShaderCache set to: False":
                    _log.Settings.ShaderCache = false;
                    break;
                // Docked or Handheld
                case "EnableDockedMode set to: True":
                    _log.Settings.Docked = true;
                    break;
                case "EnableDockedMode set to: False":
                    _log.Settings.Docked = false;
                    break;
                // PPTC Cache
                case "EnablePtc set to: True":
                    _log.Settings.Pptc = true;
                    break;
                case "EnablePtc set to: False":
                    _log.Settings.Pptc = false;
                    break;
                // FS Integrity check
                case "EnableFsIntegrityChecks set to: True":
                    _log.Settings.FsIntegrityChecks = true;
                    break;
                case "EnableFsIntegrityChecks set to: False":
                    _log.Settings.FsIntegrityChecks = false;
                    break;
                // Audio Backend
                case "AudioBackend set to:":
                    _log.Settings.AudioBackend = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // Memory Manager Mode
                case "MemoryManagerMode set to:":
                    _log.Settings.MemoryManager = settingsMatch[0].Groups[2].Value.Trim();
                    switch (_log.Settings.MemoryManager)
                    {
                        case "HostMappedUnsafe":
                            _log.Settings.MemoryManager = "Unsafe";
                            break;
                        // TODO: Add more memory manager modes
                    }

                    break;
                // Hypervisor
                case "UseHypervisor set to:":
                    _log.Settings.Hypervisor = settingsMatch[0].Groups[2].Value.Trim();
                    // If the OS is windows or linux, set hypervisor to 'N/A' because it's only on macOS
                    if (_log.Hardware.Os.ToLower() == "windows" || _log.Hardware.Os.ToLower() == "linux")
                    {
                        _log.Settings.Hypervisor = "N/A";
                    }

                    break;
                // Ldn Mode
                case "MultiplayerMode set to:":
                    _log.Settings.MultiplayerMode = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                case "DramSize set to:":
                    _log.Settings.DramSize = settingsMatch[0].Groups[2].Value.Trim();
                    break;
                // This is just in case EVERYTHING fails
                default:
                    _log.Settings.ResScale = "Failed";
                    _log.Settings.AnisotropicFiltering = "Failed";
                    _log.Settings.AspectRatio = "Failed";
                    _log.Settings.GraphicsBackend = "Failed";
                    _log.Settings.CustomVSyncInterval = false;
                    _log.Settings.ShaderCache = false;
                    _log.Settings.Docked = false;
                    _log.Settings.Pptc = false;
                    _log.Settings.FsIntegrityChecks = false;
                    _log.Settings.AudioBackend = "Failed";
                    _log.Settings.MemoryManager = "Failed";
                    _log.Settings.Hypervisor = "Failed";
                    break;
            }
        }
    }

    void GetMods()
    {
        MatchCollection modsMatch = Regex.Matches(_log.RawLogContent,
            "Found\\s(enabled)?\\s?mod\\s\\'(.+?)\\'\\s(\\[.+?\\])");
        _log.Game.Mods = modsMatch.ToString();
    }

    void GetCheats()
    {
        MatchCollection cheatsMatch = Regex.Matches(_log.RawLogContent,
            @"Installing cheat\s'(.+)'(?!\s\d{2}:\d{2}:\d{2}\.\d{3}\s\|E\|\sTamperMachine\sCompile)");
        _log.Game.Cheats = cheatsMatch.ToString();
    }

    void GetAppName()
    {
        Match appNameMatch = Regex.Match(_log.RawLogContent,
            @"Loader [A-Za-z]*: Application Loaded:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        _log.Game.Name = appNameMatch.ToString();
    }
    
    void GetNotes()
    {
        string GetControllerNotes()
        {
            MatchCollection controllerNotesMatch = Regex.Matches(_log.RawLogContent,
                @"Hid Configure: ([^\r\n]+)");
            if (controllerNotesMatch.Count != 0)
            {
                return controllerNotesMatch.ToString();
            }
            
            return null;
        }

        string GetOsNotes()
        {
            if (_log.Hardware.Os.ToLower().Contains("windows")
                && !_log.Settings.GraphicsBackend.Contains("Vulkan"))
            {
                if (_log.Hardware.Gpu.Contains("Intel"))
                {
                    return _notes.IntelOpenGL;
                } 
                if (_log.Hardware.Gpu.Contains("AMD"))
                {
                    return _notes.AMDOpenGL;
                }
            }

            if (_log.Hardware.Os.ToLower().Contains("macos") && !_log.Hardware.Cpu.Contains("Intel"))
            {
                return _notes.IntelMac;
            }

            return null;
        }

        string GetCpuNotes()
        {
            if (_log.Hardware.Cpu.ToLower().Contains("VirtualApple"))
            {
                return _notes.Rosetta;
            }
            
            return null;
        }

        string GetLogNotes()
        {
            if (_log.Emulator.EnabledLogs.Contains("Debug"))
            {
                return _notes.DebugLogs;
            }

            if (_log.Emulator.EnabledLogs.Length < 4)
            {
                return _notes.MissingLogs;
            }
            
            return null;
        }

        void GetSettingsNotes()
        {
            if (_log.Settings.AudioBackend == "Dummy")
            {
                _log.Notes.Add(_notes.DummyAudio);
            } 
            
            if (_log.Settings.Pptc == false)
            {
                _log.Notes.Add(_notes.Pptc);
            }
            
            if (_log.Settings.ShaderCache == false)
            {
                _log.Notes.Add(_notes.ShaderCache);
            }
            
            if (_log.Settings.DramSize != "" && !_log.Game.Mods.Contains("4K"))
            {
                _log.Notes.Add(_notes.DramSize);   
            }
            
            if (_log.Settings.MemoryManager == "SoftwarePageTable")
            {
                _log.Notes.Add(_notes.SoftwareMemory);
            }
            
            if (_log.Settings.VSyncMode == "Unbounded")
            {
                _log.Notes.Add(_notes.VSync);
            }
            
            if (_log.Settings.FsIntegrityChecks == false)
            {
                _log.Notes.Add(_notes.FsIntegrity);   
            }
            
            if (_log.Settings.BackendThreading == false)
            {
                _log.Notes.Add(_notes.BackendThreadingAuto);
            }
            
            if (_log.Settings.CustomVSyncInterval)
            {
                _log.Notes.Add(_notes.CustomRefreshRate);   
            }

            if (_log.Settings.IgnoreMissingServices)
            {
                _log.Notes.Add(_notes.ServiceError);
            }
        }

        void GetEmulatorNotes()
        {
            if (ContainsError(["Cache collision found"], _log.Errors))
            {
                _log.Errors.Add(_notes.ShaderCacheCollision);
            } 
                
            if (ContainsError([
                    "ResultFsInvalidIvfcHash",
                    "ResultFsNonRealDataVerificationFailed",], _log.Errors))
            {
                _log.Errors.Add(_notes.HashError);
            }
                
            if (ContainsError([
                    "Ryujinx.Graphics.Gpu.Shader.ShaderCache.Initialize()",
                    "System.IO.InvalidDataException: End of Central Directory record could not be found",
                    "ICSharpCode.SharpZipLib.Zip.ZipException: Cannot find central directory",
                ], _log.Errors))
            {
                _log.Errors.Add(_notes.ShaderCacheCorruption);
            }
                
            if (ContainsError(["MissingKeyException"], _log.Errors))
            {
                _log.Errors.Add(_notes.MissingKeys);
            }
                
            if (ContainsError(["ResultFsPermissionDenied"], _log.Errors))
            {
                _log.Errors.Add(_notes.PermissionError);
            }
                
            if (ContainsError(["ResultFsTargetNotFound"], _log.Errors))
            {
                _log.Errors.Add(_notes.FsTargetError);
            }

            if (ContainsError(["ServiceNotImplementedException"], _log.Errors))
            {
                _log.Errors.Add(_notes.MissingServices);
            }

            if (ContainsError(["ErrorOutOfDeviceMemory"], _log.Errors))
            {
                _log.Errors.Add(_notes.VramError);
            }

            if (ContainsError(["ResultKvdbInvalidKeyValue (2020-0005)"], _log.Errors))
            {
                _log.Errors.Add(_notes.SaveDataIndex);
            }

            Match gameCrashMatch =
                Regex.Match(_log.RawLogContent, "/\\(ResultErrApplicationAborted \\(\\d{4}-\\d{4}\\)\\)/");

            if (gameCrashMatch.Success)
            {
                _log.Errors.Add(_notes.GameCrashed);
            }
        }
        
        Match latestTimestamp = Regex.Matches(_log.RawLogContent, @"(\d{2}:\d{2}:\d{2}\.\d{3})\s+?\|")[-1];
        if (latestTimestamp.Success)
        {
            _log.Notes.Add("ℹ️ Time elapsed: " + latestTimestamp.Value);
        }

        if (IsDefaultUserProfile(_log.RawLogContent))
        {
            _log.Notes.Add(_notes.DefaultProfile);
        }
        
        _log.Notes.Add(GetControllerNotes());
        _log.Notes.Add(GetOsNotes());
        _log.Notes.Add(GetCpuNotes());

        if (_log.Emulator.Firmware == "Unknown" || _log.Emulator.Firmware == null
            && _log.Game.Name != "Unknown" || _log.Game.Name == null)
        {
            _log.Notes.Add(_notes.Firmware);
        }
        
        _log.Notes.Add(GetLogNotes());
        GetSettingsNotes();
        GetEmulatorNotes();

        if (IsUsingMetal(_log.RawLogContent))
        {
            _log.Notes.Add(_notes.Metal);
        }

        var ryujinxVersion = GetEmuVersion();

        if (ryujinxVersion.VersionType == RyujinxVersion.Custom)
        {
            _log.FatalErrors.Add(_fatalErrors.Custom);
        } else if (ryujinxVersion.VersionType == RyujinxVersion.OriginalProjectLdn)
        {
            _log.FatalErrors.Add(_fatalErrors.OriginalLdn);
        }
        else if (ryujinxVersion.VersionType == RyujinxVersion.OriginalProject)
        {
            _log.FatalErrors.Add(_fatalErrors.Original);
        } else if (ryujinxVersion.VersionType == RyujinxVersion.Mirror)
        {
            _log.FatalErrors.Add(_fatalErrors.Mirror);
        }
        
    }
    
}    
    

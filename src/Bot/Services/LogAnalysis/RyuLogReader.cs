using System.Text.RegularExpressions;

namespace RyuBot.Services;

public class RyuLogReader
{
    public LogAnalysis log;
    
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
        MatchCollection gameNameMatch = Regex.Matches(log.RawLogContent, @"Loader [A-Za-z]*: Application Loaded:\s([^;\n\r]*)", RegexOptions.Multiline);
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

            MatchCollection bidsMatchAll = Regex.Matches(log.RawLogContent,
                @"Build ids found for (?:title|application) ([a-zA-Z0-9]*):[\n\r]*((?:\s+.*[\n\r]+)+)");
            if (!bidsMatchAll.None() && bidsMatchAll.Count > 0)
            {
                // this whole thing might not work properly
                string bidsMatch = bidsMatchAll[-1].ToString();
                string appIDFromBids;
                string BuildIDs;

                if (bidsMatch[0].ToString() is not null)
                {
                    appIDFromBids = bidsMatch[0].ToString().Trim().ToUpper();
                }
                else
                {
                    appIDFromBids = "Unknown";
                }
                if (bidsMatch[1].ToString() is not null)
                {
                    // this might not work
                    BuildIDs = bidsMatch[1].ToString().Trim().ToUpper();
                } else
                {
                    BuildIDs = "Unknown";
                }
                
                log.Game.Name = gameName;
                log.Game.AppId = appID;
                log.Game.AppIdBids = appIDFromBids;
                log.Game.BuildIDs = BuildIDs;
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

        foreach (string line in log.RawLogContent.Split("\n"))
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
        
        log.LogErrors = errors;
    }

    string[] sizes = ["KB", "KiB", "MB", "MiB", "GB", "GiB"];

    void GetHardwareInfo()
    {
        // CPU
        Match cpuMatch = Regex.Match(log.RawLogContent, @"CPU:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (cpuMatch.Success)
        {
            log.Hardware.Cpu = cpuMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            log.Hardware.Cpu = "Unknown";
        }
        
        // RAM
        Match ramMatch = Regex.Match(log.RawLogContent,
            @$"RAM: Total ([\d.]+) ({sizes}) ; Available ([\d.]+) ({sizes})", RegexOptions.Multiline);
        if (ramMatch.Success)
        {
            double ramAvailable = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[3].Value));
            double ramTotal = ConvertGiBtoMiB(Convert.ToDouble(ramMatch.Groups[1].Value));
            log.Hardware.Ram = $"{ramAvailable:.0f}/{ramTotal:.0f} MiB";
        }
        else
        {
            log.Hardware.Ram = "Unknown";
        }

        // Operating System (OS)
        Match osMatch = Regex.Match(log.RawLogContent, @"Operating System:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (osMatch.Success)
        {
            log.Hardware.Os = osMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            log.Hardware.Os = "Unknown";
        }

        // GPU
        Match gpuMatch = Regex.Match(log.RawLogContent, @"PrintGpuInformation:\s([^;\n\r]*)",
            RegexOptions.Multiline);
        if (gpuMatch.Success)
        {
            log.Hardware.Gpu = gpuMatch.Groups[1].Value.TrimEnd();
            // If android logs starts showing up, we can detect android GPUs here
        }
        else
        {
            log.Hardware.Gpu = "Unknown";
        }
    }

    void GetEmuInfo()
    {
        // Ryujinx Version check
        foreach (string line in log.RawLogContent.Split("\n"))
        {
            // Greem's Stable build
            if (LogAnalysisPatterns.StableVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.Stable, line.Split("\n")[-1].Trim());
            } 
            // Greem's Canary build
            else if (LogAnalysisPatterns.CanaryVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.Canary, line.Split("\n")[-1].Trim());
            }
            // Custom build
            else if (LogAnalysisPatterns.CustomVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.Custom, line.Split("\n")[-1].Trim());
            } 
            // PR build
            else if (LogAnalysisPatterns.PrVersion.IsMatch(line) || LogAnalysisPatterns.OriginalPrVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.Pr, line.Split("\n")[-1].Trim());
            } 
            // Original Project build
            else if (LogAnalysisPatterns.OriginalProjectVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.OriginalProject, line.Split("\n")[-1].Trim());
            } 
            // Original Project LDN build
            else if (LogAnalysisPatterns.OriginalProjectLdnVersion.IsMatch(line))
            {
                log.EmulatorInfo.Version = (RyujinxVersion.OriginalProjectLdn, line.Split("\n")[-1].Trim());
            }
        }
        
        // Logs Enabled ?
        Match logsMatch = Regex.Match(log.RawLogContent, @"Logs Enabled:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (logsMatch.Success)
        {
            log.EmulatorInfo.EnabledLogs = logsMatch.Groups[1].Value.TrimEnd();
        }
        else
        {
            log.EmulatorInfo.EnabledLogs = "Unknown";
        }
        
        // Firmware
        Match firmwareMatch = Regex.Match(log.RawLogContent, @"Firmware Version:\s([^;\n\r]*)", RegexOptions.Multiline);
        if (firmwareMatch.Success)
        {
            log.EmulatorInfo.Firmware = firmwareMatch.Groups[-1].Value.Trim();
        }
        else
        {
            log.EmulatorInfo.Firmware = "Unknown";
        }
        
    }

    void GetSettingValue()
    {
        Match settingMatch = Regex.Match(log.RawLogContent, @"LogValueChange:\s([^;\n\r]*)");
        if (settingMatch.Success)
        {
            // Unfinished
        }
        
    }
    
}

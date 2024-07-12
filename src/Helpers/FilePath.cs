using System.IO;

using DotNetPath = System.IO.Path;

namespace Volte.Helpers;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
public record FilePath
{

    #region Global common paths

    public static readonly FilePath Logs = new("logs", true);

    public static readonly FilePath Data = new("data", true);

    public static readonly FilePath ConfigFile = Data.Resolve("volte.json", false);

    #endregion
    
    public string Path { get; }
    public bool IsDirectory { get; }

    public FilePath(string path, bool? isDirectory = null)
    {
        Path = path;
        IsDirectory = isDirectory ?? (Directory.Exists(path) && !File.Exists(path));
    }

    public FilePath Resolve(string subPath, bool? isDirectory = null)
        => new(
            !Path.EndsWith('/') && !subPath.StartsWith('/') 
                ? $"{Path}/{subPath}" 
                : Path + subPath,
            isDirectory
            );
    

    public static FilePath operator /(FilePath left, string right) => left.Resolve(right);

    public static FilePath operator --(FilePath curr) => 
        curr.TryGetParent(out var parentDir) 
            ? parentDir 
            : curr;
    

    public bool TryGetParent(out FilePath filePath)
    {
        var parentDir = Directory.GetParent(Path);
        if (parentDir == null)
        {
            filePath = null;
            return false;
        }

        filePath = new FilePath(parentDir.FullName);
        return true;
    }
    
    public string Extension => !IsDirectory ? DotNetPath.GetExtension(Path) : null;

    public bool ExistsAsFile => File.Exists(Path);
    public bool ExistsAsDirectory => Directory.Exists(Path);

    public void Create()
    {
        if (IsDirectory)
            Directory.CreateDirectory(Path);
        else if (!ExistsAsFile)
            File.Create(Path).Close();
    }
    
    public List<FilePath> GetFiles() => 
        IsDirectory 
            ? Directory.GetFiles(Path).Select(path => new FilePath(path, false)).ToList() 
            : null;
    
    public List<FilePath> GetSubdirectories() => 
        IsDirectory 
            ? Directory.GetDirectories(Path).Select(path => new FilePath(path, true)).ToList()
            : null;

    public void WriteAllText(string text) => File.WriteAllText(Path, text);
    public void WriteAllBytes(byte[] bytes) => File.WriteAllBytes(Path, bytes);
    public void WriteAllLines(IEnumerable<string> lines) => File.WriteAllLines(Path, lines);
    
    public void AppendAllText(string text) => File.AppendAllText(Path, text);
    public void AppendAllLines(IEnumerable<string> lines) => File.AppendAllLines(Path, lines);

    public string ReadAllText(Encoding encoding = null) => File.ReadAllText(Path, encoding ?? Encoding.UTF8);
    public string[] ReadAllLines(Encoding encoding = null) => File.ReadAllLines(Path, encoding ?? Encoding.UTF8);
    public byte[] ReadAllBytes() => File.ReadAllBytes(Path);
    
    public FileStream OpenRead() => File.OpenRead(Path);
    public FileStream OpenWrite() => File.OpenWrite(Path);

    public override string ToString() => Path;
}
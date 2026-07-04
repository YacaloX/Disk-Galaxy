namespace DiskGalaxy.Core.Models;

public static class FileCategoryExtensions
{
    private static readonly Dictionary<string, FileCategory> ExtensionMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = FileCategory.Image, [".jpeg"] = FileCategory.Image, [".png"] = FileCategory.Image,
        [".gif"] = FileCategory.Image, [".bmp"] = FileCategory.Image, [".webp"] = FileCategory.Image,
        [".tiff"] = FileCategory.Image, [".svg"] = FileCategory.Image, [".ico"] = FileCategory.Image,
        [".raw"] = FileCategory.Image, [".psd"] = FileCategory.Image, [".heic"] = FileCategory.Image,

        [".mp4"] = FileCategory.Video, [".mkv"] = FileCategory.Video, [".avi"] = FileCategory.Video,
        [".mov"] = FileCategory.Video, [".wmv"] = FileCategory.Video, [".flv"] = FileCategory.Video,
        [".webm"] = FileCategory.Video, [".m4v"] = FileCategory.Video, [".mpg"] = FileCategory.Video,
        [".mpeg"] = FileCategory.Video,

        [".pdf"] = FileCategory.Document, [".doc"] = FileCategory.Document, [".docx"] = FileCategory.Document,
        [".xls"] = FileCategory.Document, [".xlsx"] = FileCategory.Document, [".ppt"] = FileCategory.Document,
        [".pptx"] = FileCategory.Document, [".txt"] = FileCategory.Document, [".rtf"] = FileCategory.Document,
        [".odt"] = FileCategory.Document, [".csv"] = FileCategory.Document, [".md"] = FileCategory.Document,

        [".mp3"] = FileCategory.Audio, [".wav"] = FileCategory.Audio, [".flac"] = FileCategory.Audio,
        [".aac"] = FileCategory.Audio, [".ogg"] = FileCategory.Audio, [".wma"] = FileCategory.Audio,
        [".m4a"] = FileCategory.Audio, [".opus"] = FileCategory.Audio,

        [".zip"] = FileCategory.Archive, [".rar"] = FileCategory.Archive, [".7z"] = FileCategory.Archive,
        [".tar"] = FileCategory.Archive, [".gz"] = FileCategory.Archive, [".bz2"] = FileCategory.Archive,
        [".xz"] = FileCategory.Archive, [".iso"] = FileCategory.Archive,

        [".exe"] = FileCategory.Executable, [".dll"] = FileCategory.Executable, [".so"] = FileCategory.Executable,
        [".dylib"] = FileCategory.Executable, [".appimage"] = FileCategory.Executable, [".msi"] = FileCategory.Executable,
        [".bin"] = FileCategory.Executable,

        [".cs"] = FileCategory.Code, [".py"] = FileCategory.Code, [".js"] = FileCategory.Code,
        [".ts"] = FileCategory.Code, [".html"] = FileCategory.Code, [".css"] = FileCategory.Code,
        [".cpp"] = FileCategory.Code, [".c"] = FileCategory.Code, [".h"] = FileCategory.Code,
        [".java"] = FileCategory.Code, [".rs"] = FileCategory.Code, [".go"] = FileCategory.Code,
        [".swift"] = FileCategory.Code, [".kt"] = FileCategory.Code, [".tsx"] = FileCategory.Code,
        [".jsx"] = FileCategory.Code, [".json"] = FileCategory.Code, [".xml"] = FileCategory.Code,
        [".yaml"] = FileCategory.Code, [".yml"] = FileCategory.Code, [".toml"] = FileCategory.Code,
        [".sql"] = FileCategory.Code, [".sh"] = FileCategory.Code, [".bat"] = FileCategory.Code,
        [".ps1"] = FileCategory.Code,

        [".ttf"] = FileCategory.Font, [".otf"] = FileCategory.Font, [".woff"] = FileCategory.Font,
        [".woff2"] = FileCategory.Font,

        [".db"] = FileCategory.Database, [".sqlite"] = FileCategory.Database, [".mdb"] = FileCategory.Database,

        [".config"] = FileCategory.Config, [".cfg"] = FileCategory.Config, [".ini"] = FileCategory.Config,
        [".env"] = FileCategory.Config,

        [".tmp"] = FileCategory.Temporary, [".temp"] = FileCategory.Temporary, [".bak"] = FileCategory.Temporary,
        [".swp"] = FileCategory.Temporary,
    };

    public static FileCategory FromExtension(string extension) =>
        ExtensionMap.TryGetValue(extension, out var category) ? category : FileCategory.Unknown;

    public static FileCategory FromPath(string path) =>
        FromExtension(Path.GetExtension(path));
}

namespace Domain.Constants;

public static class FileConstants
{
    public static readonly Dictionary<string, string> ExtensionToType =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // --- Images ---
            { "jpg", "Image" },
            { "jpeg", "Image" },
            { "png", "Image" },
            { "gif", "Image" },
            { "bmp", "Image" },
            { "tiff", "Image" },
            { "tif", "Image" },
            { "svg", "Vector Image" },
            { "webp", "Image" },
            { "ico", "Icon" },

            // --- Documents ---
            { "pdf", "Document" },
            { "doc", "Document" },
            { "docx", "Document" },
            { "odt", "Document" },
            { "rtf", "Document" },
            { "txt", "Text File" },
            { "md", "Markdown" },

            // --- Spreadsheets ---
            { "xls", "Spreadsheet" },
            { "xlsx", "Spreadsheet" },
            { "csv", "Spreadsheet" },
            { "ods", "Spreadsheet" },

            // --- Presentations ---
            { "ppt", "Presentation" },
            { "pptx", "Presentation" },
            { "odp", "Presentation" },

            // --- Audio ---
            { "mp3", "Audio" },
            { "wav", "Audio" },
            { "flac", "Audio" },
            { "aac", "Audio" },
            { "ogg", "Audio" },
            { "m4a", "Audio" },
            { "wma", "Audio" },

            // --- Video ---
            { "mp4", "Video" },
            { "mkv", "Video" },
            { "avi", "Video" },
            { "mov", "Video" },
            { "wmv", "Video" },
            { "flv", "Video" },
            { "webm", "Video" },

            // --- Archives ---
            { "zip", "Archive" },
            { "rar", "Archive" },
            { "7z", "Archive" },
            { "tar", "Archive" },
            { "gz", "Archive" },
            { "bz2", "Archive" },

            // --- Code & Scripts ---
            { "c", "Source Code" },
            { "cpp", "Source Code" },
            { "cs", "Source Code" },
            { "java", "Source Code" },
            { "py", "Source Code" },
            { "js", "Source Code" },
            { "ts", "Source Code" },
            { "html", "Markup" },
            { "css", "Stylesheet" },
            { "scss", "Stylesheet" },
            { "json", "Data" },
            { "xml", "Data" },
            { "yaml", "Data" },
            { "yml", "Data" },

            // --- Fonts ---
            { "ttf", "Font" },
            { "otf", "Font" },
            { "woff", "Font" },
            { "woff2", "Font" },

            // --- Other ---
            { "exe", "Executable" },
            { "dll", "Library" },
            { "iso", "Disk Image" },
            { "dmg", "Disk Image" },
            { "apk", "Android App" },
            { "app", "Mac App" }
        };
}
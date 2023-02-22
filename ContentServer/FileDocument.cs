using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimesheetBE.ContentServer
{
    public class FileDocument
    {
        private string _Path { get; set; }

        private string _File { get; set; }

        private string _Name { get; set; }

        private FileDocumentType _Type { get; set; }

        public string Path => _Path;

        public string File => _File;

        public string Name => _Name;

        public string FileNameWithExtension => $"{_Name}{_Type.Extension}";

        public FileDocumentType DocumentType => _Type;

        private FileDocument() { }

        private FileDocument(string File) { _File = File; }

        private FileDocument(string File, string Name) { _File = File; _Name = Name; }

        private FileDocument(string File, string Name, string Path) { _File = File; _Name = Name; _Path = Path; }

        private FileDocument(string File, string Name, string Path, FileDocumentType Type) { _File = File; _Name = Name; _Path = Path; _Type = Type; }

        public static FileDocument Create() => new FileDocument();

        public static FileDocument Create(string File) => new FileDocument(File);

        public static FileDocument Create(string File, string Name) => new FileDocument(File, Name);

        public static FileDocument Create(string File, string Name, string Path) => new FileDocument(File, Name, Path);

        public static FileDocument Create(string File, string Name, string Path, FileDocumentType Type) => new FileDocument(File, Name, Path, Type);
    }
}

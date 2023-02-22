using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimesheetBE.ContentServer
{
    public class FileDocumentType
    {
        public string Extension { get; set; }

        public string MimeType { get; set; }

        public static FileDocumentType GetDocumentType(string extension)
        {
            switch (extension)
            {
                case "pdf":
                    return new FileDocumentType { Extension = ".pdf", MimeType = MIMETYPE.PDF };
                case "jpg":
                    return new FileDocumentType { Extension = ".jpg", MimeType = MIMETYPE.IMAGE };
                case "jpeg":
                    return new FileDocumentType { Extension = ".jpeg", MimeType = MIMETYPE.IMAGE };
                case "mp4":
                    return new FileDocumentType { Extension = ".mp4", MimeType = MIMETYPE.VIDEO };
                default:
                    return new FileDocumentType { Extension = ".jpg", MimeType = MIMETYPE.IMAGE };

            }
        }
    }

    public class MIMETYPE
    {
        public static string IMAGE => "image/jpeg";
        public static string PDF => "application/pdf";
        public static string VIDEO => "video/mp4";
    }
}

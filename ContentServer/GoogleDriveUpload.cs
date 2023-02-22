using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.IdentityModel.Tokens;
using TimesheetBE.Utilities;
using TimesheetBE.Utilities.Abstrctions;
using Microsoft.Extensions.Logging;

namespace TimesheetBE.ContentServer
{
    internal class GoogleDriveUpload : IBaseContentServer
    {
        private Globals _globals;
        private readonly GoogleCredential googleCredential;
        private readonly StorageClient storageClient;
        private readonly string bucketName;
        private readonly IUtilityMethods _utilityMethods;

        public GoogleDriveUpload(Globals globals, IUtilityMethods utilityMethods)
        {
            _globals = globals;
            _utilityMethods = utilityMethods;
            googleCredential = GoogleCredential.FromFile(_globals.GoogleCredentialsFIle);
            storageClient = StorageClient.Create(googleCredential);
            bucketName = _globals.GoogleStorageBucket;
        }

        public async Task<FileDocument> UploadDocumentAsync(FileDocument document)
        {
            try
            { 
                var filePath = string.Empty;

                var bytes = Convert.FromBase64String(document.File);
                var contents = new MemoryStream(bytes);

                var uploadObjectOptions = new UploadObjectOptions
                {
                    ChunkSize = UploadObjectOptions.MinimumChunkSize,
                };
                var result = await storageClient.UploadObjectAsync(
                    bucket: bucketName,
                    objectName: document.Name,
                    contentType: document.DocumentType.MimeType,
                    source: contents,
                    options: uploadObjectOptions
                );

                return FileDocument.Create(null, document.Name, result.MediaLink, document.DocumentType);

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
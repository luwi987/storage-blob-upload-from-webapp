using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Helpers
{
    public static class StorageHelper
    {
        private static readonly string[] ImageFormats = { ".jpg", ".png", ".gif", ".jpeg" };

        public static bool IsValidFile(IFormFile file)
        {
            // Check if the file is an image based on MIME type and file extension
            return file.ContentType.Contains("image") || 
                   ImageFormats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig storageConfig)
        {
            // Resize the image if it's an image file
            if (IsImage(fileName))
            {
                using var image = Image.Load(fileStream);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(600, 600),
                    Mode = ResizeMode.Max
                }));

                fileStream.Position = 0;
                image.Save(fileStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
                fileStream.Position = 0;
            }

            // Construct the Blob URI
            Uri blobUri = new Uri($"https://{storageConfig.AccountName}.blob.core.windows.net/{storageConfig.ImageContainer}/{fileName}");
            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(storageConfig.AccountName, storageConfig.AccountKey);
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = "image/jpeg" }, conditions: null);
            return true;
        }

        private static bool IsImage(string fileName)
        {
            // Check if the file extension is one of the known image formats
            return ImageFormats.Any(item => fileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
    }
}

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
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
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            return ImageFormats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
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

            Uri blobUri = new Uri($"https://{_storageConfig.AccountName}.blob.core.windows.net/{_storageConfig.ImageContainer}/{fileName}");
            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            await blobClient.UploadAsync(fileStream, true);

            return true;
        }

        private static bool IsImage(string fileName)
        {
            return ImageFormats.Any(item => fileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
    }
}

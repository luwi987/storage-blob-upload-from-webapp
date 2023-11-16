using ImageResizeWebApp.Helpers;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private const string ErrorMessageStorageDetails = "Sorry, can't retrieve your Azure storage details from appsettings.js, make sure that you add Azure storage details there.";
        private const string ErrorMessageImageContainer = "Please provide a name for your image container in Azure blob storage.";

        private readonly AzureStorageConfig storageConfig;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            storageConfig = config.Value;
        }

        private IActionResult CheckStorageConfig()
        {
            if (string.IsNullOrEmpty(storageConfig.AccountKey) || string.IsNullOrEmpty(storageConfig.AccountName))
                return BadRequest(ErrorMessageStorageDetails);

            if (string.IsNullOrEmpty(storageConfig.ImageContainer))
                return BadRequest(ErrorMessageImageContainer);

            return null;
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            var configCheck = CheckStorageConfig();
            if (configCheck != null) 
                return configCheck;

            try
            {
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                bool isUploaded = false;
                foreach (var formFile in files)
                {
                    if (StorageHelper.IsValidFile(formFile) && formFile.Length > 0)
                    {
                        using (Stream stream = formFile.OpenReadStream())
                        {
                            isUploaded = await StorageHelper.UploadFileToStorage(stream, formFile.FileName, storageConfig);
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();
                    }
                }

                if (isUploaded)
                    return new AcceptedResult();
                else
                    return BadRequest("Look like the image couldn't upload to the storage");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

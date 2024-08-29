using CLDV7112.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLDV7112.Controllers
{
    public class FileStorageController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public FileStorageController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    using (var stream = file.OpenReadStream())
                    {
                        await _azureStorageService.UploadFileAsync(file.FileName, stream);
                    }
                    ViewBag.UploadStatus = "File uploaded successfully.";
                }
                catch (Exception ex)
                {
                    ViewBag.UploadStatus = $"Error uploading file: {ex.Message}";
                }
            }
            else
            {
                ViewBag.UploadStatus = "Please select a file to upload.";
            }
            return View();
        }
    }
}
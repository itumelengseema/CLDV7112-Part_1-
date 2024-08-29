using Azure.Data.Tables;
using CLDV7112.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Azure;
using CLDV7112.Models;

namespace CLDV7112.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public ProductsController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        public IActionResult Index()
        {
            // Code to list products
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Autogenerate PartitionKey and RowKey
                    model.PartitionKey = "ProductPartition";
                    model.RowKey = Guid.NewGuid().ToString();

                    if (image != null && image.Length > 0)
                    {
                        using (var stream = image.OpenReadStream())
                        {
                            await _azureStorageService.UploadBlobAsync(image.FileName, stream);
                        }

                        model.ImageUrl = image.FileName;
                    }

                    await _azureStorageService.InsertIntoTableAsync(model, "ProductsTable");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception to the console
                    Console.WriteLine($"Error occurred while creating product: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while processing your request. Please try again.");
                }
            }
            else
            {
                // Log model state errors to the console
                foreach (var state in ModelState)
                {
                    var key = state.Key;
                    var value = state.Value;

                    foreach (var error in value.Errors)
                    {
                        // Log the error message with key to the console
                        Console.WriteLine($"ModelState Error for {key}: {error.ErrorMessage}");
                    }
                }
            }

            return View(model); // Return the same view with model if ModelState is not valid or if insert fails
        }
    }
}
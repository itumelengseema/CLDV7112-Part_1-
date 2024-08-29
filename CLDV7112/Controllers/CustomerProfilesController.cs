using CLDV7112.Models;
using CLDV7112.Services;
using Microsoft.AspNetCore.Mvc;

public class CustomerProfilesController : Controller
{
    private readonly AzureStorageService _azureStorageService;

    public CustomerProfilesController(AzureStorageService azureStorageService)
    {
        _azureStorageService = azureStorageService;
    }

    public IActionResult Index()
    {
        // Code to list customer profiles
        return View();
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> Create(CustomerProfile model)
    {
        if (ModelState.IsValid)
        {
            model.PartitionKey = "CustomerProfile"; // Properly set PartitionKey
            model.RowKey = Guid.NewGuid().ToString(); // Generate a unique RowKey

            Console.WriteLine($"PartitionKey: {model.PartitionKey}, RowKey: {model.RowKey}");

            try
            {
                await _azureStorageService.InsertIntoTableAsync(model, "CustomerProfilesTable");

                // Logging the success
                Console.WriteLine("Customer profile saved successfully.");

                return RedirectToAction(nameof(Index)); // Redirect to Index after successful insert
            }
            catch (Exception ex)
            {
                // Log the exception and add a model error
                Console.WriteLine($"An error occurred: {ex.Message}");
                ModelState.AddModelError("", "Unable to save customer profile. Please try again.");
            }
        }
        else
        {
            foreach (var value in ModelState.Values)
            {
                foreach (var error in value.Errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
        }

        return View(model); // Return the same view with model if ModelState is not valid or if insert fails
    }
}
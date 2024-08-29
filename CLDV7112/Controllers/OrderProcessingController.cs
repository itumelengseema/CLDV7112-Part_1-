using CLDV7112.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CLDV7112.Controllers
{
    public class OrderProcessingController : Controller
    {
        private readonly AzureStorageService _azureStorageService;

        public OrderProcessingController(AzureStorageService azureStorageService)
        {
            _azureStorageService = azureStorageService;
        }

        // The nested model class
        public class Order
        {
            public string OrderDetails { get; set; }
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateOrder()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order model)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.OrderDetails))
                {
                    await _azureStorageService.EnqueueMessageAsync(model.OrderDetails, "order-queue");
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }
    }
}
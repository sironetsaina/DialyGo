using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UssdController : ControllerBase
    {
        [HttpPost]
        public IActionResult HandleUssd(
    [FromForm] string sessionId,
    [FromForm] string serviceCode,
    [FromForm] string phoneNumber,
    [FromForm] string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return Content("CON Welcome to DialyGo\n1. Book Appointment\n2. Request Truck Location\n3. Exit");
            }

            switch (text)
            {
                case "1":
                    return Content("CON Choose a date:\n1. Tomorrow\n2. Next Week");
                case "2":
                    return Content("END Truck is currently at Nairobi West.");
                case "3":
                    return Content("END Goodbye!");
                default:
                    return Content("END Invalid choice. Try again.");
            }
        }
    }
}
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/Ussd")] // ✅ Make sure this matches your callback URL exactly
    public class UssdController : ControllerBase
    {
        // ✅ USSD callback endpoint (Africa's Talking sends POST requests here)
        [HttpPost]
        public IActionResult HandleUssd(
            [FromForm] string sessionId,
            [FromForm] string serviceCode,
            [FromForm] string phoneNumber,
            [FromForm] string? text)
        {
            Response.ContentType = "text/plain";

            // Initial menu
            if (string.IsNullOrEmpty(text))
            {
                return Content("CON Welcome to DialyGo\n1. Book Appointment\n2. Request Truck Location\n3. Exit");
            }

            // Handle user input
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

        // ✅ Optional GET for testing only (e.g., in browser)
        [HttpGet]
        public IActionResult Test()
        {
            return Ok("USSD endpoint active — waiting for POST requests from Africa's Talking.");
        }
    }
}

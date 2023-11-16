using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ImageResizeWebApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home/Index
        public IActionResult Index()
        {
            // Return the default view for the Index action.
            return View();
        }

        // GET: Home/Error
        public IActionResult Error()
        {
            // The ViewData dictionary is a dynamic object that provides a convenient way to pass data from a controller to a view.
            // Here, it's used to pass the Request ID in case of an error for diagnostic purposes.
            ViewData["RequestId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            // Return the view for the Error action.
            return View();
        }
    }
}

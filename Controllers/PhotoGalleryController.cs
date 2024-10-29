using Microsoft.AspNetCore.Mvc;
public class PhotoGalleryController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

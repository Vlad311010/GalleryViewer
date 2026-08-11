using Microsoft.AspNetCore.Mvc;

namespace GalleryViewer.Controllers
{
    public class GalleryController : BaseController
    {
        public IActionResult List()
        {
            return Ok();
        }
    }
}

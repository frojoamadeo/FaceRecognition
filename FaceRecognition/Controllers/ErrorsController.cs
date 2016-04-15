using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FaceRecognition.Controllers
{
    public class ErrorsController : Controller
    {
        //
        // GET: /Errors/
        public ActionResult Index()
        {
            return View();
        }
      
        public ActionResult AccessDenied()
        {
            return View();
        }
	}
}
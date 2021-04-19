using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    [Autenticado]
    public class HomeController : Controller
    {
        private DAEntities db = new DAEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }


    }
}
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
        
        public ActionResult Index()
        {
            var rol = Session["Rol"].ToString();
            /*if (rol == BL.Constante.Rol.Secretaria)
                return RedirectToAction("Index", "Secretaria");
            if (rol == BL.Constante.Rol.Coordinador)
                return RedirectToAction("Index", "Coordinador");
            if (rol == BL.Constante.Rol.Direccion)
                return RedirectToAction("Index", "Direccion");*/

            return View();
        }


    }
}
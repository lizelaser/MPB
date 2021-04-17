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
            var comprobante = db.TipoComprobante.ToList();
            ViewBag.Boleta = comprobante.Where(c=>c.Descripcion=="BL").Select(c=>c.Serie).ToList();
            ViewBag.Factura = comprobante.Where(c => c.Descripcion == "FT").Select(c => c.Serie).ToList();
            return View();
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }


    }
}
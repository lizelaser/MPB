using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class NotasTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new NotasController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Notas.Count());
        }

        public void RegistrarTest()
        {
            DAEntities db = new DAEntities();
            var controller = new NotasController();
            var result = controller.Registrar() as ViewResult;
            bool viewBagPeriodo = result.ViewBag.PeriodoActual == (from p in db.Periodo where p.Estado == true select p.Denominacion).SingleOrDefault(); ;

            Assert.IsTrue(viewBagPeriodo);
        }
        public void GuardarTest()
        {
            DAEntities db = new DAEntities();
            var controller = new NotasController();
            var result = controller.Guardar(db.Alumno.First().Id,db.Curso.First().Id,10,"NINGUNO") as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

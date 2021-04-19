using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;
using Web.Controllers.Direccion;

namespace Web.UnitTest
{
    [TestClass]
    public class PeriodoTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new PeriodoController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Periodo.Count());
        }


        [TestMethod]
        public void GuardarTest()
        {
            var controller = new PeriodoController();
            var result = controller.Guardar(0,"PRUEBA",DateTime.Now.ToString("yyyy-MM-dd"),DateTime.Now.ToString("yyyy-MM-dd"),true) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

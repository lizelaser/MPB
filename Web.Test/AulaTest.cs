using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class AulaTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new AulaController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Aula.Count());
        }


        [TestMethod]
        public void GuardarTest()
        {
            var aula = new Aula()
            {
                Denominacion = "PRUEBA",
                Estado = true
            };
            var controller = new AulaController();
            var result = controller.Guardar(aula) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.response);
        }
    }
}

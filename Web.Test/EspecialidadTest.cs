using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class EspecialidadTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new EspecialidadController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Especialidad.Count());
        }


        [TestMethod]
        public void MantenerEditarTest()
        {
            var db = new DAEntities();
            var especialidad = db.Especialidad.FirstOrDefault();
            var controller = new EspecialidadController();
            var result = controller.Mantener(especialidad.Id) as ViewResult;
            var viewEspecialidad = (Especialidad)result.Model;
            Assert.AreEqual(especialidad.Id, viewEspecialidad.Id);
        }

        [TestMethod]
        public void GuardarTest()
        {
            var especialidad = new Especialidad()
            {
                Denominacion = "PRUEBA",
                Matricula = 150m,
                Mensualidad = 150m,
                Cuotas = 2,
                Estado = true
            };
            var controller = new EspecialidadController();
            var result = controller.Guardar(especialidad) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

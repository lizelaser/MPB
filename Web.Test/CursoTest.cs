using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class CursoTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CursoController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Curso.Count());
        }

        [TestMethod]
        public void MantenerTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CursoController();
            var result = controller.Mantener(0) as ViewResult;
            Assert.IsTrue(result.ViewBag.EspecialidadId.Count == db.Especialidad.Count());
        }


        [TestMethod]
        public void GuardarTest()
        {
            DAEntities db = new DAEntities();
            var rng = new Random();
            var cod = rng.Next(10000000, 99999999);

            var curso = new Curso()
            {
                EspecialidadId = db.Especialidad.FirstOrDefault().Id,
                Denominacion = "PRUEBA",
                Codigo = cod.ToString(),
                Matricula = 150m,
                Mensualidad = 150m,
                Cuotas = 2,
                Estado = true
            };
            var controller = new CursoController();
            var result = controller.Guardar(curso) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

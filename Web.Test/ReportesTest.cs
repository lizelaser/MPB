using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotativa;
using System;
using System.Linq;
using Web.Controllers.Secretaria;

namespace Web.UnitTest
{
    [TestClass]
    public class ReportesTest
    {
        [TestMethod]
        public void ReportesAlumnoTest()
        {
            var controller = new ReportesController();
            var result = controller.ReportesAlumno();
            Assert.IsTrue(result is ViewAsPdf);
        }

        [TestMethod]
        public void ReportesPersonalTest()
        {
            var controller = new ReportesController();
            var result = controller.ReportesPersonal();
            Assert.IsTrue(result is ViewAsPdf);
        }

        [TestMethod]
        public void FichaMatriculaTest()
        {
            var db = new DAEntities();
            var controller = new ReportesController();
            var result = controller.FichaMatricula(db.Alumno.First().Id);
            Assert.IsTrue(result is ViewAsPdf);
        }

        [TestMethod]
        public void ReportesMatriculaTest()
        {
            var db = new DAEntities();
            var controller = new ReportesController();
            var result = controller.ReportesMatricula(db.Especialidad.First().Id,null,null,null,null);
            Assert.IsTrue(result is ViewAsPdf);
        }

        [TestMethod]
        public void ReportesCuentasPorCobrarTest()
        {
            var controller = new ReportesController();
            var result = controller.ReportesDeudas(null);
            Assert.IsTrue(result is ViewAsPdf);
        }
    }
}

using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class MatriculaTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new MatriculaController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Matricula.Count());
        }

        [TestMethod]
        public void VerificarMatriculaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new MatriculaController();
            var result = controller.VerificarMatricula("74379437") as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void VerificarCursoRepetidoTest()
        {
            DAEntities db = new DAEntities();
            var controller = new MatriculaController();
            var result = controller.VerificarCursoRepetido(db.Alumno.First().Id,db.Curso.First().Id) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        //[TestMethod]
        //public void RegistrarTest()
        //{
        //    DAEntities db = new DAEntities();
        //    var controller = new MatriculaController();
        //    var detalles = new List<MatriculaDetalle>();
        //    detalles.Add(new MatriculaDetalle()
        //    {
        //        CursoId=1
        //    });
        //    var matricula = new
        //    {
        //        CondicionEstudioId = 2,
        //        PeriodoId = 8,
        //        IndPagoUnico = false,
        //        AlumnoId = 3,
        //        Monto = 200,
        //        Observacion = "NINGUNO",
        //    };
        //    var result = controller.Registrar(matricula.CondicionEstudioId, matricula.PeriodoId,matricula.IndPagoUnico, matricula.AlumnoId, null, matricula.Monto, matricula.Observacion, detalles) as JsonResult;
        //    var rm = result.Data as Comun.ResponseModel;
        //    Assert.IsTrue(rm.response);
        //}
    }
}

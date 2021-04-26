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
    public class AlumnoTest
    {

        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new AlumnoController();
            var result = controller.Tabla("",1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros==db.Alumno.Count());
        }

        [TestMethod]
        public void MantenerCrearTest()
        {
            DAEntities db = new DAEntities();
            var controller = new AlumnoController();
            var result = controller.Mantener(0) as ViewResult;
            List<Especialidad> especialidades = result.ViewBag.Especialidades;
            Assert.IsTrue(especialidades.Count==db.Especialidad.Count());
        }

        [TestMethod]
        public void MantenerEditarTest()
        {
            var db = new DAEntities();
            var alumno = db.Alumno.FirstOrDefault();
            var controller = new AlumnoController();
            var result = controller.Mantener(alumno.Id) as ViewResult;
            var viewAlumno = (Alumno)result.Model;
            Assert.AreEqual(alumno.Id,viewAlumno.Id);
        }

        [TestMethod]
        public void GuardarTest()
        {
            var rng = new Random();
            var cod = rng.Next(10000000, 99999999);
            var controller = new AlumnoController();
            var result = controller.Guardar(0,new List<int> {1}, cod.ToString(), "PRUEBA","PRUEBA","PRUEBA",$"{cod}000",null,null,null,null,true) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }


    }
}

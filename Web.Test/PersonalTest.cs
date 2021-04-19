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
    public class PersonalTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new PersonalController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Personal.Count());
        }

        [TestMethod]
        public void MantenerCrearTest()
        {
            DAEntities db = new DAEntities();
            var controller = new PersonalController();
            var result = controller.Mantener(0) as ViewResult;
            List<TipoPersonal> tipos = result.ViewBag.TiposPersonal;
            Assert.IsTrue(tipos.Count==db.TipoPersonal.Count());
        }

        [TestMethod]
        public void MantenerEditarTest()
        {
            var db = new DAEntities();
            var personal = db.Personal.FirstOrDefault();
            var controller = new PersonalController();
            var result = controller.Mantener(personal.Id) as ViewResult;
            var viewPersonal = (Personal)result.Model;
            Assert.AreEqual(personal.Id, viewPersonal.Id);
        }

        [TestMethod]
        public void GuardarTest()
        {
            var rng = new Random();
            var cod = rng.Next(10000000, 99999999);
            var controller = new PersonalController();
            var result = controller.Guardar(0, new List<int> { 1 }, cod.ToString(), "PRUEBA", "PRUEBA", "PRUEBA", "EXAMPLE@TEST.COM", null, null, null, null, true) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.response);
        }

    }
}

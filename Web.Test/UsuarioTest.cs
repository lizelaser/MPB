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
    public class UsuarioTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new UsuarioController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Usuario.Count());
        }

        [TestMethod]
        public void MantenerTest()
        {
            DAEntities db = new DAEntities();
            var controller = new UsuarioController();
            var result = controller.Mantener(0) as ViewResult;
            List<Rol> roles = result.ViewBag.RolId;
            Assert.IsTrue(roles.Count == db.Rol.Count());
        }


        [TestMethod]
        public void GuardarTest()
        {
            DAEntities db = new DAEntities();
            var controller = new UsuarioController();
            var user = new Usuario()
            {
                Id=1,
                RolId = 1,
                PersonalId = 2,
                Nombre = "GAAAAA",
                Correo = "EXAMPLE@PRUEBA.COM",
                Clave = "e10adc3949ba59abbe56e057f20f883e",
            };
            var result = controller.Guardar(user, "true") as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

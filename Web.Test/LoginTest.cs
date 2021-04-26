using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class LoginTest
    {
        [TestMethod]
        public void AutenticarTest()
        {
            var controller = new LoginController();
            var result = controller.Autenticar(new Models.UsuarioVm() {
                Usuario = "ADMIN@GMAIL.COM",
                Clave = "123456"
            }) as JsonResult;
            Assert.IsTrue(result.Data is Comun.ResponseModel);
        }
    }
}

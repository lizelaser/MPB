using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rotativa;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class PagosTest
    {
        [TestMethod]
        public void TablaPagosTest()
        {
            DAEntities db = new DAEntities();
            var controller = new PagosController();
            var result = controller.TablaPagos("",1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.CajaMovimiento.Where(x => x.EstadoId != 4).Count());
        }

        [TestMethod]
        public void TablaCobranzasTest()
        {
            DAEntities db = new DAEntities();
            var controller = new PagosController();
            var result = controller.TablaCobranzas("",1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.CuentasPorCobrar.Where(x => x.EstadoId.Equals(2)).Count());
        }

        [TestMethod]
        public void BoletaTest()
        {
            var controller = new PagosController();
            var result = controller.Boleta(1);
            Assert.IsTrue(result is ViewAsPdf);
        }

        [TestMethod]
        public void FacturaTest()
        {
            var controller = new PagosController();
            var result = controller.Factura(1);
            Assert.IsTrue(result is ViewAsPdf);
        }
    }
}

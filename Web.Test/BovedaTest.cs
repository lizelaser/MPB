using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class BovedaTest
    {
        [TestMethod]
        public void TablaBovedaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.TablaBovedas(1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Boveda.Count());
        }

        [TestMethod]
        public void TablaCajaMovimientoTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.TablaMovimientos(1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.BovedaMovimiento.Count());
        }

        [TestMethod]
        public void TablaCajaDiarioTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.TablaCajaDiario(1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.CajaDiario.Count());
        }


        [TestMethod]
        public void AperturaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.Apertura(5000) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void CierreTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.Cierre(1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void TransferenciaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.Transferencia(2,"CAJA A","Transferencia para pago de servicios",500) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void EntradasSalidasTest()
        {
            DAEntities db = new DAEntities();
            var controller = new BovedaController();
            var result = controller.EntradasSalidas(3,"EGRESOS OTROS","Transferencia BCP",100) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}


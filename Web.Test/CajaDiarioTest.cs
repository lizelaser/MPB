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
    public class CajaDiarioTest
    {
        [TestMethod]
        public void TablaCajaDiarioTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CajaDiarioController();
            var result = controller.TablaCajasDiario(1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.CajaDiario.Count());
        }

        [TestMethod]
        public void AsignarCajaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CajaDiarioController();
            var result = controller.AsignarCaja(db.Caja.First().Id,"CAJA A",db.Usuario.First().Id,"TEST",1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void CerrarCajasTest()
        {
            DAEntities db = new DAEntities();
            var detalles = new List<BovedaMovimiento>();
            detalles.Add(new BovedaMovimiento()
            {
                CajaDiarioId= db.CajaDiario.First().Id,
                OperacionId= db.Operacion.First().Id,
                Fecha= DateTime.Now,
                Glosa = "TEST",
                Importe = 300
            });

            var controller = new CajaDiarioController();
            var result = controller.CerrarCajas(10, detalles) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }

        [TestMethod]
        public void AnularMovimientoTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CajaDiarioController();
            var result = controller.AnularMovimiento(null) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}

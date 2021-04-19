using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers.Secretaria;

namespace Web.UnitTest
{
    [TestClass]
    public class CuentasPorCobrarTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new CuentasPorCobrarController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.CuentasPorCobrar.Count());
        }

        [TestMethod]
        public void RegistrarTest()
        {
            var controller = new CuentasPorCobrarController();
            var result = controller.Registrar() as ViewResult;
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void GuardarTest()
        {
            DAEntities db = new DAEntities();


            var detalles = new List<CuentasPorCobrarDetalle>();
            detalles.Add(new CuentasPorCobrarDetalle()
            {
                ConceptoPagoId = 3,
                ItemId = 3,
                Descuento = 0,
                Cantidad = 1,
                Importe = 300
            });

            var cuentasPorCobrar = new
            {
                AlumnoId = db.Alumno.First().Id,
                Fecha = DateTime.Now,
                Total = 150m,
                Descripcion = "PAGO POR CERTIFICADO"
            };

            var controller = new CuentasPorCobrarController();
            var result = controller.Guardar(cuentasPorCobrar.AlumnoId,cuentasPorCobrar.Fecha.ToString("yyyy-MM-dd"), cuentasPorCobrar.Total,cuentasPorCobrar.Descripcion, detalles) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.response);
        }
    }
}

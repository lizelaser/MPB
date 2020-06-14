using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class CajaMovimientoDetalleVm: CajaMovimientoDetalle
    {
        public string Descripcion { get; set; }
        public decimal ValorUnitario { get; set; }

    }
}

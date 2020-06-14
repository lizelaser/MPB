using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class CajaMovimientoVm: CajaMovimiento
    {
        public string AlumnoNombres { get; set; }
        public string PersonalNombres { get; set; }
        public string PersonaNombres { get; set; }
        public string Dni { get; set; } // Field for Voucher
        public string Direccion { get; set; } // Field for Voucher
        public string OperacionDenominacion { get; set; }
        public string EstadoDenominacion { get; set; }
        public string Observacion { get; set; } // Field for Vouchers
    }
}

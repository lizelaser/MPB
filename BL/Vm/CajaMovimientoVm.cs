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
        public string OperacionDenominacion { get; set; }
        public string EstadoDenominacion { get; set; }
    }
}

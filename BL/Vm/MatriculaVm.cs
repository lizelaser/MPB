using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class MatriculaVm: Matricula
    {
        public string PeriodoDenominacion { get; set; }
        public string AlumnoCodigo { get; set; }
        public string AlumnoNombres { get; set; }
        public string CondicionEstudioDenominacion { get; set; }
        public string EspecialidadDenominacion { get; set; }
        public string VoucherPago { get; set; }
        public string EstadoDenominacion { get; set; }
    }
}

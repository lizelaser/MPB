using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace BL
{
    public class NotasVm: Notas
    {
        public string AlumnoNombres { get; set; }
        public string CursoDenominacion { get; set; }
        public string PeriodoDenominacion { get; set; }
    }
}

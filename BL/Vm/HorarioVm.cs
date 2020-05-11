using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class HorarioVm:Horario
    {
        public string PeriodoDenominacion { get; set; }
        public string CursoDenominacion { get; set; }
        public string AulaDenominacion { get; set; }

    }
}

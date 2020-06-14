using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class MatriculaDetalleVm: MatriculaDetalle
    {
        public string CodigoCurso { get; set; }
        public string CursoDenominacion { get; set; }
        public int HorasTeoria { get; set; }
        public int HorasPractica { get; set; }
        public int TotalHoras { get; set; }
        public decimal Creditos { get; set; }

    }
}

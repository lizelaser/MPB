using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class EspecialidadCursoVm
    {
        public decimal MatriculaE { get; set; }
        public decimal MensualidadE { get; set; }
        public int CuotasE { get; set; }
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Denominacion { get; set; }
        public Nullable<decimal> Credito { get; set; }
        public string ReqCurso { get; set; }
        public Nullable<decimal> ReqCredito { get; set; }
    }
}

using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class MatriculaVm
    {
        public int Id { get; set; }
        public Nullable<decimal> Monto { get; set; }
        public string Observacion { get; set; }
    }
}

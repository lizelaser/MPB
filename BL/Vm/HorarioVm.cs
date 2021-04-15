using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class HorarioVm
    {
        public int Id { get; set; }
        public string Periodo { get; set; }
        public string Curso { get; set; }
        public string Aula { get; set; }
        public string Docente { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string Dias { get; set; }

    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DA
{
    using System;
    using System.Collections.Generic;
    
    public partial class Notas
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public int CursoId { get; set; }
        public int PeriodoId { get; set; }
        public System.DateTime Fecha { get; set; }
        public decimal Nota { get; set; }
        public string Observacion { get; set; }
    
        public virtual Periodo Periodo { get; set; }
        public virtual Curso Curso { get; set; }
        public virtual Alumno Alumno { get; set; }
    }
}

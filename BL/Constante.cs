using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class Constante
    {
        public static class Rol
        {
            // sincronizado con la base de datos
            public const string Administrador = "SA";
            public const string Secretaria = "SE";
            public const string Coordinador = "CO";
            public const string Direccion = "DI";
        }
        public static class ValorTabla
        {
            // sincronizado con la base de datos
            public const int TipoPersonal = 1;
            public const int Aulas = 2;
            public const int MaterialAcademico = 3;
            public const int TipoEstudio = 4;
            public const int EstadoMatriculaPago = 5;
        }
        public static class Menu {
            public static List<string> Listar(string rol)
            {
                var mnuAdministrador = new List<string> {"Usuario", "Personal", "Alumno", "Matricula", "Pagos", "CuentasPorCobrar" ,"Curso","Especialidad", "Caja","CajaDiario","Boveda","Periodo","Aula","Horario"};
                var mnuSecretaria = new List<string> {"Usuario","Personal","Alumno","Matricula","Pagos", "CuentasPorCobrar", "Curso","Especialidad","Caja", "CajaDiario","Boveda","Periodo", "Aula", "Horario" };
                var mnuCoordinador = new List<string> {"Usuario", "Personal", "Alumno", "Matricula","Pagos", "CuentasPorCobrar", "Curso", "Especialidad", "Caja", "CajaDiario", "Boveda", "Periodo", "Aula", "Horario" };
                var mnuDireccion = new List<string> {"Usuario", "Personal", "Alumno", "Matricula", "Pagos", "CuentasPorCobrar", "Curso", "Especialidad", "Caja", "CajaDiario", "Boveda", "Periodo", "Aula", "Horario" };
                switch (rol)
                {
                    case Rol.Administrador: return mnuAdministrador;
                    case Rol.Secretaria: return mnuSecretaria;
                    case Rol.Coordinador: return mnuCoordinador;
                    case Rol.Direccion: return mnuDireccion;
                    default: return null;
                }
            }
        }
        
    }
}

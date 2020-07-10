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

        public static class Menu {
            public static List<string> Listar(string rol)
            {
                var mnuAdministrador = new List<string> {"Usuario", "Personal", "Alumno","Curso","Especialidad", "Caja","CajaDiario","Boveda","Periodo","Aula","Horario"};
                var mnuSecretaria = new List<string> {"Personal","Alumno","Matricula","Pagos", "CuentasPorCobrar"};
                var mnuCoordinador = new List<string> {"Personal", "Alumno","Curso", "Especialidad","Horario","Notas" };
                var mnuDireccion = new List<string> {"Usuario", "Personal", "Alumno","Curso", "Especialidad", "Caja", "Periodo", "Aula", "Horario", "Notas" };
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

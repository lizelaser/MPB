using DA;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class AlumnoBL:Repositorio<Alumno>
    {
        public static List<Alumno> Buscar(string dni)
        {
            using (var context = new DAEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var alumnos = context.Alumno.OrderBy(x => x.Id)
                                        .Where(x => x.Dni.Contains(dni))
                                        .Take(5)
                                        .ToList();

                return alumnos;
            }
        }

        public static List<Alumno> Search(string nombres)
        {
            using (var context = new DAEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var alumno = context.Alumno.OrderBy(x => x.Id)
                                        .Where(x => x.Paterno.Contains(nombres) || x.Materno.Contains(nombres) || x.Nombres.Contains(nombres))
                                        .Take(5)
                                        .ToList();
                return alumno;
            }
        }

    }
}

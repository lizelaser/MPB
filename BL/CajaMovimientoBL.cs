using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class CajaMovimientoBL:Repositorio<CajaMovimiento>
    {

        public static List<CajaMovimiento> Buscar(int nro)
        {
            using (var context = new DAEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var movimientos = context.CajaMovimiento.OrderBy(x => x.Id)
                                        .Where(x => x.Id == nro && x.EstadoId == 3)
                                        .Take(5)
                                        .Include(x=>x.Operacion)
                                        .Include(x=>x.Estado)
                                        .Include(x=>x.Alumno)
                                        .Include(x=>x.Personal)
                                        .ToList();

                return movimientos;
            }
        }


    }
}

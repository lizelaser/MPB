using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class PersonalBL : Repositorio<Personal>
    {
        public static List<Personal> Buscar(string nombres)
        {
            using (var context = new DAEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var personal = context.Personal.OrderBy(x => x.Id)
                                        .Where(x => x.Paterno.Contains(nombres) || x.Materno.Contains(nombres) || x.Nombres.Contains(nombres))
                                        .Take(5)
                                        .ToList();
                return personal;
            }
        }
    }
}

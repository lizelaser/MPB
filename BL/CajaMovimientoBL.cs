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
        public bool Registrar(CajaMovimiento cajamovimiento)
        {
            try
            {
                using (var context = new DAEntities())
                {
                    context.Entry(cajamovimiento).State = EntityState.Added;
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }


    }
}

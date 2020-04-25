using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class MatriculaBL:Repositorio<Matricula>
    {
        public bool Registrar(Matricula matricula)
        {
            try
            {
                using (var context = new DAEntities())
                {
                    context.Entry(matricula).State = EntityState.Added;
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

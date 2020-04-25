using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class ConceptoPagoBL: Repositorio<ConceptoPago>
    {
        public static ConceptoPago Find(int id)
        {
            var concepto = new ConceptoPago();
            try
            {
                using (var context = new DAEntities())
                {
                    // Esta consulta incluye el detalle del comprobante, y el producto que tiene cada comprobante. Me refiero a sus relaciones
                    concepto = context.ConceptoPago.Where(x => x.Id == id)
                                              .Single();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return concepto;
        }

    }
}

using DA;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class CursoBL:Repositorio<Curso>
    {
        public List<Curso> Buscar (string codigo)
        {
            using (var context = new DAEntities())
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var cursos = context.Curso.OrderBy(x => x.Denominacion)
                                        .Where(x => x.Denominacion.Contains(codigo))
                                        .Take(5)
                                        .ToList();

                return cursos;
            }
        }

        public Curso Obt(int id)
        {
            var curso = new Curso();
            try
            {
                using (var context = new DAEntities())
                {
                    curso = context.Curso
                                    .Where(x => x.Id == id)
                                    .Single();
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return curso;
        }


    }
}

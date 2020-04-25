using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers.Direccion
{
    [Autenticado]
    public class PeriodoController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Periodo> Periodos;
        private Paginador<Periodo> ListadoPeriodos;
        public ActionResult Index(string denominacion, int pagina=1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.Periodo.Count();
                // We get the 'records page' from the student table
                Periodos = db.Periodo.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                if (!string.IsNullOrEmpty(denominacion))
                {
                    Periodos = db.Periodo.Where(x => x.Denominacion.Contains(denominacion)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Periodo.Where(x => x.Denominacion.Contains(denominacion)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoPeriodos = new Paginador<Periodo>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Periodos
                };
            }
            //we send the pagination class to the view
            return View(ListadoPeriodos);
        }

        public ActionResult ListarTodo()
        {
            return View(PeriodoBL.Listar());
        }
        [HttpGet]
        public ActionResult Mantener(int id=0)
        {
            if(id==0)
            {
                return View(new Periodo() { Estado=true});
            }
            else
            {
                return View(PeriodoBL.Obtener(id));
            }
        }
        [HttpPost]
        public ActionResult Guardar(Periodo obj, string activo)
        {
            var rm = new Comun.ResponseModel();
            obj.Estado = string.IsNullOrEmpty(activo) ? false : true;

            try
            {
                if (obj.Id == 0)
                {
                    obj.Estado = true;
                    PeriodoBL.Crear(obj);
                }
                else
                {
                    PeriodoBL.ActualizarParcial(obj, x => x.Denominacion, x => x.FechaInicio, 
                        x => x.FechaFin, x => x.Estado);
                }

            }
            catch(Exception eX)
            {
                rm.SetResponse(false,eX.Message);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);

        }
    }
}

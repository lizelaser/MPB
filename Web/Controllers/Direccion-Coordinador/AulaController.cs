using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_aula_todo)]
    public class AulaController : Controller
    {

        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Aula> Aulas;
        private Paginador<Aula> ListadoAulas;

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Tabla(string denominacion="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;
                using (db = new DAEntities())
                {
                    // Total number of records in the student table
                    TotalRegistros = db.Aula.Count();
                    // We get the 'records page' from the student table
                    Aulas = db.Aula.OrderBy(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .ToList();
                    if (!string.IsNullOrEmpty(denominacion))
                    {
                        Aulas = db.Aula.Where(x => x.Denominacion.Contains(denominacion)).OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = db.Aula.Where(x => x.Denominacion.Contains(denominacion)).Count();
                    }
                    // Total number of pages in the student table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list "Especialidad" only with the required fields to avoid serialization problems
                    var SubAulas = Aulas.Select(S => new Aula
                    {
                        Id = S.Id,
                        Denominacion = S.Denominacion
                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoAulas = new Paginador<Aula>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubAulas
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoAulas;
                }
                //we send the pagination class to the view
                return Json(rm, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Mantener(int id = 0)
        {
            if (id == 0)
            {
                return View(new Aula());
            }

            else
            {
                return View(AulaBL.Obtener(id));
            }

        }
        [HttpPost]
        public ActionResult Guardar(Aula obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (obj.Id == 0)
                {
                    AulaBL.Crear(obj);
                }
                else
                {
                    AulaBL.ActualizarParcial(obj, x => x.Denominacion);
                }
                rm.SetResponse(true);
                rm.href = Url?.Action("Index", "Aula");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }
        

    }
}

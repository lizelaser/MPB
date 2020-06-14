using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_horario_todo)]
    public class HorarioController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Horario> Horarios;
        private Paginador<HorarioVm> ListadoHorarios;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string denominacion, int pagina)
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
                    // Total number of records in the horario table
                    TotalRegistros = db.Horario.Count();
                    // We get the 'records page' from the horario table
                    Horarios = db.Horario.OrderBy(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x=>x.Periodo)
                                                     .Include(x=>x.Curso)
                                                     .Include(x=>x.Aula)
                                                     .ToList();

                    //We list "Horarios" only with the required fields to avoid serialization problems

                    var SubHorarios = Horarios.Select(S => new HorarioVm {
                        PeriodoDenominacion = S.Periodo.Denominacion,
                        CursoDenominacion = S.Curso.Denominacion,
                        AulaDenominacion = S.Curso.Denominacion,
                        Hora = S.Hora,
                        CantidadHora = S.CantidadHora,
                        Dia = S.Dia
                    }).ToList();

                    if (!string.IsNullOrEmpty(denominacion))
                    {
                        SubHorarios = SubHorarios.Where(x => x.CursoDenominacion.Contains(denominacion)).OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = SubHorarios.Where(x => x.CursoDenominacion.Contains(denominacion)).Count();
                    }
                    // Total number of pages in the student table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoHorarios = new Paginador<HorarioVm>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubHorarios
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoHorarios;
                }
                //we send the pagination class to the view
                return Json(rm, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult Mantener(int id = 0)
        {
            ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion");
            ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion");
            ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion");
            if (id == 0)
            {
                return View(new Horario());
            }
                
            else
            {
                return View(HorarioBL.Obtener(id));
            }
               
        }
        [HttpPost]
        public ActionResult Guardar(Horario obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (obj.Id == 0)
                {
                    ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion", obj.PeriodoId);
                    ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion", obj.CursoId);
                    ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion", obj.AulaId);

                    HorarioBL.Crear(obj);
                }
                else
                {
                    ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion", obj.PeriodoId);
                    ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion", obj.CursoId);
                    ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion", obj.AulaId);
                    HorarioBL.ActualizarParcial(obj, x => x.PeriodoId, x => x.CursoId, x => x.AulaId, x => x.Hora,
                        x => x.CantidadHora, x => x.Dia);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Horario");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Eliminar(int id)
        {

            var horario = HorarioBL.Obtener(id);

            HorarioBL.Eliminar(db,horario);

            db.SaveChanges();

            return RedirectToAction("Index", "Horario");

        }
    }
}

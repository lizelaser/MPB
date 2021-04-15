using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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
                                                     .Include(x=>x.Personal)
                                                     .ToList();

                    //We list "Horarios" only with the required fields to avoid serialization problems

                    var SubHorarios = Horarios.Select(S => new HorarioVm {
                        Id = S.Id,
                        Periodo = S.Periodo.Denominacion,
                        Curso = S.Curso.Denominacion,
                        Aula = S.Aula.Denominacion,
                        Docente= S.Personal.Paterno + " " + S.Personal.Nombres,
                        HoraInicio = S.HoraInicio.ToString(),
                        HoraFin = S.HoraFin.ToString(),
                        Dias = S.Dias
                    }).ToList();

                    if (!string.IsNullOrEmpty(denominacion))
                    {
                        SubHorarios = SubHorarios.Where(x => x.Curso.ToLower().Contains(denominacion.ToLower())).OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = SubHorarios.Where(x => x.Curso.ToLower().Contains(denominacion.ToLower())).Count();
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
            db.Configuration.LazyLoadingEnabled = false;
            var periodoActual = db.Periodo.Where(p => p.Estado).FirstOrDefault();
            var isEven = periodoActual.Denominacion.Split('-')[1]=="II";

            ViewBag.ListCursos = null;
            ViewBag.SelEspecialidad = null;

            ViewBag.PeriodoDenominacion = periodoActual?.Denominacion;
            ViewBag.PeriodoId = periodoActual?.Id;
            ViewBag.Especialidades = db.Especialidad.ToList();
            ViewBag.Aulas = db.Aula.ToList();
            ViewBag.Docentes = (from p in db.Personal
                                join pt in db.Personal_Tipo on p.Id equals pt.PersonalId
                                where pt.TipoPersonalId == 2
                                select p).ToList();

            if (id == 0)
            {
                return View(new Horario());
            }
                
            else
            {
                var horario = db.Horario.Include(x=> x.Curso).Single(x=> x.Id == id);

                ViewBag.ListCursos = db.Curso.Where(x => x.EspecialidadId == horario.Curso.EspecialidadId).ToList();
                ViewBag.SelEspecialidad = db.Especialidad.Find(horario.Curso.EspecialidadId);
                return View(HorarioBL.Obtener(id));
            }
               
        }
        [HttpPost]
        public ActionResult CursosPorEspecialidad(int id)
        {
            var rm = new Comun.ResponseModel();
            var periodoActual = db.Periodo.Where(p => p.Estado).FirstOrDefault();
            if (periodoActual!=null)
            {
                var isEven = periodoActual.Denominacion.Split('-')[1] == "II";
                var cursos = isEven ? db.Curso.Where(c => c.EspecialidadId == id && c.Ciclo % 2 == 0) : db.Curso.Where(c => c.EspecialidadId == id && c.Ciclo % 2 != 0);
                var filterCursos = cursos.Select(c => new { Id = c.Id, Denominacion = c.Denominacion }).ToList();
                rm.SetResponse(true);
                rm.result = filterCursos;
            }
            else
            {
                rm.SetResponse(false,"NO EXITE UN PERIODO ACADÉMICO ACTIVO");
            }
            return Json(rm);
        }
        [HttpPost]
        public ActionResult Guardar(Horario obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                var periodo = db.Periodo.Where(p => p.Estado).SingleOrDefault();
                if (periodo == null)
                {
                    rm.SetResponse(false, "NO EXISTE UN PERIODO ACADÉMICO ACTIVO");
                }
                else if (string.IsNullOrEmpty(obj.Dias))
                {
                    rm.SetResponse(false, "SELECCIONE LOS DÍAS DEL HORARIO");
                }
                else if (obj.CursoId <= 0 || obj.AulaId <= 0)
                {
                    rm.SetResponse(false, "SELECCIONE EL CURSO Y/O EL AULA");
                }
                else
                {
                    if (obj.Id == 0)
                    {
                        obj.PeriodoId = periodo.Id;
                        HorarioBL.Crear(obj);
                    }
                    else
                    {
                        HorarioBL.ActualizarParcial(obj, x => x.CursoId, x => x.AulaId, x => x.DocenteId, x => x.HoraInicio,
                            x => x.HoraFin, x => x.Dias);
                    }
                    rm.SetResponse(true);
                    rm.href = Url.Action("Index", "Horario");
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm);
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

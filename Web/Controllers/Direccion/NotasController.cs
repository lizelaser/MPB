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
    [PermisoAttribute(Permiso = RolesMenu.menu_notas_todo)]
    public class NotasController : Controller
    {
        DAEntities db;
        private readonly int RegistrosPorPagina = 5;
        private List<Notas> Notas;
        private Paginador<NotasVm> ListadoNotas;

        public NotasController()
        {
            db = new DAEntities();
        }
        public ActionResult Index()
        {
            ViewBag.PeriodoDisponible = (from p in db.Periodo where p.Estado == true select p).Any();

            return View();
        }

        public ActionResult Tabla(string nombres, int pagina)
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
                    TotalRegistros = db.Notas.Count();
                    // We get the 'records page' from the horario table
                    Notas = db.Notas.OrderByDescending(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x => x.Periodo)
                                                     .Include(x => x.Curso)
                                                     .Include(x => x.Alumno)
                                                     .ToList();

                    //We list "Horarios" only with the required fields to avoid serialization problems

                    var SubNotas = Notas.Select(S => new NotasVm
                    {
                        Id = S.Id,
                        PeriodoDenominacion = S.Periodo.Denominacion,
                        CursoDenominacion = S.Curso.Denominacion,
                        AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                        Fecha = S.Fecha,
                        Nota = S.Nota,
                        Observacion = S.Observacion
                    }).ToList();

                    if (!string.IsNullOrEmpty(nombres))
                    {
                        SubNotas = SubNotas.Where(x => x.AlumnoNombres.Contains(nombres)).OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = SubNotas.Where(x => x.AlumnoNombres.Contains(nombres)).Count();
                    }
                    // Total number of pages in the student table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoNotas = new Paginador<NotasVm>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubNotas
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoNotas;
                }
                //we send the pagination class to the view
                return Json(rm, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public ActionResult BuscarAlumno(string codigo)
        {

            return Json(AlumnoBL.LookFor(codigo));
        }

        [HttpPost]
        public ActionResult ListarEspecialidadPorAlumno(int? id)
        {

            var especialidades = (from e in db.Especialidad
                                  join ae in db.Alumno_Especialidad on e.Id equals ae.EspecialidadId
                                  join a in db.Alumno on ae.AlumnoId equals a.Id
                                  where a.Id == id
                                  select new { Id = e.Id, Denominacion = e.Denominacion }).ToList();

            return Json(especialidades);
        }

        [HttpPost]
        public ActionResult ListarCursosPorEspecialidad(int? id, int? alumno_id)
        {
            var periodo_id = (from p in db.Periodo where p.Estado == true select p.Id).SingleOrDefault();

            var cursos = (from c in db.Curso
                          join md in db.MatriculaDetalle
                          on c.Id equals md.CursoId
                          join m in db.Matricula
                          on md.MatriculaId equals m.Id
                          join cc in db.CuentasPorCobrar
                          on m.Id equals cc.MatriculaId
                          where m.EspecialidadId == id && m.PeridoId == periodo_id && m.AlumnoId == alumno_id && cc.EstadoId == 3
                          select new { Id = c.Id, Denominacion = c.Denominacion }).ToList();
            return Json(cursos);
        }

        [HttpPost]
        public ActionResult CursoSeleccionado(int ? id)
        {
            var curso = (from c in db.Curso where c.Id == id select c.Id).SingleOrDefault();
            return Json(curso);
        }

        public ActionResult Registrar()
        {
            ViewBag.PeriodoDisponible = (from p in db.Periodo where p.Estado == true select p).Any();

            ViewBag.PeriodoActual = (from p in db.Periodo where p.Estado == true select p.Denominacion).SingleOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Guardar(int ? alumno_id, int ? curso_id, decimal ? nota, string observacion )
        {
            var rm = new Comun.ResponseModel();
            try
            {
                var periodo_disponible = (from p in db.Periodo where p.Estado == true select p).Any();
                if (!periodo_disponible) {
                    rm.message = "PERIODO NO DISPONIBLE";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    var periodo_id = (from p in db.Periodo where p.Estado == true select p.Id).SingleOrDefault();

                    if (alumno_id == null)
                    {
                        rm.message = "POR FAVOR, SELECCIONE UN ALUMNO";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (curso_id == null)
                        {
                            rm.message = "POR FAVOR, SELECCIONE UN CURSO";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (nota == null)
                            {
                                rm.message = "COMPLETE EL CAMPO NOTA";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                var nota_alumno = new Notas();
                                nota_alumno.PeriodoId = periodo_id;
                                nota_alumno.AlumnoId = alumno_id.Value;
                                nota_alumno.CursoId = curso_id.Value;
                                nota_alumno.Fecha = DateTime.Now;
                                nota_alumno.Nota = nota.Value;
                                nota_alumno.Observacion = observacion;
                                NotasBL.Crear(nota_alumno);

                                rm.SetResponse(true);
                                rm.href = Url.Action("Index", "Notas");
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }
    }
}

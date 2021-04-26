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

        public ActionResult Tabla(string nombres="", int pagina=1)
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
                        var filtered = SubNotas.Where(x => x.AlumnoNombres.ToLower().Contains(nombres.ToLower()));
                        SubNotas = filtered.OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = filtered.Count();
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
        public ActionResult BuscarAlumno(string dni)
        {
            var alumno = AlumnoBL.Buscar(dni);

            return Json(alumno);
        }

        [HttpPost]
        public ActionResult ValidarMatricula(int idAlumno)
        {
            var rm = new Comun.ResponseModel();
            var periodoActual = (from p in db.Periodo where p.Estado == true select p).SingleOrDefault();

            try
            {
                if (periodoActual == null)
                {
                    rm.SetResponse(false, "NO EXISTE UN PERIODO ACADÉMICO ACTIVO");
                }
                else
                {
                    var verificar_matricula = (from a in db.Alumno
                                               join m in db.Matricula on a.Id equals m.AlumnoId
                                               join cc in db.CuentasPorCobrar on m.Id equals cc.MatriculaId
                                               where m.AlumnoId == idAlumno && m.PeridoId == periodoActual.Id && cc.EstadoId == 3
                                               select a).Any();
                    if (verificar_matricula)
                    {
                        rm.SetResponse(true);
                    }
                    else
                    {
                        rm.SetResponse(false);
                        rm.message = "EL ALUMNO NO ESTÁ MATRICULADO EN EL PERIODO ACADÉMICO ACTUAL";
                    }

                }
            }
            catch (Exception e)
            {
                rm.SetResponse(false, e.Message, true);
            }

            return Json(rm);
        }

        [HttpPost]
        public ActionResult ListarEspecialidadPorAlumno(int id)
        {

            var detalles = db.MatriculaDetalle
                .Include(md => md.Matricula)
                .Include(md => md.Curso)
                .Where(md => md.Matricula.AlumnoId == id).ToList();

            var especialidades = new List<dynamic>();
            foreach (var item in detalles)
            {
                var esId = item.Matricula.EspecialidadId ?? item.Curso.EspecialidadId;
                if (!especialidades.Any(e=>e.Id==esId))
                {
                    var es = db.Especialidad.Find(esId);
                    especialidades.Add(new {Id=es.Id, Denominacion=es.Denominacion });
                }
                
            }

            return Json(especialidades);
        }

        [HttpPost]
        public ActionResult ListarCursosPorEspecialidad(int? id, int? alumno_id)
        {
            var rm = new Comun.ResponseModel();

            try
            {
                var periodoActual = (from p in db.Periodo where p.Estado == true select p).SingleOrDefault();

                if (periodoActual == null)
                {
                    rm.SetResponse(false, "NO EXISTE UN PERIODO ACADÉMICO ACTIVO");
                }
                else
                {
                    var cursos = (from c in db.Curso
                                  join md in db.MatriculaDetalle
                                  on c.Id equals md.CursoId
                                  join m in db.Matricula
                                  on md.MatriculaId equals m.Id
                                  join cc in db.CuentasPorCobrar
                                  on m.Id equals cc.MatriculaId
                                  where m.EspecialidadId == id && m.PeridoId == periodoActual.Id && m.AlumnoId == alumno_id && cc.EstadoId == 3
                                  select c).ToList();

                    var cursos0 = new List<dynamic>();
                    foreach (var item in cursos)
                    {
                        var exist = db.Notas.Any(n => n.PeriodoId == periodoActual.Id && n.AlumnoId == alumno_id && n.CursoId == item.Id);
                        if (!exist)
                        {
                            cursos0.Add(new { Id = item.Id, Denominacion = item.Denominacion });
                        }
                    }

                    rm.SetResponse(true);
                    rm.result = cursos0;
                }
            }
            catch (Exception e)
            {
                rm.SetResponse(false, e.Message, true);
            }
            return Json(rm);
        }

        [HttpPost]
        public ActionResult CursoSeleccionado(int ? id)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                var curso = (from c in db.Curso where c.Id == id select c.Id).SingleOrDefault();
                if (curso<1)
                {
                    rm.SetResponse(false, "EL CURSO NO SE ENCUENTRA REGISTRADO");
                }
                else
                {
                    rm.SetResponse(true);
                    rm.result = curso;
                }
            }
            catch (Exception e)
            {
                rm.SetResponse(false, e.Message, true);
            }
            
            return Json(rm);
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
                                rm.href = Url?.Action("Index", "Notas");
                            }
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message, true);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }
    }
}

using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_matricula_todo)]
    public class MatriculaController : Controller
    {
        private DAEntities db = new DAEntities();
        private List<Matricula> Matriculas;
        private Paginador<MatriculaVm> ListadoMatriculas;
        private readonly int RegistrosPorPagina = 5;
        // GET: Matricula
        public ActionResult Index()
        {
            var periodo_actual = (from pa in db.Periodo where pa.Estado == true select pa).Any();
            ViewBag.PeriodoDisponible = periodo_actual;

            return View();
        }
        [HttpPost]
        public ActionResult Tabla(int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {

                int TotalRegistros = 0;

                // Total number of records in Matricula table with pending status
                TotalRegistros = db.Matricula.Count();
                // We get the 'records page' from the Cuentas Por Cobrar table
                Matriculas = db.Matricula.OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Periodo)
                                                 .Include(x=>x.CondicionEstudio)
                                                 .Include(x => x.Alumno)
                                                 .ToList();
                // Total number of pages in the Cuentas por Cobrar table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);


                //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
                var SubMatriculas = Matriculas.Select(S => new MatriculaVm
                {
                    Id = S.Id,
                    PeriodoDenominacion = S.Periodo.Denominacion,
                    Fecha = S.Fecha,
                    AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                    CondicionEstudioDenominacion = S.CondicionEstudio.Denominacion,
                    Monto = S.Monto,
                    Observacion = S.Observacion
                }).ToList();


                // We instantiate the 'Paging class' and assign the new values
                ListadoMatriculas = new Paginador<MatriculaVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubMatriculas
                };

                rm.SetResponse(true);
                rm.result = ListadoMatriculas;
            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BuscarAlumno(string dni)
        {
            return Json(AlumnoBL.Buscar(dni));
        }

        [HttpPost]
        public JsonResult VerificarMatricula(string dni)
        {
            var rm = new Comun.ResponseModel();

            try
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                var existe_periodo = (from ep in db.Periodo where ep.Estado == true select ep).Any();
                if (existe_periodo == false)
                {
                    rm.message = "NO SE ENCONTRÓ PERIODO ACADÉMICO DISPONIBLE";
                    rm.SetResponse(false);
                }
                else
                {
                    var periodo_actual = (from p in db.Periodo where p.Estado == true select p).SingleOrDefault();
                    var condicion_especialidad = (from ce in db.CondicionEstudio where ce.Denominacion.Equals("ESPECIALIDAD") select ce).SingleOrDefault();
                    var existe_alumno = (from ea in db.Alumno where ea.Dni == dni select ea).Any();

                    if (existe_alumno == false)
                    {
                        rm.message = "NO SE ENCONTRÓ AL ALUMNO";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        var alumno_a_matricular = (from a in db.Alumno where a.Dni.Equals(dni) select a).SingleOrDefault();
                        var esta_matriculado = (from m in db.Matricula where m.CondicionEstudioId == condicion_especialidad.Id && m.AlumnoId == alumno_a_matricular.Id && m.PeridoId == periodo_actual.Id select m).Any();

                        if (esta_matriculado == true)
                        {
                            rm.message = "ESTE ALUMNO YA ESTÁ MATRICULADO EN EL PERIODO ACADÉMICO " + periodo_actual.Denominacion;
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            var datos_alumno = new string[3];
                            datos_alumno[0] = Convert.ToString(alumno_a_matricular.Id);
                            datos_alumno[1] = alumno_a_matricular.Dni;
                            datos_alumno[2] = alumno_a_matricular.Paterno + " " + alumno_a_matricular.Materno + " " + alumno_a_matricular.Nombres;
                            rm.SetResponse(true);
                            rm.result = datos_alumno;
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm);
        }

        [HttpPost]
        public ActionResult VerificarCursoRepetido(int ? id_alumno, int ? id_curso)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (id_alumno == null)
                {
                    rm.message = "ERROR, EL ID DEL ALUMNO NO DEBE SER NULO";
                    rm.SetResponse(false, rm.message);

                }
                else
                {
                    var alumno_registrado = (from ar in db.Alumno where ar.Id == id_alumno.Value select ar).Any();

                    if (alumno_registrado == false)
                    {
                        rm.message = "ALUMNO NO ENCONTRADO";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (id_curso == null)
                        {
                            rm.message = "ERROR, EL ID DEL CURSO NO DEBE SER NULO";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            var existe_curso = (from ec in db.Curso where ec.Id == id_curso.Value select ec).Any();
                            var periodo_actual = (from pa in db.Periodo where pa.Estado == true select pa).SingleOrDefault();
                            if (existe_curso == false)
                            {
                                rm.message = "CURSO NO ENCONTRADO";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                var matricula_condicion_curso = (from cm in db.Matricula where cm.PeridoId == periodo_actual.Id && cm.CondicionEstudioId == 2 && cm.AlumnoId == id_alumno.Value select cm).ToList();

                                if (matricula_condicion_curso.Count > 0)
                                {
                                    var aux = true;

                                    foreach (var item in matricula_condicion_curso)
                                    {
                                        if (aux == true)
                                        {
                                            var matricula_detalles = (from md in db.MatriculaDetalle
                                                                      where md.MatriculaId == item.Id
                                                                      select md).ToList();

                                            foreach (var elem in matricula_detalles)
                                            {
                                                if (elem.CursoId == id_curso.Value)
                                                {
                                                    rm.message = "EL ALUMNO YA SE INSCRIBIÓ EN ESTE CURSO";
                                                    rm.SetResponse(false, rm.message);
                                                    aux = false;
                                                    break;
                                                }
                                                else
                                                {
                                                    rm.SetResponse(true);
                                                }
                                            }

                                        }
                                        else
                                        {
                                            break;
                                        }


                                    }

                                }
                                else
                                {
                                    rm.SetResponse(true);
                                }

                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm);
        }


        [HttpPost]
        public ActionResult SeleccionarEspecialidad(int id, int alumnoid)
        {
            var rm = new Comun.ResponseModel();
            var EspecialidadSeleccionada = EspecialidadBL.Obtener(id);

            var periodo_impar = db.Database.SqlQuery<Periodo>(@"select * from Periodo cross apply  string_split(Denominacion ,'-') where value = 'I'and Estado = 1").Any();
            var periodo_par = db.Database.SqlQuery<Periodo>(@"select * from Periodo cross apply  string_split(Denominacion ,'-') where value = 'II'and Estado = 1").Any();

            if (periodo_impar)
            {
                var cursos = (from c in db.Curso
                              join e in db.Especialidad
                              on c.EspecialidadId equals e.Id
                              where e.Id == EspecialidadSeleccionada.Id && c.Ciclo % 2 != 0
                              select new EspecialidadCursoVm{ MatriculaE = e.Matricula, MensualidadE = e.Mensualidad, CuotasE = e.Cuotas, Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, ReqCurso = c.ReqCurso, ReqCredito = c.ReqCredito}).ToList();

                var ListadoCursos = new List<EspecialidadCursoVm>();
                for (var i = 0; i < cursos.Count(); i++)
                {
                    if (cursos[i].ReqCurso == null && cursos[i].ReqCredito == null)
                    {
                        var id_curso = cursos[i].Id;
                        var aprobo_curso = (from n in db.Notas
                                            join c in db.Curso
                                            on n.CursoId equals c.Id
                                            join e in db.Especialidad
                                            on c.EspecialidadId equals e.Id
                                            where n.CursoId == id_curso && n.Nota >= 11
                                            select n).Any();

                        if (!aprobo_curso)
                        {
                            ListadoCursos.Add(new EspecialidadCursoVm()
                            {
                                MatriculaE = cursos[i].MatriculaE,
                                MensualidadE = cursos[i].MensualidadE,
                                CuotasE = cursos[i].CuotasE,
                                Id = cursos[i].Id,
                                Codigo = cursos[i].Codigo,
                                Denominacion = cursos[i].Denominacion,
                                Credito = cursos[i].Credito,
                                ReqCurso = cursos[i].ReqCurso,
                                ReqCredito = cursos[i].ReqCredito
                            });
                        }
                    }
                    else if (cursos[i].ReqCurso != null)
                    {

                        var id_curso = cursos[i].Id;
                        var aprobo_curso = (from n in db.Notas
                                            join c in db.Curso
                                            on n.CursoId equals c.Id
                                            join e in db.Especialidad
                                            on c.EspecialidadId equals e.Id
                                            where n.CursoId == id_curso && n.Nota >= 11
                                            select n).Any();

                        string[] aux = cursos[i].ReqCurso.Split(',');
                        var requisitos = aux.ToList();
                        List<bool> bool_aux = new List<bool>();
                        foreach (var req in requisitos)
                        {
                            var requisito_aprobado = (from n in db.Notas
                                                      join c in db.Curso
                                                      on n.CursoId equals c.Id
                                                      where c.Codigo == req && n.Nota >= 11
                                                      select n).Any();
                            bool_aux.Add(requisito_aprobado);
                        }
                        if (!bool_aux.Contains(false) && !aprobo_curso)
                        {
                            ListadoCursos.Add(new EspecialidadCursoVm()
                            {
                                MatriculaE = cursos[i].MatriculaE,
                                MensualidadE = cursos[i].MensualidadE,
                                CuotasE = cursos[i].CuotasE,
                                Id = cursos[i].Id,
                                Codigo = cursos[i].Codigo,
                                Denominacion = cursos[i].Denominacion,
                                Credito = cursos[i].Credito,
                                ReqCurso = cursos[i].ReqCurso,
                                ReqCredito = cursos[i].ReqCredito
                            });
                        }
                    }
                    else
                    {
                        // Evaluar que no este vacío para sumar los creditos aprobados del alumno
                        var existen_notas = (from n in db.Notas
                                             join a in db.Alumno
                                             on n.AlumnoId equals a.Id
                                             where n.AlumnoId == alumnoid && n.Nota >= 11
                                             select n).Any();

                        if (existen_notas)
                        {
                            var total_creditos = (from c in db.Curso
                                                  join n in db.Notas
                                                  on c.Id equals n.CursoId
                                                  join a in db.Alumno
                                                  on n.AlumnoId equals a.Id
                                                  where n.AlumnoId == alumnoid && n.Nota >= 11
                                                  select c.Credito).Sum();
                            if (cursos[i].ReqCredito <= total_creditos)
                            {
                                ListadoCursos.Add(new EspecialidadCursoVm()
                                {
                                    MatriculaE = cursos[i].MatriculaE,
                                    MensualidadE = cursos[i].MensualidadE,
                                    CuotasE = cursos[i].CuotasE,
                                    Id = cursos[i].Id,
                                    Codigo = cursos[i].Codigo,
                                    Denominacion = cursos[i].Denominacion,
                                    Credito = cursos[i].Credito,
                                    ReqCurso = cursos[i].ReqCurso,
                                    ReqCredito = cursos[i].ReqCredito
                                });
                            }
                        }

                    }
                }

                rm.result = ListadoCursos;
                rm.SetResponse(true);
            }
            else if (periodo_par)
            {
                var cursos = (from c in db.Curso
                              join e in db.Especialidad
                              on c.EspecialidadId equals e.Id
                              where e.Id == EspecialidadSeleccionada.Id && c.Ciclo % 2 == 0
                              select new EspecialidadCursoVm { MatriculaE = e.Matricula, MensualidadE = e.Mensualidad, CuotasE = e.Cuotas, Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, ReqCurso = c.ReqCurso, ReqCredito = c.ReqCredito }).ToList();

                var ListadoCursos = new List<EspecialidadCursoVm>();
                for (var i = 0; i < cursos.Count(); i++)
                {
                    if (cursos[i].ReqCurso == null && cursos[i].ReqCredito == null)
                    {
                        var id_curso = cursos[i].Id;
                        var aprobo_curso = (from n in db.Notas
                                            join c in db.Curso
                                            on n.CursoId equals c.Id
                                            join e in db.Especialidad
                                            on c.EspecialidadId equals e.Id
                                            where n.CursoId == id_curso && n.Nota >= 11
                                            select n).Any();

                        if (!aprobo_curso)
                        {
                            ListadoCursos.Add(new EspecialidadCursoVm()
                            {
                                MatriculaE = cursos[i].MatriculaE,
                                MensualidadE = cursos[i].MensualidadE,
                                CuotasE = cursos[i].CuotasE,
                                Id = cursos[i].Id,
                                Codigo = cursos[i].Codigo,
                                Denominacion = cursos[i].Denominacion,
                                Credito = cursos[i].Credito,
                                ReqCurso = cursos[i].ReqCurso,
                                ReqCredito = cursos[i].ReqCredito
                            });
                        }
                    }
                    else if (cursos[i].ReqCurso != null)
                    {

                        var id_curso = cursos[i].Id;
                        var aprobo_curso = (from n in db.Notas
                                            join c in db.Curso
                                            on n.CursoId equals c.Id
                                            join e in db.Especialidad
                                            on c.EspecialidadId equals e.Id
                                            where n.CursoId == id_curso && n.Nota >= 11
                                            select n).Any();

                        string[] aux = cursos[i].ReqCurso.Split(',');
                        var requisitos = aux.ToList();
                        List<bool> bool_aux = new List<bool>();
                        foreach (var req in requisitos)
                        {
                            var requisito_aprobado = (from n in db.Notas
                                                      join c in db.Curso
                                                      on n.CursoId equals c.Id
                                                      where c.Codigo == req && n.Nota >= 11
                                                      select n).Any();
                            bool_aux.Add(requisito_aprobado);
                        }
                        if (!bool_aux.Contains(false) && !aprobo_curso)
                        {
                            ListadoCursos.Add(new EspecialidadCursoVm()
                            {
                                MatriculaE = cursos[i].MatriculaE,
                                MensualidadE = cursos[i].MensualidadE,
                                CuotasE = cursos[i].CuotasE,
                                Id = cursos[i].Id,
                                Codigo = cursos[i].Codigo,
                                Denominacion = cursos[i].Denominacion,
                                Credito = cursos[i].Credito,
                                ReqCurso = cursos[i].ReqCurso,
                                ReqCredito = cursos[i].ReqCredito
                            });
                        }
                    }
                    else
                    {
                        // Evaluar que no este vacío para sumar los creditos aprobados del alumno
                        var existen_notas = (from n in db.Notas
                                             join a in db.Alumno
                                             on n.AlumnoId equals a.Id
                                             where n.AlumnoId == alumnoid && n.Nota >= 11
                                             select n).Any();

                        if (existen_notas)
                        {
                            var total_creditos = (from c in db.Curso
                                                  join n in db.Notas
                                                  on c.Id equals n.CursoId
                                                  join a in db.Alumno
                                                  on n.AlumnoId equals a.Id
                                                  where n.AlumnoId == alumnoid && n.Nota >= 11
                                                  select c.Credito).Sum();
                            if (cursos[i].ReqCredito <= total_creditos)
                            {
                                ListadoCursos.Add(new EspecialidadCursoVm()
                                {
                                    MatriculaE = cursos[i].MatriculaE,
                                    MensualidadE = cursos[i].MensualidadE,
                                    CuotasE = cursos[i].CuotasE,
                                    Id = cursos[i].Id,
                                    Codigo = cursos[i].Codigo,
                                    Denominacion = cursos[i].Denominacion,
                                    Credito = cursos[i].Credito,
                                    ReqCurso = cursos[i].ReqCurso,
                                    ReqCredito = cursos[i].ReqCredito
                                });
                            }
                        }

                    }
                }
                rm.result = cursos;
                rm.SetResponse(true);
            }
            else
            {
                rm.message = "PERIODO NO DEFINIDO";
                rm.SetResponse(false);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SeleccionarCurso(int id)
        {
            var rm = new Comun.ResponseModel();

            var EspecialidadSeleccionada = EspecialidadBL.Obtener(id);

            var periodo_impar = db.Database.SqlQuery<Periodo>(@"select * from Periodo cross apply  string_split(Denominacion ,'-') where value = 'I'and Estado = 1").Any();
            var periodo_par = db.Database.SqlQuery<Periodo>(@"select * from Periodo cross apply  string_split(Denominacion ,'-') where value = 'II'and Estado = 1").Any();

            if (periodo_impar)
            {
                var cursos = (from c in db.Curso
                              join e in db.Especialidad
                              on c.EspecialidadId equals e.Id
                              where e.Id == EspecialidadSeleccionada.Id && c.Ciclo % 2 != 0
                              select new { Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, Matricula = c.Matricula, Mensualidad = c.Mensualidad, Cuotas = c.Cuotas, Duracion = c.Duracion, TotalHoras = c.TotalHoras }).ToList();
                
                rm.result = cursos;
                rm.SetResponse(true);
            }
            else if (periodo_par)
            {
                var cursos = (from c in db.Curso
                              join e in db.Especialidad
                              on c.EspecialidadId equals e.Id
                              where e.Id == EspecialidadSeleccionada.Id && c.Ciclo % 2 == 0
                              select new { Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, Matricula = c.Matricula, Mensualidad = c.Mensualidad, Cuotas = c.Cuotas, Duracion = c.Duracion, TotalHoras = c.TotalHoras }).ToList();

                rm.result = cursos;
                rm.SetResponse(true);
            }
            else
            {
                rm.message = "PERIODO NO DEFINIDO";
                rm.SetResponse(false);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ConceptosPagoPorModalidad(bool ind_pago_unico)
        {
            var rm = new Comun.ResponseModel();

            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            var conceptos = new List<ConceptoPago>();

            if (ind_pago_unico)
            {
                conceptos = (from cp in db.ConceptoPago where (cp.Id == 1 || cp.Id == 2) && cp.Item != 0 select cp).ToList(); 
            }
            else
            {
                conceptos = (from cp in db.ConceptoPago where cp.Id == 1 && cp.Item != 0 select cp).ToList();
            }

            rm.result = conceptos;

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Registrar()
        {

            var periodo_actual = (from pa in db.Periodo where pa.Estado == true select pa).Any();
            ViewBag.PeriodoDisponible = periodo_actual;

            // We recover current enrollment period
            List<Periodo> periodoactual = (from p in db.Periodo where (p.Estado == true) select p).ToList();
            ViewBag.PeriodoActual = periodoactual;

            //We send initial state of 'Ind Pago Unico'
            ViewBag.IndPagoUnico = false;

            // We send the list of specialties to the study conditions (Especialidad, Curso)
            List<Especialidad> data = EspecialidadBL.Listar();
            SelectList listaEspecialidad = new SelectList(data, "Id", "Denominacion");
            ViewBag.Especialidades = listaEspecialidad;
            ViewBag.EspecialidadesCurso = listaEspecialidad;

            // We send payment concepts (Enrollment of course)
            List<ConceptoPago> conceptos = db.ConceptoPago.Where(x => x.Id==4 && x.Item!=0).ToList();
            ViewBag.ConceptoPago = conceptos;

            // We send the ids of the study conditions
            List<CondicionEstudio> condicionEspecialidad = (db.CondicionEstudio.Where(x => x.Denominacion == "ESPECIALIDAD")).ToList();
            List<CondicionEstudio> condicionCurso = (db.CondicionEstudio.Where(x=>x.Denominacion=="CURSO")).ToList();
            ViewBag.CondicionEspecialidad = condicionEspecialidad;
            ViewBag.CondicionCurso = condicionCurso;

            return View();
        }

        [HttpPost]
        public ActionResult ListarEspecialidadPorAlumno(int ? id)
        {

            var especialidades = (from e in db.Especialidad join ae in db.Alumno_Especialidad on e.Id equals ae.EspecialidadId
                                  join a in db.Alumno on ae.AlumnoId equals a.Id where a.Id == id select new{ Id = e.Id, Denominacion = e.Denominacion }).ToList();

            return Json(especialidades);
        }


        [HttpPost]
        public ActionResult Registrar(int ? CondicionEstudioId, int ? PeriodoId, bool IndPagoUnico, int ? AlumnoId, int ? EspecialidadId, decimal ? Monto, string Observacion, List<MatriculaDetalle> MatriculaDetalle, List<CuentasPorCobrarDetalle> CuentasPorCobrarDetalle)
        {
            var rm = new Comun.ResponseModel();

            int EstadoId = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e.Id).SingleOrDefault();
            int cantidad = 1;
            decimal descuento = 0;
            string descripcion = "";

            try
            {
                if (CondicionEstudioId == null)
                {
                    rm.message = "LA CONDICIÓN DE ESTUDIOS NO DEBE SER NULA";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    var condicion_estudio = (from ce in db.CondicionEstudio where ce.Id == CondicionEstudioId.Value select ce).SingleOrDefault();

                    if (PeriodoId == null)
                    {
                        rm.message = "EL PERIODO ACADÉMICO NO DEBE SER NULO";
                        rm.SetResponse(false, rm.message);
                    }

                    else
                    {
                        var periodo_activo = (from p in db.Periodo where p.Id == PeriodoId.Value && p.Estado == true select p).Any();

                        if (periodo_activo == false)
                        {
                            rm.message = "NO HAY PERIODO ACADÉMICO DISPONIBLE";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (AlumnoId == null)
                            {
                                rm.message = "INGRESE UN ALUMNO, POR FAVOR";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                if (Monto == null)
                                {
                                    rm.message = "DEBE SELECCIONAR LOS CURSOS A MATRICULAR";
                                    rm.SetResponse(false, rm.message);
                                }
                                else
                                {
                                    if (condicion_estudio.Denominacion == "ESPECIALIDAD")
                                    {

                                        //REGISTRO MATRÍCULA

                                        Matricula matricula = new Matricula();
                                        matricula.Fecha = DateTime.Now;
                                        matricula.CondicionEstudioId = CondicionEstudioId.Value;
                                        matricula.AlumnoId = AlumnoId.Value;
                                        matricula.PeridoId = PeriodoId.Value;
                                        matricula.EspecialidadId = EspecialidadId;
                                        matricula.Monto = Monto.Value;
                                        matricula.Observacion = Observacion;
                                        matricula.IndPagoMatricula = true;
                                        matricula.IndPagoUnico = IndPagoUnico;
                                        matricula.Proceso = "R";

                                        // REGISTRO DE LA MATRÍCULA DETALLE
                                        foreach (var detalle in MatriculaDetalle)
                                        {
                                            matricula.MatriculaDetalle.Add(new MatriculaDetalle()
                                            {
                                                CursoId = detalle.CursoId
                                            });
                                        }

                                        //SAVE CHANGES ENROLLMENT IN DATABASE

                                        MatriculaBL.Crear(matricula);

                                        //Recuperamos el id (identity) del registro guardado
                                        int matricula_id = matricula.Id;

                                        //REGISTRO CUENTAS POR COBRAR
                                        CuentasPorCobrar cobranza = new CuentasPorCobrar();
                                        cobranza.MatriculaId = matricula_id;
                                        cobranza.EstadoId = EstadoId;
                                        cobranza.AlumnoId = AlumnoId.Value;
                                        cobranza.Fecha = DateTime.Now;
                                        cobranza.Total = Monto.Value;

                                        if (IndPagoUnico)
                                        {
                                            descripcion = "PAGO ÚNICO MATRÍCULA";
                                        }
                                        else
                                        {
                                            descripcion = "PAGO POR MATRÍCULA";
                                        }
                                        cobranza.Descripcion = descripcion;

                                        CuentasPorCobrarBL.Crear(cobranza);

                                        //RECOVER ID FROM ACCOUNT RECEIVABLE
                                        var cobranza_id = cobranza.Id;

                                        //REGISTRAMOS LAS CUENTAS POR COBRAR DETALLE RELACIONADOS A LOS CONCEPTOS DE PAGO
                                        CuentasPorCobrarDetalle cuentasdetalle = new CuentasPorCobrarDetalle();
                                        foreach (var item in CuentasPorCobrarDetalle)
                                        {
                                            cuentasdetalle.CuentasPorCobrarId = cobranza_id;
                                            cuentasdetalle.ConceptoPagoId = item.ConceptoPagoId;
                                            cuentasdetalle.ItemId = item.ItemId;
                                            cuentasdetalle.Cantidad = cantidad;
                                            cuentasdetalle.Descuento = descuento;
                                            cuentasdetalle.Importe = item.Importe;
                                            CuentasPorCobrarDetalleBL.Crear(cuentasdetalle);
                                        }
                                    }
                                    else //CONDICIÓN CURSO
                                    {
                                        //REGISTRO MATRÍCULA
                                        Matricula matricula = new Matricula();
                                        matricula.Fecha = DateTime.Now;
                                        matricula.CondicionEstudioId = CondicionEstudioId.Value;
                                        matricula.AlumnoId = AlumnoId.Value;
                                        matricula.PeridoId = PeriodoId.Value;
                                        matricula.Monto = Monto.Value;
                                        matricula.Observacion = Observacion;
                                        matricula.IndPagoMatricula = false;
                                        matricula.IndPagoUnico = false;

                                        //REGISTRO MATRÍCULA DETALLE
                                        foreach (var detalle in MatriculaDetalle)
                                        {
                                            matricula.MatriculaDetalle.Add(new MatriculaDetalle()
                                            {
                                                CursoId = detalle.CursoId
                                            });
                                        }

                                        MatriculaBL.Crear(matricula);

                                        //Recuperamos el id (identity) del registro guardado
                                        int matricula_id = matricula.Id;

                                        descripcion = "PAGO POR INSCRIPCIÓN";

                                        //REGISTRO CUENTAS POR COBRAR
                                        CuentasPorCobrar cobranza = new CuentasPorCobrar();
                                        cobranza.MatriculaId = matricula_id;
                                        cobranza.EstadoId = EstadoId;
                                        cobranza.AlumnoId = AlumnoId.Value;
                                        cobranza.Fecha = DateTime.Now;
                                        cobranza.Total = Monto.Value;
                                        cobranza.Descripcion = descripcion;

                                        CuentasPorCobrarBL.Crear(cobranza);

                                        //RECOVER ID FROM ACCOUNT RECEIVABLE
                                        var cobranza_id = cobranza.Id;

                                        //REGISTRAMOS LAS CUENTAS POR COBRAR DETALLE RELACIONADO A LOS CONCEPTOS DE PAGO
                                        CuentasPorCobrarDetalle cuentasdetalle = new CuentasPorCobrarDetalle();
                                        foreach (var item in CuentasPorCobrarDetalle)
                                        {
                                            cuentasdetalle.CuentasPorCobrarId = cobranza_id;
                                            cuentasdetalle.ConceptoPagoId = item.ConceptoPagoId;
                                            cuentasdetalle.ItemId = item.ItemId;
                                            cuentasdetalle.Cantidad = cantidad;
                                            cuentasdetalle.Descuento = descuento;
                                            cuentasdetalle.Importe = item.Importe;
                                            CuentasPorCobrarDetalleBL.Crear(cuentasdetalle);
                                        }

                                    }
                                    rm.message = "LA MATRÍCULA SE REGISTRÓ CON ÉXITO";
                                    rm.SetResponse(true, rm.message);
                                    rm.href = Url.Action("Index", "Matricula");
                                }

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
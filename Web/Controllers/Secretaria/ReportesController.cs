using BL;
using DA;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers.Secretaria
{

    public class ReportesController : Controller
    {
        private DAEntities db;
        private List<Matricula> Matriculas;
        private Paginador<MatriculaVm> ListadoMatriculas;
        private readonly int RegistrosPorPagina = 5;

        public ReportesController()
        {
            db = new DAEntities();
        }

        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult HeaderPDF()
        {
            return View("HeaderPDF");
        }

        [AllowAnonymous]
        public ActionResult FooterPDF()
        {
            return View("FooterPDF");
        }

        [AllowAnonymous]
        public ActionResult HeaderEnrollment()
        {
            return View("HeaderEnrollment");
        }

        [AllowAnonymous]
        public ActionResult HeaderInputs(int id)
        {
            var id_usuario = id;

            ViewBag.NombreUsuario = (from u in db.Usuario where u.Id == id_usuario select u.Nombre).SingleOrDefault();

            var caja_asignada = (from ca in db.CajaDiario where ca.UsuarioId == id_usuario && ca.IndBoveda == false select ca).SingleOrDefault();
            ViewBag.SaldoInicial = caja_asignada?.SaldoInicial??0;
            ViewBag.SaldoFinal = caja_asignada?.SaldoFinal??0;
            ViewBag.PorcentajeEntradas = caja_asignada!=null?decimal.Round(((100 * caja_asignada.Entradas) / caja_asignada.SaldoFinal).Value, 2):0;
            ViewBag.Fecha = caja_asignada?.FechaInicio??DateTime.Now;

            var cajero = (from u in db.Usuario
                          join p in db.Personal
                          on u.PersonalId equals p.Id
                          where u.Id == id_usuario
                          select p.Paterno + " " + p.Materno + " " + p.Nombres).SingleOrDefault();
            ViewBag.Cajero = cajero;

            var caja = caja_asignada!=null?(from c in db.Caja where c.Id == caja_asignada.CajaId select c.Denominacion).SingleOrDefault():"CAJA NO DISPONIBLE";
            ViewBag.Caja = caja;

            var estado_caja = caja_asignada!=null?(from ec in db.CajaDiario where ec.Id == caja_asignada.Id select ec.IndCierre).SingleOrDefault():false;
            ViewBag.Estado = estado_caja;

            return View("HeaderInputs");
        }

        [AllowAnonymous]
        public ActionResult HeaderOutputs(int id)
        {
            int usuario_id = id;

            ViewBag.NombreUsuario = (from u in db.Usuario where u.Id == usuario_id select u.Nombre).SingleOrDefault();

            var caja_asignada = (from ca in db.CajaDiario where ca.UsuarioId == usuario_id && ca.IndBoveda == false select ca).SingleOrDefault();
            ViewBag.SaldoInicial = caja_asignada?.SaldoInicial??0;
            ViewBag.SaldoFinal = caja_asignada?.SaldoFinal??0;
            ViewBag.PorcentajeSalidas = caja_asignada != null ? decimal.Round(((100 * caja_asignada.Salidas??1m) / caja_asignada.SaldoFinal??1m), 2):0;
            ViewBag.Fecha = caja_asignada?.FechaInicio??DateTime.Now;

            var cajero = (from u in db.Usuario
                          join p in db.Personal
                          on u.PersonalId equals p.Id
                          where u.Id == usuario_id
                          select p.Paterno + " " + p.Materno + " " + p.Nombres).SingleOrDefault();
            ViewBag.Cajero = cajero;

            var caja = caja_asignada != null ? (from c in db.Caja where c.Id == caja_asignada.CajaId select c.Denominacion).SingleOrDefault():"CAJA NO ASIGNADA";
            ViewBag.Caja = caja;

            var estado_caja = caja_asignada != null ? (from ec in db.CajaDiario where ec.Id == caja_asignada.Id select ec.IndCierre).SingleOrDefault():false;
            ViewBag.Estado = estado_caja;

            return View("HeaderOutputs");
        }


        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult ReportesAlumno()
        {
            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderPDF", "Reportes", null, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesAlumno", db.Alumno.ToList())
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0"
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(30, 10, 10, 10)
            };

        }

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult ReportesPersonal()
        {
            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderPDF", "Reportes", null, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesPersonal", db.Personal.ToList())
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0"
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(30, 10, 10, 10)
            };

        }

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult PrincipalMatriculas()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ListarEspecialidades()
        {
            var especialidades = (from e in db.Especialidad
                                  select new { Id = e.Id, Denominacion = e.Denominacion }).ToList();

            return Json(especialidades);

        }

        [HttpPost]
        public ActionResult ListarCursosPorEspecialidad(int ? id)
        {
            var cursos = (from c in db.Curso where c.EspecialidadId == id select new { Id = c.Id, Denominacion = c.Denominacion }).ToList();
            return Json(cursos);
        }

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult ReportesMatricula(int ? especialidad_id, int ? curso_id, bool ? modalidad_id, string alumno_nombres, DateTime ? fecha_matricula)
        {

            var Matriculas = (from e in db.Especialidad
                     join m in db.Matricula
                     on e.Id equals m.EspecialidadId
                     join a in db.Alumno
                     on m.AlumnoId equals a.Id
                     join cc in db.CuentasPorCobrar
                     on a.Id equals cc.AlumnoId
                     join cm in db.CajaMovimiento
                     on cc.CajaMovimientoId equals cm.Id
                     where m.CondicionEstudioId == 1
                     select new MatriculaVm
                     {
                         AlumnoCodigo = a.Codigo,
                         AlumnoNombres = a.Paterno + " " + a.Materno + " " + a.Nombres,
                         EspecialidadId = m.EspecialidadId,
                         EspecialidadDenominacion = e.Denominacion,
                         Proceso = m.Proceso,
                         Fecha = m.Fecha,
                         VoucherPago = cm.Serie + "-" + cm.Numero,
                         IndPagoUnico = m.IndPagoUnico

                     }).ToList();


            if (!especialidad_id.Equals(null))
            {
                Matriculas = Matriculas.Where(x => x.EspecialidadId == especialidad_id).ToList();
            }

            if (!curso_id.Equals(null))
            {
                Matriculas = (from e in db.Especialidad
                              join c in db.Curso
                              on e.Id equals c.EspecialidadId
                              join md in db.MatriculaDetalle
                              on c.Id equals md.CursoId
                              join m in db.Matricula
                              on md.MatriculaId equals m.Id
                              join a in db.Alumno
                              on m.AlumnoId equals a.Id
                              join cc in db.CuentasPorCobrar
                              on a.Id equals cc.AlumnoId
                              join cm in db.CajaMovimiento
                              on cc.CajaMovimientoId equals cm.Id
                              where md.CursoId == curso_id.Value && m.CondicionEstudioId == 1
                              select new MatriculaVm{

                                  AlumnoCodigo = a.Codigo,
                                  AlumnoNombres = a.Paterno + " " + a.Materno + " " + a.Nombres,
                                  EspecialidadId = m.EspecialidadId,
                                  EspecialidadDenominacion = e.Denominacion,
                                  Proceso = m.Proceso,
                                  Fecha = m.Fecha,
                                  VoucherPago = cm.Serie + "-" + cm.Numero,
                                  IndPagoUnico = m.IndPagoUnico

                              }).ToList();
            }

            if (!modalidad_id.Equals(null))
            {
                Matriculas = Matriculas.Where(x => x.IndPagoUnico == modalidad_id.Value).ToList();
            }
            if (!string.IsNullOrEmpty(alumno_nombres))
            {
                Matriculas = Matriculas.Where(x => x.AlumnoNombres.Contains(alumno_nombres)).ToList();
            }
            if (!fecha_matricula.Equals(null))
            {

                Matriculas = Matriculas.Where(x => x.Fecha.ToString("dd/MM/yyyy") == fecha_matricula.Value.ToString("dd/MM/yyyy")).ToList();
            }


            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderPDF", "Reportes", null, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesMatricula", Matriculas)
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0 "
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(30, 10, 10, 10)
                
            };

        }

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult FichaMatricula(int id)
        {

            var periodo_actual = (from p in db.Periodo where p.Estado == true select p.Denominacion).SingleOrDefault();


            var matricula = (from m in db.Matricula
                             where m.Id == id
                             select new MatriculaVm
                             {
                                 Id = m.Id,
                                 AlumnoNombres = m.Alumno.Paterno + " " + m.Alumno.Materno + " " + m.Alumno.Nombres,
                                 PeriodoDenominacion = m.Periodo.Denominacion,
                                 Fecha = m.Fecha

                             }).SingleOrDefault();

            ViewBag.Periodo = periodo_actual;
            ViewBag.Alumno = matricula.AlumnoNombres;
            ViewBag.Fecha = matricula.Fecha;

            var matricula_detalles = db.MatriculaDetalle.Where(x => x.MatriculaId == id)
                                                        .OrderBy(x => x.Id)
                                                        .Include(x => x.Matricula)
                                                        .Include(x => x.Curso).ToList();

            //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
            var subdetalles = matricula_detalles.Select(S => new MatriculaDetalleVm
            {
                Id = S.Id,
                CodigoCurso = S.Curso.Codigo,
                CursoDenominacion = S.Curso.Denominacion,
                HorasTeoria = S.Curso.HorasTeoria ?? 1,
                HorasPractica = S.Curso.HorasPractica ?? 1,
                TotalHoras = S.Curso.TotalHoras ?? 1,
                Creditos = S.Curso.Credito ?? 1

            }).ToList();

            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderEnrollment", "Reportes", null, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("FichaMatricula",subdetalles)
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0 "
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(30, 10, 10, 10)

            };
        }

        [HttpPost]
        public ActionResult Tabla(string nombres, int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {

                int TotalRegistros = 0;

                int EstadoId = (from e in db.Estado where e.Denominacion.Equals("PAGADO") select e.Id).SingleOrDefault();
                var condicion_especialidad = (from ce in db.CondicionEstudio where ce.Denominacion.Equals("ESPECIALIDAD") select ce.Id)
                    .SingleOrDefault();

                // Total number of records in Matricula table with pending status
                TotalRegistros = db.Matricula.Where(x => x.CondicionEstudioId == condicion_especialidad).Count();
                // We get the 'records page' from the Cuentas Por Cobrar table
                Matriculas = db.Matricula.Include(x => x.Periodo)
                                                 .Include(x => x.CondicionEstudio)
                                                 .Include(x => x.Alumno)
                                                 .Include(x => x.CuentasPorCobrar)
                                                 .Where(x => x.CondicionEstudioId == condicion_especialidad)
                                                 .OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList()
                                                 .Where(x=> x.CuentasPorCobrar.All(y => y.EstadoId == EstadoId))
                                                 .ToList();

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

                if (!string.IsNullOrEmpty(nombres))
                {
                    var filtered = SubMatriculas.Where(x => x.AlumnoNombres.ToLower().Contains(nombres.ToLower()));
                    SubMatriculas = filtered.OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = filtered.Count();
                }

                // Total number of pages in the Cuentas por Cobrar table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);


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

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult PrincipalDeudas()
        {
            return View();
        }

        public JsonResult BuscarAlumno(string nombres)
        {
            return Json(AlumnoBL.Search(nombres));
        }


        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_todo)]
        public ActionResult ReportesDeudas(string nombres)
        {
            //We recover pendient state from db
            var estado_pendiente = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e.Id).SingleOrDefault();
            // We get the 'records page' from the Cuentas Por Cobrar table
            var Cobranzas = db.CuentasPorCobrar.Where(x => x.EstadoId.Equals(estado_pendiente)).OrderBy(x => x.Id)
                                             .Include(x => x.Alumno)
                                             .Include(x => x.Estado)
                                             .ToList();

            //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
            var SubCobranzas = Cobranzas.Select(S => new CuentasPorCobrarVm
            {
                Id = S.Id,
                MatriculaId = S.MatriculaId,
                AlumnoCodigo = S.Alumno.Codigo,
                AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                Fecha = S.Fecha,
                Total = S.Total,
                EstadoDenominacion = S.Estado.Denominacion,
                Descripcion = S.Descripcion,
                FechaVencimiento = S.FechaVencimiento

            }).ToList();

            if (!string.IsNullOrEmpty(nombres))
            {
                SubCobranzas = SubCobranzas.Where(x => x.AlumnoNombres.Contains(nombres)).OrderBy(x => x.Id).ToList();
            }

            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderPDF", "Reportes", null, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesDeudas", SubCobranzas)
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0"
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(30, 10, 10, 10)
            };
        }


        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_entradas)]
        public ActionResult ReportesIngreso()
        {
            var rm = new Comun.ResponseModel();

            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);



            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            var caja_asignada = (from ca in db.CajaDiario where ca.UsuarioId == UsuarioActualId && ca.IndBoveda == false select ca).SingleOrDefault();

            var estado_anulado = (from e in db.Estado where e.Denominacion.Equals("ANULADO") select e).SingleOrDefault();


            // We get the 'records page' from the caja movimiento table
            var Entradas = caja_asignada!=null?db.CajaMovimiento.Where(x => x.IndEntrada == true && x.CajaDiarioId.Equals(caja_asignada.Id) && x.EstadoId != estado_anulado.Id)
                                                .Include(x => x.Alumno)
                                                .Include(x => x.Personal)
                                                .Include(x => x.Operacion)
                                                .Include(x => x.Estado)
                                                .ToList():new List<CajaMovimiento>();

            //We list 'caja movimientos' only with the required fields to avoid serialization problems
            var SubEntradas = new List<CajaMovimientoVm>();

            for (var i = 0; i < Entradas.Count; i++)
            {

                if (Entradas[i].PersonalId != null && Entradas[i].AlumnoId == null)
                {
                    SubEntradas.Add(new CajaMovimientoVm()
                    {
                        Id = Entradas[i].Id,
                        CajaDiarioId = Entradas[i].CajaDiarioId,
                        AlumnoId = Entradas[i].AlumnoId,
                        PersonalId = Entradas[i].PersonalId,
                        PersonaNombres = Entradas[i].Personal.Paterno + " " + Entradas[i].Personal.Materno + " " + Entradas[i].Personal.Nombres,
                        OperacionId = Entradas[i].OperacionId,
                        OperacionDenominacion = Entradas[i].Operacion.Denominacion,
                        Serie = Entradas[i].Serie,
                        Numero = Entradas[i].Numero,
                        EstadoId = Entradas[i].EstadoId,
                        EstadoDenominacion = Entradas[i].Estado.Denominacion,
                        Fecha = Entradas[i].Fecha,
                        Total = Entradas[i].Total,
                        Descripcion = Entradas[i].Descripcion
                    });
                }
                //Otherwise when foreign key AlumnoId is not null and PersonalId is null
                //to avoid navigation problems too
                else
                {
                    SubEntradas.Add(new CajaMovimientoVm()
                    {
                        Id = Entradas[i].Id,
                        CajaDiarioId = Entradas[i].CajaDiarioId,
                        AlumnoId = Entradas[i].AlumnoId,
                        PersonaNombres = Entradas[i].Alumno.Paterno + " " + Entradas[i].Alumno.Materno + " " + Entradas[i].Alumno.Nombres,
                        PersonalId = Entradas[i].PersonalId,
                        OperacionId = Entradas[i].OperacionId,
                        OperacionDenominacion = Entradas[i].Operacion.Denominacion,
                        Serie = Entradas[i].Serie,
                        Numero = Entradas[i].Numero,
                        EstadoId = Entradas[i].EstadoId,
                        EstadoDenominacion = Entradas[i].Estado.Denominacion,
                        Fecha = Entradas[i].Fecha,
                        Total = Entradas[i].Total,
                        Descripcion = Entradas[i].Descripcion
                    });
                }

            }

            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderInputs", "Reportes", new {id = UsuarioActualId}, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesIngreso", SubEntradas)
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0 "
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(40, 10, 10, 10)

            };

        }

        [Autenticado]
        [PermisoAttribute(Permiso = RolesMenu.menu_reporte_salidas)]
        public ActionResult ReportesEgreso()
        {

            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);



            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            var caja_asignada = (from ca in db.CajaDiario where ca.UsuarioId == UsuarioActualId && ca.IndBoveda == false select ca).SingleOrDefault();

            var estado_anulado = (from e in db.Estado where e.Denominacion.Equals("ANULADO") select e).SingleOrDefault();
            // We get the 'records page' from the caja movimiento table
            var Salidas = caja_asignada!=null?db.CajaMovimiento.Where(x => x.IndEntrada == false && x.CajaDiarioId.Equals(caja_asignada.Id) && x.EstadoId != estado_anulado.Id)
                                                .Include(x => x.Alumno)
                                                .Include(x => x.Personal)
                                                .Include(x => x.Operacion)
                                                .Include(x => x.Estado)
                                                .ToList():new List<CajaMovimiento>();

            //We list 'caja movimientos' only with the required fields to avoid serialization problems
            var SubSalidas = new List<CajaMovimientoVm>();

            for (var i = 0; i < Salidas.Count; i++)
            {

                if (Salidas[i].PersonalId != null && Salidas[i].AlumnoId == null)
                {
                    SubSalidas.Add(new CajaMovimientoVm()
                    {
                        Id = Salidas[i].Id,
                        CajaDiarioId = Salidas[i].CajaDiarioId,
                        AlumnoId = Salidas[i].AlumnoId,
                        PersonalId = Salidas[i].PersonalId,
                        PersonaNombres = Salidas[i].Personal.Paterno + " " + Salidas[i].Personal.Materno + " " + Salidas[i].Personal.Nombres,
                        OperacionId = Salidas[i].OperacionId,
                        OperacionDenominacion = Salidas[i].Operacion.Denominacion,
                        Serie = Salidas[i].Serie,
                        Numero = Salidas[i].Numero,
                        EstadoId = Salidas[i].EstadoId,
                        EstadoDenominacion = Salidas[i].Estado.Denominacion,
                        Fecha = Salidas[i].Fecha,
                        Total = Salidas[i].Total,
                        Descripcion = Salidas[i].Descripcion
                    });
                }
                //Otherwise when foreign key AlumnoId is not null and PersonalId is null
                //to avoid navigation problems too
                else
                {
                    SubSalidas.Add(new CajaMovimientoVm()
                    {
                        Id = Salidas[i].Id,
                        CajaDiarioId = Salidas[i].CajaDiarioId,
                        AlumnoId = Salidas[i].AlumnoId,
                        PersonaNombres = Salidas[i].Alumno.Paterno + " " + Salidas[i].Alumno.Materno + " " + Salidas[i].Alumno.Nombres,
                        PersonalId = Salidas[i].PersonalId,
                        OperacionId = Salidas[i].OperacionId,
                        OperacionDenominacion = Salidas[i].Operacion.Denominacion,
                        Serie = Salidas[i].Serie,
                        Numero = Salidas[i].Numero,
                        EstadoId = Salidas[i].EstadoId,
                        EstadoDenominacion = Salidas[i].Estado.Denominacion,
                        Fecha = Salidas[i].Fecha,
                        Total = Salidas[i].Total,
                        Descripcion = Salidas[i].Descripcion
                    });
                }

            }

            // Define la URL de la Cabecera 
            string _headerUrl = Url?.Action("HeaderOutputs", "Reportes", new {id = UsuarioActualId}, "http");
            // Define la URL del Pie de página
            string _footerUrl = Url?.Action("FooterPDF", "Reportes", null, "http");

            return new ViewAsPdf("ReportesEgreso", SubSalidas)
            {
                // Establece la Cabecera y el Pie de página
                CustomSwitches = "--header-html " + _headerUrl + " --header-spacing 0 " +
                                 "--footer-html " + _footerUrl + " --footer-spacing 0 "
                ,
                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(40, 10, 10, 10)

            };
        }

    }
}

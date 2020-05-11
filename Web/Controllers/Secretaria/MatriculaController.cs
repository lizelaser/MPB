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
    public class MatriculaController : Controller
    {
        private DAEntities db = new DAEntities();
        private List<Matricula> Matriculas;
        private Paginador<MatriculaVm> ListadoMatriculas;
        private readonly int RegistrosPorPagina = 5;
        // GET: Matricula
        public ActionResult Index()
        {
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
                                                 .Include(x => x.Estado)
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
                    EstadoDenominacion = S.Estado.Denominacion,
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
        public ActionResult SeleccionarEspecialidad(int id)
        {
            var EspecialidadSeleccionada = EspecialidadBL.Obtener(id);
            var cursos = (from c in db.Curso
                                  join e in db.Especialidad
                                  on c.EspecialidadId equals e.Id
                                  where e.Id == EspecialidadSeleccionada.Id
                                  select new {MatriculaE = e.Matricula, MensualidadE = e.Mensualidad,CuotasE = e.Cuotas, Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, Matricula = c.Matricula, Mensualidad = c.Mensualidad, Cuotas=c.Cuotas, Duracion=c.Duracion, TotalHoras=c.TotalHoras }).ToList();
            return Json(cursos, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SeleccionarCurso(int id)
        {
            var EspecialidadSeleccionada = EspecialidadBL.Obtener(id);
            var cursos = (from c in db.Curso
                          join e in db.Especialidad
                          on c.EspecialidadId equals e.Id
                          where e.Id == EspecialidadSeleccionada.Id
                          select new { Id = c.Id, Codigo = c.Codigo, Denominacion = c.Denominacion, Credito = c.Credito, Matricula = c.Matricula, Mensualidad = c.Mensualidad, Cuotas = c.Cuotas, Duracion = c.Duracion, TotalHoras = c.TotalHoras }).ToList();
            return Json(cursos, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Registrar()
        {
            List<Periodo> periodoactual = (from p in db.Periodo where (p.Estado == true) select p).ToList();

            ViewBag.PeriodoActual = periodoactual;
            List<Especialidad> data = EspecialidadBL.Listar();
            SelectList listaEspecialidad = new SelectList(data, "Id", "Denominacion");
            ViewBag.Especialidades = listaEspecialidad;
            ViewBag.EspecialidadesCurso = listaEspecialidad;
            List<ConceptoPago> conceptos = db.ConceptoPago.Where(x => (x.Id==1 || x.Id==2) && x.Item!=0).ToList();
            ViewBag.ConceptoPago = conceptos;
            List<CondicionEstudio> condicionEspecialidad = (db.CondicionEstudio.Where(x => x.Denominacion == "ESPECIALIDAD")).ToList();
            List<CondicionEstudio> condicionCurso = (db.CondicionEstudio.Where(x=>x.Denominacion=="CURSO")).ToList();
            ViewBag.CondicionEspecialidad = condicionEspecialidad;
            ViewBag.CondicionCurso = condicionCurso;

            return View();
        }


        [HttpPost]
        public ActionResult Registrar(string CondicionEstudio, string Fecha, string PeriodoId, string AlumnoId, string Monto, string Observacion, List<MatriculaDetalle> MatriculaDetalle, List<CuentasPorCobrarDetalle> CuentasPorCobrarDetalle)
        {
            var rm = new Comun.ResponseModel();
            var estado = (db.Estado.Where(x=>x.Id==2)).Single();
            int EstadoId = estado.Id;
            int CondicionEstudioId = Convert.ToInt32(CondicionEstudio);
            bool IndPagoMatricula = true;
            bool IndPagoUnico = true;
            string Serie = "001";
            string Numero = "0000003";
            int cantidad = 1;
            decimal descuento = 0;
            string descripcion = "PAGO POR MATRICULA";

            try
            {
                if (CondicionEstudioId == 1)
                {//CONDICION ESTUDIO == ESPECIALIDAD
                 //REGISTRO MATRÍCULA
                    Matricula matricula = new Matricula();
                    matricula.Fecha = Convert.ToDateTime(Fecha);
                    matricula.CondicionEstudioId = CondicionEstudioId;
                    matricula.AlumnoId = Convert.ToInt32(AlumnoId);
                    matricula.PeridoId = Convert.ToInt32(PeriodoId);
                    matricula.EstadoId = EstadoId;
                    matricula.Monto = Convert.ToDecimal(Monto);
                    matricula.Observacion = Observacion;
                    matricula.IndPagoMatricula = IndPagoMatricula;
                    matricula.IndPagoUnico = IndPagoUnico;

                    // REGISTR DE LA MATRÍCULA DETALLE
                    foreach (var detalle in MatriculaDetalle)
                    {
                        matricula.MatriculaDetalle.Add(new MatriculaDetalle()
                        {
                            CursoId = detalle.CursoId
                        });
                    }

                    // REGISTRO DE CUENTAS POR COBRAR 1:1
                    matricula.CuentasPorCobrar.Add(new CuentasPorCobrar()
                    {
                        AlumnoId = Convert.ToInt32(AlumnoId),
                        EstadoId = EstadoId,
                        Serie = Serie,
                        Numero = Numero,
                        Fecha = Convert.ToDateTime(Fecha),
                        Total = Convert.ToDecimal(Monto),
                        Descripcion = descripcion
                    });

                    MatriculaBL.Crear(matricula);
                    //Recuperamos el id (identity) del registro guardado
                    int idMatricula = matricula.Id;

                    //REGISTRAMOS LAS CUENTAS POR COBRAR DETALLE RELACIONADOS A LOS CONCEPTOS DE PAGO
                    CuentasPorCobrarDetalle cuentasdetalle = new CuentasPorCobrarDetalle();
                    foreach (var item in CuentasPorCobrarDetalle)
                    {
                        cuentasdetalle.CuentasPorCobrarId = idMatricula;
                        cuentasdetalle.ConceptoPagoId = item.ConceptoPagoId;
                        cuentasdetalle.ItemId = item.ItemId;
                        cuentasdetalle.Cantidad = cantidad;
                        cuentasdetalle.Descuento = descuento;
                        cuentasdetalle.Importe = item.Importe;
                        CuentasPorCobrarDetalleBL.Crear(cuentasdetalle);
                    }

                }
                else{//CONDICION ESTUDIO == CURSO
                     //REGISTRO MATRÍCULA
                        Matricula matricula = new Matricula();
                        matricula.Fecha = Convert.ToDateTime(Fecha);
                        matricula.CondicionEstudioId = CondicionEstudioId;
                        matricula.AlumnoId = Convert.ToInt32(AlumnoId);
                        matricula.PeridoId = Convert.ToInt32(PeriodoId);
                        matricula.EstadoId = EstadoId;
                        matricula.Monto = Convert.ToDecimal(Monto);
                        matricula.Observacion = Observacion;
                        matricula.IndPagoMatricula = IndPagoMatricula;
                        matricula.IndPagoUnico = IndPagoUnico;

                        //REGISTRO MATRÍCULA DETALLE
                        foreach (var detalle in MatriculaDetalle)
                        {
                            matricula.MatriculaDetalle.Add(new MatriculaDetalle()
                            {
                                CursoId = detalle.CursoId
                            });
                        }

                        //REGISTRO CUENTAS POR COBRAR 1:1
                        matricula.CuentasPorCobrar.Add(new CuentasPorCobrar()
                        {
                            AlumnoId = Convert.ToInt32(AlumnoId),
                            EstadoId = EstadoId,
                            Serie = Serie,
                            Numero = Numero,
                            Fecha = Convert.ToDateTime(Fecha),
                            Total = Convert.ToDecimal(Monto),
                            Descripcion = descripcion
                        });

                        MatriculaBL.Crear(matricula);

                        //Recuperamos el id (identity) del registro guardado
                        int idMatricula = matricula.Id;

                        //REGISTRAMOS LAS CUENTAS POR COBRAR DETALLE RELACIONADO A LOS CONCEPTOS DE PAGO
                        CuentasPorCobrarDetalle cuentasdetalle = new CuentasPorCobrarDetalle();
                        foreach (var item in CuentasPorCobrarDetalle)
                        {
                            cuentasdetalle.CuentasPorCobrarId = idMatricula;
                            cuentasdetalle.ConceptoPagoId = item.ConceptoPagoId;
                            cuentasdetalle.ItemId = item.ItemId;
                            cuentasdetalle.Cantidad = cantidad;
                            cuentasdetalle.Descuento = descuento;
                            cuentasdetalle.Importe = item.Importe;
                            CuentasPorCobrarDetalleBL.Crear(cuentasdetalle);
                        }

                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index","Matricula");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

    }
}
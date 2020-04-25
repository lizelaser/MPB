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
        private MatriculaBL matric = new MatriculaBL();

        // GET: Matricula
        public ActionResult Index()
        {
            return View(MatriculaBL.Listar(includeProperties:"Periodo,Alumno,CondicionEstudio"));
        }
        [HttpPost]
        public ActionResult Tabla()
        {
            //logistica datatable
            var draw = Request.Form.GetValues("draw").FirstOrDefault();
            var start = Request.Form.GetValues("start").FirstOrDefault();
            var length = Request.Form.GetValues("length").FirstOrDefault();
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();
            int pageSize, skip, recordsTotal;
            pageSize = length != null ? Convert.ToInt32(length) : 0;
            skip = start != null ? Convert.ToInt32(start) : 0;
            recordsTotal = 0;

            var lst = new List<MatriculaVm>();
            using (var db = new DAEntities())
            {
                var query = (from d in db.Matricula select new MatriculaVm{Id=d.Id,Monto=d.Monto,Observacion=d.Observacion});

                if (searchValue != "")
                    query = query.Where(d => d.Observacion.Contains(searchValue));
                //Sorting    
                if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDir);
                }

                recordsTotal = query.Count();
                lst = query.Skip(skip).Take(pageSize).ToList();
            }

            return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = lst });
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
            string mensaje = "";
            var estado = (db.Estado.Where(x=>x.Id==2)).Single();
            int EstadoId = estado.Id;
            //var condicion = (db.CondicionEstudio.Where(x => x.Id == 1)).Single();
            //int CondicionEstudioId = condicion.Id;
            int CondicionEstudioId = Convert.ToInt32(CondicionEstudio);
            bool IndPagoMatricula = true;
            bool IndPagoUnico = true;
            string Serie = "001";
            string Numero = "0000001";
            int cantidad = 1;
            decimal descuento = 0;
            bool IndEntrada = true;

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
                        IndEntrada = IndEntrada
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

                    mensaje = "MATRÍCULA GUARDADA CON ÉXITO";
                }
                else if (CondicionEstudioId == 2){//CONDICION ESTUDIO == CURSO
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
                            IndEntrada = IndEntrada
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
                    mensaje = "MATRÍCULA GUARDADA CON ÉXITO";

                }
                    
                else
                {
                    mensaje = "ERROR AL GUARDAR LA MATRÍCULA";
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(mensaje, JsonRequestBehavior.AllowGet);
        }

    }
}
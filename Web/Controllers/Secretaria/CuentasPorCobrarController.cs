using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Newtonsoft.Json;

namespace Web.Controllers.Secretaria
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_cuentasporcobrar_todo)]
    public class CuentasPorCobrarController : Controller
    {
        private DAEntities db;
        private readonly int RegistrosPorPagina = 5;
        private List<CuentasPorCobrar> Deudas;
        private Paginador<CuentasPorCobrarVm> ListadoDeudas;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string nombres="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.CuentasPorCobrar.Count();
                // We get the 'records page' from the student table
                Deudas = db.CuentasPorCobrar.OrderByDescending(x => x.Id)
                                                 .Include(x => x.Alumno)
                                                 .Include(X => X.Estado)
                                                 .ToList();


                //We list courses only with the required fields to avoid serialization problems
                var SubDeudas = Deudas.Select(S => new CuentasPorCobrarVm
                {
                    Id = S.Id,
                    AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                    MatriculaId = S.MatriculaId,
                    Fecha = Convert.ToDateTime(S.Fecha),
                    Total = S.Total,
                    EstadoDenominacion = S.Estado.Denominacion,
                    Descripcion = S.Descripcion

                });

                if (!string.IsNullOrEmpty(nombres))
                {

                    SubDeudas = SubDeudas.Where(x => 
                        x.AlumnoNombres.ToLower().Contains(nombres.ToLower())
                    );
                }

                SubDeudas = SubDeudas.OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                TotalRegistros = Deudas.Count();

                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                // We instantiate the 'Paging class' and assign the new values
                ListadoDeudas = new Paginador<CuentasPorCobrarVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubDeudas.ToList(),
                };

                rm.SetResponse(true);
                rm.result = ListadoDeudas;
            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult BuscarAlumno(string dni)
        {
            var alumno = AlumnoBL.Buscar(dni);
            return Json(alumno);
        }

        public ActionResult Registrar()
        {
            DAEntities db = new DAEntities();
            // LISTAMOS LOS CONCEPTOS OTROS INGRESOS ()
            List<ConceptoPago> conceptos = (from cp in db.ConceptoPago where cp.Id.Equals(3) && !cp.Item.Equals(0) select cp).ToList();
            SelectList ListaConceptos = new SelectList(conceptos, "Item","Concepto");
            ViewBag.ListaConceptos = ListaConceptos;
            return View();
        }

        [HttpPost]
        public ActionResult SeleccionarConcepto(int item, string denominacion)
        {
            DAEntities db = new DAEntities();
            var concepto = (from cp in db.ConceptoPago where cp.Concepto.Equals(denominacion) && cp.Item.Equals(item) select new { Id = cp.Id, Item = cp.Item, Concepto = cp.Concepto, Precio = cp.Precio }).SingleOrDefault();
            return Json(concepto, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Guardar(int? AlumnoId, string Fecha, decimal?Total, string Descripcion, List<CuentasPorCobrarDetalle> CuentasPorCobrarDetalle)
        {
            DAEntities db = new DAEntities();
            var rm = new Comun.ResponseModel();
            //Asignamos el estado pendiente a la cuenta por cobrar
            int EstadoId = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e.Id).SingleOrDefault();
            DateTime endofDay = DateTime.Now.AddDays(1).AddMilliseconds(-1);

            try
            {
                if (AlumnoId != null && Fecha != "" && Total != null && CuentasPorCobrarDetalle != null)
                {
                    //Registramos la cuenta por cobrar

                    CuentasPorCobrar cobranzas = new CuentasPorCobrar();
                    cobranzas.AlumnoId = (int)AlumnoId;
                    cobranzas.EstadoId = EstadoId;
                    cobranzas.Fecha = Convert.ToDateTime(Fecha);
                    cobranzas.Total = (decimal)Total;
                    cobranzas.Descripcion = Descripcion;
                    cobranzas.FechaVencimiento = endofDay;

                    //Verify if exist any 'cuenta por cobrar' that already exist in BD

                    var existe_cobranza = (from cc in db.CuentasPorCobrar where cc.Id == cobranzas.Id select cc).Any();

                    if (!existe_cobranza)
                    {
                        //Registramos los detalles de las cuentas por cobrar
                        CuentasPorCobrarBL.Crear(cobranzas);

                        //Recuperamos id de la cuenta por cobrar
                        int CuentaPorCobrarId = cobranzas.Id;

                        CuentasPorCobrarDetalle detalles = new CuentasPorCobrarDetalle();
                        foreach (var item in CuentasPorCobrarDetalle)
                        {
                            detalles.CuentasPorCobrarId = CuentaPorCobrarId;
                            detalles.ConceptoPagoId = item.ConceptoPagoId;
                            detalles.ItemId = item.ItemId;
                            detalles.Cantidad = item.Cantidad;
                            detalles.Descuento = item.Descuento;
                            detalles.Importe = item.Importe;
                            CuentasPorCobrarDetalleBL.Crear(detalles);
                        }

                        rm.SetResponse(true);
                        rm.href = Url?.Action("Index", "CuentasPorCobrar");
                    }
                    else
                    {
                        rm.SetResponse(false, "Esta cuenta por cobrar ya fue registrada");
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

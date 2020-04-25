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
    public class PagosController : Controller
    {
        private DAEntities db = new DAEntities();
        private CajaMovimientoBL CajaMov = new CajaMovimientoBL();

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagina"></param>
        /// <returns></returns>
        private readonly int RegistrosPorPagina = 5;
        private List<CajaMovimiento> Pagos;
        private Paginador<CajaMovimiento> ListadoPagos;
        private List<CuentasPorCobrar> Cobranzas;
        private Paginador<CuentasPorCobrar> ListadoCobranzas;
        // GET: Pago
        public ActionResult Index(int pagina = 1)
        {
            int TotalRegistros = 0;
            // MOVIMIENTOS DE CAJA: PAGOS
            using (db = new DAEntities())
            {
                // Total number of records in the caja movimiento table
                TotalRegistros = db.CajaMovimiento.Count();
                // We get the 'records page' from the caja movimiento table
                Pagos = db.CajaMovimiento.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Alumno)
                                                 .Include(x => x.Operacion)
                                                 .Include(x=>x.CajaDiario)
                                                 .ToList();
                // Total number of pages in the caja movimiento table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoPagos = new Paginador<CajaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Pagos
                };
                ViewBag.ListadoPagos = ListadoPagos;
                
            }
            // ARQUEO DE CAJA/ ENTRADAS A CAJA
            using (db = new DAEntities())
            {
                TotalRegistros = db.CajaMovimiento.Where(x => x.IndEntrada == true).Count();
                Pagos = db.CajaMovimiento.Where(x=>x.IndEntrada==true).OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Alumno)
                                                 .Include(x => x.Operacion)
                                                 .Include(x => x.CajaDiario)
                                                 .ToList();
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                ListadoPagos = new Paginador<CajaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Pagos
                };
                ViewBag.Entradas = ListadoPagos;

            }
            // ARQUEO DE CAJA/ SALIDAS A CAJA
            using (db = new DAEntities())
            {
                TotalRegistros = db.CajaMovimiento.Where(x => x.IndEntrada == false).Count();
                Pagos = db.CajaMovimiento.Where(x => x.IndEntrada == false).OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Alumno)
                                                 .Include(x => x.Operacion)
                                                 .Include(x => x.CajaDiario)
                                                 .ToList();
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                ListadoPagos = new Paginador<CajaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Pagos
                };
                ViewBag.Salidas = ListadoPagos;

            }
            //MOVIMIENTOS DE CAJA: CUENTAS POR COBRAR
            using (db = new DAEntities())
            {
                // Total number of records in the cuentas por cobrar table
                TotalRegistros = db.CuentasPorCobrar.Count();
                // We get the 'records page' from the cuentas por cobrar table
                Cobranzas = db.CuentasPorCobrar.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Alumno)
                                                 .Include(x => x.Matricula)
                                                 .Include(x => x.Estado)
                                                 .ToList();
                // Total number of pages in the boveda table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoCobranzas = new Paginador<CuentasPorCobrar>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Cobranzas
                };
                ViewBag.ListadoCobranzas = ListadoCobranzas;
            }
            //List operaciones table
            List<Operacion> operaciones = OperacionBL.Listar();
            SelectList listaOperaciones = new SelectList(operaciones, "Id", "Denominacion");
            ViewBag.Operaciones = listaOperaciones;
            return View();
        }

        public ActionResult Buscar(string nombres)
        {
            return Json(AlumnoBL.Buscar(nombres));
        }

        [HttpPost]
        public ActionResult SeleccionarConcepto(ConceptoPago objConceptoPago)
        {
            //var data = db.ConceptoPago.Where(x => x.Id == objConceptoPago.Id).FirstOrDefault();
            var data = ConceptoPagoBL.Obtener(objConceptoPago.Id);           
            return Json(data, JsonRequestBehavior.AllowGet);
        }


    }
}
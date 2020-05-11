using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    public class BovedaController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Boveda> Bovedas;
        private Paginador<Boveda> ListadoBovedas;
        private List<BovedaMovimiento> BovedasMovimiento;
        private Paginador<BovedaMovimiento> ListadoBovedasMovimiento;
        private List<CajaDiario> CajasDiario;
        private Paginador<CajaDiario> ListadoCajasDiario;
        public ActionResult Index(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the boveda table
                TotalRegistros = db.Boveda.Count();
                // We get the 'records page' from the boveda table
                var actual = db.Boveda.Where(x => !x.IndCierre).Take(1).ToList();

                var size = actual.Any() ? 4 : RegistrosPorPagina;

                Bovedas = db.Boveda.Where(x => x.IndCierre).OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * size)
                                                 .Take(size)
                                                 .ToList();

                if (actual.Any())
                {
                    Bovedas.Insert(0, actual.First());
                }
                
                
                // Total number of pages in the boveda table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / size);
                // We instantiate the 'Paging class' and assign the new values
                ListadoBovedas = new Paginador<Boveda>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Bovedas
                };
                ViewBag.ListadoBovedas = ListadoBovedas;
            }
            using (db = new DAEntities())
            {
                // Total number of records in the boveda movimiento table
                TotalRegistros = db.BovedaMovimiento.Count();
                // We get the 'records page' from the boveda movimiento table
                BovedasMovimiento = db.BovedaMovimiento.OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.CajaDiario)
                                                 .Include(x => x.Operacion)
                                                 .ToList();
                // Total number of pages in the boveda movimiento table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoBovedasMovimiento = new Paginador<BovedaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = BovedasMovimiento
                };
                ViewBag.ListadoBovedasMovimiento = ListadoBovedasMovimiento;

                //Objeto Boveda Activa

                List<Boveda> bovedaActual = db.Boveda.Where(x => x.IndCierre == false).ToList();

                ViewBag.BovedaActual = bovedaActual;

                Boveda actual = (from b in db.Boveda where b.IndCierre.Equals(false) select b).SingleOrDefault();
                ViewBag.Cards = actual;

                //Lista de Operaciones 
                var Operaciones = (from o in db.Operacion where o.IndTipo.Equals(true) select new { Id = o.Id, Operacion = o.Denominacion }).ToList();
                SelectList listaOperaciones = new SelectList(Operaciones, "Id","Operacion");
                ViewBag.ListaOperaciones = listaOperaciones;
                ViewBag.Operaciones = listaOperaciones;

                //Lista de CajasDisponibles
                var CajasDisponibles = (from cd in db.CajaDiario
                                        join c in db.Caja
                                        on cd.CajaId equals c.Id
                                        where cd.IndCierre == false && cd.IndBoveda == false
                                        select new { Id = cd.Id, Caja = c.Denominacion }).ToList();

                SelectList listaCajas = new SelectList(CajasDisponibles, "Id", "Caja");
                ViewBag.CajasDisponibles = listaCajas;

            }
            using (db = new DAEntities())
            {
                // Total number of records in the caja diario table
                TotalRegistros = db.CajaDiario.Count();
                // We get the 'records page' from the caja diario table
                CajasDiario = db.CajaDiario.OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Usuario)
                                                 .Include(x=>x.Caja)
                                                 .ToList();
                // Total number of pages in the caja diario table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoCajasDiario = new Paginador<CajaDiario>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = CajasDiario
                };
                ViewBag.ListadoCajasDiario = ListadoCajasDiario;
            }
            return View();
        }

        // PAGINATION 'Boveda Movimiento'
        [HttpPost]
        public ActionResult TablaMovivimiento(int pagina)
        {
            var rm = new Comun.ResponseModel();
            int TotalRegistros = 0;
            try
            {
                using (db = new DAEntities())
                {
                    // Total number of records in the boveda movimiento table
                    TotalRegistros = db.BovedaMovimiento.Count();
                    // We get the 'records page' from the boveda movimiento table
                    BovedasMovimiento = db.BovedaMovimiento.OrderByDescending(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x => x.CajaDiario)
                                                     .Include(x => x.Operacion)
                                                     .ToList();
                    // Total number of pages in the boveda movimiento table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                    // We instantiate the 'Paging class' and assign the new values
                    ListadoBovedasMovimiento = new Paginador<BovedaMovimiento>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = BovedasMovimiento
                    };
                    ViewBag.ListadoBovedasMovimiento = ListadoBovedasMovimiento;
                }

                rm.SetResponse(true);
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);

        }

        // PAGINATION 'Cajas Diario'

        //Método de Apertura de la Bóveda
        [HttpPost]
        public ActionResult Apertura(string FechaInicio, string SaldoInicial)
        {
            var rm = new Comun.ResponseModel();
            decimal SaldoFinal = Convert.ToDecimal(SaldoInicial);
            decimal Entradas = 0;
            decimal Salidas = 0;
            try
            {
                //Aperturamos la bóveda con lo ingresado por el administrador
                Boveda boveda = new Boveda();
                boveda.SaldoInicial = Convert.ToDecimal(SaldoInicial);
                boveda.SaldoFinal = SaldoFinal;
                boveda.Entradas = Entradas;
                boveda.Salidas = Salidas;
                boveda.FechaInicio = Convert.ToDateTime(FechaInicio);
                BovedaBL.Crear(boveda);

                //Recuperamos los datos de la bóveda actual
                var id = Convert.ToString(boveda.Id);
                var fechaInicial = Convert.ToString(boveda.FechaInicio);
                var saldoInicial = Convert.ToString(boveda.SaldoInicial);

                //Agregamos los datos a un array para enviarnos como resultado a la vista
                var bovedaActual = new string[3];
                bovedaActual[0] = id;
                bovedaActual[1] = fechaInicial;
                bovedaActual[2] = saldoInicial;
                rm.SetResponse(true);
                rm.result = bovedaActual;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);

        }

        //Método Cierre Bóveda
        [HttpPatch]
        public ActionResult Cierre(string Id, string FechaInicio, string FechaFin, string SaldoInicial, string SaldoFinal, string Entradas, string Salidas)
        {
            var rm = new Comun.ResponseModel();
            var IndCierre = true;
            try
            {
                Boveda boveda = new Boveda();
                boveda.Id = Convert.ToInt32(Id);
                boveda.FechaInicio = Convert.ToDateTime(FechaInicio);
                boveda.FechaFin = Convert.ToDateTime(FechaFin);
                boveda.SaldoInicial = Convert.ToDecimal(SaldoInicial);
                boveda.SaldoFinal = Convert.ToDecimal(SaldoFinal);
                boveda.Entradas = Convert.ToDecimal(Entradas);
                boveda.Salidas = Convert.ToDecimal(Salidas);
                boveda.IndCierre = IndCierre;
                BovedaBL.ActualizarParcial(boveda, x=>x.FechaInicio,x=>x.FechaFin,x=>x.SaldoInicial, x=>x.SaldoFinal, x=>x.Entradas, x=>x.Salidas, x=>x.IndCierre);

                //Recuperamos los datos de la bóveda actual
                var fechaInicial = Convert.ToString(boveda.FechaInicio);
                var fechaFin = Convert.ToString(boveda.FechaFin);
                var saldoInicial = Convert.ToString(boveda.SaldoInicial);
                var saldoFinal = Convert.ToString(boveda.SaldoFinal);
                var entradas = Convert.ToString(boveda.Entradas);
                var salidas = Convert.ToString(boveda.Salidas);
                var indCierre = Convert.ToString(boveda.IndCierre);
                //Agregamos los datos a un array para enviarnos como resultado a la vista
                var bovedaActual = new string[7];
                bovedaActual[0] = fechaInicial;
                bovedaActual[1] = fechaFin;
                bovedaActual[2] = saldoInicial;
                bovedaActual[3] = saldoFinal;
                bovedaActual[4] = entradas;
                bovedaActual[5] = salidas;
                bovedaActual[6] = indCierre;
                rm.SetResponse(true);
                rm.result = bovedaActual;

            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);
        }


        //Método Transferencia de Bóveda
        [HttpPost]
        public ActionResult Transferencia(int CajaDiarioId, string CajaDenominacion, int OperacionId, string OperacionDenominacion, string Glosa, string Importe)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                Boveda boveda = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();
                CajaDiario cajaDiario = (from cd in db.CajaDiario where cd.Id == CajaDiarioId select cd).SingleOrDefault();

                //--------datos de la boveda actual-------//
                int BovedaId = boveda.Id;
                decimal BovedaSaldoInicial = boveda.SaldoInicial;
                decimal BovedaEntradas = (boveda.Entradas).Value;
                decimal SalidasBoveda = (boveda.Salidas).Value;
                SalidasBoveda = SalidasBoveda + Convert.ToDecimal(Importe);
                decimal SaldoFinalBoveda = BovedaSaldoInicial + BovedaEntradas - SalidasBoveda;

                //--------datos de la caja diario actual -----//
                decimal CajaSaldoInicial = cajaDiario.SaldoInicial + Convert.ToDecimal(Importe);
                decimal CajaSalidas = (cajaDiario.Salidas).Value;
                decimal EntradasCaja = (cajaDiario.Entradas).Value;
                decimal SaldoFinalCaja = CajaSaldoInicial + EntradasCaja - CajaSalidas;

                //Realizamos la transferencia a caja: BovedaMovimiento
                BovedaMovimiento movimiento = new BovedaMovimiento();
                movimiento.BovedaId = BovedaId;
                movimiento.CajaDiarioId = CajaDiarioId;
                movimiento.OperacionId = OperacionId;
                movimiento.Fecha = DateTime.Now;
                movimiento.Glosa = Glosa;
                movimiento.Importe = Convert.ToDecimal(Importe);
                BovedaMovimientoBL.Crear(movimiento);

                //Sumamos el importe a las salidas de la boveda
                Boveda actualBoveda = new Boveda();
                actualBoveda.Id = BovedaId;
                actualBoveda.Salidas = SalidasBoveda;
                actualBoveda.SaldoFinal = SaldoFinalBoveda;
                BovedaBL.ActualizarParcial(actualBoveda, x => x.Salidas, x => x.SaldoFinal);

                //Agregamos el importe a las entradas de la caja diario
                CajaDiario actualCaja = new CajaDiario();
                actualCaja.Id = CajaDiarioId;
                actualCaja.SaldoInicial = CajaSaldoInicial;
                actualCaja.SaldoFinal = SaldoFinalCaja;
                CajaDiarioBL.ActualizarParcial(actualCaja, x => x.SaldoInicial, x => x.SaldoFinal);

                //Recuperamos los datos del movimiento que enviaremos a nuestra vista

                var datos = new string[11];
                datos[0] = Convert.ToString(movimiento.Id);
                datos[1] = Convert.ToString(movimiento.CajaDiarioId);
                datos[2] = Convert.ToString(movimiento.Fecha);
                datos[3] = OperacionDenominacion;
                datos[4] = movimiento.Glosa;
                datos[5] = Convert.ToString(movimiento.Importe);
                datos[6] = Convert.ToString(BovedaEntradas);
                datos[7] = Convert.ToString(SalidasBoveda);
                datos[8] = Convert.ToString(SaldoFinalBoveda);
                datos[9] = Convert.ToString(SaldoFinalCaja);

                /* Eliminamos el espacio y luego lo unimos con una subraya el nombre de la caja
                 para poder compararlo con el nombre del id de la caja coincidente en la vista*/
                string[] aux = CajaDenominacion.Split(' ');
                string CajaNombre = string.Join("_", aux);
                datos[10] = CajaNombre;

                rm.SetResponse(true);
                rm.result = datos;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);

        }

        //Método Entradas/Salidas de Bóveda
        [HttpPost]
        public ActionResult EntradasSalidas(int OperacionId, string OperacionDenominacion, string Glosa, string Importe)
        {
            var rm = new Comun.ResponseModel();

            try
            {
                //bóveda abierta
                Boveda boveda = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();

                //--------datos de la boveda actual-------//
                int BovedaId = boveda.Id;
                decimal BovedaSaldoInicial = boveda.SaldoInicial;
                decimal BovedaEntradas = (boveda.Entradas).Value;
                decimal BovedaSalidas = (boveda.Salidas).Value;
                decimal BovedaSaldoFinal = 0;

                if (OperacionDenominacion == "TRANSFERENCIA CUENTA BANCARIA" || OperacionDenominacion == "EGRESO AJUSTE BOVEDA") // OPERACION TRANSFERENCIA BANCARIA O EGRESO AJUSTE BÓVEDA
                {
                    BovedaSalidas = BovedaSalidas + Convert.ToDecimal(Importe);
                    BovedaSaldoFinal = BovedaSaldoInicial + BovedaEntradas - BovedaSalidas;

                    // ACTUALIZAMOS LA BÓVEDA
                    Boveda actual = new Boveda();
                    actual.Id = BovedaId;
                    actual.Salidas = BovedaSalidas;
                    actual.SaldoFinal = BovedaSaldoFinal;
                    BovedaBL.ActualizarParcial(actual, x => x.Salidas, x => x.SaldoFinal);
                }

                if (OperacionDenominacion == "INGRESO AJUSTE BOVEDA") // OPERACIÓN INGRESO AJUSTE BÓVEDA
                {
                    BovedaEntradas = BovedaEntradas + Convert.ToDecimal(Importe);
                    BovedaSaldoFinal = BovedaSaldoInicial + BovedaEntradas - BovedaSalidas;

                    // ACTUALIZAMOS LA BÓVEDA
                    Boveda actual = new Boveda();
                    actual.Id = BovedaId;
                    actual.Entradas = BovedaEntradas;
                    actual.SaldoFinal = BovedaSaldoFinal;
                    BovedaBL.ActualizarParcial(actual, x => x.Entradas, x => x.SaldoFinal);
                }

                //Realizamos la transferencia a caja: BovedaMovimiento
                BovedaMovimiento movimiento = new BovedaMovimiento();
                movimiento.BovedaId = BovedaId;
                movimiento.OperacionId = OperacionId;
                movimiento.Fecha = DateTime.Now;
                movimiento.Glosa = Glosa;
                movimiento.Importe = Convert.ToDecimal(Importe);
                BovedaMovimientoBL.Crear(movimiento);

                //Recuperamos los datos del movimiento de bóveda que enviaremos a nuestra vista
                var datos = new string[8];
                datos[0] = Convert.ToString(movimiento.Id);
                datos[1] = Convert.ToString(movimiento.Fecha);
                datos[2] = OperacionDenominacion;
                datos[3] = movimiento.Glosa;
                datos[4] = Convert.ToString(movimiento.Importe);

                datos[5] = Convert.ToString(BovedaEntradas);
                datos[6] = Convert.ToString(BovedaSalidas);
                datos[7] = Convert.ToString(BovedaSaldoFinal);

                rm.SetResponse(true);
                rm.result = datos;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);

        }



    }
}

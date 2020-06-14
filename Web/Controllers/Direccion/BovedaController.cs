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
    [PermisoAttribute(Permiso = RolesMenu.menu_boveda_todo)]
    public class BovedaController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Boveda> Bovedas;
        private Paginador<Boveda> ListadoBovedas;
        private List<BovedaMovimiento> BovedasMovimiento;
        private Paginador<BovedaMovimientoVm> ListadoBovedasMovimiento;
        private List<CajaDiario> CajasDiario;
        private Paginador<CajaDiarioVm> ListadoCajasDiario;
        public ActionResult Index()
        {

            using (db = new DAEntities())
            {
                ViewBag.ListadoBovedas = BovedaBL.Listar();
                ViewBag.ListadoBovedasMovimiento = BovedaMovimientoBL.Listar(includeProperties: "CajaDiario, Operacion");
                ViewBag.ListadoCajasDiario = CajaDiarioBL.Listar(includeProperties: "Usuario, Caja");

                //Objeto Boveda Activa

                List<Boveda> bovedaActual = db.Boveda.Where(x => x.IndCierre == false).ToList();

                ViewBag.BovedaActual = bovedaActual;

                Boveda actual = (from b in db.Boveda where b.IndCierre.Equals(false) select b).SingleOrDefault();
                ViewBag.Cards = actual;

                //Lista de Operaciones 
                var Operaciones = (from o in db.Operacion where o.IndTipo.Equals(true) select new { Id = o.Id, Operacion = o.Denominacion }).ToList();
                SelectList listaOperaciones = new SelectList(Operaciones, "Id","Operacion");
                ViewBag.Operaciones = listaOperaciones;

                //Transfer Operation for 'Transfer Modal'
                var egreso_ajuste_boveda = (from ot in db.Operacion where ot.Denominacion.Equals("TRANSFERENCIA EGRESO BOVEDA") select ot).SingleOrDefault();
                ViewBag.OperacionId = egreso_ajuste_boveda.Id;
                ViewBag.OperacionDenominacion = egreso_ajuste_boveda.Denominacion;

                //Lista de CajasDisponibles
                var CajasDisponibles = (from cd in db.CajaDiario
                                        join c in db.Caja
                                        on cd.CajaId equals c.Id
                                        where cd.IndCierre == false && cd.IndBoveda == false
                                        select new { Id = cd.Id, Caja = c.Denominacion }).ToList();

                SelectList listaCajas = new SelectList(CajasDisponibles, "Id", "Caja");
                ViewBag.CajasDisponibles = listaCajas;

            }
            return View();
        }

        //PAGINATION 'Boveda'
        [HttpPost]
        public ActionResult TablaBovedas(int pagina)
        {
            var rm = new Comun.ResponseModel();
            int TotalRegistros = 0;
            try
            {
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

                    //We list "Boveda Movimiento" only with the required fields to avoid serialization problems
                    var SubBovedas = Bovedas.Select(S => new Boveda
                    {
                        Id = S.Id,
                        SaldoInicial = S.SaldoInicial,
                        Entradas = S.Entradas,
                        Salidas = S.Salidas,
                        SaldoFinal = S.SaldoFinal,
                        FechaInicio = S.FechaInicio,
                        FechaFin = S.FechaFin,
                        IndCierre = S.IndCierre

                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoBovedas = new Paginador<Boveda>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubBovedas
                    };
                }
                rm.SetResponse(true);
                rm.result = ListadoBovedas;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        // PAGINATION 'Boveda Movimiento'
        [HttpPost]
        public ActionResult TablaMovimientos(int pagina)
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


                    //We list "Boveda Movimiento" only with the required fields to avoid serialization problems
                    var SubMovimientos = BovedasMovimiento.Select(S => new BovedaMovimientoVm
                    {
                        Id = S.Id,
                        CajaDiarioId = S.CajaDiarioId,
                        Fecha = S.Fecha,
                        OperacionDenominacion = S.Operacion.Denominacion,
                        Glosa = S.Glosa,
                        Importe = S.Importe

                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoBovedasMovimiento = new Paginador<BovedaMovimientoVm>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubMovimientos
                    };
                    
                }

                rm.SetResponse(true);
                rm.result = ListadoBovedasMovimiento;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);

        }

        // PAGINATION 'Cajas Diario'

        public ActionResult TablaCajaDiario(int pagina)
        {
            var rm = new Comun.ResponseModel();
            int TotalRegistros = 0;
            try
            {
                using (db = new DAEntities())
                {
                    // Total number of records in the caja diario table
                    TotalRegistros = db.CajaDiario.Count();
                    // We get the 'records page' from the caja diario table
                    CajasDiario = db.CajaDiario.OrderByDescending(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x => x.Usuario)
                                                     .Include(x => x.Caja)
                                                     .ToList();
                    // Total number of pages in the caja diario table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list "Boveda Movimiento" only with the required fields to avoid serialization problems
                    var SubCajasDiario = CajasDiario.Select(S => new CajaDiarioVm
                    {
                        Id = S.Id,
                        CajaDenominacion = S.Caja.Denominacion,
                        UsuarioNombre = S.Usuario.Nombre,
                        SaldoInicial = S.SaldoInicial,
                        SaldoFinal = S.SaldoFinal,
                        FechaInicio = S.FechaInicio,
                        FechaFin = S.FechaFin,
                        IndCierre = S.IndCierre,
                        IndBoveda = S.IndBoveda

                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoCajasDiario = new Paginador<CajaDiarioVm>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubCajasDiario
                    };
                    
                }

                rm.SetResponse(true);
                rm.result = ListadoCajasDiario;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);

        }

        //Método de Apertura de la Bóveda
        [HttpPost]
        public ActionResult Apertura(decimal ? SaldoInicial)
        {
            var rm = new Comun.ResponseModel();
            decimal SaldoFinal = SaldoInicial.Value;
            decimal Entradas = 0;
            decimal Salidas = 0;
            try
            {
                if (SaldoInicial == null || SaldoInicial < 0 || SaldoInicial == 0)
                {
                    rm.message = "EL SALDO INICIAL NO DEBE SER NULO, NEGATIVO NI CERO";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    //Aperturamos la bóveda con lo ingresado por el administrador
                    Boveda boveda = new Boveda();
                    boveda.SaldoInicial = Convert.ToDecimal(SaldoInicial);
                    boveda.SaldoFinal = SaldoFinal;
                    boveda.Entradas = Entradas;
                    boveda.Salidas = Salidas;
                    boveda.FechaInicio = DateTime.Now;
                    BovedaBL.Crear(boveda);

                    rm.message = "APERTURA EXITOSA";
                    rm.SetResponse(true);

                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);

        }

        //Método Cierre Bóveda
        [HttpPatch]
        public ActionResult Cierre(int ? BovedaId)
        {
            var rm = new Comun.ResponseModel();
            var IndCierre = true;
            decimal Entradas = 0;
            decimal Salidas = 0;
            try
            {

                if (BovedaId == null)
                {
                    rm.message = "EL ID DE LA BÓVEDA NO DEBE SER NULA";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    var existe_boveda = (from bb in db.Boveda where bb.Id == BovedaId.Value select bb).Any();

                    if (existe_boveda == false)
                    {
                        rm.message = "LA BÓVEDA A CERRAR NO EXISTE";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        var boveda_actual = (from ba in db.Boveda where ba.Id == BovedaId.Value select ba).SingleOrDefault();

                        if (boveda_actual.IndCierre == true)
                        {
                            rm.message = "ESTA BÓVEDA YA ESTÁ CERRADA";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            // We closing current vault
                            Boveda boveda = new Boveda();
                            boveda.Id = BovedaId.Value;
                            boveda.FechaFin = DateTime.Now;
                            boveda.IndCierre = IndCierre;
                            BovedaBL.ActualizarParcial(boveda, x => x.FechaFin, x => x.IndCierre);

                            // Open new vault with data from the previous vault
                            Boveda actual = new Boveda();
                            actual.SaldoInicial = (boveda_actual.SaldoFinal).Value;
                            actual.SaldoFinal = (boveda_actual.SaldoFinal).Value;
                            actual.Entradas = Entradas;
                            actual.Salidas = Salidas;
                            actual.FechaInicio = DateTime.Now;
                            BovedaBL.Crear(actual);

                            rm.message = "CIERRE DE BÓVEDA EXITOSO";
                            rm.SetResponse(true, rm.message);
                            rm.result = actual;

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);
        }


        //Método Transferencia de Bóveda
        [HttpPost]
        public ActionResult Transferencia(int ? CajaDiarioId, string CajaDenominacion, string Glosa, decimal ? Importe)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (CajaDiarioId == null || CajaDenominacion == "SELECCIONE CAJA")
                {
                    rm.message = "DEBE SELECCIONAR UNA CAJA";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    if (Importe == null || Importe == 0 || Importe < 0)
                    {
                        rm.message = "EL IMPORTE NO DEBE SER CERO, NULO NI NEGATIVO";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (Glosa == "")
                        {
                            rm.message = "COMPLETE EL CAMPO GLOSA";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            Boveda boveda = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();
                            CajaDiario cajaDiario = (from cd in db.CajaDiario where cd.Id == CajaDiarioId select cd).SingleOrDefault();
                            Operacion operacion = (from o in db.Operacion where o.Denominacion.Equals("TRANSFERENCIA EGRESO BOVEDA") select o).SingleOrDefault();

                            if (Importe > boveda.SaldoFinal)
                            {
                                rm.message = "EL IMPORTE DEBE SER MENOR QUE EL TOTAL EN BOVEDA";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
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
                                movimiento.OperacionId = operacion.Id;
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

                                //Agregamos el importe al saldo inicial de la caja diario
                                CajaDiario actualCaja = new CajaDiario();
                                actualCaja.Id = CajaDiarioId.Value;
                                actualCaja.SaldoInicial = CajaSaldoInicial;
                                actualCaja.SaldoFinal = SaldoFinalCaja;
                                CajaDiarioBL.ActualizarParcial(actualCaja, x => x.SaldoInicial, x => x.SaldoFinal);

                                //Recuperamos los datos del movimiento que enviaremos a nuestra vista

                                var datos = new string[3];
                                datos[0] = Convert.ToString(BovedaEntradas);
                                datos[1] = Convert.ToString(SalidasBoveda);
                                datos[2] = Convert.ToString(SaldoFinalBoveda);

                                rm.message = "TRANSFERENCIA EXITOSA";
                                rm.SetResponse(true, rm.message);
                                rm.result = datos;

                            }

                        }

                    }

                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);

        }

        //Método Entradas/Salidas de Bóveda
        [HttpPost]
        public ActionResult EntradasSalidas(int ? OperacionId, string OperacionDenominacion, string Glosa, decimal ? Importe)
        {
            var rm = new Comun.ResponseModel();

            try
            {

                if (OperacionId == null || OperacionDenominacion == "SELECCIONE OPERACION")
                {
                    rm.message = "DEBE SELECCIONAR UNA OPERACIÓN";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    //CURRENT VAULT
                    var boveda_actual = (from sb in db.Boveda where sb.IndCierre == false select sb).SingleOrDefault();
                    int BovedaId = boveda_actual.Id;
                    decimal BovedaSaldoInicial = boveda_actual.SaldoInicial;
                    decimal BovedaEntradas = (boveda_actual.Entradas).Value;
                    decimal BovedaSalidas = (boveda_actual.Salidas).Value;
                    decimal BovedaSaldoFinal = 0;

                    //-------Operations related to modal Inputs Outputs
                    var transferencia_cuenta_bancaria = (from tcc in db.Operacion where tcc.Denominacion.Equals("TRANSFERENCIA CUENTA BANCARIA") select tcc.Denominacion).SingleOrDefault();
                    var ingreso_ajuste_boveda = (from iab in db.Operacion where iab.Denominacion.Equals("INGRESO AJUSTE BOVEDA") select iab.Denominacion).SingleOrDefault();
                    var egreso_ajuste_boveda = (from eab in db.Operacion where eab.Denominacion.Equals("EGRESO AJUSTE BOVEDA") select eab.Denominacion).SingleOrDefault();

                    if (!(OperacionDenominacion == transferencia_cuenta_bancaria || OperacionDenominacion == ingreso_ajuste_boveda || OperacionDenominacion == egreso_ajuste_boveda))
                    {
                        rm.message = "OPERACION INVÁLIDA";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (Importe == null || Importe <= 0)
                        {
                            rm.message = "EL IMPORTE NO DEBE SER NULO, NEGATIVO NI CERO";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (Importe > boveda_actual.SaldoFinal)
                            {
                                rm.message = "EL IMPORTE DEBE SER MENOR AL TOTAL EN BOVEDA";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                if (Glosa == "")
                                {
                                    rm.message = "COMPLETE EL CAMPO GLOSA";
                                    rm.SetResponse(false, rm.message);
                                }
                                else
                                {

                                    if (OperacionDenominacion == transferencia_cuenta_bancaria || OperacionDenominacion == egreso_ajuste_boveda)
                                    {
                                        BovedaSalidas = BovedaSalidas + Importe.Value;
                                        BovedaSaldoFinal = BovedaSaldoInicial + BovedaEntradas - BovedaSalidas;

                                        // ACTUALIZAMOS LA BÓVEDA
                                        Boveda actual = new Boveda();
                                        actual.Id = BovedaId;
                                        actual.Salidas = BovedaSalidas;
                                        actual.SaldoFinal = BovedaSaldoFinal;
                                        BovedaBL.ActualizarParcial(actual, x => x.Salidas, x => x.SaldoFinal);
                                    }

                                    else
                                    {
                                        BovedaEntradas = BovedaEntradas + Importe.Value;
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
                                    movimiento.OperacionId = OperacionId.Value;
                                    movimiento.Fecha = DateTime.Now;
                                    movimiento.Glosa = Glosa;
                                    movimiento.Importe = Convert.ToDecimal(Importe);
                                    BovedaMovimientoBL.Crear(movimiento);

                                    //Recuperamos los datos del movimiento de bóveda que enviaremos a nuestra vista
                                    var datos = new string[3];
                                    datos[0] = Convert.ToString(BovedaEntradas);
                                    datos[1] = Convert.ToString(BovedaSalidas);
                                    datos[2] = Convert.ToString(BovedaSaldoFinal);

                                    rm.message = "OPERACIÓN REALIZADA CON ÉXITO";
                                    rm.SetResponse(true, rm.message);
                                    rm.result = datos;

                                }

                            }

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);

        }



    }
}

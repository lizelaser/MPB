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
    public class CajaDiarioController : Controller
    {
        private DAEntities db = new DAEntities();
        private CajaDiarioVm cd = new CajaDiarioVm();
        private CajaDiario caja = new CajaDiario();
        /// <summary>
        /// El action index es el método por el cuál el usuario puede visualizar los listados
        /// de saldos de la institución ya sea que estén asignados o no
        /// </summary>
        /// <param name="denominacion">Denominación de la caja</param>
        /// <param name="pagina">Número de p+agina</param>
        /// <returns>Retorna el listado de saldos de caja asignados y totales</returns>
        /// 
        private readonly int RegistrosPorPagina = 5;
        private List<CajaDiario> CajasAsignadas;
        private Paginador<CajaDiarioVm> ListadoAsignadas;
        private List<CajaDiario> CajasDiario;
        private Paginador<CajaDiarioVm> ListadoCajasDiario;

        // GET: CAJA DIARIO
        public ActionResult Index()
        {
            using (db = new DAEntities())
            {
                //Lista de USUARIOS DISPONIBLES

                var UsuariosDisponibles = (from p in db.Personal join
                                           u in db.Usuario
                                           on p.Id equals u.PersonalId
                                           where u.IndUso.Equals(false)
                                           select new{ Id = u.Id, Usuario = p.Paterno + " " + p.Materno + ", " + p.Nombres }).ToList();

                SelectList usuarios = new SelectList(UsuariosDisponibles, "Id", "Usuario");
                ViewBag.UsuariosDisponibles = usuarios;

                //Lista de CAJAS DISPONIBLES

                var CajasDisponibles = (from c in db.Caja
                                           where c.IndUso.Equals(false)
                                           select new { Id = c.Id, Caja = c.Denominacion }).ToList();

                SelectList listaCajas = new SelectList(CajasDisponibles, "Id", "Caja");
                ViewBag.CajasDisponibles = listaCajas;
                // MONTO TOTAL EN BÓVEDA
                var montoTotal = (from b in db.Boveda where b.IndCierre == false select b.SaldoFinal).SingleOrDefault();
                ViewBag.MontoTotal = montoTotal;
            }

            return View();
        }

        //FILTRO, PAGINACIÓN Y LISTADO CAJAS ASIGNADAS
        [HttpPost]
        public ActionResult TablaAsignadas(int pagina)
        {
            var rm = new Comun.ResponseModel();
            int TotalRegistros = 0;

            using (db = new DAEntities())
            {
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    // Total number of records in the caja diario table
                    TotalRegistros = db.CajaDiario.Where(x=>x.IndBoveda.Equals(false) || x.IndCierre.Equals(false)).Count();
                    // We get the 'records page' from the caja diario table
                    CajasAsignadas = db.CajaDiario.Where(x => x.IndBoveda == false || x.IndCierre == false).OrderBy(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x => x.Caja)
                                                     .Include(x => x.Usuario)
                                                     .ToList();
                    // Total number of pages in the caja diario table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list 'assigned boxes' only with the required fields to avoid serialization problems
                    var SubAsignadas = CajasAsignadas.Select(S => new CajaDiarioVm
                    {
                        Id = S.Id,
                        CajaDenominacion = S.Caja.Denominacion,
                        UsuarioNombre = S.Usuario.Nombre,
                        FechaInicio = S.FechaInicio,
                        FechaFin = S.FechaFin,
                        SaldoInicial = S.SaldoInicial,
                        Entradas = S.Entradas,
                        Salidas = S.Salidas,
                        SaldoFinal = S.SaldoFinal,
                        IndCierre = S.IndCierre,
                        IndBoveda = S.IndBoveda

                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoAsignadas = new Paginador<CajaDiarioVm>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubAsignadas
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoAsignadas;

                }
                catch (Exception ex)
                {
                    rm.SetResponse(false, ex.Message);
                }

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        //FILTRO, PAGINACIÓN Y LISTADO CAJAS DIARIO
        [HttpPost]
        public ActionResult TablaCajasDiario(int pagina)
        {
            var rm = new Comun.ResponseModel();
            int TotalRegistros = 0;

            using (db = new DAEntities())
            {
                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    // Total number of records in the boveda table
                    TotalRegistros = db.CajaDiario.Count();

                    // We get the 'records page' from the boveda table
                    CajasDiario = db.CajaDiario.OrderByDescending(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .Include(x => x.Caja)
                                                     .Include(x => x.Usuario)
                                                     .ToList();
                    // Total number of pages in the boveda table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list 'assigned boxes' only with the required fields to avoid serialization problems
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
                    rm.SetResponse(true);
                    rm.result = ListadoCajasDiario;

                }
                catch (Exception ex)
                {
                    rm.SetResponse(false, ex.Message);
                }

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AsignarCaja(int CajaId, string CajaDenominacion, int UsuarioId, decimal SaldoInicial)
        {
            var rm = new Comun.ResponseModel();
            decimal entradas = 0;
            decimal salidas = 0;
            decimal saldoFinal = SaldoInicial + entradas - salidas;

            try
            {
                if (CajaId != 0 && UsuarioId != 0) // Condición para verificar haya seleccionado al usuario y su caja correspondiente
                {
                    // -- Datos de la bóveda actual -- //
                    Boveda boveda = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();
                    int BovedaId = boveda.Id;
                    decimal BovedaSaldoInicial = boveda.SaldoInicial;
                    decimal BovedaEntradas = (boveda.Entradas).Value;
                    decimal SalidasBoveda = (boveda.Salidas).Value;
                    SalidasBoveda = SalidasBoveda + Convert.ToDecimal(SaldoInicial);
                    decimal SaldoFinalBoveda = BovedaSaldoInicial + BovedaEntradas - SalidasBoveda;

                    //Asignamos Caja
                    CajaDiario cajadiario = new CajaDiario();
                    cajadiario.CajaId = CajaId;
                    cajadiario.UsuarioId = UsuarioId;
                    cajadiario.SaldoInicial = SaldoInicial;
                    cajadiario.Entradas = entradas;
                    cajadiario.Salidas = salidas;
                    cajadiario.SaldoFinal = saldoFinal;
                    cajadiario.FechaInicio = DateTime.Now;
                    cajadiario.IndCierre = false;
                    cajadiario.IndBoveda = false;

                    // Guardamos la caja diario finalmente
                    CajaDiarioBL.Crear(cajadiario);

                    //Recuperamos el Id de la caja diario
                    int CajaDiarioId = cajadiario.Id;

                    //Asignamos la operación correspondiente con asignaciones de caja
                    int OperacionId = (from o in db.Operacion where o.Denominacion == "TRANSFERENCIA EGRESO BOVEDA" select o.Id).SingleOrDefault();

                    //Guardamos la asignación de caja en bóveda movimiento siempre y cuando saldo inicial sea diferente de 0.00 soles
                    if (SaldoInicial!=0)
                    {
                        BovedaMovimiento movimiento = new BovedaMovimiento();
                        movimiento.BovedaId = BovedaId;
                        movimiento.CajaDiarioId = CajaDiarioId;
                        movimiento.OperacionId = OperacionId;
                        movimiento.Fecha = DateTime.Now;
                        movimiento.Glosa = "INGRESO A " + CajaDenominacion;
                        movimiento.Importe = Convert.ToDecimal(SaldoInicial);
                        BovedaMovimientoBL.Crear(movimiento);
                    }

                    //Actualizamos los IndUso de la caja y usuario correspondientes
                    Caja caja = new Caja();
                    caja.Id = CajaId;
                    caja.IndUso = true;
                    CajaBL.ActualizarParcial(caja,x=>x.IndUso);

                    Usuario usuario = new Usuario();
                    usuario.Id = UsuarioId;
                    usuario.IndUso = true;
                    UsuarioBL.ActualizarParcial(usuario,x=>x.IndUso);

                    //Actualizamos el Saldo Final de la Bóveda actual
                    Boveda actual = new Boveda();
                    actual.Id = BovedaId;
                    actual.Salidas = SalidasBoveda;
                    actual.SaldoFinal = SaldoFinalBoveda;
                    BovedaBL.ActualizarParcial(actual, x => x.Salidas, x => x.SaldoFinal);

                    //Recover data that send from asignación de caja to view
                    Usuario Usuario = UsuarioBL.Obtener(UsuarioId);
                    var MontoTotal = (from b in db.Boveda where b.IndCierre.Equals(false) select b.SaldoFinal).SingleOrDefault();

                    var datos = new string[9];
                    datos[0] = Convert.ToString(cajadiario.CajaId);
                    datos[1] = Convert.ToString(cajadiario.UsuarioId);
                    datos[2] = Convert.ToString(cajadiario.Id);
                    datos[3] = CajaDenominacion;
                    datos[4] = Usuario.Nombre;
                    datos[5] = Convert.ToString(cajadiario.FechaInicio);
                    datos[6] = Convert.ToString(cajadiario.SaldoInicial);
                    datos[7] = Convert.ToString(cajadiario.SaldoFinal);
                    datos[8] = Convert.ToString(MontoTotal.Value);

                    rm.SetResponse(true);
                    rm.result = datos;
                    
                }

                else
                {
                    rm.SetResponse(false);
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        [HttpPatch]
        public ActionResult CerrarCajas(decimal SaldoFinal, List<BovedaMovimiento> CajasACerrar)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                //Asignamos el IndBoveda a true al cerrar las cajas
                var CajasAsignadas = (from cd in db.CajaDiario where cd.IndBoveda == false && cd.IndCierre == true select cd).ToList();

                foreach(var item in CajasAsignadas)
                {
                    item.IndBoveda = true;
                }
                db.SaveChanges();

                // -- Datos de la bóveda actual -- //
                Boveda boveda = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();
                int BovedaId = boveda.Id;
                decimal BovedaSaldoInicial = boveda.SaldoInicial;
                decimal BovedaSalidas = (boveda.Salidas).Value;
                decimal EntradasBoveda = (boveda.Entradas).Value;
                EntradasBoveda = EntradasBoveda + Convert.ToDecimal(SaldoFinal);
                decimal SaldoFinalBoveda = BovedaSaldoInicial + EntradasBoveda - BovedaSalidas;

                // -- Operacion Ingreso Ajuste Boveda para el cierre de cajas --//
                Operacion operacionIngresoAjusteBoveda = (from o in db.Operacion where o.Denominacion == "TRANSFERENCIA INGRESO BOVEDA" select o).SingleOrDefault();

                //Actualizamos el Saldo Final de la Bóveda actual
                Boveda actual = new Boveda();
                actual.Id = BovedaId;
                actual.Entradas = EntradasBoveda;
                actual.SaldoFinal = SaldoFinalBoveda;
                BovedaBL.ActualizarParcial(actual, x => x.Entradas, x => x.SaldoFinal);

                //Creamos Movimientos de Bóveda correspondientes con el cierre de cada una de las cajas
                BovedaMovimiento movimiento = new BovedaMovimiento();
                foreach (var item in CajasACerrar)
                {
                    movimiento.BovedaId = BovedaId;
                    movimiento.CajaDiarioId = item.CajaDiarioId;
                    movimiento.OperacionId = operacionIngresoAjusteBoveda.Id;
                    movimiento.Fecha = DateTime.Now;
                    movimiento.Glosa = item.Glosa;
                    movimiento.Importe = item.Importe;
                    BovedaMovimientoBL.Crear(movimiento);

                }


                //Actualizamos el IndUso de los usuarios y las cajas
                var usuariosEnUso = (from u in db.Usuario where u.IndUso.Equals(true) select u).ToList();
                foreach (var item in usuariosEnUso)
                {
                    item.IndUso = false;
                }
                db.SaveChanges();

                var cajasEnUso = (from c in db.Caja where c.IndUso.Equals(true) select c).ToList();
                foreach (var item in cajasEnUso)
                {
                    item.IndUso = false;
                }
                db.SaveChanges();

                //Llamamos a las cajas asignadas para retornarlas a la vista
                //var Asignadas = (from ca in db.CajaDiario where ca.IndCierre.Equals(false) || ca.IndBoveda.Equals(false) select ca).ToList();
                //rm.result = Asignadas;
                rm.SetResponse(true);
 
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ModuloCierre()
        {
            var rm = new Comun.ResponseModel();
            try
            {
                List<CajaDiario> Asignadas = (from ca in db.CajaDiario where ca.IndCierre.Equals(false) || ca.IndBoveda.Equals(false) select ca).ToList();
                var SubAsignadas = Asignadas.Select(S => new CajaDiarioVm
                {
                    Id = S.Id,
                    CajaDenominacion = S.Caja.Denominacion,
                    UsuarioNombre = S.Usuario.Nombre,
                    FechaInicio = S.FechaInicio,
                    FechaFin = S.FechaFin,
                    SaldoInicial = S.SaldoInicial,
                    Entradas = S.Entradas,
                    Salidas = S.Salidas,
                    SaldoFinal = S.SaldoFinal,
                    IndCierre = S.IndCierre,
                    IndBoveda = S.IndBoveda
                }).ToList();

                rm.result = SubAsignadas;
                rm.SetResponse(true);
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);
        }

    }
}


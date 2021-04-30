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
    [PermisoAttribute(Permiso = RolesMenu.menu_cajadiario_todo)]
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
                                           where u.IndUso.Equals(false) && u.Rol.Denominacion.Equals("Secretaria")
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
                ViewBag.MontoTotal = (from b in db.Boveda where !b.IndCierre select b.SaldoFinal).SingleOrDefault();
            }

            return View();
        }

        //FILTRO, PAGINACIÓN Y LISTADO CAJAS ASIGNADAS
        [HttpPost]
        public ActionResult TablaAsignadas(int pagina=1)
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
        public ActionResult TablaCajasDiario(int pagina=1)
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
        public ActionResult AsignarCaja(int ? CajaId, string CajaDenominacion, int ? UsuarioId, string UsuarioNombres, decimal ? SaldoInicial)
        {
            var rm = new Comun.ResponseModel();

            var boveda = (from b in db.Boveda where !b.IndCierre select b).SingleOrDefault();

            try
            {
                if (boveda == null)
                {
                    rm.SetResponse(false, "LA BÓVEDA NO HA SIDO APERTURADA");
                    return Json(rm, JsonRequestBehavior.AllowGet);
                }

                // -- Datos de la bóveda actual -- //
                int BovedaId = boveda.Id;
                decimal BovedaSaldoInicial = boveda.SaldoInicial;
                decimal BovedaEntradas = (boveda.Entradas).Value;
                decimal SalidasBoveda = (boveda.Salidas).Value;
                SalidasBoveda = SalidasBoveda + Convert.ToDecimal(SaldoInicial);
                decimal SaldoFinalBoveda = BovedaSaldoInicial + BovedaEntradas - SalidasBoveda;

                if (CajaId == null || CajaDenominacion == "SELECCIONE CAJA")
                {
                    rm.message = "SELECCIONE UNA CAJA";
                    rm.SetResponse(false, rm.message);
                }
                else if (UsuarioId == null || UsuarioNombres == "SELECCIONE USUARIO")
                {
                    rm.message = "SELECCIONE UN USUARIO";
                    rm.SetResponse(false, rm.message);
                }
                else if (SaldoInicial == null || SaldoInicial < 0)
                {
                    rm.message = "EL SALDO INICIAL NO DEBE SER NULO NI NEGATIVO";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    var caja_seleccionada = (from c in db.Caja where c.Id == CajaId.Value select c).SingleOrDefault();
                    var usuario_seleccionado = (from u in db.Usuario where u.Id == UsuarioId.Value select u).SingleOrDefault();


                    if (SaldoInicial > SaldoFinalBoveda)
                    {
                        rm.message = "EL SALDO INICIAL DEBE SER MENOR AL TOTAL EN BÓVEDA";
                        rm.SetResponse(false, rm.message);
                    }
                    else{

                        if (usuario_seleccionado.IndUso == true || caja_seleccionada.IndUso == true)
                        {
                            rm.message = "LA CAJA O USUARIO YA ESTÁN ASIGNADOS";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            //Asignamos Caja
                            CajaDiario cajadiario = new CajaDiario();
                            cajadiario.CajaId = CajaId.Value;
                            cajadiario.UsuarioId = UsuarioId.Value;
                            cajadiario.SaldoInicial = SaldoInicial.Value;
                            cajadiario.Entradas = 0;
                            cajadiario.Salidas = 0;
                            cajadiario.SaldoFinal = SaldoInicial.Value;
                            cajadiario.FechaInicio = DateTime.Now;
                            cajadiario.IndCierre = false;
                            cajadiario.IndBoveda = false;

                            // Guardamos la caja diario finalmente
                            CajaDiarioBL.Crear(cajadiario);

                            //Recuperamos el Id de la caja diario
                            int CajaDiarioId = cajadiario.Id;

                            //Asignamos la operación correspondiente con asignaciones de caja
                            int OperacionId = (from o in db.Operacion where o.Denominacion == "TRANSFERENCIA EGRESO BOVEDA" select o.Id).SingleOrDefault();

                            //Guardamos la asignación de caja en bóveda movimiento siempre y cuando saldo inicial sea diferente de 0.00 soles, sino sólo registramos la caja diario
                            if (SaldoInicial != 0)
                            {
                                BovedaMovimiento movimiento = new BovedaMovimiento();
                                movimiento.BovedaId = BovedaId;
                                movimiento.CajaDiarioId = CajaDiarioId;
                                movimiento.OperacionId = OperacionId;
                                movimiento.Fecha = DateTime.Now;
                                movimiento.Glosa = "APERTURA DE " + CajaDenominacion;
                                movimiento.Importe = Convert.ToDecimal(SaldoInicial);
                                BovedaMovimientoBL.Crear(movimiento);
                            }

                            //Actualizamos los IndUso de la caja y usuario correspondientes
                            Caja caja = new Caja();
                            caja.Id = CajaId.Value;
                            caja.IndUso = true;
                            CajaBL.ActualizarParcial(caja, x => x.IndUso);

                            Usuario usuario = new Usuario();
                            usuario.Id = UsuarioId.Value;
                            usuario.IndUso = true;
                            UsuarioBL.ActualizarParcial(usuario, x => x.IndUso);

                            //Actualizamos las Salidas y el Saldo Final de la Bóveda actual
                            Boveda actual = new Boveda();
                            actual.Id = BovedaId;
                            actual.Salidas = SalidasBoveda;
                            actual.SaldoFinal = SaldoFinalBoveda;
                            BovedaBL.ActualizarParcial(actual, x => x.Salidas, x => x.SaldoFinal);

                            //Recover data that send from asignación de caja to view
                            Usuario Usuario = UsuarioBL.Obtener(UsuarioId.Value);
                            var MontoTotal = (from b in db.Boveda where b.IndCierre.Equals(false) select b.SaldoFinal).SingleOrDefault();

                            var datos = new string[9];
                            datos[0] = Convert.ToString(cajadiario.CajaId);
                            datos[1] = Convert.ToString(cajadiario.UsuarioId);
                            datos[2] = Convert.ToString(MontoTotal.Value);

                            rm.message = "ASIGNACIÓN REALIZADA CON ÉXITO";
                            rm.SetResponse(true, rm.message);
                            rm.result = datos;

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message,true);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        [HttpPatch]
        public ActionResult CerrarCajas(decimal SaldoFinal, List<BovedaMovimiento> CajasACerrar)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                var bovedaActual = (from b in db.Boveda where b.IndCierre == false select b).SingleOrDefault();

                if (bovedaActual == null)
                {
                    rm.SetResponse(false, "LA BÓVEDA NO HA SIDO APERTURADA");
                }

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

                // -- Operacion Transferencia Ingreso Boveda para el cierre de cajas --//
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
                rm.SetResponse(true,"CAJAS CERRADAS CORRECTAMENTE");
 
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message,true);
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
            return Json(rm);
        }

        [HttpPost]
        public JsonResult ObtenerMovimiento(int nro)
        {
            var movimiento = CajaMovimientoBL.Buscar(nro);


            List<CajaMovimientoVm> datos = new List<CajaMovimientoVm>();

            foreach (var item in movimiento)
            {

                if (item.PersonalId!=null && item.AlumnoId==null)
                {
                    datos = movimiento.Select(S => new CajaMovimientoVm
                    {
                        Id = S.Id,
                        CajaDiarioId = S.CajaDiarioId,
                        AlumnoId = S.AlumnoId,
                        PersonalId = S.PersonalId,
                        PersonaNombres = S.Personal.Paterno + " " + S.Personal.Materno + " " + S.Personal.Nombres,
                        OperacionId = S.OperacionId,
                        OperacionDenominacion = S.Operacion.Denominacion,
                        EstadoId = S.EstadoId,
                        EstadoDenominacion = S.Estado.Denominacion,
                        Fecha = S.Fecha,
                        Total = S.Total,
                        Descripcion = S.Descripcion,
                        IndEntrada = S.IndEntrada

                    }).ToList();
                }
                else
                {
                    datos = movimiento.Select(S => new CajaMovimientoVm
                    {
                        Id = S.Id,
                        CajaDiarioId = S.CajaDiarioId,
                        AlumnoId = S.AlumnoId,
                        PersonalId = S.PersonalId,
                        PersonaNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                        OperacionId = S.OperacionId,
                        OperacionDenominacion = S.Operacion.Denominacion,
                        EstadoId = S.EstadoId,
                        EstadoDenominacion = S.Estado.Denominacion,
                        Fecha = S.Fecha,
                        Total = S.Total,
                        Descripcion = S.Descripcion,
                        IndEntrada = S.IndEntrada

                    }).ToList();
                }

            }

            return Json(datos);
        }

        [HttpPatch]
        public ActionResult AnularMovimiento(int ? Id)
        {
            var rm = new Comun.ResponseModel();

            try
            {
                //State canceled
                int estado_anulado = (from e in db.Estado where e.Denominacion.Equals("ANULADO") select e.Id).SingleOrDefault();
                
                if (Id == null)
                {
                    rm.message = "EL NRO DE MOVIMIENTO NO DEBE SER NULO";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    //Movement Box to cancel
                    var movimiento_caja = (from mc in db.CajaMovimiento where mc.Id == Id.Value select mc).SingleOrDefault();

                    if (movimiento_caja==null)
                    {
                        rm.SetResponse(false, "NO EXISTE MOVIMIENTO DE CAJA ASOCIADO AL NÚMERO DE MOVIMIENTO INGRESADO");
                    }
                    else
                    {
                        if (movimiento_caja.EstadoId == estado_anulado)
                        {
                            rm.message = "ESTE MOVIMIENTO YA FUE ANULADO";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            int cajadiario_id = movimiento_caja.CajaDiarioId;

                            //Daily cash associated with movement to cancel
                            var caja_diario = (from cd in db.CajaDiario where cd.Id == cajadiario_id select cd).SingleOrDefault();
                            var saldo_inicial = caja_diario.SaldoInicial;
                            var entradas = caja_diario.Entradas;
                            var salidas = caja_diario.Salidas;
                            var saldo_final = caja_diario.SaldoFinal;

                            if (caja_diario.IndCierre == true)
                            {
                                rm.message = "LA CAJA ASOCIADA AL MOVIMIENTO ESTÁ CERRADA";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                if (movimiento_caja.AlumnoId != null) // Movement associated to student
                                {

                                    //Recover 'pendiente' state for associated account receivable
                                    var estado_pendiente = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e).SingleOrDefault();

                                    //Recover 'cuenta por cobrar' associated to movement
                                    var cuenta_por_cobrar = (from cc in db.CuentasPorCobrar where cc.CajaMovimientoId == Id.Value select cc).SingleOrDefault();

                                    //Update account receivable associated to movement
                                    CuentasPorCobrar cobranza = new CuentasPorCobrar();
                                    cobranza.Id = cuenta_por_cobrar.Id;
                                    cobranza.EstadoId = estado_pendiente.Id;
                                    cobranza.CajaMovimientoId = null;
                                    CuentasPorCobrarBL.ActualizarParcial(cobranza, x => x.EstadoId, x => x.CajaMovimientoId);

                                    //Update movement of box
                                    CajaMovimiento movimiento = new CajaMovimiento();
                                    movimiento.Id = Id.Value;
                                    movimiento.EstadoId = estado_anulado;
                                    CajaMovimientoBL.ActualizarParcial(movimiento, x => x.EstadoId);

                                    //Update daily cash associated to movement to cancel according if is an output or entrie
                                    CajaDiario diario = new CajaDiario();

                                    //Recover fields of movement that which are necessary for update daily cash
                                    decimal importe = (movimiento_caja.Total).Value;

                                    if (movimiento_caja.IndEntrada == true)
                                    {
                                        diario.Id = cajadiario_id;
                                        diario.Entradas = entradas - Convert.ToDecimal(importe);
                                        diario.SaldoFinal = (saldo_final - Convert.ToDecimal(importe));
                                        CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);

                                    }
                                    else
                                    {
                                        diario.Id = cajadiario_id;
                                        diario.Salidas = salidas - Convert.ToDecimal(importe);
                                        diario.SaldoFinal = (saldo_final + Convert.ToDecimal(importe));
                                        CajaDiarioBL.ActualizarParcial(diario, x => x.Salidas, x => x.SaldoFinal);
                                    }

                                }
                                else // Movement associated to personal
                                {
                                    //Update movement of box
                                    CajaMovimiento movimiento = new CajaMovimiento();
                                    movimiento.Id = Id.Value;
                                    movimiento.EstadoId = estado_anulado;
                                    CajaMovimientoBL.ActualizarParcial(movimiento, x => x.EstadoId);

                                    //Update daily cash associated to movement to cancel according if is an output or entrie
                                    CajaDiario diario = new CajaDiario();

                                    //Recover fields of movement that which are necessary for cancel operation
                                    decimal importe = (movimiento_caja.Total).Value;

                                    if (movimiento_caja.IndEntrada == true)
                                    {
                                        diario.Id = cajadiario_id;
                                        diario.Entradas = entradas - Convert.ToDecimal(importe);
                                        diario.SaldoFinal = (saldo_final - Convert.ToDecimal(importe));
                                        CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);

                                    }
                                    else
                                    {
                                        diario.Id = cajadiario_id;
                                        diario.Salidas = salidas - Convert.ToDecimal(importe);
                                        diario.SaldoFinal = (saldo_final + Convert.ToDecimal(importe));
                                        CajaDiarioBL.ActualizarParcial(diario, x => x.Salidas, x => x.SaldoFinal);
                                    }

                                }

                                rm.message = "OPERACIÓN REALIZADA CON ÉXITO";
                                rm.SetResponse(true, rm.message);
                            }
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message,true);
            }

            return Json(rm);
        }

    }
}


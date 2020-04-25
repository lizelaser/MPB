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
        private List<CajaDiario> CajasDiario;
        private Paginador<CajaDiario> ListadoCajasDiario;
        // GET: Alumno
        public ActionResult Index(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the caja diario table
                TotalRegistros = db.CajaDiario.Count();
                // We get the 'records page' from the caja diario table
                CajasDiario = db.CajaDiario.Where(x=>x.IndBoveda==false || x.IndCierre == false ).OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Caja)
                                                 .Include(x=>x.Usuario)
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
                ViewBag.ListadoCajasAsignadas = ListadoCajasDiario;
            }
            using (db = new DAEntities())
            {
                // Total number of records in the boveda table
                TotalRegistros = db.CajaDiario.Count();
                // We get the 'records page' from the boveda table
                CajasDiario = db.CajaDiario.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Caja)
                                                 .Include(x => x.Usuario)
                                                 .ToList();
                // Total number of pages in the boveda table
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

            using (db = new DAEntities())
            {
                //Lista de USUARIOS DISPONIBLES

                var UsuariosDisponibles = (from p in db.Personal join
                                           u in db.Usuario
                                           on p.Id equals u.PersonalId
                                           join cd in db.CajaDiario
                                           on u.Id equals cd.UsuarioId into ucd
                                           from cd in ucd.DefaultIfEmpty()
                                           where (cd.UsuarioId).Equals(null) && (u.Activo).Equals(true)
                                           select new{ Id = u.Id, Usuario = p.Paterno + " " + p.Materno + ", " + p.Nombres }).ToList();

                SelectList usuarios = new SelectList(UsuariosDisponibles, "Id", "Usuario");
                ViewBag.UsuariosDisponibles = usuarios;

                //Lista de CAJAS DISPONIBLES

                var CajasDisponibles = (from c in db.Caja
                                           join cd in db.CajaDiario
                                           on c.Id equals cd.CajaId into ccd
                                           from cd in ccd.DefaultIfEmpty()
                                           where cd.CajaId.Equals(null)
                                           select new { Id = c.Id, Caja = c.Denominacion }).ToList();

                SelectList listaCajas = new SelectList(CajasDisponibles, "Id", "Caja");
                ViewBag.CajasDisponibles = listaCajas;
                // MONTO TOTAL EN BÓVEDA
                var montoTotal = (from b in db.Boveda where b.IndCierre == false select b.SaldoFinal).SingleOrDefault();
                ViewBag.MontoTotal = montoTotal;
            }

            return View();
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
                    int OperacionId = (from o in db.Operacion where o.Denominacion == "EGRESO AJUSTE BOVEDA" select o.Id).SingleOrDefault();
                    //Guardamos la asignación de caja en bóveda movimiento
                    BovedaMovimiento movimiento = new BovedaMovimiento();
                    movimiento.BovedaId = BovedaId;
                    movimiento.CajaDiarioId = CajaDiarioId;
                    movimiento.OperacionId = OperacionId;
                    movimiento.Fecha = DateTime.Now;
                    movimiento.Glosa = "INGRESO A " + CajaDenominacion;
                    movimiento.Importe = Convert.ToDecimal(SaldoInicial);
                    BovedaMovimientoBL.Crear(movimiento);

                    //Actualizamos el Saldo Final de la Bóveda actual
                    Boveda actual = new Boveda();
                    actual.Id = BovedaId;
                    actual.Salidas = SalidasBoveda;
                    actual.SaldoFinal = SaldoFinalBoveda;
                    BovedaBL.ActualizarParcial(actual, x => x.Salidas, x => x.SaldoFinal);

                    rm.SetResponse(true);
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
        public ActionResult CerrarCajas(decimal SaldoFinal)
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

                //Actualizamos el Saldo Final de la Bóveda actual
                Boveda actual = new Boveda();
                actual.Id = BovedaId;
                actual.Entradas = EntradasBoveda;
                actual.SaldoFinal = SaldoFinalBoveda;
                BovedaBL.ActualizarParcial(actual, x => x.Entradas, x => x.SaldoFinal);

                rm.SetResponse(true);
 
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

    }
}


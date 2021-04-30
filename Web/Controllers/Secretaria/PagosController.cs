using BL;
using DA;
using Rotativa;
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
    [PermisoAttribute(Permiso = RolesMenu.menu_pagos_todo)]
    public class PagosController : Controller
    {
        private DAEntities db = new DAEntities();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pagina"></param>
        /// <returns></returns>
        private readonly int RegistrosPorPagina = 5;
        private List<CajaMovimiento> Pagos;
        private Paginador<CajaMovimientoVm> ListadoPagos;
        private List<CajaMovimiento> Entradas;
        private Paginador<CajaMovimientoVm> ListadoEntradas;
        private List<CajaMovimiento> Salidas;
        private Paginador<CajaMovimientoVm> ListadoSalidas;

        private List<CuentasPorCobrar> Cobranzas;
        private Paginador<CuentasPorCobrarVm> ListadoCobranzas;
        // GET: Pago
        public ActionResult Index()
        {
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            
            // CAJA DIARIO ASIGNADA AL USUARIO EN SESIÓN
            using (db = new DAEntities())
            {
                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId == UsuarioActualId && ca.IndCierre==false select ca).ToList();
                decimal SaldoInicialCaja = 0;
                decimal EntradasCaja = 0;
                decimal SalidasCaja = 0;
                decimal SaldoNetoCaja = 0;
                decimal SaldoFinalCaja = 0;
                string CajaDenominacion = "";

                foreach (var item in CajaAsignada)
                {
                    SaldoInicialCaja = item.SaldoInicial;
                    EntradasCaja = (item.Entradas).Value;
                    SalidasCaja = (item.Salidas).Value;
                    SaldoFinalCaja = (item.SaldoFinal).Value;
                    SaldoNetoCaja = (SaldoInicialCaja + EntradasCaja - SalidasCaja) - SaldoInicialCaja;
                    CajaDenominacion = item.Caja.Denominacion;
                }
                ViewBag.CajaAsignada = CajaAsignada;
                ViewBag.SaldoInicialCaja = SaldoInicialCaja;
                ViewBag.EntradasCaja = EntradasCaja;
                ViewBag.SalidasCaja = SalidasCaja;
                ViewBag.SaldoNetoCaja = SaldoNetoCaja;
                ViewBag.SaldoFinalCaja = SaldoFinalCaja;
                ViewBag.CajaDenominacion = CajaDenominacion;


                //LISTAMOS LAS OPERACIONES CORRESPONDIENTES A CAJA
                List<Operacion> operaciones = (from o in db.Operacion where o.IndTipo.Equals(false) select o).ToList();
                SelectList listaOperaciones = new SelectList(operaciones, "Id", "Denominacion");
                //Corresponding Operation with tab 'Entradas/Salidas'
                ViewBag.Operaciones = listaOperaciones;

                //'SALIDAS OTROS' OPERATION FOR MODAL 'TRANSFERIR SALDOS'
                Operacion operacion_salidas_otros = (from o in db.Operacion where o.Denominacion.Equals("SALIDAS OTROS") select o).SingleOrDefault();
                ViewBag.OperacionId = operacion_salidas_otros.Id;
                ViewBag.OperacionDenominacion = operacion_salidas_otros.Denominacion;
            }

            return View();
        }

        //FILTRO, PAGINACIÓN Y LISTADO CUENTAS POR COBRAR
        public ActionResult TablaCobranzas(string nombres="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {

                int TotalRegistros = 0;

                var EstadoPendiente = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e).SingleOrDefault();

                var cuentas = db.CuentasPorCobrar
                    .Where(x => x.EstadoId.Equals(EstadoPendiente.Id))
                    .Include(x => x.Alumno)
                    .Include(x => x.Estado)
                    .OrderBy(x => x.Id);
                // Total number of records in the Cuentas Por Cobrar table with pending status
                TotalRegistros = cuentas.Count();
                
                //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
                var SubCobranzas = cuentas.Select(S => new CuentasPorCobrarVm
                {
                    Id = S.Id,
                    MatriculaId = S.MatriculaId,
                    AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                    Fecha = S.Fecha,
                    Total = S.Total,
                    EstadoDenominacion = S.Estado.Denominacion,
                    Descripcion = S.Descripcion

                });

                if (!string.IsNullOrEmpty(nombres))
                {
                    SubCobranzas = SubCobranzas.Where(x => x.AlumnoNombres.ToLower().Contains(nombres.ToLower()));
                    TotalRegistros = SubCobranzas.Count();
                }

                SubCobranzas = SubCobranzas.Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina);

                // Total number of pages in the Cuentas por Cobrar table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                // We instantiate the 'Paging class' and assign the new values
                ListadoCobranzas = new Paginador<CuentasPorCobrarVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubCobranzas.ToList()
                };

                rm.SetResponse(true);
                rm.result = ListadoCobranzas;
            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        //FILTRO, PAGINACIÓN Y LISTADO PAGOS
        public ActionResult TablaPagos(string nombres="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();
            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                // We calling 'anulado' to omit it on our pagination
                int estado_anulado = (from ea in db.Estado where ea.Denominacion.Equals("ANULADO") select ea.Id).SingleOrDefault();

                int TotalRegistros = 0;

                // Total number of records in the caja movimiento table
                TotalRegistros = db.CajaMovimiento.Where(x=>x.EstadoId!=estado_anulado).Count();


                // We get the 'records page' from the caja movimiento table
                Pagos = db.CajaMovimiento.Where(x=>x.EstadoId!=estado_anulado).OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Alumno)
                                                 .Include(x=>x.Personal)
                                                 .Include(x=>x.Operacion)
                                                 .Include(X=>X.Estado)
                                                 .ToList();
                
                //We list 'caja movimientos' only with the required fields to avoid serialization problems
                var SubPagos = new List<CajaMovimientoVm>();

                for (var i=0; i<Pagos.Count; i++)
                {
                    //Add a item in SubPagos when foreign key PersonalId is not null and AlumnoId is null
                    //to avoid navigation problems

                    if (Pagos[i].PersonalId!=null && Pagos[i].AlumnoId==null)
                    {
                        SubPagos.Add(new CajaMovimientoVm()
                        {
                            Id = Pagos[i].Id,
                            CajaDiarioId = Pagos[i].CajaDiarioId,
                            AlumnoId = Pagos[i].AlumnoId,
                            PersonalId = Pagos[i].PersonalId,
                            PersonaNombres = Pagos[i].Personal.Paterno + " " + Pagos[i].Personal.Materno + " " + Pagos[i].Personal.Nombres,
                            OperacionId = Pagos[i].OperacionId,
                            OperacionDenominacion = Pagos[i].Operacion.Denominacion,
                            EstadoId = Pagos[i].EstadoId,
                            EstadoDenominacion = Pagos[i].Estado.Denominacion,
                            Fecha = Pagos[i].Fecha,
                            Total = Pagos[i].Total,
                            Descripcion = Pagos[i].Descripcion,
                            IndComprobante = Pagos[i].IndComprobante,
                            TipoComprobante = Pagos[i].TipoComprobante
                        });
                    }
                    //Otherwise when foreign key AlumnoId is not null and PersonalId is null
                    //to avoid navigation problems too
                    else
                    {
                        SubPagos.Add(new CajaMovimientoVm()
                        {
                            Id = Pagos[i].Id,
                            CajaDiarioId = Pagos[i].CajaDiarioId,
                            AlumnoId = Pagos[i].AlumnoId,
                            PersonaNombres = Pagos[i].Alumno.Paterno + " " + Pagos[i].Alumno.Materno + " " + Pagos[i].Alumno.Nombres,
                            PersonalId = Pagos[i].PersonalId,
                            OperacionId = Pagos[i].OperacionId,
                            OperacionDenominacion = Pagos[i].Operacion.Denominacion,
                            EstadoId = Pagos[i].EstadoId,
                            EstadoDenominacion = Pagos[i].Estado.Denominacion,
                            Fecha = Pagos[i].Fecha,
                            Total = Pagos[i].Total,
                            Descripcion = Pagos[i].Descripcion,
                            IndComprobante = Pagos[i].IndComprobante,
                            TipoComprobante = Pagos[i].TipoComprobante
                        });
                    }
                }

                if (!string.IsNullOrEmpty(nombres))
                {
                    var filtrado = SubPagos.Where(x => x.PersonaNombres.ToLower().Contains(nombres.ToLower())).OrderBy(x => x.Id);
                    SubPagos = filtrado.Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = filtrado.Count();
                }

                // Total number of pages in the caja movimiento table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                // We instantiate the 'Paging class' and assign the new values
                ListadoPagos = new Paginador<CajaMovimientoVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubPagos
                };

                rm.SetResponse(true);
                rm.result = ListadoPagos;

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }


        //FILTRO, PAGINACIÓN Y LISTADO ENTRADAS
        public ActionResult TablaEntradas(int pagina=1)
        {
            var rm = new Comun.ResponseModel();
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);


            using (db = new DAEntities())
            {
                // CAJA DIARIO ASIGNADA AL USUARIO EN SESIÓN

                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;


                    // We calling 'anulado' to omit it on our pagination
                    int estado_anulado = (from ea in db.Estado where ea.Denominacion.Equals("ANULADO") select ea.Id).SingleOrDefault();

                    if (CajaAsignada!=null) // Validate if the user has a box assigned to avoid problems if they do not have them
                    {
                        int TotalRegistros = 0;

                        // Total number of records in the caja movimiento table
                        TotalRegistros = db.CajaMovimiento.Where(x=>x.IndEntrada.Equals(true) && x.CajaDiarioId.Equals(CajaAsignada.Id) && x.EstadoId!=estado_anulado).Count();
                        // We get the 'records page' from the caja movimiento table
                        Entradas = db.CajaMovimiento.Where(x=>x.IndEntrada.Equals(true) && x.CajaDiarioId.Equals(CajaAsignada.Id) && x.EstadoId != estado_anulado).OrderByDescending(x => x.Id)
                                                         .Skip((pagina - 1) * RegistrosPorPagina)
                                                         .Take(RegistrosPorPagina)
                                                         .Include(x => x.Alumno)
                                                         .Include(x => x.Personal)
                                                         .Include(x => x.Operacion)
                                                         .Include(X => X.Estado)
                                                         .ToList();

                        // Total number of pages in the caja movimiento table
                        var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                        //We list 'caja movimientos' only with the required fields to avoid serialization problems
                        var SubEntradas = new List<CajaMovimientoVm>();

                        for (var i = 0; i < Entradas.Count; i++)
                        {
                            //Add a item in SubPagos when foreign key PersonalId is not null and AlumnoId is null
                            //to avoid navigation problems

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
                                    EstadoId = Entradas[i].EstadoId,
                                    EstadoDenominacion = Entradas[i].Estado.Denominacion,
                                    Fecha = Entradas[i].Fecha,
                                    Total = Entradas[i].Total,
                                    Descripcion = Entradas[i].Descripcion
                                });
                            }
                        }

                        // We instantiate the 'Paging class' and assign the new values
                        ListadoEntradas = new Paginador<CajaMovimientoVm>()
                        {
                            RegistrosPorPagina = RegistrosPorPagina,
                            TotalRegistros = TotalRegistros,
                            TotalPaginas = TotalPaginas,
                            PaginaActual = pagina,
                            Listado = SubEntradas
                        };

                        rm.SetResponse(true);
                        rm.result = ListadoEntradas;

                    }

                }
                catch (Exception ex)
                {
                    rm.SetResponse(false,ex.Message, true);
                }

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }



        //FILTRO, PAGINACIÓN Y LISTADO SALIDAS
        public ActionResult TablaSalidas(int pagina=1)
        {
            var rm = new Comun.ResponseModel();
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            using (db = new DAEntities())
            {
                // CAJA DIARIO ASIGNADA AL USUARIO EN SESIÓN
                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    // We calling 'anulado' to omit it on our pagination
                    int estado_anulado = (from ea in db.Estado where ea.Denominacion.Equals("ANULADO") select ea.Id).SingleOrDefault();

                    if (CajaAsignada != null) // Validate if the user has a box assigned to avoid problems if they do not have them
                    {
                        int TotalRegistros = 0;

                        // Total number of records in the caja movimiento table
                        TotalRegistros = db.CajaMovimiento.Where(x => x.IndEntrada.Equals(false) && x.CajaDiarioId.Equals(CajaAsignada.Id) && x.EstadoId!=estado_anulado).Count();
                        // We get the 'records page' from the caja movimiento table
                        Salidas = db.CajaMovimiento.Where(x => x.IndEntrada.Equals(false) && x.CajaDiarioId.Equals(CajaAsignada.Id) && x.EstadoId!=estado_anulado).OrderByDescending(x => x.Id)
                                                         .Skip((pagina - 1) * RegistrosPorPagina)
                                                         .Take(RegistrosPorPagina)
                                                         .Include(x => x.Alumno)
                                                         .Include(x => x.Personal)
                                                         .Include(x => x.Operacion)
                                                         .Include(X => X.Estado)
                                                         .ToList();

                        // Total number of pages in the caja movimiento table
                        var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                        //We list 'caja movimientos' only with the required fields to avoid serialization problems
                        var SubSalidas = new List<CajaMovimientoVm>();

                        for (var i = 0; i < Salidas.Count; i++)
                        {
                            //Add a item in SubPagos when foreign key PersonalId is not null and AlumnoId is null
                            //to avoid navigation problems

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
                                    EstadoId = Salidas[i].EstadoId,
                                    EstadoDenominacion = Salidas[i].Estado.Denominacion,
                                    Fecha = Salidas[i].Fecha,
                                    Total = Salidas[i].Total,
                                    Descripcion = Salidas[i].Descripcion
                                });
                            }
                        }

                        // We instantiate the 'Paging class' and assign the new values
                        ListadoSalidas = new Paginador<CajaMovimientoVm>()
                        {
                            RegistrosPorPagina = RegistrosPorPagina,
                            TotalRegistros = TotalRegistros,
                            TotalPaginas = TotalPaginas,
                            PaginaActual = pagina,
                            Listado = SubSalidas
                        };

                        rm.SetResponse(true);
                        rm.result = ListadoSalidas;

                    }
                }
                catch (Exception ex)
                {
                    rm.SetResponse(false, ex.Message);
                }

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }


        public ActionResult BuscarPersonal(string nombres)
        {
            return Json(PersonalBL.Buscar(nombres));
        }

        public ActionResult BuscarAlumno(string nombres)
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

        [HttpPost]
        public ActionResult ModalCobro(int CuentaPorCobrarId)
        {
            db = new DAEntities();
            var cobranza = (from cc in db.CuentasPorCobrar
                            where cc.Id.Equals(CuentaPorCobrarId)
                            select new CuentasPorCobrarVm
                            {
                                Id = cc.Id,
                                MatriculaId = cc.MatriculaId,
                                AlumnoId = cc.AlumnoId,
                                AlumnoNombres = cc.Alumno.Paterno + " " + cc.Alumno.Materno + " " + cc.Alumno.Nombres,
                                Total = cc.Total,
                                Descripcion = cc.Descripcion
                            }).SingleOrDefault();


            return Json(cobranza);
        }

        [HttpPost]
        public ActionResult GuardarCobro(int ? CuentaPorCobrarId, string TipoComprobante)
        {
            var rm = new Comun.ResponseModel();
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            using (db = new DAEntities())
            {
                // CAJA DIARIO ASIGNADA AL USUARIO EN SESIÓN
                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                //Recover initial balance, entries, outputs, final balance
                var SaldoInicial = CajaAsignada?.SaldoInicial??0;
                var Entradas = CajaAsignada?.Entradas??0;
                var Salidas = CajaAsignada?.Salidas??0;
                var SaldoFinal = CajaAsignada?.SaldoInicial??0;
                

                //Instanciamos un nuevo objeto caja diario para su posterior actualización
                CajaDiario diario = new CajaDiario();

                // WE RECOVER THE OPERATION 'Entradas Otros' FOR ASSIGN TO THE MOVEMENT
                var Operacion = (from o in db.Operacion where o.Denominacion.Equals("ENTRADAS OTROS") select o).SingleOrDefault();

                // WE RECOVER THE STATE 'Pagado' FOR ASSIGN TO THE MOVEMENT
                var estado_pagado_id = (from e in db.Estado where e.Denominacion.Equals("PAGADO") select e.Id).SingleOrDefault();

                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;

                    if (CajaAsignada != null)
                    {

                        if (CuentaPorCobrarId == null)
                        {
                            rm.message = "El NRO DE LA CUENTA POR COBRAR NO DEBE SER NULO";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (TipoComprobante == "NO DEFINIDO")
                            {
                                rm.message = "DEBE SELECCIONAR UN COMPROBANTE DE PAGO";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                // RECOVER FIELDS OF ACCOUNT RECEIVABLE
                                var cuenta_ingresante = (from ci in db.CuentasPorCobrar where ci.Id == CuentaPorCobrarId.Value select ci).SingleOrDefault();

                                // WE LISTED THE LIST OF ACCOUNT RECEIVABLE DETAILS RELATED WITH ITS ID
                                var detalles_cobranza = (from ccd in db.CuentasPorCobrarDetalle
                                                         where ccd.CuentasPorCobrarId == CuentaPorCobrarId.Value
                                                         select new
                                                         {
                                                             ConceptoPagoId = ccd.ConceptoPagoId,
                                                             ItemId = ccd.ItemId,
                                                             Cantidad = ccd.Cantidad,
                                                             Descuento = ccd.Descuento,
                                                             Importe = ccd.Importe
                                                         }).ToList();

                                if (cuenta_ingresante.EstadoId == estado_pagado_id)
                                {
                                    rm.message = "LA CUENTA POR COBRAR YA ESTÁ PAGADA";
                                    rm.SetResponse(false, rm.message);
                                }
                                else
                                {
                                    /*if (cuenta_ingresante.Fecha > cuenta_ingresante.FechaVencimiento)
                                    {
                                        rm.message = "PLAZO VENCIDO DE LA CUENTA POR COBRAR";
                                        rm.SetResponse(false, rm.message);
                                    }*/
                                    /*else
                                    {
                                        
                                    }*/
                                    var existen_series = (from tc in db.TipoComprobante where tc.CajaId == CajaAsignada.CajaId && tc.Descripcion.Equals(TipoComprobante) && tc.Estado == false select tc).Any();

                                    if (!existen_series)
                                    {
                                        var mensaje_fallo = "";

                                        if (TipoComprobante == "BL")
                                        {
                                            mensaje_fallo = "BOLETA";
                                        }
                                        else
                                        {
                                            mensaje_fallo = "FACTURA";
                                        }

                                        rm.message = "LA CAJA NO TIENE MAS SERIES PARA LA " + mensaje_fallo + ", SOLICITE A LA SUNAT";
                                        rm.SetResponse(false, rm.message);
                                    }
                                    else
                                    {

                                        if (TipoComprobante == "BL") //TIPO COMPROBANTE : BOLETA
                                        {
                                            // WE PASSED AND SAVE CHANGES 'Cuenta Por Cobrar' in 'CajaMovimiento' TABLE
                                            CajaMovimiento movimiento = new CajaMovimiento();
                                            movimiento.CajaDiarioId = CajaAsignada.Id;
                                            movimiento.AlumnoId = cuenta_ingresante.AlumnoId;
                                            movimiento.OperacionId = Operacion.Id;
                                            movimiento.EstadoId = estado_pagado_id;
                                            movimiento.Fecha = DateTime.Now;
                                            movimiento.Total = cuenta_ingresante.Total;
                                            movimiento.IndEntrada = true;
                                            movimiento.Descripcion = cuenta_ingresante.Descripcion;
                                            movimiento.IndComprobante = true;
                                            movimiento.TipoComprobante = 1;
                                            movimiento.ComprobanteDes = "BOLETA";
                                            CajaMovimientoBL.Crear(movimiento);

                                            // Recover id from movimiento object created
                                            var CajaMovimientoId = movimiento.Id;

                                            //Instanciamos un objeto de 'Caja Movimiento Detalle' para después guardarlo
                                            CajaMovimientoDetalle detalles = new CajaMovimientoDetalle();
                                            foreach (var item in detalles_cobranza)
                                            {
                                                detalles.CajaMovimientoId = CajaMovimientoId;
                                                detalles.ConceptoPagoId = item.ConceptoPagoId;
                                                detalles.ItemId = item.ItemId;
                                                detalles.Cantidad = item.Cantidad;
                                                detalles.Descuento = item.Descuento;
                                                detalles.Importe = item.Importe;

                                                CajaMovimientoDetalleBL.Crear(detalles);
                                            }

                                            // Update the assigned 'caja diario' for the user in session (Entries, Balance Final)
                                            diario.Id = CajaAsignada.Id;
                                            diario.Entradas = Entradas + cuenta_ingresante.Total;
                                            diario.SaldoFinal = SaldoInicial + (Entradas + cuenta_ingresante.Total) - Salidas;
                                            CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);

                                            //Update 'Cuentas Por Cobrar' pay assigned to student (estado pendiente => pagado) and (CajaMovimientoId)

                                            CuentasPorCobrar cobranza = new CuentasPorCobrar();
                                            cobranza.Id = CuentaPorCobrarId.Value;
                                            cobranza.EstadoId = estado_pagado_id;
                                            cobranza.CajaMovimientoId = CajaMovimientoId;
                                            CuentasPorCobrarBL.ActualizarParcial(cobranza, x => x.EstadoId, x => x.CajaMovimientoId);

                                            //Recover data that send from Egresos-Ingresos to view
                                            var datos = new string[5];
                                            datos[0] = Convert.ToString(SaldoInicial);
                                            datos[1] = Convert.ToString(diario.Entradas);
                                            datos[2] = Convert.ToString(diario.SaldoFinal);
                                            datos[3] = Convert.ToString(diario.Entradas - Salidas);
                                            datos[4] = Convert.ToString(cobranza.Id);

                                            rm.message = "PAGO REGISTRADO CON ÉXITO";
                                            rm.SetResponse(true, rm.message);
                                            rm.result = datos;
                                        }
                                        else // TIPO COMPROBANTE : FACTURA
                                        {
                                            var IGV = Convert.ToDecimal(1.18);

                                            //MULTIPLICAMOS EL TOTAL DE LA CUENTA POR COBRAR (VALOR DE VENTA) CON EL IGV

                                            var Total = cuenta_ingresante.Total * IGV;

                                            // WE PASSED AND SAVE CHANGES 'Cuenta Por Cobrar' in 'CajaMovimiento' TABLE
                                            CajaMovimiento movimiento = new CajaMovimiento();
                                            movimiento.CajaDiarioId = CajaAsignada.Id;
                                            movimiento.AlumnoId = cuenta_ingresante.AlumnoId;
                                            movimiento.OperacionId = Operacion.Id;
                                            movimiento.EstadoId = estado_pagado_id;
                                            movimiento.Fecha = DateTime.Now;
                                            movimiento.Total = Total;
                                            movimiento.IndEntrada = true;
                                            movimiento.Descripcion = cuenta_ingresante.Descripcion;
                                            movimiento.IndComprobante = true;
                                            movimiento.TipoComprobante = 2;
                                            movimiento.ComprobanteDes = "FACTURA";
                                            CajaMovimientoBL.Crear(movimiento);

                                            // Recover id from movimiento object created
                                            var CajaMovimientoId = movimiento.Id;

                                            //Instanciamos un objeto de 'Caja Movimiento Detalle' para después guardarlo
                                            CajaMovimientoDetalle detalles = new CajaMovimientoDetalle();
                                            foreach (var item in detalles_cobranza)
                                            {
                                                detalles.CajaMovimientoId = CajaMovimientoId;
                                                detalles.ConceptoPagoId = item.ConceptoPagoId;
                                                detalles.ItemId = item.ItemId;
                                                detalles.Cantidad = item.Cantidad;
                                                detalles.Descuento = item.Descuento;
                                                detalles.Importe = item.Importe;

                                                CajaMovimientoDetalleBL.Crear(detalles);
                                            }

                                            // Update the assigned 'caja diario' for the user in session (Entries, Balance Final)
                                            diario.Id = CajaAsignada.Id;
                                            diario.Entradas = Entradas + Total;
                                            diario.SaldoFinal = SaldoInicial + (Entradas + Total) - Salidas;
                                            CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);

                                            //Update 'Cuentas Por Cobrar' pay assigned to student (estado pendiente => pagado) and (CajaMovimientoId)

                                            CuentasPorCobrar cobranza = new CuentasPorCobrar();
                                            cobranza.Id = CuentaPorCobrarId.Value;
                                            cobranza.EstadoId = estado_pagado_id;
                                            cobranza.CajaMovimientoId = CajaMovimientoId;
                                            CuentasPorCobrarBL.ActualizarParcial(cobranza, x => x.EstadoId, x => x.CajaMovimientoId);

                                            //Recover data that send from Egresos-Ingresos to view
                                            var datos = new string[5];
                                            datos[0] = Convert.ToString(SaldoInicial);
                                            datos[1] = Convert.ToString(diario.Entradas);
                                            datos[2] = Convert.ToString(diario.SaldoFinal);
                                            datos[3] = Convert.ToString(diario.Entradas - Salidas);
                                            datos[4] = Convert.ToString(cobranza.Id);

                                            rm.message = "PAGO REGISTRADO CON ÉXITO";
                                            rm.SetResponse(true, rm.message);
                                            rm.result = datos;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    else
                    {
                        rm.message = "ERROR: CAJA NO ASIGNADA";
                        rm.SetResponse(false, rm.message);
                    }

                }
                catch (Exception ex)
                {
                    rm.SetResponse(false, ex.Message, true);
                }
            }

            return Json(rm);
        }

        [HttpPost]
        public ActionResult EgresosIngresos(string PersonalFiltro, int ? OperacionId, string OperacionDenominacion, int ? PersonalId, decimal ? Total, string Descripcion)
        {
            var rm = new Comun.ResponseModel();
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            try
            {
                //OBTENEMOS LA CAJA DEL USUARIO EN SESIÓN
                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                //Recover initial balance, entries, outputs, final balance
                var SaldoInicial = CajaAsignada!=null?(from ca in db.CajaDiario where ca.Id.Equals(CajaAsignada.Id) select ca.SaldoInicial).SingleOrDefault():0;
                var Entradas = CajaAsignada!=null?(from ca in db.CajaDiario where ca.Id.Equals(CajaAsignada.Id) select ca.Entradas).SingleOrDefault():0;
                var Salidas = CajaAsignada!=null?(from ca in db.CajaDiario where ca.Id.Equals(CajaAsignada.Id) select ca.Salidas).SingleOrDefault():0;
                var SaldoFinal = CajaAsignada!=null?(from ca in db.CajaDiario where ca.Id.Equals(CajaAsignada.Id) select ca.SaldoFinal).SingleOrDefault():0;
                decimal NetoCaja = 0;

                //Instanciamos un nuevo objeto caja diario para su posterior actualización
                CajaDiario diario = new CajaDiario();

                //OBTENEMOS EL ESTADO "PAGADO" PARA REALIZAR LA OPERACIÓN
                var EstadoId = (from e in db.Estado where e.Denominacion.Equals("PAGADO") select e.Id).SingleOrDefault();

                if (CajaAsignada!=null)
                {
                    if (OperacionId == null || OperacionDenominacion == "SELECCIONE OPERACION")
                    {
                        rm.message = "SELECCIONE UNA OPERACIÓN";
                        rm.SetResponse(false, rm.message);
                    }
                    else if (PersonalId == null || PersonalFiltro == "")
                    {
                        rm.message = "SELLECIONE UNA PERSONA";
                        rm.SetResponse(false, rm.message);
                    }
                    else if (Total == null || Total < 0)
                    {
                        rm.message = "EL CAMPO IMPORTE NO DEBE SER NULO NI NEGATIVO";
                        rm.SetResponse(false, rm.message);
                    }
                    else if (Descripcion == "")
                    {
                        rm.message = "COMPLETE EL CAMPO DESCRIPCIÓN";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        //REGISTRAMOS EL MOVIMIENTO DE CAJA
                        CajaMovimiento movimiento = new CajaMovimiento();

                        movimiento.CajaDiarioId = CajaAsignada.Id;
                        movimiento.PersonalId = PersonalId;
                        movimiento.OperacionId = OperacionId.Value;
                        movimiento.EstadoId = EstadoId;
                        movimiento.Fecha = DateTime.Now;
                        movimiento.Total = Total;
                        if (OperacionDenominacion.Equals("PAGO DE SERVICIOS") || OperacionDenominacion.Equals("SALIDAS OTROS") || OperacionDenominacion.Equals("FALTANTE DE CAJA"))
                        {
                            movimiento.IndEntrada = false;
                            movimiento.Descripcion = Descripcion;
                            movimiento.IndComprobante = false;

                            if (Total > CajaAsignada.SaldoFinal)
                            {
                                rm.message = "El IMPORTE DEBE SER MENOR AL SALDO TOTAL DE LA CAJA";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                CajaMovimientoBL.Crear(movimiento);

                                //ACTUALIZAMOS LA CAJA ASIGNADA AL USUARIO EN SESIÓN
                                diario.Id = CajaAsignada.Id;
                                diario.Salidas = Salidas + Total;
                                diario.SaldoFinal = SaldoInicial + Entradas - (Salidas + Total);
                                CajaDiarioBL.ActualizarParcial(diario, x => x.Salidas, x => x.SaldoFinal);
                                NetoCaja = (Entradas - diario.Salidas).Value;

                                rm.message = "OPERACIÓN REALIZADA CON ÉXITO";
                                rm.SetResponse(true, rm.message);
                            }
                        }
                        else // OperacionDenominacion.Equals("ENTRADAS OTROS")
                        {
                            movimiento.IndEntrada = true;
                            movimiento.Descripcion = Descripcion;
                            movimiento.IndComprobante = false;

                            CajaMovimientoBL.Crear(movimiento);

                            //ACTUALIZAMOS LA CAJA ASIGNADA AL USUARIO EN SESIÓN
                            diario.Id = CajaAsignada.Id;
                            diario.Entradas = Entradas + Total;
                            diario.SaldoFinal = SaldoInicial + (Entradas + Total) - Salidas;
                            CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);
                            NetoCaja = (diario.Entradas - Salidas).Value;

                            rm.message = "OPERACIÓN REALIZADA CON ÉXITO";
                            rm.SetResponse(true, rm.message);
                        }

                        //Recover data that send from Egresos-Ingresos to view
                        var datos = new string[6];
                        datos[0] = OperacionDenominacion;
                        datos[1] = Convert.ToString(SaldoInicial);
                        datos[2] = Convert.ToString(diario.Entradas);
                        datos[3] = Convert.ToString(diario.Salidas);
                        datos[4] = Convert.ToString(diario.SaldoFinal);
                        datos[5] = Convert.ToString(NetoCaja);

                        rm.result = datos;
                    }
                }
                else
                {
                    rm.SetResponse(false, "CAJA NO ASIGNADA PARA ESTE USUARIO");
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message,true);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult TransferirSaldos(int ? OperacionId, string OperacionDenominacion, decimal ? Importe, string Descripcion)
        {
            var rm = new Comun.ResponseModel();

            // WE RECOVER THE USER ID IN THE SESSION
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            // WE RECOVER THE FINAL BALANCE OF THE USER'S CASH IN SESSION
            CajaDiario caja_actual = (from cd in db.CajaDiario where cd.UsuarioId == UsuarioActualId && cd.IndCierre == false select cd).SingleOrDefault();

            // TRANSFERRING BALANCES ONLY USE 'SALIDAS OTROS' OPERATION SO
            var operacion_salidas_otros = (from o in db.Operacion where o.Denominacion.Equals("SALIDAS OTROS") select o).SingleOrDefault();

            try
            {
                if (caja_actual!=null && OperacionId != null && OperacionDenominacion!="" && Importe > 0 && Descripcion != "")
                {
                    if (OperacionId != operacion_salidas_otros.Id && OperacionDenominacion != operacion_salidas_otros.Denominacion)
                    {
                        rm.message = "OPERACIÓN INVÁLIDA";
                        rm.SetResponse(false, rm.message);
                    }
                    else if (Importe == null || Importe < 0)
                    {
                        rm.message = "EL CAMPO IMPORTE NO DEBE SER NULO NI NEGATIVO";
                        rm.SetResponse(false, rm.message);
                    }
                    else if (Descripcion == "")
                    {
                        rm.message = "COMPLETE EL CAMPO DESCRIPCIÓN";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (Importe > caja_actual.SaldoFinal)
                        {
                            rm.message = "EL IMPORTE DEBE SER MENOR AL SALDO TOTAL DE LA CAJA";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {

                            //WE GET THE BOX ASSIGNMENT FROM USER IN SESSION
                            var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                            //WE GET THE PERSONAL ASSOCIATED WITH THE USER IN SESSION
                            int PersonalId = (from p in db.Personal
                                              join u in db.Usuario on p.Id equals u.PersonalId
                                              where u.Id == UsuarioActualId
                                              select p.Id).SingleOrDefault();

                            //WE GET THE 'CREATED' STATE FOR BALANCES TRANSFER
                            int EstadoId = (from e in db.Estado where e.Denominacion.Equals("CREADO") select e.Id).SingleOrDefault();

                            // CREATE A NEW RECORD IN 'CAJA MOVIMIENTO' TABLE
                            CajaMovimiento movimiento_transf = new CajaMovimiento();
                            movimiento_transf.CajaDiarioId = CajaAsignada.Id;
                            movimiento_transf.OperacionId = OperacionId.Value;
                            movimiento_transf.EstadoId = EstadoId;
                            movimiento_transf.PersonalId = PersonalId;
                            movimiento_transf.Total = Importe;
                            movimiento_transf.Descripcion = Descripcion;
                            movimiento_transf.Fecha = DateTime.Now;
                            movimiento_transf.IndEntrada = false;
                            movimiento_transf.IndComprobante = false;

                            CajaMovimientoBL.Crear(movimiento_transf);

                            //WE RECOVER ID FROM THE CURRENT BOVEDA

                            int BovedaId = (from b in db.Boveda where b.IndCierre == false select b.Id).SingleOrDefault();

                            //RECUPERAMOS LA OPERACION 'Transferencia Egreso Boveda' PERTENECIENTE SOLO A BÓVEDA (IndTipo)
                            int movimiento_operacion_id = (from o in db.Operacion where o.Denominacion.Equals("TRANSFERENCIA INGRESO BOVEDA") && o.IndTipo.Equals(true) select o.Id).SingleOrDefault();

                            // CREATE A NEW RECORD IN 'BOVEDA MOVIMIENTO' TABLE

                            BovedaMovimiento boveda_mov = new BovedaMovimiento();
                            boveda_mov.BovedaId = BovedaId;
                            boveda_mov.CajaDiarioId = CajaAsignada.Id;
                            boveda_mov.OperacionId = movimiento_operacion_id;
                            boveda_mov.Fecha = DateTime.Now;
                            boveda_mov.Glosa = "TRANSFERENCIA DE " + CajaAsignada.Caja.Denominacion;
                            boveda_mov.Importe = Importe.Value;
                            BovedaMovimientoBL.Crear(boveda_mov);

                            // WE UPDATE BALANCES FROM CURRENT BOX ASSIGNED AND SAVE CHANGES
                            CajaDiario CajaActual = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();
                            decimal EntradasCaja = (CajaActual.Entradas).Value;
                            decimal SalidasCaja = (CajaActual.Salidas + Importe).Value;
                            decimal SaldoFinal = (CajaActual.SaldoInicial + CajaActual.Entradas - SalidasCaja).Value;

                            CajaDiario diario = new CajaDiario();
                            diario.Id = CajaActual.Id;
                            diario.Salidas = SalidasCaja;
                            diario.SaldoFinal = SaldoFinal;

                            CajaDiarioBL.ActualizarParcial(diario, x => x.Salidas, x => x.SaldoFinal);

                            //WE UPDATE BALANCES FROM CURRENT BOVEDA AND SAVE CHANGES

                            Boveda BovedaActual = (from ba in db.Boveda where ba.IndCierre.Equals(false) select ba).SingleOrDefault();
                            decimal SaldoInicialBoveda = (BovedaActual.SaldoInicial);
                            decimal EntradasBoveda = (BovedaActual.Entradas + Importe).Value;
                            decimal SalidasBoveda = (BovedaActual.Salidas).Value;
                            decimal SaldoFinalBoveda = SaldoInicialBoveda + EntradasBoveda - SalidasBoveda;

                            Boveda abierto = new Boveda();
                            abierto.Id = BovedaActual.Id;
                            abierto.Entradas = EntradasBoveda;
                            abierto.SaldoFinal = SaldoFinalBoveda;
                            BovedaBL.ActualizarParcial(abierto, x => x.Entradas, x => x.SaldoFinal);

                            //WE SEND UPDATE DATA ON SIGHT
                            var envio = new string[3];
                            envio[0] = Convert.ToString(diario.Salidas);
                            envio[1] = Convert.ToString(diario.SaldoFinal);
                            envio[2] = Convert.ToString(EntradasCaja - diario.Salidas);


                            rm.message = "TRANSFERENCIA REALIZADA CON ÉXITO";
                            rm.SetResponse(true, rm.message);
                            rm.result = envio;
                        }
                    }
                }
                else
                {
                    rm.SetResponse(false,"CAJA NO ASIGNADA PARA ESTE USUARIO");
                }
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message,true);
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        [HttpPatch]
        public ActionResult CerrarCaja()
        {
            var rm = new Comun.ResponseModel();
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            try
            {
                
                //OBTENEMOS LA CAJA DEL USUARIO EN SESIÓN
                var CajaAsignada = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca).SingleOrDefault();

                //UPDATE FINAL DATE, INDCIERRE
                CajaDiario diario = new CajaDiario();
                diario.Id = CajaAsignada.Id;
                diario.FechaFin = DateTime.Now;
                diario.IndCierre = true;
                CajaDiarioBL.ActualizarParcial(diario, x=>x.FechaFin, x=>x.IndCierre);

                //UPDATE THE USER USAGE INDICATOR RELATED TO THE ASSIGNED BOX CLOSING
                Usuario usuario = new Usuario();
                usuario.Id = UsuarioActualId;
                usuario.IndUso = false;
                UsuarioBL.ActualizarParcial(usuario, x=>x.IndUso);

                //UPDATE INDUSO FROM THE ASSIGMENT BOX
                Caja caja = new Caja();
                caja.Id = CajaAsignada.CajaId;
                caja.IndUso = false;
                CajaBL.ActualizarParcial(caja, x=>x.IndUso);

                rm.SetResponse(true);
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message,true);
            }
            return Json(rm);
        }


        public ActionResult Boleta(int id)
        {

            var cajamovimiento = (from m in db.CajaMovimiento
                             where m.Id == id
                             select new CajaMovimientoVm
                             {
                                 Id = m.Id,
                                 PersonaNombres = m.Alumno.Paterno + " " + m.Alumno.Materno + " " + m.Alumno.Nombres,
                                 Dni = m.Alumno.Dni,
                                 Direccion = m.Alumno.Direccion,
                                 Serie = m.Serie,
                                 Numero = m.Numero,
                                 Fecha = m.Fecha,
                                 Total = m.Total,
                                 Observacion = m.Descripcion
                             }).SingleOrDefault();

            if (cajamovimiento!=null)
            {
                ViewBag.Alumno = cajamovimiento.PersonaNombres;
                ViewBag.Dni = cajamovimiento.Dni;
                ViewBag.Direccion = cajamovimiento.Direccion;
                ViewBag.Fecha = cajamovimiento.Fecha;
                ViewBag.Serie = cajamovimiento.Serie;
                ViewBag.Numero = cajamovimiento.Numero;
                ViewBag.Total = cajamovimiento.Total;
                ViewBag.Observacion = cajamovimiento.Observacion;
            }

            var movimiento_detalles = db.CajaMovimientoDetalle.Where(x => x.CajaMovimientoId == id)
                                                        .OrderBy(x => x.Id)
                                                        .Include(x => x.CajaMovimiento)
                                                        .Include(x => x.ConceptoPago).ToList();


            //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
            var subdetalles = movimiento_detalles.Select(S => new CajaMovimientoDetalleVm
            {
                Id = S.Id,
                Cantidad = S.Cantidad,
                Descripcion = S.ConceptoPago.Concepto,
                ValorUnitario = (S.Importe + S.Descuento) / S.Cantidad,
                Descuento = S.Descuento,
                Importe = S.Importe

            }).ToList();


            return new ViewAsPdf("Boleta", subdetalles)
            {

                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10)

            };
        }

        public ActionResult Factura(int id)
        {

            var cajamovimiento = (from m in db.CajaMovimiento
                                  where m.Id == id
                                  select new CajaMovimientoVm
                                  {
                                      Id = m.Id,
                                      PersonaNombres = m.Alumno.Paterno + " " + m.Alumno.Materno + " " + m.Alumno.Nombres,
                                      Direccion = m.Alumno.Direccion,
                                      Serie = m.Serie,
                                      Numero = m.Numero,
                                      Fecha = m.Fecha,
                                      Total = m.Total,
                                      Observacion = m.Descripcion

                                  }).SingleOrDefault();

            if (cajamovimiento!=null)
            {
                ViewBag.Alumno = cajamovimiento.PersonaNombres;
                ViewBag.Fecha = cajamovimiento.Fecha;
                ViewBag.Serie = cajamovimiento.Serie;
                ViewBag.Numero = cajamovimiento.Numero;
                ViewBag.Total = cajamovimiento.Total;
                ViewBag.Observacion = cajamovimiento.Observacion;
            }

            var movimiento_detalles = db.CajaMovimientoDetalle.Where(x => x.CajaMovimientoId == id)
                                                        .OrderBy(x => x.Id)
                                                        .Include(x => x.CajaMovimiento)
                                                        .Include(x => x.ConceptoPago).ToList();


            //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
            var subdetalles = movimiento_detalles.Select(S => new CajaMovimientoDetalleVm
            {
                Id = S.Id,
                Cantidad = S.Cantidad,
                Descripcion = S.ConceptoPago.Concepto,
                ValorUnitario = (S.Importe + S.Descuento) / S.Cantidad,
                Descuento = S.Descuento,
                Importe = S.Importe

            }).ToList();


            return new ViewAsPdf("Factura", subdetalles)
            {

                PageSize = Rotativa.Options.Size.A4
                //,FileName = "CustomersLista.pdf" // SI QUEREMOS QUE EL ARCHIVO SE DESCARGUE DIRECTAMENTE
                ,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10)

            };
        }
    }
}
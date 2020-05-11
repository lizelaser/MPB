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
                ViewBag.Operaciones = listaOperaciones;
            }

            return View();
        }

        //FILTRO, PAGINACIÓN Y LISTADO CUENTAS POR COBRAR
        [HttpPost]
        public ActionResult TablaCobranzas(int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {

                int TotalRegistros = 0;

                var EstadoPendiente = (from e in db.Estado where e.Denominacion.Equals("PENDIENTE") select e).SingleOrDefault();

                // Total number of records in the Cuentas Por Cobrar table with pending status
                TotalRegistros = db.CuentasPorCobrar.Where(x=>x.EstadoId.Equals(EstadoPendiente.Id)).Count();
                // We get the 'records page' from the Cuentas Por Cobrar table
                Cobranzas = db.CuentasPorCobrar.Where(x=>x.EstadoId.Equals(EstadoPendiente.Id)).OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Alumno)
                                                 .Include(x => x.Estado)
                                                 .ToList();
                // Total number of pages in the Cuentas por Cobrar table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);


                //We list "Cuentas Por Cobrar" only with the required fields to avoid serialization problems
                var SubCobranzas = Cobranzas.Select(S => new CuentasPorCobrarVm
                {
                    Id = S.Id,
                    MatriculaId = S.MatriculaId,
                    AlumnoNombres = S.Alumno.Paterno + " " + S.Alumno.Materno + " " + S.Alumno.Nombres,
                    Fecha = S.Fecha,
                    Total = S.Total,
                    EstadoDenominacion = S.Estado.Denominacion,
                    Descripcion = S.Descripcion

                }).ToList();


                // We instantiate the 'Paging class' and assign the new values
                ListadoCobranzas = new Paginador<CuentasPorCobrarVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubCobranzas
                };

                rm.SetResponse(true);
                rm.result = ListadoCobranzas;
            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        //FILTRO, PAGINACIÓN Y LISTADO PAGOS
        [HttpPost]
        public ActionResult TablaPagos(int pagina)
        {
            var rm = new Comun.ResponseModel();
            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the caja movimiento table
                TotalRegistros = db.CajaMovimiento.Count();
                // We get the 'records page' from the caja movimiento table
                Pagos = db.CajaMovimiento.OrderByDescending(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Alumno)
                                                 .Include(x=>x.Personal)
                                                 .Include(x=>x.Operacion)
                                                 .Include(X=>X.Estado)
                                                 .ToList();

                // Total number of pages in the caja movimiento table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                
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
                            Descripcion = Pagos[i].Descripcion
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
                            Descripcion = Pagos[i].Descripcion
                        });
                    }
                }

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
        [HttpPost]
        public ActionResult TablaEntradas(int pagina)
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

                    if (CajaAsignada!=null) // Validate if the user has a box assigned to avoid problems if they do not have them
                    {
                        int TotalRegistros = 0;

                        // Total number of records in the caja movimiento table
                        TotalRegistros = db.CajaMovimiento.Where(x=>x.IndEntrada.Equals(true) && x.CajaDiarioId.Equals(CajaAsignada.Id)).Count();
                        // We get the 'records page' from the caja movimiento table
                        Entradas = db.CajaMovimiento.Where(x=>x.IndEntrada.Equals(true) && x.CajaDiarioId.Equals(CajaAsignada.Id)).OrderByDescending(x => x.Id)
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
                    rm.SetResponse(false,ex.Message);
                }

            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }



        //FILTRO, PAGINACIÓN Y LISTADO SALIDAS
        [HttpPost]
        public ActionResult TablaSalidas(int pagina)
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

                    if (CajaAsignada != null) // Validate if the user has a box assigned to avoid problems if they do not have them
                    {
                        int TotalRegistros = 0;

                        // Total number of records in the caja movimiento table
                        TotalRegistros = db.CajaMovimiento.Where(x => x.IndEntrada.Equals(false) && x.CajaDiarioId.Equals(CajaAsignada.Id)).Count();
                        // We get the 'records page' from the caja movimiento table
                        Salidas = db.CajaMovimiento.Where(x => x.IndEntrada.Equals(false) && x.CajaDiarioId.Equals(CajaAsignada.Id)).OrderByDescending(x => x.Id)
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
                                Serie = cc.Serie,
                                Numero = cc.Numero,
                                Total = cc.Total,
                                Descripcion = cc.Descripcion
                            }).SingleOrDefault();


            return Json(cobranza);
        }

        public ActionResult GuardarCobro(int CuentaPorCobrarId, int MatriculaId, int AlumnoId, string Serie, string Numero, decimal Total, string Descripcion)
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
                var SaldoInicial = CajaAsignada.SaldoInicial;
                var Entradas = CajaAsignada.Entradas;
                var Salidas = CajaAsignada.Salidas;
                var SaldoFinal = CajaAsignada.SaldoInicial;

                //Instanciamos un nuevo objeto caja diario para su posterior actualización
                CajaDiario diario = new CajaDiario();

                // WE RECOVER THE OPERATION 'Entradas Otros' FOR ASSIGN TO THE MOVEMENT
                var Operacion = (from o in db.Operacion where o.Denominacion.Equals("ENTRADAS OTROS") select o).SingleOrDefault();

                // WE RECOVER THE STATE 'Pagado' FOR ASSIGN TO THE MOVEMENT
                var EstadoId = (from e in db.Estado where e.Denominacion.Equals("PAGADO") select e.Id).SingleOrDefault();

                // WE LISTED THE LIST OF 'Cuenta Por Cobrar' DETAILS RELATED WITH ITS ID
                var detalles_cobranza = (from ccd in db.CuentasPorCobrarDetalle
                                         where ccd.CuentasPorCobrarId.Equals(CuentaPorCobrarId)
                                         select new
                                         {
                                             ConceptoPagoId = ccd.ConceptoPagoId,
                                             ItemId = ccd.ItemId,
                                             Cantidad = ccd.Cantidad,
                                             Descuento = ccd.Descuento,
                                             Importe = ccd.Importe

                                         }).ToList();

                try
                {
                    db.Configuration.ProxyCreationEnabled = false;
                    db.Configuration.LazyLoadingEnabled = false;
                    db.Configuration.ValidateOnSaveEnabled = false;
                    

                    if (CajaAsignada != null)
                    {
                        // WE PASSED AND SAVE CHANGES 'Cuenta Por Cobrar' in 'CajaMovimiento' TABLE
                        CajaMovimiento movimiento = new CajaMovimiento();
                        movimiento.CajaDiarioId = CajaAsignada.Id;
                        movimiento.AlumnoId = AlumnoId;
                        movimiento.OperacionId = Operacion.Id;
                        movimiento.EstadoId = EstadoId;
                        movimiento.Serie = Serie;
                        movimiento.Numero = Numero;
                        movimiento.Fecha = DateTime.Now;
                        movimiento.Total = Total;
                        movimiento.IndEntrada = true;
                        movimiento.Descripcion = Descripcion;
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
                        CajaDiarioBL.ActualizarParcial(diario, x=>x.Entradas, x=>x.SaldoFinal);

                        //Update 'Cuentas Por Cobrar' pay assigned to student (estado pendiente => pagado)

                        CuentasPorCobrar cobranza = new CuentasPorCobrar();
                        cobranza.Id = CuentaPorCobrarId;
                        cobranza.EstadoId = EstadoId;
                        CuentasPorCobrarBL.ActualizarParcial(cobranza, x=>x.EstadoId);

                        //Update 'MATRICULA' pay assigned to student (estado pendiente => pagado)
                        if (MatriculaId!=0)
                        {
                            Matricula matricula = new Matricula();
                            matricula.Id = MatriculaId;
                            matricula.EstadoId = EstadoId;
                            MatriculaBL.ActualizarParcial(matricula, x=>x.EstadoId);
                        }

                        //Recover data that send from Egresos-Ingresos to view
                        var datos = new string[5];
                        datos[0] = Convert.ToString(SaldoInicial);
                        datos[1] = Convert.ToString(diario.Entradas);
                        datos[2] = Convert.ToString(diario.SaldoFinal);
                        datos[3] = Convert.ToString(diario.Entradas - Salidas);
                        datos[4] = Convert.ToString(cobranza.Id);

                        rm.SetResponse(true);
                        rm.result = datos;
                        
                    }

                }
                catch (Exception ex)
                {
                    rm.SetResponse(false, ex.Message);
                }
            }

            return Json(rm,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EgresosIngresos(int OperacionId, string OperacionDenominacion, int PersonalId, string PersonalNombres, decimal Total, string Descripcion)
        {
            var rm = new Comun.ResponseModel();
            var Serie = "001";
            var Numero = "0000004";
            // RECUPERAMOS EL ID DEL USUARIO EN SESIÓN
            var UsuarioLogeadoId = Session["UsuarioId"];
            int UsuarioActualId = Convert.ToInt32(UsuarioLogeadoId);

            try
            {
                //OBTENEMOS LA CAJA DEL USUARIO EN SESIÓN
                var CajaAsignadaId = (from ca in db.CajaDiario where ca.UsuarioId.Equals(UsuarioActualId) && ca.IndCierre.Equals(false) select ca.Id).SingleOrDefault();

                //Recover initial balance, entries, outputs, final balance
                var SaldoInicial = (from ca in db.CajaDiario where ca.Id.Equals(CajaAsignadaId) select ca.SaldoInicial).SingleOrDefault();
                var Entradas = (from ca in db.CajaDiario where ca.Id.Equals(CajaAsignadaId) select ca.Entradas).SingleOrDefault();
                var Salidas = (from ca in db.CajaDiario where ca.Id.Equals(CajaAsignadaId) select ca.Salidas).SingleOrDefault();
                var SaldoFinal = (from ca in db.CajaDiario where ca.Id.Equals(CajaAsignadaId) select ca.SaldoFinal).SingleOrDefault();
                decimal NetoCaja = 0;

                //Instanciamos un nuevo objeto caja diario para su posterior actualización
                CajaDiario diario = new CajaDiario();

                //OBTENEMOS EL ESTADO "PAGADO" PARA REALIZAR LA OPERACIÓN
                var EstadoId = (from e in db.Estado where e.Denominacion.Equals("PAGADO") select e.Id).SingleOrDefault();
                

                //REGISTRAMOS EL MOVIMIENTO DE CAJA
                CajaMovimiento movimiento = new CajaMovimiento();

                movimiento.CajaDiarioId = CajaAsignadaId;
                movimiento.PersonalId = PersonalId;
                movimiento.OperacionId = OperacionId;
                movimiento.EstadoId = EstadoId;
                movimiento.Serie = Serie;
                movimiento.Numero = Numero;
                movimiento.Fecha = DateTime.Now;
                movimiento.Total = Total;
                if (OperacionDenominacion.Equals("PAGO DE SERVICIOS") || OperacionDenominacion.Equals("SALIDAS OTROS"))
                {
                    movimiento.IndEntrada = false;

                    //ACTUALIZAMOS LA CAJA ASIGNADA AL USUARIO EN SESIÓN
                    diario.Id = CajaAsignadaId;
                    diario.Salidas = Salidas + Total;
                    diario.SaldoFinal = SaldoInicial + Entradas - (Salidas + Total);
                    CajaDiarioBL.ActualizarParcial(diario, x=>x.Salidas, x => x.SaldoFinal);
                    NetoCaja = (Entradas - diario.Salidas).Value;
                    
                }
                else // OperacionDenominacion.Equals("ENTRADAS OTROS")
                {
                    movimiento.IndEntrada = true;

                    //ACTUALIZAMOS LA CAJA ASIGNADA AL USUARIO EN SESIÓN
                    diario.Id = CajaAsignadaId;
                    diario.Entradas = Entradas + Total;
                    diario.SaldoFinal = SaldoInicial + (Entradas + Total) - Salidas;
                    CajaDiarioBL.ActualizarParcial(diario, x => x.Entradas, x => x.SaldoFinal);
                    NetoCaja = (diario.Entradas - Salidas).Value;
                }
                movimiento.Descripcion = Descripcion;
                CajaMovimientoBL.Crear(movimiento);

                //Recover data that send from Egresos-Ingresos to view
                var datos = new string[6];
                datos[0] = OperacionDenominacion;
                datos[1] = Convert.ToString(SaldoInicial);
                datos[2] = Convert.ToString(diario.Entradas);
                datos[3] = Convert.ToString(diario.Salidas);
                datos[4] = Convert.ToString(diario.SaldoFinal);
                datos[5] = Convert.ToString(NetoCaja);

                rm.SetResponse(true);
                rm.result = datos;
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);
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
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);
        }

    }
}
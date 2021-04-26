using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers.Direccion
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_periodo_todo)]
    public class PeriodoController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Periodo> Periodos;
        private Paginador<Periodo> ListadoPeriodos;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string denominacion="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;
                using (db = new DAEntities())
                {
                    // Total number of records in the student table
                    TotalRegistros = db.Periodo.Count();
                    // We get the 'records page' from the student table
                    Periodos = db.Periodo.OrderByDescending(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .ToList();
                    if (!string.IsNullOrEmpty(denominacion))
                    {
                        Periodos = db.Periodo.Where(x => x.Denominacion.Contains(denominacion)).OrderByDescending(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = db.Periodo.Where(x => x.Denominacion.Contains(denominacion)).Count();
                    }
                    // Total number of pages in the student table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list "Especialidad" only with the required fields to avoid serialization problems
                    var SubPeriodos = Periodos.Select(S => new Periodo
                    {
                        Id = S.Id,
                        Denominacion = S.Denominacion,
                        FechaInicio = S.FechaInicio,
                        FechaFin = S.FechaFin,
                        Estado = S.Estado
                        
                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoPeriodos = new Paginador<Periodo>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubPeriodos
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoPeriodos;
                }
                //we send the pagination class to the view
                return Json(rm, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpGet]
        public ActionResult Mantener(int id)
        {
            if(id==0)
            {
                return View(new Periodo() { Estado=true});
            }
            else
            {
                return View(PeriodoBL.Obtener(id));
            }
        }
        [HttpPost]
        public ActionResult Guardar(int? idPeriodo, string Denominacion, string FechaInicio, string FechaFin, bool Estado)
        {
            var rm = new Comun.ResponseModel();

            var existePeriodoActivo = db.Periodo.Where(p => p.Estado).Any();

            try
            {
                if (idPeriodo == null)
                {
                    rm.message = "El id del periodo es inválido o nulo";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    if (existePeriodoActivo)
                    {
                        rm.SetResponse(false, "Ya existe un periodo académico activo");
                    }
                    else
                    {
                        if (Denominacion == "")
                        {
                            rm.message = "Complete el campo Denominacion";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (FechaInicio == "")
                            {
                                rm.message = "Seleccione fecha de inicio del periodo";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                if (FechaFin == "")
                                {
                                    rm.message = "Seleccione fecha de fin del periodo";
                                    rm.SetResponse(false, rm.message);
                                }
                                else
                                {
                                    DateTime fecha_inicio = DateTime.ParseExact(FechaInicio, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                                    DateTime fecha_fin = DateTime.ParseExact(FechaFin, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                                    Periodo periodo = new Periodo();
                                    if (idPeriodo == 0)
                                    {
                                        periodo.Denominacion = Denominacion;
                                        periodo.FechaInicio = fecha_inicio;
                                        periodo.FechaFin = fecha_fin;
                                        periodo.Estado = true;
                                        PeriodoBL.Crear(periodo);
                                    }
                                    else
                                    {
                                        periodo.Id = idPeriodo.Value;
                                        periodo.Denominacion = Denominacion;
                                        periodo.FechaInicio = fecha_inicio;
                                        periodo.FechaFin = fecha_fin;
                                        periodo.Estado = Estado;

                                        PeriodoBL.ActualizarParcial(periodo, x => x.Denominacion, x => x.FechaInicio,
                                            x => x.FechaFin, x => x.Estado);
                                    }

                                    rm.SetResponse(true);
                                    rm.href = Url?.Action("Index", "Periodo");
                                }

                            }
                        }
                    }

                }


            }
            catch(Exception eX)
            {
                rm.SetResponse(false,eX.Message,true);
            }
            return Json(rm,JsonRequestBehavior.AllowGet);

        }
    }
}

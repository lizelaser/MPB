using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_caja_todo)]
    public class CajaController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Caja> Cajas;
        private Paginador<Caja> ListadoCajas;
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
                    // Total number of records in the caja table
                    TotalRegistros = db.Caja.Count();
                    // We get the 'records page' from the caja table
                    Cajas = db.Caja.OrderBy(x => x.Id)
                                                     .Skip((pagina - 1) * RegistrosPorPagina)
                                                     .Take(RegistrosPorPagina)
                                                     .ToList();
                    if (!string.IsNullOrEmpty(denominacion))
                    {
                        Cajas = db.Caja.Where(x => x.Denominacion.Contains(denominacion)).OrderBy(x => x.Id)
                            .Skip((pagina - 1) * RegistrosPorPagina)
                            .Take(RegistrosPorPagina).ToList();
                        TotalRegistros = db.Caja.Where(x => x.Denominacion.Contains(denominacion)).Count();
                    }
                    // Total number of pages in the caja table
                    var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                    //We list "Cajas" only with the required fields to avoid serialization problems
                    var SubCajas = Cajas.Select(S => new Caja
                    {
                        Id = S.Id,
                        Denominacion = S.Denominacion
                    }).ToList();

                    // We instantiate the 'Paging class' and assign the new values
                    ListadoCajas = new Paginador<Caja>()
                    {
                        RegistrosPorPagina = RegistrosPorPagina,
                        TotalRegistros = TotalRegistros,
                        TotalPaginas = TotalPaginas,
                        PaginaActual = pagina,
                        Listado = SubCajas
                    };

                    rm.SetResponse(true);
                    rm.result = ListadoCajas;
                }
                //we send the pagination class to the view
                return Json(rm,JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult Mantener(int id = 0)
        {

            if(id == 0)
            {
                return View(new Caja());
            }
            else
            {
                return View(CajaBL.Obtener(id));
            }

        }
        [HttpPost]
        public ActionResult Guardar(Caja obj)
        {
            var rm = new Comun.ResponseModel();

            try
            {
                if(obj.Id==0)
                {
                    CajaBL.Crear(obj);
                }
                else
                {
                    CajaBL.ActualizarParcial(obj,x=>x.Denominacion);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index","Caja");
            }
            catch(Exception ex)
            {
                rm.SetResponse(false,ex.Message);
            }

            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Eliminar(DAEntities db , int id)
        {
            var caja = CajaBL.Obtener(db, id);
            CajaBL.Eliminar(db,caja);
            return RedirectToAction("Index");

        }
    }
}

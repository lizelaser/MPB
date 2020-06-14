using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Newtonsoft.Json;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_personal_todo)]
    public class PersonalController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Personal> Trabajadores;
        private Paginador<PersonalVm> ListadoPersonal;
        // GET: Personal
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string dni, int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Personal.Count();
                // We get the 'records page' from the student table
                Trabajadores = db.Personal.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();

                if (!string.IsNullOrEmpty(dni))
                {
                    Trabajadores = db.Personal.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Personal.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                //We list "Especialidad" only with the required fields to avoid serialization problems
                var SubPersonal = Trabajadores.Select(S => new PersonalVm
                {
                    Id = S.Id,
                    Dni = S.Dni,
                    PersonalNombres = S.Paterno +  " " + S.Materno + " " + S.Nombres,
                    Celular = S.Celular,
                    Correo = S.Correo,
                    Estado = S.Estado

                }).ToList();

                // We instantiate the 'Paging class' and assign the new values
                ListadoPersonal = new Paginador<PersonalVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubPersonal
                };
                rm.SetResponse(true);
                rm.result = ListadoPersonal;
            }
            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Mantener(int id = 0)
        {
            if (id == 0)
                return View(new Personal() { Estado = true });
            else
                return View(PersonalBL.Obtener(id));
        }
        [HttpPost]
        public ActionResult Guardar(Personal obj, string activo)
        {
            var rm = new Comun.ResponseModel();
            obj.Estado = string.IsNullOrEmpty(activo) ? false : true;
            try
            {
                obj.FechaMod = DateTime.Now;
                if (obj.Id == 0)
                {
                    obj.FechaReg = DateTime.Now;                    
                    obj.Estado = true;
                    PersonalBL.Crear(obj);
                }
                else
                {
                    PersonalBL.ActualizarParcial(obj, x => x.Nombres, x => x.Paterno, x => x.Materno, x => x.Dni,
                        x => x.Nacimiento, x => x.Direccion, x => x.Celular, x => x.Estado, x => x.FechaMod, 
                        x => x.Honorario);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Personal");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Eliminar(int id)
        {
            var personal = PersonalBL.Obtener(id);
            PersonalBL.Eliminar(db, personal);
            return RedirectToAction("Index");

        }
    }
}
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
    public class PersonalController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Personal> Personal;
        private Paginador<Personal> ListadoPersonal;
        // GET: Personal
        public ActionResult Index(string dni, int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.Personal.Count();
                // We get the 'records page' from the student table
                Personal = db.Personal.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                if (!string.IsNullOrEmpty(dni))
                {
                    Personal = db.Personal.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Personal.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoPersonal = new Paginador<Personal>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Personal
                };
            }
            //we send the pagination class to the view
            return View(ListadoPersonal);
        }

        public ActionResult ListarTodo()
        {
            return View(PersonalBL.Listar());
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
    }
}
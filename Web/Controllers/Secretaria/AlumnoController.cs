using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    public class AlumnoController : Controller
    {

        private DAEntities db;
        private readonly int RegistrosPorPagina = 5;
        private List<Alumno> Alumnos;
        private Paginador<Alumno> ListadoAlumnos;
        // GET: Alumno
        public ActionResult Index(string dni, int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.Alumno.Count();
                // We get the 'records page' from the student table
                Alumnos = db.Alumno.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                if (!string.IsNullOrEmpty(dni))
                {
                    Alumnos = db.Alumno.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Alumno.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoAlumnos = new Paginador<Alumno>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Alumnos
                };
            }
            //we send the pagination class to the view
            return View(ListadoAlumnos);
        }

        public ActionResult ListarTodo()
        {
            return View(AlumnoBL.Listar());
        }

        public ActionResult Mantener(int id = 0)
        {
            //ViewBag.cboTipoPersonal = new SelectList(ValorTablaBL.Listar(x => x.TablaId == Constante.ValorTabla.TipoPersonal && x.ItemId > 0), "ItemId", "Denominacion");

            if (id == 0)
            {
                return View(new Alumno() { Estado = true });
            }

            else
            {
                return View(AlumnoBL.Obtener(id));
            }

        }
        [HttpPost]
        public ActionResult Guardar(Alumno obj, string activo)
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
                    AlumnoBL.Crear(obj);
                }
                else
                {
                    obj.FechaMod = DateTime.Now;
                    AlumnoBL.ActualizarParcial(obj, x => x.Nombres, x => x.Paterno, x => x.Materno, x => x.Dni,
                        x => x.Nacimiento, x => x.Direccion, x => x.Celular, x => x.Estado, x => x.FechaMod 
                        );
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Alumno");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }
    }
}
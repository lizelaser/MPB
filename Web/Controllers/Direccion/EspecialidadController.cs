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
    [PermisoAttribute(Permiso = RolesMenu.menu_especialidad_todo)]
    public class EspecialidadController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Especialidad> Especialidades;
        private Paginador<Especialidad> ListadoEspecialidades;
        // GET: Especialidad
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string denominacion, int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Especialidad.Count();
                // We get the 'records page' from the student table
                Especialidades = db.Especialidad.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();

                if (!string.IsNullOrEmpty(denominacion))
                {
                    Especialidades = db.Especialidad.Where(x => x.Denominacion.Contains(denominacion)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Especialidad.Where(x => x.Denominacion.Contains(denominacion)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                //We list "Especialidad" only with the required fields to avoid serialization problems
                var SubEspecialidades = Especialidades.Select(S => new Especialidad
                {
                    Id = S.Id,
                    Denominacion = S.Denominacion,
                    Matricula = S.Matricula,
                    Mensualidad = S.Mensualidad,
                    Cuotas = S.Cuotas

                }).ToList();

                // We instantiate the 'Paging class' and assign the new values
                ListadoEspecialidades = new Paginador<Especialidad>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubEspecialidades
                };
                rm.SetResponse(true);
                rm.result = ListadoEspecialidades;
            }
            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Mantener(int id = 0)
        {
            if (id == 0)
                return View(new Especialidad());
            else
                return View(EspecialidadBL.Obtener(id));
        }
        [HttpPost]
        public ActionResult Guardar(Especialidad obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (obj.Id == 0)
                {
                    EspecialidadBL.Crear(obj);
                }
                else
                {
                    EspecialidadBL.ActualizarParcial(obj, x => x.Denominacion, x => x.Matricula, x => x.Mensualidad, x => x.Cuotas);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Especialidad");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        [HttpDelete]
        public ActionResult Eliminar(int id)
        {
            var especialidad = EspecialidadBL.Obtener(id);
            EspecialidadBL.Eliminar(db, especialidad);
            return RedirectToAction("Index");

        }

    }
}

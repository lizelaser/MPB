using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_curso_todo)]
    public class CursoController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Curso> Cursos;
        private Paginador<CursoVm> ListadoCursos;
        // GET: Alumno
        public ActionResult Index()
        {
            var TotalRegistros = (from c in db.Curso select c).Count();
            return View(CursoBL.Listar(includeProperties:"Especialidad"));

        }

        public ActionResult Tabla(string denominacion="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();
            
            using (db = new DAEntities())
            {
                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Curso.Count();
                // We get the 'records page' from the student table
                Cursos = db.Curso.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Especialidad)
                                                 .ToList();
                if (!string.IsNullOrEmpty(denominacion))
                {
                    var filter = db.Curso.Where(x => x.Denominacion.ToLower().Contains(denominacion.ToLower()));
                    Cursos = filter.OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).Include(x => x.Especialidad).ToList();
                    TotalRegistros = filter.Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int) Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);


                //We list courses only with the required fields to avoid serialization problems
                var SubCursos = Cursos.Select(S => new CursoVm
                {
                    Id = S.Id,
                    Codigo = S.Codigo,
                    Denominacion = S.Denominacion,
                    Credito = S.Credito,
                    NombreEspecialidad = S.Especialidad.Denominacion

                }).ToList();


                // We instantiate the 'Paging class' and assign the new values
                ListadoCursos = new Paginador<CursoVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubCursos
                };
                
                rm.SetResponse(true);
                rm.result = ListadoCursos;
            }

            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListarTodo()
        {
            return View(CursoBL.Listar(includeProperties:"Especialidad")); ;
        }

        public ActionResult Mantener(int id = 0)
        {
            ViewBag.EspecialidadId = db.Especialidad.ToList();

            if (id == 0)
            {
                return View(new Curso());
            }

            else
            {
                
                return View(CursoBL.Obtener(id));
            }
                
        }
        [HttpPost]
        public ActionResult Guardar(Curso obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (obj.Id == 0)
                {
                    CursoBL.Crear(obj);
                }
                else
                {
                    CursoBL.Actualizar(obj);
                }
                rm.SetResponse(true);
                rm.href = Url?.Action("Index", "Curso");
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
            var curso = CursoBL.Obtener(id);
            CursoBL.Eliminar(db, curso);
            return RedirectToAction("Index");

        }

    }
}

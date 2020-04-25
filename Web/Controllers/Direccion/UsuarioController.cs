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
    [Autenticado]
    public class UsuarioController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Usuario> Usuarios;
        private Paginador<Usuario> ListadoUsuarios;
        // GET: Usuario
        public ActionResult Index(string nombre, int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.Usuario.Count();
                // We get the 'records page' from the student table
                Usuarios = db.Usuario.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x => x.Personal)
                                                 .Include(x=>x.Rol)
                                                 .ToList();
                if (!string.IsNullOrEmpty(nombre))
                {
                    Usuarios = db.Usuario.Where(x => x.Nombre.Contains(nombre)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).Include(x => x.Personal).Include(x=>x.Rol).ToList();
                    TotalRegistros = db.Usuario.Where(x => x.Nombre.Contains(nombre)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoUsuarios = new Paginador<Usuario>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Usuarios
                };
            }
            //we send the pagination class to the view
            return View(ListadoUsuarios);
        }

        public ActionResult ListarTodo()
        {
            return View(UsuarioBL.Listar(includeProperties:"Personal,Rol"));
        }
        public ActionResult Mantener(int id=0)
        {

            if (id == 0)
            {
                ViewBag.PersonalId = new SelectList(db.Personal, "Id", "Paterno");
                ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion");
                return View(new Usuario() { Activo = true, IndCambio = false });
            }       
            else
            {
                ViewBag.PersonalId = new SelectList(db.Personal, "Id", "Paterno");
                ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion");
                return View(UsuarioBL.Obtener(id));
            } 

        }

        [HttpPost]
        public ActionResult Guardar(Usuario usuario,string activo)
        {
            var rm = new Comun.ResponseModel();
            usuario.Activo = string.IsNullOrEmpty(activo) ? false : true;
            try
            {
                if (usuario.Id == 0)
                {
                    ViewBag.PersonalId = new SelectList(db.Personal, "Id", "Paterno", usuario.PersonalId);
                    ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion", usuario.RolId);
                    usuario.Nombre = usuario.Nombre;
                    usuario.Clave = usuario.Correo;
                    usuario.IndCambio = false;
                    usuario.Activo = true;
                    UsuarioBL.Crear(usuario);
                }
                else
                {
                    usuario.IndCambio = true;
                    ViewBag.PersonalId = new SelectList(db.Personal, "Id", "Paterno", usuario.PersonalId);
                    ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion", usuario.RolId);
                    UsuarioBL.ActualizarParcial(usuario, x=>x.Nombre, x => x.Correo, x => x.Activo, x => x.RolId, x=>x.PersonalId);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Usuario");
                
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message);                
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

       

        //public ActionResult Listar() {
        //    var user = UsuarioBL.Listar();

        //    return Json(user.Select(x => new { x.Id, x.Nombre }), JsonRequestBehavior.AllowGet);
        //}
    }
}
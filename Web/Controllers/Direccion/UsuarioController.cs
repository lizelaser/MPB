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
    [PermisoAttribute(Permiso = RolesMenu.menu_usuario_todo)]
    public class UsuarioController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Usuario> Usuarios;
        private Paginador<UsuarioViewModel> ListadoUsuarios;
        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Tabla(string nombre, int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Usuario.Count();
                // We get the 'records page' from the student table
                Usuarios = db.Usuario.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .Include(x=>x.Personal)
                                                 .Include(x=>x.Rol)
                                                 .ToList();

                if (!string.IsNullOrEmpty(nombre))
                {
                    Usuarios = db.Usuario.Where(x => x.Nombre.Contains(nombre)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina)
                        .Include(x=>x.Personal)
                        .Include(x=>x.Rol)
                        .ToList();
                    TotalRegistros = db.Usuario.Where(x => x.Nombre.Contains(nombre)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                //We list "Especialidad" only with the required fields to avoid serialization problems
                var SubUsuarios = Usuarios.Select(S => new UsuarioViewModel
                {
                    Id = S.Id,
                    Nombre = S.Nombre,
                    Correo = S.Correo,
                    PersonalNombres = S.Personal.Paterno + " " + S.Personal.Materno + " " + S.Personal.Nombres,
                    RolDenominacion = S.Rol.Denominacion,
                    Activo = S.Activo

                }).ToList();

                // We instantiate the 'Paging class' and assign the new values
                ListadoUsuarios = new Paginador<UsuarioViewModel>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubUsuarios
                };
                rm.SetResponse(true);
                rm.result = ListadoUsuarios;
            }
            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Mantener(int id=0)
        {
            ViewBag.PersonalId = new SelectList(db.Personal.ToList().Select(p => {
                p.Paterno = p.Paterno + " " + p.Materno + " " + p.Nombres;
                return p;
            }), "Id", "Paterno");
            ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion");


            if (id == 0)
            {

                return View(new Usuario() { Activo = true, IndCambio = false });
            }       
            else
            {
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
                    usuario.Clave = Comun.HashHelper.SHA256("123456");
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
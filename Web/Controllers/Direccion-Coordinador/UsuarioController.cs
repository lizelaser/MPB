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
        public ActionResult Tabla(string nombre="", int pagina=1)
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
                    var filter = db.Usuario.Where(x => x.Nombre.ToLower().Contains(nombre.ToLower()));
                    Usuarios = filter.OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina)
                        .Include(x=>x.Personal)
                        .Include(x=>x.Rol)
                        .ToList();
                    TotalRegistros = filter.Count();
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

            ViewBag.RolId = db.Rol.ToList();

            var personal = (from p in db.Personal
                            join u in db.Usuario on p.Id equals u.PersonalId into ps
                            from u in ps.DefaultIfEmpty()
                            where u.PersonalId.Equals(null)
                            select p).ToList().Select(p =>
                            {
                                p.Nombres = p.Paterno + " " + p.Materno + " " + p.Nombres;
                                return p;
                            }).ToList();

            if (id == 0)
            {

                ViewBag.PersonalId = personal;

                return View(new Usuario() { Activo = true, IndCambio = false });
            }       
            else
            {
                var current = (from p in db.Personal
                               join u in db.Usuario on p.Id equals u.PersonalId
                               where u.Id == id
                               select p).SingleOrDefault();

                if (current!=null)
                {
                    current.Nombres = current.Paterno + " " + current.Materno + " " + current.Nombres;
                    personal.Add(current);
                }

                ViewBag.PersonalId = personal;

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
                var u = db.Usuario.Include(x => x.Personal).Where(x => x.Personal.Id == usuario.PersonalId).SingleOrDefault();
                
                if (u!=null)
                {
                    var personalNombres = u.Personal.Paterno + " " + u.Personal.Materno + " " + u.Personal.Nombres;
                    rm.SetResponse(false, $"EL PERSONAL {personalNombres} YA TIENE UN USUARIO ASIGNADO EN EL SISTEMA");
                }
                else
                {
                    if (usuario.PersonalId < 1 || usuario.RolId < 1 || string.IsNullOrEmpty(usuario.Nombre) || string.IsNullOrEmpty(usuario.Correo) || string.IsNullOrEmpty(usuario.Clave))
                    {
                        rm.SetResponse(false, "ASEGURESE DE COMPLETAR LOS CAMPOS REQUERIDOS");
                    }
                    else
                    {
                        if (usuario.Id == 0)
                        {
                            usuario.Clave = Comun.HashHelper.MD5(usuario.Correo.ToLower().Split('@')[0]);
                            usuario.IndCambio = false;
                            usuario.Activo = true;
                            UsuarioBL.Crear(usuario);
                        }
                        else
                        {
                            usuario.IndCambio = true;
                            ViewBag.PersonalId = new SelectList(db.Personal, "Id", "Paterno", usuario.PersonalId);
                            ViewBag.RolId = new SelectList(db.Rol, "Id", "Denominacion", usuario.RolId);
                            UsuarioBL.ActualizarParcial(usuario, x => x.Nombre, x => x.Correo, x => x.Activo, x => x.RolId, x => x.PersonalId);
                        }
                        rm.SetResponse(true);
                        rm.href = Url?.Action("Index", "Usuario");
                    }
                }
                
            }
            catch (Exception ex)
            {
                rm.SetResponse(false,ex.Message,true);                
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

       

        //public ActionResult Listar() {
        //    var user = UsuarioBL.Listar();

        //    return Json(user.Select(x => new { x.Id, x.Nombre }), JsonRequestBehavior.AllowGet);
        //}
    }
}
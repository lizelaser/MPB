using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BL;
using Web.Models;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        [NoLogin]
        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        [Recaptcha(Name = "Captcha")]
        public JsonResult Autenticar(UsuarioVm u)
        {
            var rm = new Comun.ResponseModel();
            //password = Comun.HashHelper.MD5(password);
           // u.Clave = u.Clave.ToLower();

            var usuario= UsuarioBL.Obtener(x => x.Correo == u.Usuario && x.Clave == u.Clave && x.Activo,includeProperties:"Rol");
            if (usuario != null)
            {
                if (!usuario.IndCambio)
                {
                    rm.SetResponse(true);
                    rm.function = "nuevaclave(" + usuario.Id + ",'" + usuario.Correo + "');";
                }
                else
                {
                    AddSesion(usuario.Id, ref rm);
                    Session["UsuarioId"] = usuario.Id;
                    Session["Rol"] = usuario.Rol.Codigo;
                    Session["UsuarioRol"] = usuario.Nombre + " - " + usuario.Rol.Denominacion;
                    Session["mnu"] = Constante.Menu.Listar(usuario.Rol.Codigo);
                    rm.SetResponse(true);
                    rm.href = Url.Action("Index", "Home");
                }
            }
            else
            {
                rm.SetResponse(false, "Usuario o Clave Incorrecta");
            }
            return Json(rm);
        }
        private void AddSesion(int usuarioId, ref Comun.ResponseModel rm)
        {
            Comun.SessionHelper.AddUserToSession(usuarioId.ToString());
            rm.SetResponse(true);
            rm.href = Url.Action("Index", "Home");
        }
        public ActionResult Logout()
        {
            Comun.SessionHelper.DestroyUserSession();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public JsonResult CambiarClave(int id, string usuario, string clave)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                //var enc = Comun.HashHelper.MD5(clave);
                UsuarioBL.ActualizarParcial(new DA.Usuario { Id = id, Clave = clave, IndCambio = true }, x => x.Clave, x => x.IndCambio);
                rm.SetResponse(true);
                Autenticar(new UsuarioVm { Usuario = usuario, Clave = clave });
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm);
        }
    }
}
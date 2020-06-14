using DA;
using BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{

    public class Autorizacion
    {
        public static bool TienePermiso(RolesMenu valor)
        {
            bool permiso_denegado = false;
            try
            {
                DAEntities db = new DAEntities();
                int usuario_id = ObtenerUsuario();
                var usuario = UsuarioBL.Obtener(usuario_id);
                if (usuario!=null)
                {
                    var rol = RolBL.Obtener((usuario.RolId).Value);
                    var permiso = (from pd in db.PermisoDenegadoRol where pd.MenuId.Equals((int)valor) && pd.RolId.Equals(rol.Id) select pd).Any();
                    permiso_denegado = !permiso;
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
                
             return permiso_denegado;

        }

        public static int ObtenerUsuario()
        {
            var id = HttpContext.Current.Session["UsuarioId"];
            var usuarioId = Convert.ToInt32(id);
            return usuarioId;
        }

    }
}
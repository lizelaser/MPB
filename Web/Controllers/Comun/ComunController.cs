using System.Web.Mvc;
using System.Configuration;

namespace Web.Controllers
{
    public class ComunController : Controller
    {

        public static string ObtenerEmpresa()
        {
            return ConfigurationManager.AppSettings["Empresa"];
        }
    }


}
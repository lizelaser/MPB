using Comun;
using BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Reflection;
using Web.Models;
using System.Configuration;
using System.Net;
using System.IO;

namespace Web.Controllers
{
    // Si no estamos logeado, regresamos al login
    public class AutenticadoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!SessionHelper.ExistUserInSession())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Index"
                }));
                return;
            }

            var permiso = HttpContext.Current.Session["UsuarioId"];
            if (permiso == null)
            {
                SessionHelper.DestroyUserSession();
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Login",
                    action = "Index"
                }));
                return;
            }

            //string controlador = filterContext.RouteData.Values["Controller"].ToString();
            //if (controlador.ToLower() == "home") return;


            //if (!permiso.Any(x => x.Modulo == controlador))
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            //    {
            //        controller = "Home",
            //        action = "SinPermiso"
            //    }));
            //}
        }
    }

    // Si no tenemos permiso a un modulo, nos redirige al index/home
    public class PermisoAttribute : ActionFilterAttribute
    {
        public RolesMenu Permiso { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (!Autorizacion.TienePermiso(this.Permiso))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "AccesoDenegado"
                }));
            }
        }
    }

    // Si estamos logeado ya no podemos acceder a la página de Login
    public class NoLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (SessionHelper.ExistUserInSession())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index"
                }));
            }
        }
    }

    //public static class RecaptchaExtensions
    //{
    //    public static IHtmlString Recaptcha(this HtmlHelper @this)
    //    {
    //        return Recaptcha(@this, "RecaptchaPublicKey");
    //    }
    //    public static IHtmlString Recaptcha(this HtmlHelper @this, string publicKeyId)
    //    {
    //        var publicKey = ConfigurationManager.AppSettings[publicKeyId];
    //        return DoRecaptcha(@this, publicKey);
    //    }

    //    private static IHtmlString DoRecaptcha(this HtmlHelper @this, string publicKey)
    //    {
    //        var tagBuilder = new TagBuilder("script");
    //        tagBuilder.Attributes.Add("type", "text/javascript");
    //        tagBuilder.Attributes.Add("src", string.Concat("http://www.google.com/recaptcha/api/challenge?k=", publicKey));

    //        return MvcHtmlString.Create(tagBuilder.ToString(TagRenderMode.Normal));
    //    }
    //}

    public class RecaptchaAttribute : ActionFilterAttribute
    {
        public string Name { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.RequestContext.HttpContext.Request;
            var challenge = request.Form["recaptcha_challenge_field"];
            var response = request.Form["recaptcha_response_field"];
            const string postUrl = "http://localhost:19407/Home/Index";
            var result = PerformPost(request.UserHostAddress, challenge, response, postUrl);
            if (!result)
            {
                filterContext.Controller.ViewData.ModelState.AddModelError
                   (Name ?? string.Empty, "Recaptcha incorrecto");
            }
        }

        private bool PerformPost(string remoteip, string challenge, string response, string postUrl)
        {
            var request = WebRequest.Create(postUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            var stream = request.GetRequestStream();
            var privateKey = ConfigurationManager.AppSettings["ReCaptchaPrivateKey"];
            using (var sw = new StreamWriter(stream))
            {
                const string data = "privatekey={0}&remoteip={1}&challenge={2}&response={3}";
                sw.Write(data, privateKey, remoteip, challenge, response);
            }
            var recaptchaResponse = request.GetResponse();
            string recaptchaData = null;
            var recaptchaStream = recaptchaResponse.GetResponseStream();
            if (recaptchaStream != null)
            {
                using (var sr = new StreamReader(recaptchaStream))
                {
                    recaptchaData = sr.ReadToEnd();
                }
                return ParseResponse(recaptchaData);
            }
            else return false;
        }

        private static bool ParseResponse(string recaptchaData)
        {
            var reader = new StringReader(recaptchaData);
            var first = reader.ReadLine();
            var result = false;
            if (first != null)
            {
                first = first.ToLowerInvariant();
                bool.TryParse(first, out result);
            }

            return result;
        }

    }

}
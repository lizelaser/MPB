using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    [Autenticado]
    public class HorarioController : Controller
    {
        private DAEntities db = new DAEntities();
        public ActionResult Index()
        {
            
            return View(HorarioBL.Listar(includeProperties:"Periodo,Curso,Aula"));
        }
        public ActionResult Mantener(int id = 0)
        {
            ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion");
            ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion");
            ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion");
            if (id == 0)
            {
                return View(new Horario());
            }
                
            else
            {
                return View(HorarioBL.Obtener(id));
            }
               
        }
        [HttpPost]
        public ActionResult Guardar(Horario obj)
        {
            var rm = new Comun.ResponseModel();
            try
            {
                if (obj.Id == 0)
                {
                    ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion", obj.PeriodoId);
                    ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion", obj.CursoId);
                    ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion", obj.AulaId);

                    HorarioBL.Crear(obj);
                }
                else
                {
                    ViewBag.PeriodoId = new SelectList(db.Periodo, "Id", "Denominacion", obj.PeriodoId);
                    ViewBag.CursoId = new SelectList(db.Curso, "Id", "Denominacion", obj.CursoId);
                    ViewBag.AulaId = new SelectList(db.Aula, "Id", "Denominacion", obj.AulaId);
                    HorarioBL.ActualizarParcial(obj, x => x.PeriodoId, x => x.CursoId, x => x.AulaId, x => x.Hora,
                        x => x.CantidadHora, x => x.Dia);
                }
                rm.SetResponse(true);
                rm.href = Url.Action("Index", "Horario");
            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Eliminar(int id)
        {

            var horario = HorarioBL.Obtener(id);

            HorarioBL.Eliminar(db,horario);

            db.SaveChanges();

            return RedirectToAction("Index", "Horario");

        }
    }
}

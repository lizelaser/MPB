using DA;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Web.Mvc;
using Web.Controllers;

namespace Web.UnitTest
{
    [TestClass]
    public class HorarioTest
    {
        [TestMethod]
        public void TablaTest()
        {
            DAEntities db = new DAEntities();
            var controller = new HorarioController();
            var result = controller.Tabla("", 1) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsTrue(rm.result.TotalRegistros == db.Horario.Count());
        }

        [TestMethod]
        public void MantenerTest()
        {
            DAEntities db = new DAEntities();
            var controller = new HorarioController();
            var result = controller.Mantener(0) as ViewResult;
            bool viewBagPeriodo = result.ViewBag.PeriodoId == db.Periodo.Where(p => p.Estado).FirstOrDefault().Id;
            var viewBagEspecialidades = result.ViewBag.Especialidades.Count == db.Especialidad.Count();
            var viewBagAulas = result.ViewBag.Aulas.Count == db.Aula.Count();
            var viewBagDocentes = result.ViewBag.Docentes.Count == (from p in db.Personal
                                                              join pt in db.Personal_Tipo on p.Id equals pt.PersonalId
                                                              where pt.TipoPersonalId == 2
                                                              select p).Count();

            Assert.IsTrue(viewBagPeriodo && viewBagEspecialidades && viewBagAulas && viewBagDocentes);
        }

        [TestMethod]
        public void GuardarTest()
        {
            DAEntities db = new DAEntities();

            var horario = new Horario()
            {
                PeriodoId = db.Periodo.Where(p => p.Estado == true).First().Id,
                CursoId = db.Curso.First().Id,
                AulaId = db.Aula.First().Id,
                HoraInicio = DateTime.Now.TimeOfDay,
                HoraFin = DateTime.Now.TimeOfDay,
                Dias = "LUNES",
                DocenteId = db.Personal.First().Id
            };
            var controller = new HorarioController();
            var result = controller.Guardar(horario) as JsonResult;
            var rm = result.Data as Comun.ResponseModel;
            Assert.IsFalse(rm.isException);
        }
    }
}
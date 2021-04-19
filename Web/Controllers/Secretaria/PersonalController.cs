using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Web.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_personal_todo)]
    public class PersonalController : Controller
    {
        private DAEntities db = new DAEntities();
        private readonly int RegistrosPorPagina = 5;
        private List<Personal> Trabajadores;
        private Paginador<PersonalVm> ListadoPersonal;
        // GET: Personal
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string dni, int pagina)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Personal.Count();
                // We get the 'records page' from the student table
                Trabajadores = db.Personal.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();

                if (!string.IsNullOrEmpty(dni))
                {
                    Trabajadores = db.Personal.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Personal.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                //We list "Especialidad" only with the required fields to avoid serialization problems
                var SubPersonal = Trabajadores.Select(S => new PersonalVm
                {
                    Id = S.Id,
                    Dni = S.Dni,
                    PersonalNombres = S.Paterno +  " " + S.Materno + " " + S.Nombres,
                    Celular = S.Celular,
                    Correo = S.Correo,
                    Estado = S.Estado

                }).ToList();

                // We instantiate the 'Paging class' and assign the new values
                ListadoPersonal = new Paginador<PersonalVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubPersonal
                };
                rm.SetResponse(true);
                rm.result = ListadoPersonal;
            }
            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Mantener(int id = 0)
        {
            ViewBag.TiposPersonal = db.TipoPersonal.ToList();
            if (id == 0)
            {
                return View(new Personal() { Estado = true });
            }
            else
            {
                ViewBag.TipoPersonal = db.Personal_Tipo.Where(pt => pt.PersonalId == id)
                    .Select(pt => pt.TipoPersonalId).ToList();
                return View(PersonalBL.Obtener(id));

            }
        }
        [HttpPost]
        public ActionResult Guardar(int? idPersonal, List<int> TiposPersonalId, string Dni, string Paterno, string Materno, string Nombres, string Correo, string Celular, string Nacimiento, string Direccion, decimal ? Honorario, bool Estado)
        {
            var rm = new Comun.ResponseModel();
            var correo = !String.IsNullOrEmpty(Correo) ? Correo : null;
            var celular = !String.IsNullOrEmpty(Celular) ? Celular : null;
            var direccion = !String.IsNullOrEmpty(Direccion) ? Direccion : null;
            DateTime? nacimiento = !String.IsNullOrEmpty(Nacimiento) ? DateTime.ParseExact(Nacimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateTime?)null;
            var honorario = Honorario != null ? Honorario : null;


            try
            {
                if (idPersonal == null)
                {
                    rm.message = "El id del personal es inválido o nulo";
                    rm.SetResponse(false, rm.message);
                }
                else
                {
                    if (Dni == "")
                    {
                        rm.message = "Complete el campo Dni";
                        rm.SetResponse(false, rm.message);
                    }
                    else
                    {
                        if (Paterno == "")
                        {
                            rm.message = "Complete el campo Apellido Paterno";
                            rm.SetResponse(false, rm.message);
                        }
                        else
                        {
                            if (Materno == "")
                            {
                                rm.message = "Complete el campo Apellido Materno";
                                rm.SetResponse(false, rm.message);
                            }
                            else
                            {
                                if (Nombres == "")
                                {
                                    rm.message = "Complete el campo Nombres";
                                    rm.SetResponse(false, rm.message);
                                }
                                else
                                {
                                    Personal personal = new Personal();
                                    if (idPersonal == 0)
                                    {
                                        personal.Paterno = Paterno;
                                        personal.Materno = Materno;
                                        personal.Nombres = Nombres;
                                        personal.Dni = Dni;
                                        personal.Nacimiento = nacimiento;
                                        personal.Direccion = direccion;
                                        personal.Celular = celular;
                                        personal.Correo = correo;
                                        personal.Honorario = honorario;
                                        personal.FechaReg = DateTime.Now;
                                        personal.Estado = true;
                                        PersonalBL.Crear(personal);
                                    }
                                    else
                                    {
                                        personal.Id = idPersonal.Value;
                                        personal.Paterno = Paterno;
                                        personal.Materno = Materno;
                                        personal.Nombres = Nombres;
                                        personal.Dni = Dni;
                                        personal.Nacimiento = nacimiento;
                                        personal.Direccion = direccion;
                                        personal.Celular = celular;
                                        personal.Correo = correo;
                                        personal.Honorario = honorario;
                                        personal.Estado = Estado;
                                        personal.FechaMod = DateTime.Now;
                                        PersonalBL.ActualizarParcial(personal, x => x.Nombres, x => x.Paterno, x => x.Materno, x => x.Dni,
                                            x => x.Nacimiento, x => x.Direccion, x => x.Celular, x => x.Correo, x => x.Honorario, x => x.Estado, x => x.FechaMod
                                            );
                                    }

                                    var tiposPersonalDb = db.Personal_Tipo.Where(pt => pt.PersonalId == personal.Id);
                                    db.Personal_Tipo.RemoveRange(tiposPersonalDb);
                                    db.SaveChanges();

                                    var personal_tipo = TiposPersonalId.Select(tp => new Personal_Tipo
                                    {
                                        PersonalId = personal.Id,
                                        TipoPersonalId = tp
                                    });

                                    db.Personal_Tipo.AddRange(personal_tipo);
                                    db.SaveChanges();

                                    rm.SetResponse(true);
                                    rm.href = Url?.Action("Index", "Personal");
                                }
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            {
                rm.SetResponse(false, ex.Message);
            }
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Eliminar(int id)
        {
            var personal = PersonalBL.Obtener(id);
            PersonalBL.Eliminar(db, personal);
            return RedirectToAction("Index");

        }
    }
}
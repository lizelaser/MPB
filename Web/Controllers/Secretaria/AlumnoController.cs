using BL;
using DA;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Web.Models;

namespace Web.Controllers
{
    [Autenticado]
    [PermisoAttribute(Permiso = RolesMenu.menu_alumno_todo)]
    public class AlumnoController : Controller
    {

        private DAEntities db;
        private readonly int RegistrosPorPagina = 5;
        private List<Alumno> Alumnos;
        private Paginador<AlumnoVm> ListadoAlumnos;

        public AlumnoController()
        {
            db = new DAEntities();
        }

        // GET: Alumno
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Tabla(string dni="", int pagina=1)
        {
            var rm = new Comun.ResponseModel();

            using (db = new DAEntities())
            {
                db.Configuration.ProxyCreationEnabled = false;
                db.Configuration.LazyLoadingEnabled = false;
                db.Configuration.ValidateOnSaveEnabled = false;

                int TotalRegistros = 0;

                // Total number of records in the student table
                TotalRegistros = db.Alumno.Count();
                // We get the 'records page' from the student table
                Alumnos = db.Alumno.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();

                if (!string.IsNullOrEmpty(dni))
                {
                    Alumnos = db.Alumno.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Alumno.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);

                //We list "Especialidad" only with the required fields to avoid serialization problems
                var SubAlumnos = Alumnos.Select(S => new AlumnoVm
                {
                    Id = S.Id,
                    Dni = S.Dni,
                    AlumnoNombres = S.Paterno + " " + S.Materno + " " + S.Nombres,
                    Celular = S.Celular,
                    Correo = S.Correo,
                    Estado = S.Estado

                }).ToList();

                // We instantiate the 'Paging class' and assign the new values
                ListadoAlumnos = new Paginador<AlumnoVm>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = SubAlumnos
                };
                rm.SetResponse(true);
                rm.result = ListadoAlumnos;
            }
            //we send the pagination class to the view
            return Json(rm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Mantener(int id = 0)
        {
            //ViewBag.cboTipoPersonal = new SelectList(ValorTablaBL.Listar(x => x.TablaId == Constante.ValorTabla.TipoPersonal && x.ItemId > 0), "ItemId", "Denominacion");

            ViewBag.Especialidades = db.Especialidad.ToList();
            if (id == 0)
            {
                return View(new Alumno() { Estado = true });
            }

            else
            {
                ViewBag.EspecialidadesAlumno = db.Alumno_Especialidad.Where(ae => ae.AlumnoId == id)
                    .Select(ae=>ae.EspecialidadId).ToList();
                var alumno = AlumnoBL.Obtener(id);
                
                return View(alumno);
            }

        }
        [HttpPost]
        public ActionResult Guardar(int? idAlumno, List<int> EspecialidadesId, string Dni, string Paterno, string Materno, string Nombres, string Codigo, string Correo, string Celular, string Nacimiento, string Direccion, bool Estado)
        {
            var rm = new Comun.ResponseModel();
            //DateTime? fecha_nacimiento = Nacimiento != null ? Convert.ToDateTime(Nacimiento) : null;
            var correo = !String.IsNullOrEmpty(Correo) ? Correo : null;
            var celular = !String.IsNullOrEmpty(Celular) ? Celular : null;
            var direccion = !String.IsNullOrEmpty(Direccion) ? Direccion : null;
            DateTime? nacimiento = !String.IsNullOrEmpty(Nacimiento) ? DateTime.ParseExact(Nacimiento, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateTime?)null;

            var alumno_exists = db.Alumno.Where(a=>a.Dni==Dni && a.Codigo==Codigo).SingleOrDefault();

            try
            {
                if (idAlumno==null)
                {
                    rm.message = "El id del alumno es inválido o nulo";
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
                                    if (Codigo == "")
                                    {
                                        rm.message = "Complete el campo Codigo";
                                        rm.SetResponse(false, rm.message);
                                    }
                                    else
                                    {
                                        Alumno alumno = new Alumno();
                                        if (idAlumno == 0)
                                        {
                                            alumno.Paterno = Paterno;
                                            alumno.Materno = Materno;
                                            alumno.Nombres = Nombres;
                                            alumno.Dni = Dni;
                                            alumno.Nacimiento = nacimiento;
                                            alumno.Direccion = direccion;
                                            alumno.Celular = correo;
                                            alumno.Correo = celular;
                                            alumno.Codigo = Codigo;
                                            alumno.FechaReg = DateTime.Now;
                                            alumno.Estado = true;
                                            AlumnoBL.Crear(alumno);
                                        }
                                        else
                                        {
                                            alumno.Id = idAlumno.Value;
                                            alumno.Paterno = Paterno;
                                            alumno.Materno = Materno;
                                            alumno.Nombres = Nombres;
                                            alumno.Dni = Dni;
                                            alumno.Nacimiento = nacimiento;
                                            alumno.Direccion = direccion;
                                            alumno.Celular = celular;
                                            alumno.Correo = correo;
                                            alumno.Codigo = Codigo;
                                            alumno.Estado = Estado;
                                            alumno.FechaMod = DateTime.Now;
                                            AlumnoBL.ActualizarParcial(alumno, x => x.Nombres, x => x.Paterno, x => x.Materno, x => x.Dni,
                                                x => x.Nacimiento, x => x.Direccion, x => x.Celular, x => x.Correo, x => x.Codigo, x => x.Estado, x => x.FechaMod
                                                );
                                        }

                                        var especialidadesdb = db.Alumno_Especialidad.Where(ae=>ae.AlumnoId==alumno.Id);
                                        db.Alumno_Especialidad.RemoveRange(especialidadesdb);
                                        db.SaveChanges();

                                        var alumno_especialidad = EspecialidadesId.Select(e => new Alumno_Especialidad
                                        {
                                            AlumnoId = alumno.Id,
                                            EspecialidadId = e
                                        });

                                        db.Alumno_Especialidad.AddRange(alumno_especialidad);
                                        db.SaveChanges();
                              
                                        rm.SetResponse(true);
                                        rm.href = Url?.Action("Index", "Alumno");

                                    }
                                }
                            }

                        }
                    }
                }

            }
            catch (Exception ex)
            
            {
                rm.SetResponse(false, ex.Message,true);
            }
            return Json(rm);
        }

        public ActionResult Eliminar(int id)
        {
            var alumno = AlumnoBL.Obtener(id);
            AlumnoBL.Eliminar(db, alumno);
            return RedirectToAction("Index");

        }
    }
}
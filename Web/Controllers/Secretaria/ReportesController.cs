using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers.Secretaria
{
    public class ReportesController : Controller
    {
        private DAEntities db;
        private readonly int RegistrosPorPagina = 10;
        private List<Alumno> Alumnos;
        private Paginador<Alumno> ListadoAlumnos;

        public ActionResult ReportesAlumno(string dni, int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
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
                // We instantiate the 'Paging class' and assign the new values
                ListadoAlumnos = new Paginador<Alumno>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Alumnos
                };
            }
            //we send the pagination class to the view
            return View(ListadoAlumnos);
        }

        private List<Personal> Personales;
        private Paginador<Personal> ListadoPersonal;
        public ActionResult ReportesPersonal(string dni, int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.Personal.Count();
                // We get the 'records page' from the student table
                Personales = db.Personal.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                if (!string.IsNullOrEmpty(dni))
                {
                    Personales = db.Personal.Where(x => x.Dni.Contains(dni)).OrderBy(x => x.Id)
                        .Skip((pagina - 1) * RegistrosPorPagina)
                        .Take(RegistrosPorPagina).ToList();
                    TotalRegistros = db.Personal.Where(x => x.Dni.Contains(dni)).Count();
                }
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoPersonal = new Paginador<Personal>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Personales
                };
            }
            //we send the pagination class to the view
            return View(ListadoPersonal);
        }

        private List<CajaMovimiento> Ingresos;
        private Paginador<CajaMovimiento> ListadoIngresos;
        public ActionResult ReportesIngreso(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.CajaMovimiento.Where(x=>x.IndEntrada==true).Count();
                // We get the 'records page' from the student table
                Ingresos = db.CajaMovimiento.Where(x => x.IndEntrada==true).OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoIngresos = new Paginador<CajaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Ingresos
                };
            }
            //we send the pagination class to the view
            return View(ListadoIngresos);
        }

        private List<CajaMovimiento> Egresos;
        private Paginador<CajaMovimiento> ListadoEgresos;
        public ActionResult ReportesEgreso(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.CajaMovimiento.Where(x => x.IndEntrada == false).Count();
                // We get the 'records page' from the student table
                Egresos = db.CajaMovimiento.Where(x => x.IndEntrada == false).OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoEgresos = new Paginador<CajaMovimiento>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Egresos
                };
            }
            //we send the pagination class to the view
            return View(ListadoEgresos);
        }

        private List<CuentasPorCobrar> Deudas;
        private Paginador<CuentasPorCobrar> ListadoDeudas;
        public ActionResult ReportesDeudas(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the student table
                TotalRegistros = db.CuentasPorCobrar.Count();
                // We get the 'records page' from the student table
                Deudas = db.CuentasPorCobrar.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();

                // Total number of pages in the student table
                var TotalPaginas = (int)Math.Ceiling((double)TotalRegistros / RegistrosPorPagina);
                // We instantiate the 'Paging class' and assign the new values
                ListadoDeudas = new Paginador<CuentasPorCobrar>()
                {
                    RegistrosPorPagina = RegistrosPorPagina,
                    TotalRegistros = TotalRegistros,
                    TotalPaginas = TotalPaginas,
                    PaginaActual = pagina,
                    Listado = Deudas
                };
            }
            //we send the pagination class to the view
            return View(ListadoDeudas);
        }
    }
}

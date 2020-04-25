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
    [Autenticado]
    public class CuentasPorCobrarController : Controller
    {
        private DAEntities db;
        private readonly int RegistrosPorPagina = 5;
        private List<CuentasPorCobrar> Deudas;
        private Paginador<CuentasPorCobrar> ListadoDeudas;
        public ActionResult Index(int pagina = 1)
        {
            int TotalRegistros = 0;
            using (db = new DAEntities())
            {
                // Total number of records in the boveda table
                TotalRegistros = db.CuentasPorCobrar.Count();
                // We get the 'records page' from the boveda table
                Deudas = db.CuentasPorCobrar.OrderBy(x => x.Id)
                                                 .Skip((pagina - 1) * RegistrosPorPagina)
                                                 .Take(RegistrosPorPagina)
                                                 .ToList();
                // Total number of pages in the boveda table
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
            return View(ListadoDeudas);
        }
        
    }
}

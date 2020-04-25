using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class CajaDiarioVm: CajaDiario {
        DAEntities db = new DAEntities();
        public decimal GetEntradas(int id)
        {
            var entradas = (from e in db.CajaMovimiento where e.IndEntrada == true && e.CajaDiarioId == id select e).ToList();
            var eCaja = entradas.Sum(x => x.Total);
            return (decimal)eCaja;
        }
        public decimal GetSalidas(int id)
        {
            var salidas = (from s in db.CajaMovimiento where s.IndEntrada == false && s.CajaDiarioId == id select s).ToList();
            var sCaja = salidas.Sum(x => x.Total);
            return (decimal)sCaja;
        }
        public decimal GetFinalCaja(int id)
        {
            var cajadiarioactual = (from f in db.CajaDiario where f.IndCierre == false && f.Id == id select f).Single();
            var resultado = (cajadiarioactual.SaldoInicial + GetEntradas(cajadiarioactual.Id)) - GetSalidas(cajadiarioactual.Id);
            return resultado;
        }
    }
}

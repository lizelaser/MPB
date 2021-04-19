using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comun
{
    /// <summary>
    /// Esta clase ha sido creada con la finalidad de crear una comunicación con el modelo, ya sea retornando una respuesta o un objeto.
    /// Ejm: Cuando hacemos un INSERT, posiblemente no se haya realizado el INSERT porque hay un paso previo que debemos hacer, con esta clase podemos especificar cual es el paso previo que falta.
    /// </summary>
    public class ResponseModel
    {
        public dynamic result { get; set; }
        public bool response { get; set; }
        public bool isException { get; set; }
        public string message { get; set; }
        public string href { get; set; }
        public string function { get; set; }

        public ResponseModel()
        {
            this.response = false;
            this.message = "Ocurrio un error inesperado";
            this.isException = false;
        }

        public void SetResponse(bool r, string m = "", bool e=false)
        {
            this.response = r;
            this.message = m;
            this.isException = e;

            if (!r && m == "") this.message = "Ocurrio un error inesperado";
        }
    }
}

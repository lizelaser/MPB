using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class MenuVm: Menu
    {
        public virtual ICollection<Rol> Rol { get; set; }
    }
}

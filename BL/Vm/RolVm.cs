using DA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BL
{
    public class RolVm: Rol
    {
        public virtual ICollection<Menu> Menu { get; set; }
    }
}

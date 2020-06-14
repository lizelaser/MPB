using DA;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public enum RolesMenu
    {
        #region Menus
        menu_usuario_todo = 1,
        menu_personal_todo = 6,
        menu_alumno_todo = 11,
        menu_matricula_todo = 16,
        menu_pagos_todo = 18,
        menu_cuentasporcobrar_todo = 20,
        menu_curso_todo = 22,
        menu_especialidad_todo = 27,
        menu_caja_todo = 32,
        menu_cajadiario_todo = 37,
        menu_boveda_todo = 40,
        menu_periodo_todo = 43,
        menu_aula_todo = 48,
        menu_horario_todo = 53,
        menu_reporte_todo = 58,
        menu_reporte_entradas = 59,
        menu_reporte_salidas = 60
        #endregion
    }
}

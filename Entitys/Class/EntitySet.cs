using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entitys.Class
{
    using DbFrame.BaseClass;
    public class EntitySet
    {

        public static void Register(DbTable tabs)
        {
            //系统表 Start
            tabs.Register(typeof(Entitys.SysClass.Sys_AppLog));
            tabs.Register(typeof(Entitys.SysClass.Sys_Function));
            tabs.Register(typeof(Entitys.SysClass.Sys_Menu));
            tabs.Register(typeof(Entitys.SysClass.Sys_MenuFunction));
            tabs.Register(typeof(Entitys.SysClass.Sys_Role));
            tabs.Register(typeof(Entitys.SysClass.Sys_RoleMenuFunction));
            tabs.Register(typeof(Entitys.SysClass.Sys_User));
            tabs.Register(typeof(Entitys.SysClass.Sys_UserRole));
            //系统表 End
            tabs.Register(typeof(Member));
        }

    }
}

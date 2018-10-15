using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace Ulises5000_AT_HMI_CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult Ed137RecVar(Session session)
        {
            session.Log("Begin Ed137RecVar");

            int varInt = int.Parse(session["ED137REC"]);

            session["ED137REC"] = (varInt & 0x01).ToString();
            session["RECDUAL"]  = (varInt & 0x02) != 0 ? "1" : "0";

            return ActionResult.Success;
        }
    }
}

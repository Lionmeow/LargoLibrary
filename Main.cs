using SRML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LargoLibrary
{
    public class Main : ModEntryPoint
    {
        public static Version Version = new Version(1, 3);

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }

        public override void Load()
        {

        }

        public override void PostLoad()
        {

        }
    }
}
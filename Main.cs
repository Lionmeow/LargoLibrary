﻿using SRML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LargoLibrary
{
    public class Main : ModEntryPoint
    {
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
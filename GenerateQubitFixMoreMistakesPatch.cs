using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace LargoLibrary
{
    [HarmonyPatch(typeof(GenerateQuantumQubit), "Awake")]
    static class GenerateQubitFixMoreMistakesPatch
    {
        public static void Prefix(GenerateQuantumQubit __instance)
        {
            __instance.AppearanceApplicator = __instance.GetComponentInParent<SlimeAppearanceApplicator>();
        }
    }
}

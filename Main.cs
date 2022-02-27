using SRML;
using SRML.SR;
using SRML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace LargoLibrary
{
    public class Main : ModEntryPoint
    {
        public static Version Version = new Version(1, 5);

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
        }

        public override void Load()
        {
            LargoGenerator.replaceElements = new Dictionary<string, SlimeAppearanceElement>()
            {
                { "RadAura", GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PINK_RAD_LARGO).AppearancesDefault[0].Structures[1].Element }
            };
            LargoGenerator.defaultBody = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.PINK_SLIME).AppearancesDefault[0].Structures[0].Element;
        }

        public override void PostLoad()
        {
        }

        private static Dictionary<Type, UnityEngine.Object[]> cache = new Dictionary<Type, UnityEngine.Object[]>();
        internal static T LoadResource<T>(string name) where T : UnityEngine.Object
        {
            if (!cache.ContainsKey(typeof(T)))
                cache[typeof(T)] = Resources.FindObjectsOfTypeAll<T>();
            return ((T[])cache[typeof(T)]).First(x => x.name == name);
        }
    }
}
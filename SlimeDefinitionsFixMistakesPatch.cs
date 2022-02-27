using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;

namespace LargoLibrary
{
    [HarmonyPatch(typeof(SlimeDefinitions), "RefreshIndexes")]
    static class SlimeDefinitionsFixMistakesPatch
    {
        public static bool Prefix(SlimeDefinitions __instance)
        {
            foreach (SlimeDefinition slime in __instance.Slimes)
            {
                try
                {
                    __instance.slimeDefinitionsByIdentifiable.Add(slime.IdentifiableId, slime);
                    if (slime.IsLargo)
                    {
                        if (slime.BaseSlimes.Length == 2)
                        {
                            if (slime.BaseSlimes[0].Diet.Produces.Length > 0 && slime.BaseSlimes[1].Diet.Produces.Length > 0 && !__instance.largoDefinitionByBasePlorts.ContainsKey(new SlimeDefinitions.PlortPair(slime.BaseSlimes[0].Diet.Produces[0], slime.BaseSlimes[1].Diet.Produces[0])))
                                __instance.largoDefinitionByBasePlorts.Add(new SlimeDefinitions.PlortPair(slime.BaseSlimes[0].Diet.Produces[0], slime.BaseSlimes[1].Diet.Produces[0]), slime);
                            if (!__instance.largoDefinitionByBaseDefinitions.ContainsKey(new SlimeDefinitions.SlimeDefinitionPair(slime.BaseSlimes[0], slime.BaseSlimes[1])))
                                __instance.largoDefinitionByBaseDefinitions.Add(new SlimeDefinitions.SlimeDefinitionPair(slime.BaseSlimes[0], slime.BaseSlimes[1]), slime);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error("Exception caught while attempting to process slime.", (object)"name", (object)slime.Name, (object)"Exception", (object)ex.Message, (object)"Stacktrace", (object)ex.StackTrace.ToString());
                }
            }
            return false;
        }
    }
}

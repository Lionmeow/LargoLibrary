using SRML.SR;
using SRML.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LargoLibrary
{
    public static class LargoGenerator
    {
        public static List<Identifiable.Id> generatedIds = new List<Identifiable.Id>();

        public static SlimeAppearance CombineAppearances(Identifiable.Id slime1, Identifiable.Id slime2, SlimeDefinition largoDefinition, GameObject largoGameobject, bool radLargo = false)
        {
            SlimeAppearance baseAppearance = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(slime1).AppearancesDefault[0];
            SlimeAppearance addonAppearance = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(slime2).AppearancesDefault[0];
            SlimeAppearance radAppearance = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(Identifiable.Id.RAD_SLIME).AppearancesDefault[0];

            SlimeAppearance largoAppearance = PrefabUtils.DeepCopyObject(baseAppearance) as SlimeAppearance;
            largoDefinition.AppearancesDefault[0] = largoAppearance;
            largoAppearance.DependentAppearances = new SlimeAppearance[2]
            {
                baseAppearance,
                addonAppearance
            };
            SlimeAppearanceStructure[] structures = largoAppearance.Structures;
            if (radLargo)
            {
                foreach (SlimeAppearanceStructure slimeAppearanceStructure in structures)
                {
                    slimeAppearanceStructure.DefaultMaterials = radAppearance.Structures[0].DefaultMaterials;
                }
            }
            else
            {
                foreach (SlimeAppearanceStructure slimeAppearanceStructure in structures)
                {
                    slimeAppearanceStructure.DefaultMaterials = addonAppearance.Structures[0].DefaultMaterials;
                }
            }
            int currStructure = 0;
            List<SlimeAppearanceStructure> newStructures = structures.ToList();
            foreach (SlimeAppearanceStructure structure in addonAppearance.Structures)
            {
                if (currStructure != 0)
                {
                    newStructures.Add(new SlimeAppearanceStructure(structure));
                }
                currStructure++;
            }
            largoAppearance.Structures = newStructures.ToArray();
            SlimeAppearance.Palette palette = largoAppearance.ColorPalette = addonAppearance.ColorPalette;
            largoAppearance.Face = addonAppearance.Face;
            largoGameobject.GetComponent<SlimeAppearanceApplicator>().Appearance = largoAppearance;

            if (addonAppearance.CrystalAppearance != null)
            {
                largoAppearance.CrystalAppearance = addonAppearance.CrystalAppearance;
            }
            if (addonAppearance.ExplosionAppearance != null)
            {
                largoAppearance.ExplosionAppearance = addonAppearance.ExplosionAppearance;
            }
            if (addonAppearance.GlintAppearance != null)
            {
                largoAppearance.GlintAppearance = addonAppearance.GlintAppearance;
            }
            if (addonAppearance.QubitAppearance != null)
            {
                largoAppearance.QubitAppearance = addonAppearance.QubitAppearance;
            }
            if (addonAppearance.ShockedAppearance != null)
            {
                largoAppearance.ShockedAppearance = addonAppearance.ShockedAppearance;
            }
            if (addonAppearance.TornadoAppearance != null)
            {
                largoAppearance.TornadoAppearance = addonAppearance.TornadoAppearance;
            }
            if (addonAppearance.VineAppearance != null)
            {
                largoAppearance.VineAppearance = addonAppearance.VineAppearance;
            }

            return largoAppearance;
        }

        public static bool CreateLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId, bool useSecondaryAppearanceAsMain = false)
        {
            GameObject baseObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(slime1);
            SlimeDefinition baseDefinition = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(slime1);
            SlimeAppearance baseAppearance = baseDefinition.AppearancesDefault[0];

            GameObject addonObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(slime2);
            SlimeDefinition addonDefinition = SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(slime2);
            SlimeAppearance addonAppearance = addonDefinition.AppearancesDefault[0];

            SlimeDefinition largoDefinition = ScriptableObject.CreateInstance<SlimeDefinition>();
            largoDefinition.AppearancesDefault = new SlimeAppearance[1];
            largoDefinition.BaseModule = baseDefinition.BaseModule;
            largoDefinition.BaseSlimes = new SlimeDefinition[2]
            {
                baseDefinition,
                addonDefinition
            };
            largoDefinition.CanLargofy = false;
            largoDefinition.Diet = SlimeDiet.Combine(addonDefinition.Diet, baseDefinition.Diet);
            largoDefinition.FavoriteToys = addonDefinition.FavoriteToys.Union(baseDefinition.FavoriteToys).ToArray();
            largoDefinition.IdentifiableId = largoId;
            largoDefinition.IsLargo = false;
            largoDefinition.PrefabScale = 2;
            largoDefinition.SlimeModules = baseDefinition.SlimeModules;
            largoDefinition.Sounds = baseDefinition.Sounds;

            GameObject largoGameobject = PrefabUtils.CopyPrefab(addonObject);
            largoGameobject.transform.localScale = new Vector3(2f, 2f, 2f);
            largoGameobject.GetComponent<PlayWithToys>().slimeDefinition = largoDefinition;
            largoGameobject.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = largoDefinition;
            largoGameobject.GetComponent<SlimeEat>().slimeDefinition = largoDefinition;
            largoGameobject.GetComponent<Identifiable>().id = largoId;
            largoGameobject.GetComponent<Vacuumable>().size = Vacuumable.Size.LARGE;

            foreach (Component component in baseObject.GetComponents(typeof(Component)))
            {
                Type type = component.GetType();
                if (largoGameobject.GetComponent(type) == null)
                {
                    largoGameobject.AddComponent(type).GetCopyOf(component);
                }
            }

            SlimeAppearance largoAppearance;
            if (slime1 == Identifiable.Id.RAD_SLIME)
                largoAppearance = CombineAppearances(Identifiable.Id.PINK_RAD_LARGO, slime2, largoDefinition, largoGameobject, true);
            else if (slime2 == Identifiable.Id.RAD_SLIME)
                largoAppearance = CombineAppearances(slime1, Identifiable.Id.PINK_RAD_LARGO, largoDefinition, largoGameobject, true);
            else
            {
                if (useSecondaryAppearanceAsMain)
                    largoAppearance = CombineAppearances(slime2, slime1, largoDefinition, largoGameobject);
                else
                    largoAppearance = CombineAppearances(slime1, slime2, largoDefinition, largoGameobject);
            }

            LookupRegistry.RegisterIdentifiablePrefab(largoGameobject);
            SlimeRegistry.RegisterSlimeDefinition(largoDefinition);
            Identifiable.LARGO_CLASS.Add(largoId);

            string[] name = largoId.ToString().ToLower().Split('_');
            int i = 0;
            foreach (string namePiece in name)
            {
                name[i] = namePiece[0].ToString().ToUpper() + namePiece.Substring(1);
                i++;
            }
            string finalName = string.Join(" ", name).Replace("Slime", "Largo");

            TranslationPatcher.AddActorTranslation("l." + largoId.ToString().ToLower(), finalName);
            generatedIds.Add(largoId);

            return true;
        }

    }
}

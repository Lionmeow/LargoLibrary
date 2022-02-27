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
        public static Dictionary<string, SlimeAppearanceElement> replaceElements;
        public static string[] dontReplaceMats = new string[]
        {
            "slimeLuckyCatAttach",
            "slimeRadAura"
        };

        private static SlimeDefinitions slimeDefinitions = SRSingleton<GameContext>.Instance.SlimeDefinitions;
        internal static SlimeAppearanceElement defaultBody;

        public static SlimeAppearance CombineAppearances(SlimeDefinition definition, GameObject gameObject, bool swapAppearances = false, SlimeAppearance.AppearanceSaveSet saveSet = SlimeAppearance.AppearanceSaveSet.CLASSIC)
        {
            SlimeAppearance baseAppearance = swapAppearances ?
                slimeDefinitions.GetSlimeByIdentifiableId(definition.BaseSlimes[0].IdentifiableId).AppearancesDefault[0] :
                slimeDefinitions.GetSlimeByIdentifiableId(definition.BaseSlimes[1].IdentifiableId).AppearancesDefault[0];
            SlimeAppearance addonAppearance = swapAppearances ?
                slimeDefinitions.GetSlimeByIdentifiableId(definition.BaseSlimes[1].IdentifiableId).AppearancesDefault[0] :
                slimeDefinitions.GetSlimeByIdentifiableId(definition.BaseSlimes[0].IdentifiableId).AppearancesDefault[0];

            SlimeAppearance largoAppearance = PrefabUtils.DeepCopyObject(baseAppearance) as SlimeAppearance;
            largoAppearance.DependentAppearances = new SlimeAppearance[2]
            {
                baseAppearance,
                addonAppearance
            };
            SlimeAppearanceStructure[] structures = largoAppearance.Structures;
            foreach (SlimeAppearanceStructure slimeAppearanceStructure in structures)
            {
                if (!slimeAppearanceStructure.DefaultMaterials.Any((Material material) => dontReplaceMats.Contains(material.name)))
                    slimeAppearanceStructure.DefaultMaterials = addonAppearance.Structures[0].DefaultMaterials;
            }
            List<SlimeAppearanceStructure> newStructures = structures.ToList();
            int currStructure = 0;
            foreach (SlimeAppearanceStructure structure in addonAppearance.Structures)
            {
                if (currStructure == 0)
                {
                    if (addonAppearance.Structures[0].Element != defaultBody)
                        newStructures[0] = addonAppearance.Structures[0];
                }
                else
                {
                    SlimeAppearanceStructure structure1 = new SlimeAppearanceStructure(structure);
                    if (replaceElements.ContainsKey(structure1.Element.name))
                        structure1.Element = replaceElements[structure1.Element.name];
                    newStructures.Add(structure1);
                }
                currStructure++;
            }
            largoAppearance.Structures = newStructures.ToArray();
            SlimeAppearance.Palette palette = largoAppearance.ColorPalette = addonAppearance.ColorPalette;
            largoAppearance.Face = addonAppearance.Face;
            largoAppearance.Face.OnEnable();

            if (addonAppearance.CrystalAppearance != null)
                largoAppearance.CrystalAppearance = addonAppearance.CrystalAppearance;
            if (addonAppearance.ExplosionAppearance != null)
                largoAppearance.ExplosionAppearance = addonAppearance.ExplosionAppearance;
            if (addonAppearance.GlintAppearance != null)
                largoAppearance.GlintAppearance = addonAppearance.GlintAppearance;
            if (addonAppearance.ShockedAppearance != null)
                largoAppearance.ShockedAppearance = addonAppearance.ShockedAppearance;
            if (addonAppearance.TornadoAppearance != null)
                largoAppearance.TornadoAppearance = addonAppearance.TornadoAppearance;
            if (addonAppearance.VineAppearance != null)
                largoAppearance.VineAppearance = addonAppearance.VineAppearance;
            if (definition.BaseSlimes.Any(x => x.IdentifiableId == Identifiable.Id.QUANTUM_SLIME))
            {
                SlimeAppearance qubit = (SlimeAppearance)PrefabUtils.DeepCopyObject(largoAppearance);
                List<SlimeAppearanceStructure> quantumStructures = new List<SlimeAppearanceStructure>();
                foreach (SlimeAppearanceStructure structure in qubit.Structures)
                {
                    if (structure.DefaultMaterials[0].HasProperty("_TopColor"))
                    {
                        SlimeAppearanceStructure structure1 = new SlimeAppearanceStructure(structure);
                        Material mat = GameObject.Instantiate(Main.LoadResource<Material>("slimeQuantumBase_ghost"));
                        mat.SetColor("_TopColor", structure1.DefaultMaterials[0].GetColor("_TopColor"));
                        mat.SetColor("_MiddleColor", structure1.DefaultMaterials[0].GetColor("_MiddleColor"));
                        mat.SetColor("_BottomColor", structure1.DefaultMaterials[0].GetColor("_BottomColor"));
                        structure1.DefaultMaterials[0] = mat;
                        quantumStructures.Add(structure1);
                    }
                }
                qubit.Structures = quantumStructures.ToArray();
                largoAppearance.QubitAppearance = qubit;
            }

            //SRML.Console.Console.Log((gameObject.GetComponent<SlimeAppearanceApplicator>().Appearance == null).ToString() + $" ({largoDefinition.IdentifiableId})");
            SceneContext.Instance.SlimeAppearanceDirector.RegisterDependentAppearances(definition, largoAppearance);
            gameObject.GetComponent<SlimeAppearanceApplicator>().Appearance = largoAppearance;

            return largoAppearance;
        }

        public static GameObject CreateLargoObject(SlimeDefinition defintion, bool registerObject = true)
        {
            GameObject baseObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(defintion.BaseSlimes[0].IdentifiableId);
            GameObject addonObject = SRSingleton<GameContext>.Instance.LookupDirector.GetPrefab(defintion.BaseSlimes[1].IdentifiableId);

            GameObject gameObject = PrefabUtils.CopyPrefab(baseObject);
            gameObject.transform.localScale = new Vector3(2f, 2f, 2f);
            gameObject.GetComponent<PlayWithToys>().slimeDefinition = defintion;
            gameObject.GetComponent<SlimeAppearanceApplicator>().SlimeDefinition = defintion;
            gameObject.GetComponent<SlimeEat>().slimeDefinition = defintion;
            gameObject.GetComponent<Identifiable>().id = defintion.IdentifiableId;
            gameObject.GetComponent<Vacuumable>().size = Vacuumable.Size.LARGE;
            gameObject.name = "slime" + GenerateLargoName(defintion.IdentifiableId).Replace(" ", string.Empty).Replace("Largo", string.Empty);

            foreach (Component component in addonObject.GetComponents(typeof(Component)))
            {
                Type type = component.GetType();
                if (gameObject.GetComponent(type) == null)
                {
                    gameObject.AddComponent(type).GetCopyOf(component);
                }
            }

            foreach (Transform transform in addonObject.transform)
            {
                if (gameObject.transform.Find(transform.name) == null)
                {
                    GameObject child = addonObject.GetChildCopy(transform.name);
                    child.transform.parent = gameObject.transform;
                    SRML.Console.Console.Log(child.name);
                    SRML.Console.Console.Log(child.transform.parent.name);
                }
            }

            if (registerObject)
                LookupRegistry.RegisterIdentifiablePrefab(gameObject);
            return gameObject;
        }

        public static SlimeDefinition CreateLargoDefinition(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId, bool registerDef = true)
        {
            SlimeDefinition baseDefinition = slimeDefinitions.GetSlimeByIdentifiableId(slime1);
            SlimeDefinition addonDefinition = slimeDefinitions.GetSlimeByIdentifiableId(slime2);

            SlimeDefinition largoDefinition = ScriptableObject.CreateInstance<SlimeDefinition>();
            largoDefinition.AppearancesDefault = new SlimeAppearance[1];
            largoDefinition.BaseModule = baseDefinition.BaseModule;
            largoDefinition.BaseSlimes = new SlimeDefinition[2]
            {
                baseDefinition,
                addonDefinition
            };
            largoDefinition.CanLargofy = false;
            largoDefinition.IsLargo = true;
            largoDefinition.LoadDietFromBaseSlimes();
            largoDefinition.Diet.RefreshEatMap(GameContext.Instance.SlimeDefinitions, largoDefinition);
            largoDefinition.LoadFavoriteToysFromBaseSlimes();
            largoDefinition.IdentifiableId = largoId;
            largoDefinition.PrefabScale = 2;
            largoDefinition.SlimeModules = baseDefinition.SlimeModules;
            largoDefinition.Sounds = baseDefinition.Sounds;

            if (registerDef)
                SlimeRegistry.RegisterSlimeDefinition(largoDefinition);
            return largoDefinition;
        }

        public static GameObject CreateLargo(Identifiable.Id slime1, Identifiable.Id slime2, Identifiable.Id largoId, bool useSecondaryAppearanceAsMain = false, bool generateName = true)
        {
            SlimeDefinition definition = CreateLargoDefinition(slime1, slime2, largoId);
            GameObject gameObject = CreateLargoObject(definition);
            SlimeAppearance appearance = CombineAppearances(definition, gameObject, useSecondaryAppearanceAsMain);
            definition.AppearancesDefault[0] = appearance;
            
            Identifiable.LARGO_CLASS.Add(largoId);

            if (generateName)
            {
                string finalName = GenerateLargoName(largoId);
                definition.Name = finalName.Replace(" Largo", string.Empty);
                TranslationPatcher.AddActorTranslation("l." + largoId.ToString().ToLower(), finalName);
            }

            generatedIds.Add(largoId);
            return gameObject;
        }

        public static string GenerateLargoName(Identifiable.Id id)
        {
            string[] name = id.ToString().ToLower().Split('_');
            int i = 0;
            foreach (string namePiece in name)
            {
                name[i] = namePiece[0].ToString().ToUpper() + namePiece.Substring(1);
                i++;
            }
            return string.Join(" ", name).Replace("Slime", "Largo");
        }
    }
}

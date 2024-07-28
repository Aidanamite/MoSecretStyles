using HarmonyLib;
using SRML;
using SRML.Console;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using DLCPackage;
using SRML.Utils;
using MonomiPark.SlimeRancher.Regions;
using SRML.SR;
using AssetsLib;
using static AssetsLib.TextureUtils;
using static AssetsLib.MeshUtils;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;

namespace MoSecretStyles
{
    class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        internal static Dictionary<Sprite, Sprite> particleReplacements = new Dictionary<Sprite, Sprite>();
        internal static bool started = false;
        internal static System.Tuple<Identifiable.Id, SlimeAppearance> diamondSS;
        internal static Dictionary<ModSecretStyle, GameObject> ssPods = new Dictionary<ModSecretStyle, GameObject>();

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            //Console.RegisterCommand(new CustomCommand());
            TranslationPatcher.AddActorTranslation("t.secret_style_white_hole", "Blue Portal");
            TranslationPatcher.AddActorTranslation("t.secret_style_black_hole", "Orange Portal");
            TranslationPatcher.AddActorTranslation("t.secret_style_singularity", "Portal");
            TranslationPatcher.AddActorTranslation("t.secret_style_bubble", "Aloe Vera");
            TranslationPatcher.AddActorTranslation("t.secret_style_rubber", "Tyre");
            TranslationPatcher.AddActorTranslation("t.secret_style_shadow", "Shine");
            TranslationPatcher.AddActorTranslation("t.secret_style_rainbow", "Noise");
            TranslationPatcher.AddActorTranslation("t.secret_style_salty", "Pepper");
            TranslationPatcher.AddActorTranslation("t.secret_style_ruby", "Greenstone");
            TranslationPatcher.AddActorTranslation("t.secret_style_unstable", "Uranium");
            TranslationPatcher.AddActorTranslation("t.secret_style_ruins", "Moss");
            TranslationPatcher.AddActorTranslation("t.secret_style_ash", "Lava Dust");
            TranslationPatcher.AddActorTranslation("t.secret_style_steam", "Storm Cloud");
            TranslationPatcher.AddActorTranslation("t.secret_style_acid", "Toxic");
            TranslationPatcher.AddActorTranslation("t.secret_style_diamond", "Moissanite");
            TranslationPatcher.AddActorTranslation("t.secret_style_amethyst", "Opal");
            TranslationPatcher.AddActorTranslation("t.secret_style_emerald", "Hematite");
            TranslationPatcher.AddActorTranslation("t.secret_style_garnet", "Topaz");
            TranslationPatcher.AddActorTranslation("t.secret_style_sapphire", "Quartz");
            TranslationPatcher.AddActorTranslation("t.secret_style_cheez", "Blue Cheez");
            TranslationPatcher.AddActorTranslation("t.secret_style_doge", "Husky");
            TranslationPatcher.AddActorTranslation("t.secret_style_calico", "Siamese");
            TranslationPatcher.AddActorTranslation("t.secret_style_splitter", "Cell");
            TranslationPatcher.AddActorTranslation("t.secret_style_flower", "Lavender");
            TranslationPatcher.AddActorTranslation("t.secret_style_bee", "Wax Moth");
        }

        public override void PostLoad()
        {
            if (SRModLoader.IsModPresent("secretstylethings"))
                SSTInteractions.SetupPlorts();
        }
        public override void Load()
        {
            if (!SRModLoader.IsModPresent("assetslib"))
                throw new DllNotFoundException("The AssetsLib mod was not found and is required for this mod to work");
            GameContext.Instance.DLCDirector.onPackageInstalled += style =>
            {
                if (style != Id.SECRET_STYLE)
                    return;
                if (!started)
                {
                    started = true;
                    ModSecretStyle.initializing = true;
                    ModSecretStyle.onSecretStylesInitialization?.Invoke();
                    ModSecretStyle.initializing = false;
                }
                if (started && !Levels.isMainMenu())
                {
                    var podPrefab = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(go => go.name == "treasurePodCosmetic");
                    var cells = Resources.FindObjectsOfTypeAll<Region>();
                    foreach (var ss in ModSecretStyle.allSecretStyles)
                        if (ss.PodId != null && (!ssPods.TryGetValue(ss,out var pod) || !pod))
                        {
                            var cell = cells.FirstOrDefault((x) => x.name == ss.PodCell)?.root;
                            if (!cell)
                            {
                                LogError($"Could not find cell \"{ss.PodCell ?? ""}\" for {ss.Id}'s pod");
                                continue;
                            }
                            pod = podPrefab.CreateInactive();
                            pod.name = pod.name.Replace("(Clone)", "");
                            TreasurePod treasurePod = pod.GetComponent<TreasurePod>();
                            pod.transform.SetParent(cell.transform, true);
                            treasurePod.director = pod.GetComponentInParent<IdDirector>();
                            treasurePod.director.persistenceDict.Add(treasurePod, ss.PodId);
                            treasurePod.unlockedSlimeAppearanceDefinition = ss.Definition;
                            treasurePod.unlockedSlimeAppearance = ss.SecretStyle;
                            pod.transform.position = ss.PodPosition;
                            pod.transform.rotation = ss.PodRotation;
                            pod.SetActive(true);
                            ssPods[ss] = pod;
                        }
                }
            };
            
            ModSecretStyle.onSecretStylesInitialization += () =>
            {
                if (Enum.TryParse("BLACK_HOLE_SLIME",out Identifiable.Id blackHoleSlime))
                {
                    var ss = new ModSecretStyle(blackHoleSlime, new Vector3(1055.1f, 11.6f, 815.1f), Quaternion.Euler(-15, -90, 0), "cellSlimulationReef_Intro", "Portal");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_black_hole";
                    var m = new Material(Identifiable.Id.RAD_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Structures[0].DefaultMaterials[0]);
                    m.name = blackHoleSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Icon = blackHoleSlime.LoadSprite();
                    ss.Definition.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC).Icon = LoadImage(blackHoleSlime.ToObjectName() + ".png").CreateSprite();
                    m.SetColor("_TopColor", new Color(1, 0.5f, 0));
                    m.SetColor("_MiddleColor", Color.black);
                    m.SetColor("_BottomColor", new Color(1, 1, 0));
                    m.SetColor("_GlowTop", Color.black);
                }
                if (Enum.TryParse("WHITE_HOLE_SLIME", out Identifiable.Id whiteHoleSlime))
                {
                    var ss = new ModSecretStyle(whiteHoleSlime, false);
                    ss.SecretStyle.NameXlateKey = "t.secret_style_white_hole";
                    var m = new Material(Identifiable.Id.RAD_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Structures[0].DefaultMaterials[0]);
                    m.name = whiteHoleSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    m.SetColor("_TopColor", new Color(0, 0.5f, 1));
                    m.SetColor("_MiddleColor", Color.black);
                    m.SetColor("_BottomColor", new Color(0, 1, 1));
                    m.SetColor("_GlowTop", Color.black);
                    ss.SecretStyle.Face = Object.Instantiate(blackHoleSlime.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Face);
                }
                if (Enum.TryParse("BUBBLE_SLIME", out Identifiable.Id bubbleSlime))
                {
                    var ss = new ModSecretStyle(bubbleSlime, new Vector3(-157.1f, -3.5f, 436.7f), Quaternion.Euler(-15, 135, 0), "cellMoss_Entrance", "AloeVera");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_bubble";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = bubbleSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(0, 1, 0.5f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", new Color(0.5f, 1,0.75f));
                    m.SetColor("_BottomColor", color.Multiply(0.75f));
                    ss.SecretStyle.Icon = bubbleSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = new Color(0.5f, 1, 0.75f);
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.75f);
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("RUBBER_SLIME", out Identifiable.Id rubberSlime))
                {
                    var ss = new ModSecretStyle(rubberSlime, new Vector3(-50.1f, -3.2f, 227.5f), Quaternion.Euler(-15, 165, 0), "cellReef_TunnelQuarry", "Tyre");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_rubber";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = rubberSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(0.2f, 0.2f, 0.2f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.5f));
                    m.SetColor("_BottomColor", Color.black);
                    ss.SecretStyle.Icon = rubberSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("SHADOW_SLIME", out Identifiable.Id shadowSlime))
                {
                    var ss = new ModSecretStyle(shadowSlime, new Vector3(427, 14.6f, 383.2f), Quaternion.Euler(-15, 0, 0), "cellQuarry_MirrorIsland", "Shine");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_shadow";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = shadowSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(0, 0.4f, 1);
                    m.SetColor("_TopColor", Color.white);
                    m.SetColor("_MiddleColor", color);
                    m.SetColor("_BottomColor", color.Multiply(0.5f));
                    ss.SecretStyle.Icon = shadowSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white;
                    ss.SecretStyle.ColorPalette.Middle = color;
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("RAINBOW_SLIME", out Identifiable.Id rainbowSlime))
                {
                    var ss = new ModSecretStyle(rainbowSlime, new Vector3(-18.6f, 4.3f, 476.6f), Quaternion.Euler(-15, 45, 0), "cellReef_RuinsGardenOfTranquility", "Noise2");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_rainbow";
                    var m = new Material(Identifiable.Id.TABBY_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0]);
                    m.name = rainbowSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    m.SetColor("_TopColor", Color.white);
                    m.SetColor("_MiddleColor", Color.black);
                    m.SetColor("_BottomColor", Color.white);
                    var t = (m.GetTexture("_StripeTexture") as Texture2D).GetReadable();
                    var r = new System.Random();
                    t.ModifyTexturePixels((x) => {
                        var v = (float)r.NextDouble();
                        return new Color(v, 1 - v, 0);
                        });
                    m.SetTexture("_StripeTexture", t);
                    ss.SecretStyle.Icon = rainbowSlime.LoadSprite();
                    ss.Definition.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC).Icon = LoadImage(rainbowSlime.ToObjectName() + ".png").CreateSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white;
                    ss.SecretStyle.ColorPalette.Middle = Color.black;
                    ss.SecretStyle.ColorPalette.Bottom = Color.white;
                    ss.SecretStyle.ColorPalette.Ammo = Color.white.Multiply(0.5f);
                }
                if (Enum.TryParse("SALTY_SLIME", out Identifiable.Id saltySlime))
                {
                    ModSecretStyle ss;
                    if (SRModLoader.IsModPresent("mainmenuisland"))
                        ss = new ModSecretStyle(saltySlime, new Vector3(-68.4f, 6.5f, 293.3f) , Quaternion.Euler(-15, 165, 0), "cellSea_Bridgeway", "Pepper");
                    else
                        ss = new ModSecretStyle(saltySlime, new Vector3(-83.6f, 1.7f, 290.8f), Quaternion.Euler(-15, -105, 0), "cellSea_Bridgeway", "Pepper");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_salty";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = saltySlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = Color.white.Multiply(0.4f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.7f));
                    m.SetColor("_BottomColor", color.Multiply(0.2f));
                    m.SetColor("_CrackColor", Color.black);
                    ss.SecretStyle.Icon = saltySlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.black;
                    ss.SecretStyle.ColorPalette.Middle = Color.black;
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = Color.white.Multiply(0.1f);
                }
                if (Enum.TryParse("RUBY_SLIME", out Identifiable.Id rubySlime))
                {
                    var ss = new ModSecretStyle(rubySlime, new Vector3(490, 6.1f, 174), Quaternion.Euler(-15, 145, 0), "cellQuarry_CrystalVolcano", "Greenstone");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_ruby";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = rubySlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = Color.green.Multiply(0.6f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.7f));
                    m.SetColor("_BottomColor", color.Multiply(0.4f));
                    m.SetColor("_CrackColor", Color.black);
                    ss.SecretStyle.InstatiateFaces();
                    foreach (var e in ss.SecretStyle.Face.ExpressionFaces)
                    {
                        if (e.Mouth)
                        {
                            e.Mouth.SetColor("_MouthTop", e.Mouth.GetColor("_MouthTop").grayscale * color);
                            e.Mouth.SetColor("_MouthMid", e.Mouth.GetColor("_MouthMid").grayscale * color);
                            e.Mouth.SetColor("_MouthBot", e.Mouth.GetColor("_MouthBot").grayscale * color);
                        }
                        if (e.Eyes)
                        {
                            e.Eyes.SetColor("_EyeRed", e.Eyes.GetColor("_EyeRed").grayscale * color);
                            e.Eyes.SetColor("_EyeGreen", e.Eyes.GetColor("_EyeGreen").grayscale * color);
                            e.Eyes.SetColor("_EyeBlue", e.Eyes.GetColor("_EyeBlue").grayscale * color);
                        }
                    }
                    ss.SecretStyle.Icon = rubySlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.7f);
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.4f);
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("UNSTABLE_SLIME", out Identifiable.Id unstableSlime))
                {
                    var ss = new ModSecretStyle(unstableSlime, new Vector3(42.3f, -5.6f, 722.4f), Quaternion.Euler(-15, -135, 0), "cellRuins_CollapsedPlazaGarden", "Uranium");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_unstable";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = unstableSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    var color = new Color(0.2f,0.9f,0);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.7f));
                    m.SetColor("_BottomColor", color.Multiply(0.4f));
                    ss.SecretStyle.Icon = unstableSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.7f);
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.4f);
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("RUINS_SLIME", out Identifiable.Id ruinsSlime))
                {
                    var ss = new ModSecretStyle(ruinsSlime, new Vector3(198.6f, 4.3f, 845), Quaternion.Euler(-15, 45, 0), "cellRuins_BeaconTower", "Moss");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_ruins";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    var m2 = ss.Definition.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.CLASSIC);
                    m.name = ruinsSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var t = (m.GetTexture("_EmissionMap") as Texture2D).GetReadable();
                    var t2 = LoadImage(ruinsSlime.ToObjectName() + "Exotic_moss.png");
                    t.ModifyTexturePixels((j, k, l) =>
                    {
                        var c = t2.GetPixelBilinear(k, l);
                        return new Color(j.r * (1 - c.a) + c.r * c.a, j.g * (1 - c.a) + c.g * c.a, j.b * (1 - c.a) + c.b * c.a, j.a);
                    });
                    m.SetTexture("_EmissionMap", t);
                    ss.SecretStyle.Icon = ruinsSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.green;
                    ss.SecretStyle.ColorPalette.Middle = m2.ColorPalette.Middle;
                    ss.SecretStyle.ColorPalette.Bottom = m2.ColorPalette.Bottom;
                    ss.SecretStyle.ColorPalette.Ammo = m2.ColorPalette.Ammo * 0.6f + Color.green * 0.4f;
                }
                if (Enum.TryParse("ASH_SLIME", out Identifiable.Id ashSlime))
                {
                    var ss = new ModSecretStyle(ashSlime, new Vector3(86.1f, 1005.6f, 252.8f), Quaternion.Euler(-15, 45, 0), "cellDesert_TitanicValley", "LavaDust");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_ash";
                    var m = new Material(Resources.FindObjectsOfTypeAll<Material>().First((j) => j.name == "lavaDust_liquid"));
                    m.name = ashSlime.ToObjectName() + "ExoticBase";
                    m.SetColor("_Color",Color.black);
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Icon = ashSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.yellow;
                    ss.SecretStyle.ColorPalette.Middle = Color.red;
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = Color.yellow * 0.5f + Color.red * 0.5f;
                }
                if (Enum.TryParse("STEAM_SLIME", out Identifiable.Id steamSlime))
                {
                    var ss = new ModSecretStyle(steamSlime, new Vector3(-51.8f, 1025.1f, 647.7f), Quaternion.Euler(-15, -75, 0), "cellDesert_ScorchedPlainsNorthWest", "StormCloud");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_steam";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = steamSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = Color.white;
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.5f));
                    m.SetColor("_BottomColor", Color.black);
                    ss.SecretStyle.Icon = steamSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.5f);
                }
                if (Enum.TryParse("ACID_SLIME", out Identifiable.Id acidSlime))
                {
                    var ss = new ModSecretStyle(acidSlime, new Vector3(360.8f, 9.9f, 175.5f), Quaternion.Euler(-15, -45, 0), "cellQuarry_DonutHole", "Toxic");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_acid";
                    var m = new Material(Identifiable.Id.HONEY_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0]);
                    m.name = acidSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(1, 0, 1);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.5f));
                    m.SetColor("_BottomColor", Color.black);
                    ss.SecretStyle.InstatiateFaces();
                    foreach (var e in ss.SecretStyle.Face.ExpressionFaces)
                    {
                        if (e.Mouth)
                        {
                            e.Mouth.SetColor("_MouthTop", e.Mouth.GetColor("_MouthTop").grayscale * Color.green);
                            e.Mouth.SetColor("_MouthMid", e.Mouth.GetColor("_MouthMid").grayscale * Color.green);
                            e.Mouth.SetColor("_MouthBot", e.Mouth.GetColor("_MouthBot").grayscale * Color.green);
                        }
                        if (e.Eyes)
                        {
                            e.Eyes.SetColor("_EyeRed", e.Eyes.GetColor("_EyeRed").grayscale * Color.green);
                            e.Eyes.SetColor("_EyeGreen", e.Eyes.GetColor("_EyeGreen").grayscale * Color.green);
                            e.Eyes.SetColor("_EyeBlue", e.Eyes.GetColor("_EyeBlue").grayscale * Color.green);
                        }
                    }
                    ss.SecretStyle.Icon = acidSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("DIAMOND_SLIME", out Identifiable.Id diamondSlime))
                {
                    var ss = new ModSecretStyle(diamondSlime, new Vector3(100.1f, 25.8f, 349.4f), Quaternion.Euler(-15, 0, 0), "cellQuarry_NorthernTrail", "Moissanite");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_diamond";
                    var m = new Material(Identifiable.Id.CRYSTAL_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Structures[0].DefaultMaterials[0]);
                    m.name = diamondSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var s = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault((j) => j.name == "slimeShimmer");
                    if (s)
                    {
                        var m2 = new Material(s);
                        m2.name = diamondSlime.ToObjectName() + "ExoticCrown";
                        ss.SecretStyle.Structures[1].DefaultMaterials[0] = m2;
                        ss.SecretStyle.Structures[2].DefaultMaterials[0] = m2;
                    } else
                    {
                        diamondSS = new System.Tuple<Identifiable.Id, SlimeAppearance>(diamondSlime, ss.SecretStyle);
                    }
                    var color = Color.white;
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.9f));
                    m.SetColor("_BottomColor", color.Multiply(0.8f));
                    m.SetColor("_GlitterColor", Color.green);
                    ss.SecretStyle.Icon = diamondSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.5f);
                }
                if (Enum.TryParse("AMETHYST_SLIME", out Identifiable.Id amethystSlime))
                {
                    var ss = new ModSecretStyle(amethystSlime, new Vector3(-213.3f, 1020.8f, 410.5f), Quaternion.Euler(-15, 135, 0), "cellDesert_Bridges", "Opal");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_amethyst";
                    var m = new Material(Identifiable.Id.DERVISH_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Structures[0].DefaultMaterials[0]);
                    m.name = amethystSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[2].DefaultMaterials[0] = m;
                    var color = Color.blue;
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", Color.black);
                    m.SetColor("_BottomColor", Color.black);
                    ss.SecretStyle.Icon = amethystSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.5f);
                }
                if (Enum.TryParse("EMERALD_SLIME", out Identifiable.Id emeraldSlime))
                {
                    var ss = new ModSecretStyle(emeraldSlime, new Vector3(221.1f, -3, 318.5f), Quaternion.Euler(-15, -20, 0), "cellQuarry_OverUnder", "Hematite");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_emerald";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = emeraldSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[2].DefaultMaterials[0] = m;
                    m.SetColor("_TopColor", Color.white);
                    m.SetColor("_MiddleColor", Color.black);
                    m.SetColor("_BottomColor", Color.black);
                    m.SetFloatArray("_Gloss", new float[] { 100 });
                    ss.SecretStyle.Icon = emeraldSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.black;
                    ss.SecretStyle.ColorPalette.Middle = Color.black;
                    ss.SecretStyle.ColorPalette.Bottom = Color.white;
                    ss.SecretStyle.ColorPalette.Ammo = Color.white.Multiply(0.5f);
                }
                if (Enum.TryParse("GARNET_SLIME", out Identifiable.Id garnetSlime))
                {
                    var ss = new ModSecretStyle(garnetSlime, new Vector3(149.9f, 1026.8f, 604.4f), Quaternion.Euler(-15, -135, 0), "cellDesert_ScorchedPlainsNorthEast", "Topaz");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_garnet";
                    var m = new Material(Resources.FindObjectsOfTypeAll<Material>().First((j) => j.name == "indigonium"));
                    m.name = garnetSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[2].DefaultMaterials[0] = m;
                    var color = new Color(1,0.75f,0);
                    for (int k = 0; k < 8; k++)
                    {
                        var j = m.GetColor("_Color" + k + 0);
                        m.SetColor("_Color" + k + 0, (1-(1-j.grayscale) / 2) * new Color(color.r, color.g, color.b, j.a));
                        j = m.GetColor("_Color" + k + 1);
                        m.SetColor("_Color" + k + 1, (1-(1-j.grayscale) / 2) * new Color(color.r, color.g, color.b, j.a));
                    }
                    m.SetTexture("_ColorMask", LoadImage(garnetSlime.ToObjectName() + "Exotic_pattern.png"));
                    ss.SecretStyle.Icon = garnetSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white * 0.4f + color * 0.6f;
                    ss.SecretStyle.ColorPalette.Middle = color;
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.7f);
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.9f);
                }
                if (Enum.TryParse("SAPPHIRE_SLIME", out Identifiable.Id sapphireSlime))
                {
                    var ss = new ModSecretStyle(sapphireSlime, new Vector3(115.5f, -4, 141.7f), Quaternion.Euler(-15, -115, 0), "cellQuarry_Entrance", "Quartz");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_sapphire";
                    var m = new Material(Resources.FindObjectsOfTypeAll<Material>().First((j) => j.name == "strangeDiamond"));
                    m.name = sapphireSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[2].DefaultMaterials[0] = m;
                    var color = new Color(0.8f, 0.9f, 1);
                    for (int k = 0; k < 8; k++)
                    {
                        var j = m.GetColor("_Color" + k + 0);
                        m.SetColor("_Color" + k + 0, (1 - (1 - j.grayscale) / 2) * new Color(color.r, color.g, color.b, j.a));
                        j = m.GetColor("_Color" + k + 1);
                        m.SetColor("_Color" + k + 1, (1 - (1 - j.grayscale) / 2) * new Color(color.r, color.g, color.b, j.a));
                    }
                    m.SetTexture("_ColorMask", LoadImage(sapphireSlime.ToObjectName() + "Exotic_pattern.png"));
                    ss.SecretStyle.Icon = sapphireSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white * 0.4f + color * 0.6f;
                    ss.SecretStyle.ColorPalette.Middle = color;
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.7f);
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.9f);
                }
                if (Enum.TryParse("CHEEZ_SLIME", out Identifiable.Id cheezSlime))
                {
                    var ss = new ModSecretStyle(cheezSlime, new Vector3(-303, -4.8f, 806.6f), Quaternion.Euler(-15, -90, 0), "cellMoss_Archipelago", "BlueCheez");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_cheez";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = cheezSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(1, 1, 0.5f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color);
                    m.SetColor("_BottomColor", color);
                    ss.SecretStyle.InstatiateFaces();
                    foreach (var e in ss.SecretStyle.Face.ExpressionFaces)
                    {
                        if (e.Mouth)
                        {
                            e.Mouth.SetColor("_MouthTop", Color.blue.Multiply(e.Mouth.GetColor("_MouthTop").grayscale));
                            e.Mouth.SetColor("_MouthMid", Color.blue.Multiply(e.Mouth.GetColor("_MouthMid").grayscale));
                            e.Mouth.SetColor("_MouthBot", Color.blue.Multiply(e.Mouth.GetColor("_MouthBot").grayscale));
                        }
                        if (e.Eyes)
                        {
                            e.Eyes.SetColor("_EyeRed", Color.blue.Multiply(e.Eyes.GetColor("_EyeRed").grayscale));
                            e.Eyes.SetColor("_EyeGreen", Color.blue.Multiply(e.Eyes.GetColor("_EyeGreen").grayscale));
                            e.Eyes.SetColor("_EyeBlue",  Color.blue.Multiply(e.Eyes.GetColor("_EyeBlue").grayscale));
                        }
                    }
                    ss.SecretStyle.Icon = cheezSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color;
                    ss.SecretStyle.ColorPalette.Bottom = color;
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("DOGE_SLIME", out Identifiable.Id dogeSlime))
                {
                    var ss = new ModSecretStyle(dogeSlime, new Vector3(-499.5f, -2, -165.2f), Quaternion.Euler(-15, -135, 0), "cellReef_RingIsland", "Husky");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_doge";
                    var m = new Material(Identifiable.Id.SABER_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0]);
                    m.name = dogeSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    m.SetColor("_TopColor", Color.white);
                    m.SetColor("_MiddleColor", Color.gray);
                    m.SetColor("_BottomColor", Color.black);
                    m.SetTexture("_StripeTexture", LoadImage(dogeSlime.ToObjectName() + "Exotic_pattern.png"));
                    m.SetTextureScale("_texcoord", new Vector2(0.5f, 1));
                    ss.SecretStyle.Icon = dogeSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white;
                    ss.SecretStyle.ColorPalette.Middle = Color.gray;
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = Color.gray;
                }
                if (Enum.TryParse("CALICO_SLIME", out Identifiable.Id calicoSlime))
                {
                    var ss = new ModSecretStyle(calicoSlime, new Vector3(141.3f, 17.1f, 46.8f), Quaternion.Euler(-15,115,0), "cellReef_CliffCaves", "Siamese");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_calico";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = calicoSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    m.SetColor("_TopColor", Color.white);
                    m.SetColor("_MiddleColor", Color.white.Multiply(0.3f));
                    m.SetColor("_BottomColor", Color.black);
                    m.SetTexture("_StripeTexture", LoadImage(calicoSlime.ToObjectName() + "Exotic_pattern.png"));
                    m.SetTextureScale("_texcoord", new Vector2(0.5f, 1));
                    ss.SecretStyle.Icon = calicoSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = Color.white;
                    ss.SecretStyle.ColorPalette.Middle = Color.white.Multiply(0.3f);
                    ss.SecretStyle.ColorPalette.Bottom = Color.black;
                    ss.SecretStyle.ColorPalette.Ammo = Color.gray;
                }
                if (Enum.TryParse("SPLITTER_SLIME", out Identifiable.Id splitterSlime))
                {
                    var ss = new ModSecretStyle(splitterSlime, new Vector3(89.6f, 24.2f, 815.3f), Quaternion.Euler(-15, -135, 0), "cellRuins_Canal", "Cell");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_splitter";
                    var m = new Material(Identifiable.Id.PHOSPHOR_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0]);
                    m.name = splitterSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(0.2f, 0.7f, 1);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color);
                    m.SetColor("_BottomColor", color);
                    m.SetColor("_GlowTop", new Color(0.75f, 0.15f, 0));
                    m.SetFloatArray("_GlowMin", new float[] { 2 });
                    m.SetFloatArray("_GlowMax", new float[] { 2 });
                    ss.SecretStyle.Icon = splitterSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = new Color(0.5f, 0.25f, 0);
                    ss.SecretStyle.ColorPalette.Bottom = color;
                    ss.SecretStyle.ColorPalette.Ammo = color;
                }
                if (Enum.TryParse("FLOWER_SLIME", out Identifiable.Id flowerSlime))
                {
                    var ss = new ModSecretStyle(flowerSlime, new Vector3(-402.5f, 4.3f, 352.8f), Quaternion.Euler(-15, -100, 0), "cellMoss_HoneyPerch", "Lavender");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_flower";
                    var m = new Material(Identifiable.Id.PINK_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0]);
                    m.name = flowerSlime.ToObjectName() + "ExoticBase";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    var color = new Color(0, 0.8f, 0);
                    m.SetColor("_TopColor", Color.black);
                    m.SetColor("_MiddleColor", color);
                    m.SetColor("_BottomColor", color.Multiply(0.8f));
                    ss.SecretStyle.Icon = flowerSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.8f);
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.5f);
                    ss.SecretStyle.ColorPalette.Ammo = color;

                    var prefab = Identifiable.Id.PUDDLE_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE).Structures[1];
                    var flower = new GameObject("LavenderFlower", typeof(MeshRenderer), typeof(MeshFilter), typeof(SlimeAppearanceObject));
                    var mat = new Material(prefab.DefaultMaterials[0]);
                    mat.name = flowerSlime.ToObjectName() + "ExoticLavender";
                    var mesh = prefab.Element.Prefabs[1].GetComponent<MeshFilter>().mesh;
                    var multi = 4;
                    var points = new Func<Vector3, Vector3>[multi];
                    for (int j = 0; j < points.Length; j++)
                    {
                        var i = j;
                        if (j % 2 == 0)
                            points[j] = (k) => k + new Vector3(0, -1 + 0.1f * i, 0);
                        else
                            points[j] = (k) => (k + new Vector3(0, -1 + 0.1f * i, 0)).Rotate(0,30,0);
                    }
                    mesh = CreateMesh(mesh.vertices, mesh.uv, mesh.triangles, points.ToArray());
                    mesh.name = "LavenderFlower";
                    color = new Color(1, 0, 1);
                    mat.SetColor("_RedMask", color.Multiply(0.2f));
                    mat.SetColor("_GreenMask", Color.black);
                    mat.SetColor("_BlueMask", color.Multiply(0.5f));
                    flower.GetComponent<MeshRenderer>().sharedMaterial = mat;
                    flower.GetComponent<MeshFilter>().sharedMesh = mesh;
                    var apObj = flower.GetComponent<SlimeAppearanceObject>();
                    apObj.IgnoreLODIndex = true;
                    apObj.ParentBone = SlimeAppearance.SlimeBone.JiggleTop;
                    var ns = ss.SecretStyle.Structures[1];
                    ns.DefaultMaterials = new Material[] { mat };
                    ns.Element = ScriptableObject.CreateInstance<SlimeAppearanceElement>();
                    ns.Element.Name = "LavenderFlower";
                    ns.Element.Prefabs = new SlimeAppearanceObject[] { PrefabUtils.CopyPrefab(flower).GetComponent<SlimeAppearanceObject>() };
                    ns.Element.Prefabs[0].name = ns.Element.Prefabs[0].name.Replace("(Clone)","");
                    ns.ElementMaterials = new SlimeAppearanceMaterials[] { new SlimeAppearanceMaterials() { OverrideDefaults = false } };
                    ns.SupportsFaces = false;
                    GameObject.DestroyImmediate(flower.gameObject);
                }
                if (Enum.TryParse("BEE_SLIME", out Identifiable.Id beeSlime))
                {
                    var ss = new ModSecretStyle(beeSlime, new Vector3(-433.3f, 6.2f, 644.8f), Quaternion.Euler(-15, 110, 0), "cellMoss_Coast", "WaxMoth");
                    ss.SecretStyle.NameXlateKey = "t.secret_style_bee";
                    var m = new Material(ss.SecretStyle.Structures[0].DefaultMaterials[0]);
                    m.name = beeSlime.ToObjectName() + "Exotic";
                    ss.SecretStyle.Structures[0].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[1].DefaultMaterials[0] = m;
                    ss.SecretStyle.Structures[2].DefaultMaterials[0] = m;
                    var color = new Color(1, 0.7f, 0.5f);
                    m.SetColor("_TopColor", color);
                    m.SetColor("_MiddleColor", color.Multiply(0.6f));
                    m.SetColor("_BottomColor", color.Multiply(0.3f));
                    m.SetTexture("_StripeTexture", LoadImage(beeSlime.ToObjectName() + "Exotic_pattern.png"));
                    ss.SecretStyle.Icon = beeSlime.LoadSprite();
                    ss.SecretStyle.ColorPalette.Top = color;
                    ss.SecretStyle.ColorPalette.Middle = color.Multiply(0.6f);
                    ss.SecretStyle.ColorPalette.Bottom = color.Multiply(0.3f);
                    ss.SecretStyle.ColorPalette.Ammo = color.Multiply(0.6f);
                }
            };
            SceneContext.Instance.SlimeAppearanceDirector.onSlimeAppearanceChanged += (x, y) =>
            {
                var flag = y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE;
                if (x.IdentifiableId.ToString() == "WHITE_HOLE_SLIME")
                {
                    var c = flag ? new Color(0, 0.5f, 1) : Color.white;
                    var cg = new ColorGroup();
                    cg.AddColor(new Color(c.r, c.g, c.b, 0),0);
                    cg.AddColor(c,0.5f);
                    cg.AddColor(new Color(c.r, c.g, c.b, 0),1);
                    foreach (var ident in Resources.FindObjectsOfTypeAll<Identifiable>())
                    {
                        if (ident.id == x.IdentifiableId)
                        {
                            foreach (var p in ident.GetComponentsInChildren<ParticleSystem>(true))
                            {
                                var s = p.textureSheetAnimation;
                                for (int j = 0; j < s.spriteCount; j++)
                                {
                                    var sp = s.GetSprite(j);
                                    if (!sp)
                                        continue;
                                    if (flag ? particleReplacements.TryGetValue(sp, out var r) : particleReplacements.TryFind((l) => l.Value == sp, (l) => l.Key, out r))
                                    {
                                        s.SetSprite(j, r);
                                        continue;
                                    }
                                    if (!flag)
                                        continue;
                                    r = sp.texture.GetReadable().CreateSprite();
                                    r.texture.ModifyTexturePixels((z, i, k) => cg.GetColor(i) * cg.GetColor(k));
                                    s.SetSprite(j, r);
                                    particleReplacements.Add(sp, r);
                                }
                                var m1 = p.main;
                                m1.startColor = new ParticleSystem.MinMaxGradient(c);
                                var m2 = p.colorBySpeed;
                                m2.color = new ParticleSystem.MinMaxGradient(c);
                                var m3 = p.colorOverLifetime;
                                m2.color = new ParticleSystem.MinMaxGradient(c);
                            }
                        }
                    }
                }
                if (x.IdentifiableId.ToString() == "RUBY_SLIME")
                {
                    var c = flag ? new Color(0, 0.6f, 0) : Color.red;
                    foreach (var ident in Resources.FindObjectsOfTypeAll<Identifiable>())
                    {
                        if (ident.id == x.IdentifiableId)//ident.id.LargoContains(x.IdentifiableId))
                        {
                            var p = ident.transform.Find("particleGem(Clone)").GetComponent<ParticleSystem>();
                            var s = p.textureSheetAnimation;
                            for (int j = 0; j < s.spriteCount; j++)
                            {
                                var sp = s.GetSprite(j);
                                if (!sp)
                                    continue;
                                if (flag ? particleReplacements.TryGetValue(sp, out var r) : particleReplacements.TryFind((l) => l.Value == sp, (l) => l.Key, out r))
                                {
                                    s.SetSprite(j, r);
                                    continue;
                                }
                                if (!flag)
                                    continue;
                                r = sp.texture.GetReadable().CreateSprite();
                                r.texture.ModifyTexturePixels((z, i, k) => z * c);// cg.GetColor(i) * cg.GetColor(k));
                                s.SetSprite(j, r);
                                particleReplacements.Add(sp, r);
                            }
                            var m1 = p.main;
                            m1.startColor = new ParticleSystem.MinMaxGradient(c);
                            var m2 = p.colorBySpeed;
                            m2.color = new ParticleSystem.MinMaxGradient(c);
                            var m3 = p.colorOverLifetime;
                            m2.color = new ParticleSystem.MinMaxGradient(c);
                        }
                    }
                }
                if (x.IdentifiableId.ToString() == "BLACK_HOLE_SLIME" && Enum.TryParse("WHITE_HOLE_SLIME",out Identifiable.Id whiteHoleSlime))
                {
                    var dir = SceneContext.Instance.SlimeAppearanceDirector;
                    var def = dir.SlimeDefinitions.GetSlimeByIdentifiableId(whiteHoleSlime);
                    var app = def.GetAppearanceForSet(y.SaveSet);
                    dir.UpdateChosenSlimeAppearance(def, app);
                }
            };
        }
        public static void LogError(object message) => Debug.LogError($"[{modName}]: " + message);
    }

    static class ExtentionMethods
    {
        public static string ToObjectName(this Identifiable.Id id)
        {
            var s = id.ToString().ToLowerInvariant().Split('_').ToList();
            s.Insert(0,s[s.Count - 1]);
            s.RemoveAt(s.Count - 1);
            for (int i = 1; i < s.Count; i++)
                s[i] = char.ToUpperInvariant(s[i][0]) + s[i].Remove(0, 1);
            return s.Join(delimiter: "");
        }

        static Dictionary<string, bool> LargoCache = new Dictionary<string, bool>();
        public static bool LargoContains(this Identifiable.Id id, Identifiable.Id slime)
        {
            if (LargoCache.TryGetValue(id.ToString() + " " + slime.ToString(), out var r))
                return r;
            if (Identifiable.IsLargo(id))
            {
                var checks = new List<Identifiable.Id>();
                var changes = 1;
                while (changes > 0)
                {
                    changes = 0;
                    foreach (var i in GameContext.Instance.SlimeDefinitions.largoDefinitionByBaseDefinitions)
                        if (i.Value.IdentifiableId == id || checks.Exists((z) => z == i.Value.IdentifiableId))
                        {
                            if (checks.Contains(i.Key.SlimeDefinition1.IdentifiableId))
                            {
                                if (i.Key.SlimeDefinition1.IdentifiableId == slime) { r = true; goto end; }
                                changes++;
                                checks.Add(i.Key.SlimeDefinition1.IdentifiableId);
                            }
                            if (checks.Contains(i.Key.SlimeDefinition2.IdentifiableId))
                            {
                                if (i.Key.SlimeDefinition2.IdentifiableId == slime) { r = true; goto end; }
                                changes++;
                                checks.Add(i.Key.SlimeDefinition2.IdentifiableId);
                            }
                        }
                }
            }
            r = id == slime;
        end:
            LargoCache.Add(id.ToString() + " " + slime.ToString(), r);
            return r;
        }
        internal static Sprite LoadSprite(this Identifiable.Id id) => LoadImage(id.ToObjectName() + "Exotic.png").CreateSprite();

        public static void InstatiateFaces(this SlimeAppearance appearance)
        {
            var face = appearance.Face = Object.Instantiate(appearance.Face);
            for (int i = 0; i < face.ExpressionFaces.Length; i++)
            {
                var f = face.ExpressionFaces[i];
                if (f.Mouth)
                    f.Mouth = Object.Instantiate(f.Mouth);
                if (f.Eyes)
                    f.Eyes = Object.Instantiate(f.Eyes);
                face.ExpressionFaces[i] = f;
            }
            face.OnEnable();
        }
    }

    [SRML.Utils.Enum.EnumHolder]
    static class Ids
    {
        public static PediaDirector.Id RAINBOW_SLIME;
    }

    [HarmonyPatch(typeof(PediaDirector), "GetPediaId")]
    class Patch_GetPediaId
    {
        static void Postfix(Identifiable.Id identId, ref PediaDirector.Id? __result)
        {
            if (identId.ToString() == "RAINBOW_SLIME")
                __result = Ids.RAINBOW_SLIME;
        }
    }

    public class ModSecretStyle
    {
        internal static List<ModSecretStyle> allSecretStyles = new List<ModSecretStyle>();
        public delegate void SecretStylesInit();
        public static SecretStylesInit onSecretStylesInitialization;
        internal static bool initializing;
        public readonly Identifiable.Id Id;
        public readonly Vector3 PodPosition;
        public readonly Quaternion PodRotation;
        public readonly string PodId;
        public readonly string PodCell;
        public readonly SlimeDefinition Definition;
        public readonly SlimeAppearance SecretStyle;
        public bool ShowInSSMenu;
        public ModSecretStyle(Identifiable.Id slimeId, Vector3 podSpawnPosition, Quaternion podSpawnRotation, string podSpawnCell, string podId, bool showInSSMenu = true)
        {
            if (!initializing)
                throw new UnauthorizedAccessException("new ModSecretStyle instances can only be created during the secret styles initalization. Please use the ModSecretStyle.onSecretStylesInitialization event instead");
            if (!Identifiable.IsSlime(slimeId))
                throw new ArgumentException("Secret styles can only for slimes. " + slimeId + " is not a slime", "slimeId");
            if (allSecretStyles.Exists((x) => x.Id == slimeId))
                throw new ArgumentException("A secret style has already been created for " + slimeId, "slimeId");
            if (string.IsNullOrEmpty(podId))
                podId = null;
            if (podId != null && allSecretStyles.Exists((x) => x.PodId == podId))
                throw new ArgumentException("A secret style has already been created for " + podId, "podId");
            Id = slimeId;
            Definition = SceneContext.Instance.SlimeAppearanceDirector.SlimeDefinitions.GetSlimeByIdentifiableId(slimeId);
            if (Definition.Appearances.Any((x) => x.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE))
                throw new ArgumentException("A secret style already exists for " + slimeId, "slimeId");
            SecretStyle = Object.Instantiate(Definition.AppearancesDefault[0]);
            SecretStyle.SaveSet = SlimeAppearance.AppearanceSaveSet.SECRET_STYLE;
            SecretStyle.name = slimeId.ToObjectName() + "AppearanceExotic";
            Definition.AppearancesDynamic.Add(SecretStyle);
            ShowInSSMenu = showInSSMenu;
            if (podId != null)
            {
                PodId = SRML.SR.SaveSystem.ModdedStringRegistry.ClaimID("pod", podId);
                DLCDirector.SECRET_STYLE_TREASURE_PODS.Add(PodId);
                PodCell = podSpawnCell;
                PodPosition = podSpawnPosition;
                PodRotation = podSpawnRotation;
            }
            else
                PodId = null;
            allSecretStyles.Add(this);
        }
        public ModSecretStyle(Identifiable.Id slimeId, bool showInSSMenu = true) : this(slimeId, default, default, null, null, showInSSMenu) { }
    }

    [HarmonyPatch(typeof(SlimeAppearanceUI), "ShouldShowSlimeInList")]
    class Patch_ShouldShowSlimeInList
    {
        static void Postfix(SlimeDefinition slime, ref bool __result)
        {
            var mss = ModSecretStyle.allSecretStyles.FirstOrDefault((x) => x.Definition == slime);
            if (mss != null)
                __result = mss.ShowInSSMenu;
            if (__result && SceneContext.Instance.PediaDirector.GetPediaId(slime.IdentifiableId) == null)
            {
                __result = false;
                Debug.LogWarning($"[SlimeAppearanceUI]: Slime {slime.IdentifiableId} does not have a registered pedia Id");
            }
        }
    }

    [HarmonyPatch(typeof(SceneContext),"Start")]
    class Patch_SceneStart
    {
        static void Prefix()
        {
            if (Main.diamondSS != null)
            {
                var s = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault((j) => j.name == "slimeShimmer");
                if (s)
                {
                    var m = new Material(s);
                    m.name = Main.diamondSS.Item1.ToObjectName() + "ExoticCrown";
                    Main.diamondSS.Item2.Structures[1].DefaultMaterials[0] = m;
                    Main.diamondSS.Item2.Structures[2].DefaultMaterials[0] = m;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SlimeAppearancePopupUI), "OnBundleAvailable")]
    class Patch_AppearancePopup
    {
        static void Postfix(SlimeAppearancePopupUI __instance, MessageDirector messageDirector)
        {
            if (ModSecretStyle.allSecretStyles.Exists((x) => __instance.idEntry == x.SecretStyle && x.Id.ToString() == "BLACK_HOLE_SLIME"))
                __instance.appearanceName.text = messageDirector.GetBundle("actor").Get("t.secret_style_singularity");
        }
    }

    [HarmonyPatch(typeof(SlimeAppearanceUI), "CreateButton")]
    class Patch_AppearanceUICreateButton
    {
        public static bool calling = false;
        static void Prefix() => calling = true;
        static void Postfix() => calling = false;
    }

    [HarmonyPatch(typeof(SlimeAppearanceUI), "Select")]
    class Patch_AppearanceUISelect
    {
        static void Postfix(SlimeAppearanceUI __instance, SlimeDefinition slime)
        {
            if (slime.IdentifiableId.ToString() == "BLACK_HOLE_SLIME")
                __instance.appearanceTwoNameText.SetKey(MessageUtil.Qualify("actor", "t.secret_style_singularity"));
        }
    }

    [HarmonyPatch(typeof(Identifiable), "GetName")]
    class Patch_GetIdentifiableName
    {
        static void Postfix(Identifiable.Id id, ref string __result)
        {
            if (Patch_AppearanceUICreateButton.calling && id.ToString() == "BLACK_HOLE_SLIME")
                __result = GameContext.Instance.MessageDirector.GetBundle("pedia").Get("t.singularity_slimes");
        }
    }


    /*class CustomCommand : ConsoleCommand { 
        public override string Usage => "activateallss";
        public override string ID => "activateallss";
        public override string Description => "just an example command";
        public override bool Execute(string[] args)
        {
            foreach (var s in Identifiable.SLIME_CLASS)
            {
                var def = GameContext.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(s);
                var a = def.GetAppearanceForSet(SlimeAppearance.AppearanceSaveSet.SECRET_STYLE);
                if (a)
                    SceneContext.Instance.SlimeAppearanceDirector.UpdateChosenSlimeAppearance(def, a);
            }
            return true;
        }
    }*/

    class SSTInteractions
    {
        public static void SetupPlorts()
        {
            string[] items = "FLOWER_PLORT|CALICO_PLORT|CHEEZ_PLORT|DOGE_PLORT|SPLITTER_PLORT|UNSTABLE_PLORT|RUBY_PLORT|SHADOW_PLORT|SALTY_PLORT|RUBBER_PLORT|DIAMOND_PLORT|AMETHYST_PLORT|EMERALD_PLORT|SAPPHIRE_PLORT|BUBBLE_PLORT|ACID_PLORT|STEAM_PLORT|ASH_PLORT".Split('|');
            foreach (var item in items)
                if (Enum.TryParse(item, out Identifiable.Id plort))
                    Secret_Style_Things.Utils.SlimeUtils.SecretStyleData.Add(plort, new Secret_Style_Things.Utils.SecretStyleData(plort.LoadSprite()));
            
            if (Enum.TryParse("FLOWER_PLORT",out Identifiable.Id flowerPlort))
                Secret_Style_Things.Utils.SlimeUtils.ExtraApperanceApplicators.Add(flowerPlort, (x,y) => {
                    var flower = x.Find("tangleFlower_LOD1").GetComponent<MeshRenderer>();
                    if (y.SaveSet == SlimeAppearance.AppearanceSaveSet.SECRET_STYLE)
                    {
                        if (flower.enabled)
                        {
                            flower.enabled = false;
                            var lavender = new GameObject("LavenderFlower", typeof(MeshFilter), typeof(MeshRenderer)).transform;
                            lavender.SetParent(x, false);
                            lavender.localPosition = new Vector3(0, 0.9f, 0);
                            lavender.GetComponent<MeshFilter>().sharedMesh = y.Structures[1].Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                            lavender.GetComponent<MeshRenderer>().sharedMaterial = y.Structures[1].DefaultMaterials[0];
                        }
                    } else
                    {
                        flower.enabled = true;
                        if (x.Find("LavenderFlower"))
                            Object.DestroyImmediate(x.Find("LavenderFlower").gameObject);
                    }
                });
        }
    }
}
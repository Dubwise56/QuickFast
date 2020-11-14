using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace QuickFast
{
    public class Settings : ModSettings
    {
        private static HashSet<HairDef> _hairfilter;

        public static HashSet<HairDef> hairfilter
        {
            get
            {
                if (_hairfilter == null)
                {
                    _hairfilter = new HashSet<HairDef>();
                    if (DefToStrings == null)
                    {
                        DefToStrings = new List<string>();
                    }
                    foreach (var defToString in DefToStrings)
                    {
                        var foo = DefDatabase<HairDef>.GetNamed(defToString);
                        if (foo != null)
                        {
                            _hairfilter.Add(foo);
                        }
                    }
                }

                return _hairfilter;
            }
        }

        public static float EquipModPC = 0.2f;
        public static int EquipModTicks = 10;
        public static bool FlatRate = true;
        public static bool HatsSleeping = true;
        public static bool HideHats = true;
        public static bool HideJackets = true;
        public static bool HatsOnlyWhileDrafted = false;
        public static bool HideHairUnderHats = true;
        public static bool ChangeEquipSpeed = true;
        private string buf;
        private Listing_Standard listing_Standard;

        public void DoWindowContents(Rect canvas)
        {
            Rect nifta = canvas.ContractedBy(40f);
            listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (nifta.width - 40f) / 2f;

            listing_Standard.Begin(canvas.ContractedBy(60f));

            listing_Standard.GapLine();
            listing_Standard.Label("Apparel visibility");
            listing_Standard.CheckboxLabeled("Hide hats when sleeping", ref HatsSleeping);
            listing_Standard.CheckboxLabeled("Hide hats when indoors", ref HideHats);
            listing_Standard.CheckboxLabeled("Hide jackets when indoors", ref HideJackets);
            listing_Standard.CheckboxLabeled("Hats only while drafted", ref HatsOnlyWhileDrafted);
            listing_Standard.CheckboxLabeled("Hide hair under hats", ref HideHairUnderHats);
            GUI.color = Color.green;
            listing_Standard.Label("Press Ctrl + H while pawns are selected to show or hide their hairstyle");
            GUI.color = Color.white;
            listing_Standard.GapLine();
            listing_Standard.Label("Apparel equip speed");
            listing_Standard.CheckboxLabeled("Change equip speeds", ref ChangeEquipSpeed);
            if (ChangeEquipSpeed)
            {
                listing_Standard.CheckboxLabeled("Same speed for all apparel", ref FlatRate);

                if (FlatRate)
                {
                    listing_Standard.LabelDouble("Equip speed Ticks", $"{EquipModTicks} ticks");
                    listing_Standard.IntEntry(ref EquipModTicks, ref buf);
                }
                else
                {
                    listing_Standard.LabelDouble("Equip speed %", $"{EquipModPC.ToStringPercent()}");
                    EquipModPC = listing_Standard.Slider(EquipModPC, 0, 1f);
                }
            }


            listing_Standard.End();
        }

        public static List<string> DefToStrings = new List<string>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ChangeEquipSpeed, "ChangeEquipSpeed");
            Scribe_Values.Look(ref HatsOnlyWhileDrafted, "HatsOnlyWhileDrafted");
            Scribe_Values.Look(ref HideHairUnderHats, "HideHairUnderHats");
            Scribe_Values.Look(ref FlatRate, "FlatRate");
            Scribe_Values.Look(ref HideHats, "HatsIndoors");
            Scribe_Values.Look(ref HatsSleeping, "HatsSleeping");
            Scribe_Values.Look(ref EquipModPC, "EquipModPC");
            Scribe_Values.Look(ref EquipModTicks, "EquipModTicks");
            Scribe_Values.Look(ref HideJackets, "HideJackets");
            Scribe_Collections.Look(ref DefToStrings, "hairFilter", LookMode.Value);
        }
    }


    [HarmonyPatch(typeof(UIRoot_Play))]
    [HarmonyPatch(nameof(UIRoot_Play.UIRootUpdate))]
    public static class h_UIRootOnGUI
    {
        public static void Postfix()
        {

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.H))
            {
                foreach (var pawn in Find.Selector.SelectedObjects.OfType<Pawn>())
                {
                    if (Settings.hairfilter.Contains(pawn.story.hairDef))
                    {
                        Settings.hairfilter.Remove(pawn.story.hairDef);
                        Log.Warning($"Removed {pawn.story.hairDef.defName} from hair filter");
                    }
                    else
                    {
                        Settings.hairfilter.Add(pawn.story.hairDef);
                        Log.Warning($"Added {pawn.story.hairDef.defName} to hair filter");
                    }

                }
                Settings.DefToStrings = new List<string>();
                foreach (var s in Settings.hairfilter)
                {
                    Settings.DefToStrings.Add(s.defName);
                }
                QuickFast.Settings.Write();
            }
        }
    }


    public class QuickFast : Mod
    {
        public QuickFast(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("QuickFast");
            harmony.PatchAll();

            Settings = base.GetSettings<Settings>();

            MethodInfo b__0 = null;
            MethodInfo b__2 = null;

            foreach (var type in GenTypes.AllTypes)
            {
                foreach (var methodInfo in type.GetMethods(AccessTools.all))
                {
                    if (methodInfo.Name.Contains("<LayDown>b__0"))
                    {
                        b__0 = methodInfo;
                    }

                    if (methodInfo.Name.Contains("<LayDown>b__2"))
                    {
                        b__2 = methodInfo;
                    }

                    if (b__0 != null && b__2 != null)
                    {
                        break;
                    }
                }
            }

            if (b__0 != null && b__2 != null)
            {
                var Prefix = new HarmonyMethod(typeof(QuickFast).GetMethod("Prefix_0"));
                harmony.Patch(b__0, Prefix);

                Prefix = new HarmonyMethod(typeof(QuickFast).GetMethod("Prefix_2"));
                harmony.Patch(b__2, Prefix);
            }
        }

        public static Settings Settings;

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Quick Fast";
        }

        public static void Prefix_0(object __instance)
        {
            if (!Settings.HatsSleeping)
            {
                return;
            }

            var toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;
            var bed = toil.actor.CurrentBed();
            if (bed != null && toil.actor.RaceProps.Humanlike && !bed.def.building.bed_showSleeperBody)
            {
                toil.actor.Drawer.renderer.graphics.ClearCache();
                toil.actor.Drawer.renderer.graphics.apparelGraphics.Clear();
            }
        }

        public static void Prefix_2(object __instance)
        {
            if (!Settings.HatsSleeping)
            {
                return;
            }

            var toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;

            if (toil.actor.RaceProps.Humanlike)
            {
                toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnInternal), typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool))]
    public static class Patch_RenderPawnInternal
    {

        public static Vector3 offset(Vector3 vec)
        {
            if (!Settings.HideHairUnderHats)
            {
                vec.y += -0.0036f;
            }
            return vec;
        }

        public static FieldInfo HideHairUnderHats = AccessTools.Field(typeof(Settings), "HideHairUnderHats");

        public static MethodInfo m_offset = AccessTools.Method(typeof(Patch_RenderPawnInternal), "offset");

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found = false;
            var foudnb = false;
            var struc = instructions.ToList();
            for (var index = 0; index < struc.Count; index++)
            {
                var instruction = struc[index];
                if (foudnb is false && instruction.opcode == OpCodes.Ldloc_S && struc[index + 1].opcode == OpCodes.Brtrue_S && struc[index + 2].opcode == OpCodes.Ldarg_S)
                {
                    foudnb = true;
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Ldloc_S, (byte)13);
                    yield return new CodeInstruction(OpCodes.Call, m_offset);
                    yield return new CodeInstruction(OpCodes.Stloc_S, (byte)13);
                }
                else
                if (found is false && instruction.opcode == OpCodes.Ldc_I4_1 && struc[index + 1].opcode == OpCodes.Stloc_S && struc[index - 1].opcode == OpCodes.Brtrue_S)
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, HideHairUnderHats);
                    found = true;
                }
                else
                {
                    yield return instruction;
                }
            }

            if (found is false)
            {
                Log.Error("Couldn't find Ldc_I4_1");
            }
            if (foudnb is false)
            {
                Log.Error("Couldn't find HairMatAt_NewTemp");
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class bs
    {
        // public static readonly GraphicMeshSet biggerhair = new GraphicMeshSet(1.7f);
        public static Graphic bald = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Hairs/Shaved", ShaderDatabase.Transparent, Vector2.one, Color.clear);

        public static void ClearGraphics(Pawn pawn)
        {
            var graphics = pawn?.Drawer?.renderer?.graphics;
            if (graphics == null) return;
            if (UnityData.IsInMainThread is false) return;

            if (Settings.HideHats || (Settings.HatsOnlyWhileDrafted && !pawn.Drafted))
            {
                graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead);

                if (graphics.hairGraphic == bald)
                {
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Transparent, Vector2.one, pawn.story.hairColor);
                }
            }

            if (Settings.HideJackets)
            {
                if (graphics.apparelGraphics.Any(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Middle))
                {
                    graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell);
                }
            }
        }

        public static void ResetGraphics(Pawn pawn)
        {
            var graphics = pawn?.Drawer?.renderer?.graphics;
            if (graphics == null)
            {
                return;
            }
            if (Settings.HideHairUnderHats is false)
            {
                if (Settings.hairfilter.Contains(pawn.story.hairDef))
                {
                    graphics.hairGraphic = bald;
                }
            }

            graphics.ResolveApparelGraphics();
        }

        public static void PatherCheck(Pawn_PathFollower __instance, bool startpath)
        {
            if (Settings.HideHats is false && Settings.HideJackets is false) return;

            if (UnityData.IsInMainThread is false) return;

            var pawn = __instance.pawn;

            if (pawn.Drafted || pawn.AnimalOrWildMan()) return;

            var map = pawn.MapHeld;

            if (!__instance.nextCell.InBounds(map) || !__instance.lastCell.InBounds(map)) return;

            var graphics = pawn?.Drawer?.renderer?.graphics;

            if (graphics == null) return;

            if (startpath)
            {
                if (__instance.nextCell.UsesOutdoorTemperature(map))
                {
                    ResetGraphics(__instance.pawn);
                }
                else
                {
                    ClearGraphics(pawn);
                }
                return;
            }

            var last = __instance.lastCell.UsesOutdoorTemperature(map);
            var next = __instance.nextCell.UsesOutdoorTemperature(map);

            if (last && !next)
            {
                ClearGraphics(pawn);
            }

            if (!last && next)
            {
                ResetGraphics(__instance.pawn);
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_DraftController))]
    [HarmonyPatch(nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class H_Drafted
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            if (__instance.draftedInt)
            {
                bs.ResetGraphics(__instance.pawn);
            }
            else
            {
                bs.ClearGraphics(__instance.pawn);
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
    public static class H_StartPath
    {
        public static void Postfix(Pawn_PathFollower __instance) => bs.PatherCheck(__instance, true);
    }

    [HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.TryEnterNextPathCell))]
    [StaticConstructorOnStartup]
    public static class H_TryEnterNextPathCell
    {
        public static void Postfix(Pawn_PathFollower __instance) => bs.PatherCheck(__instance, false);
    }

    [HarmonyPatch(typeof(JobDriver_Wear), nameof(JobDriver_Wear.Notify_Starting))]
    public static class h_JobDriver_Wear
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            if (Settings.FlatRate)
            {
                __instance.duration = Settings.EquipModTicks;
            }
            else
            {
                __instance.duration = (int)(__instance.duration * Settings.EquipModPC);
            }
        }
    }


    [HarmonyPatch(typeof(JobDriver_RemoveApparel), nameof(JobDriver_RemoveApparel.Notify_Starting))]
    public static class h_JobDriver_RemoveApparel
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            if (Settings.FlatRate)
            {
                __instance.duration = Settings.EquipModTicks;
            }
            else
            {
                __instance.duration = (int)(__instance.duration * Settings.EquipModPC);
            }
        }
    }
}

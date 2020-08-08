using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public float EquipModPC = 0.2f;
        public int EquipModTicks = 10;
        public bool FlatRate = true;
        public bool HatsSleeping = true;
        public bool HatsIndoors = true;
        private string buf;
        private Listing_Standard listing_Standard;

        public void DoWindowContents(Rect canvas)
        {
            Rect nifta = canvas.ContractedBy(40f);
            listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (nifta.width - 40f) / 2f;

            listing_Standard.Begin(canvas.ContractedBy(60f));

            listing_Standard.CheckboxLabeled("Hide hats when sleeping", ref HatsSleeping);
            listing_Standard.CheckboxLabeled("Hide hats when indoors", ref HatsIndoors);

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

            listing_Standard.End();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref FlatRate, "FlatRate");
            Scribe_Values.Look(ref HatsIndoors, "HatsIndoors");
            Scribe_Values.Look(ref HatsSleeping, "HatsSleeping");
            Scribe_Values.Look(ref EquipModPC, "EquipModPC");
            Scribe_Values.Look(ref EquipModTicks, "EquipModTicks");
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
            if (!QuickFast.Settings.HatsSleeping)
            {
                return;
            }

            Toil toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;
            var bed = toil.actor.CurrentBed();
            if (bed != null && !bed.def.building.bed_showSleeperBody)
            {
                toil.actor.Drawer.renderer.graphics.ClearCache();
                toil.actor.Drawer.renderer.graphics.apparelGraphics.Clear();
            }
        }

        public static void Prefix_2(object __instance)
        {
            if (!QuickFast.Settings.HatsSleeping)
            {
                return;
            }

            Toil toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;
            toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
        }
    }


    [HarmonyPatch(typeof(Pawn_DraftController))]
    [HarmonyPatch(nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class H_Drafted
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            if (!QuickFast.Settings.HatsIndoors)
            {
                return;
            }

            if (__instance.draftedInt)
            {
                __instance.pawn?.Drawer?.renderer?.graphics?.ResolveApparelGraphics();
            }
            else
            {
                if (!__instance.pawn.Position.UsesOutdoorTemperature(__instance.pawn.Map))
                {
                    __instance.pawn.Drawer.renderer.graphics.apparelGraphics.RemoveAll(x =>
                        x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.StartPath))]
    public static class H_StartPath
    {
        public static void Postfix(Pawn_PathFollower __instance)
        {
            if (!QuickFast.Settings.HatsIndoors)
            {
                return;
            }

            if (__instance.pawn.AnimalOrWildMan())
            {
                return;
            }

            if (__instance.pawn.Drafted)
            {
                //     return;
            }

            var UsesOutdoorTemperature = __instance.nextCell.UsesOutdoorTemperature(__instance.pawn.Map);

            if (!UsesOutdoorTemperature)
            {
                __instance.pawn.Drawer.renderer.graphics.apparelGraphics.RemoveAll(x =>
                    x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead);
            }

            if (UsesOutdoorTemperature)
            {
                __instance.pawn?.Drawer?.renderer?.graphics?.ResolveApparelGraphics();
            }

        }
    }

    [HarmonyPatch(typeof(Pawn_PathFollower))]
    [HarmonyPatch(nameof(Pawn_PathFollower.TryEnterNextPathCell))]
    public static class H_TryEnterNextPathCell
    {
        public static void Postfix(Pawn_PathFollower __instance)
        {
            if (!QuickFast.Settings.HatsIndoors)
            {
                return;
            }

            if (__instance.pawn.AnimalOrWildMan())
            {
                return;
            }

            if (__instance.pawn.Drafted)
            {
                //     return;
            }

            var last = __instance.lastCell.UsesOutdoorTemperature(__instance.pawn.Map);
            var next = __instance.nextCell.UsesOutdoorTemperature(__instance.pawn.Map);

            if (last && !next)
            {
                __instance.pawn.Drawer.renderer.graphics.apparelGraphics.RemoveAll(x =>
                    x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead);
            }

            if (!last && next)
            {
                __instance.pawn?.Drawer?.renderer?.graphics?.ResolveApparelGraphics();
            }

        }
    }

    [HarmonyPatch(typeof(JobDriver_Wear))]
    [HarmonyPatch(nameof(JobDriver_Wear.Notify_Starting))]
    public static class gert
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            if (QuickFast.Settings.FlatRate)
            {
                __instance.duration = QuickFast.Settings.EquipModTicks;
            }
            else
            {
                __instance.duration = (int)(__instance.duration * QuickFast.Settings.EquipModPC);
            }
        }
    }


    [HarmonyPatch(typeof(JobDriver_RemoveApparel))]
    [HarmonyPatch(nameof(JobDriver_RemoveApparel.Notify_Starting))]
    public static class shart
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            if (QuickFast.Settings.FlatRate)
            {
                __instance.duration = QuickFast.Settings.EquipModTicks;
            }
            else
            {
                __instance.duration = (int)(__instance.duration * QuickFast.Settings.EquipModPC);
            }
        }
    }
}

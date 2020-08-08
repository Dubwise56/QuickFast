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


    public class QuickFast : Mod
    {
        public QuickFast(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("QuickFast");
            harmony.PatchAll();

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
        public static void Prefix_0(object __instance)
        {
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
            Toil toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;
            toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
        }
    }


    //[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnInternal), typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool))]
    //[HarmonyBefore("rimworld.facialstuff.mod")]
    //public static class Patch_PawnRenderer
    //{
    //    private static void Prefix(PawnRenderer __instance, ref bool portrait)
    //    {
    //        var pawn = __instance.pawn;
    //        if (portrait == false && pawn.CurrentBed() != null)
    //        {
    //            pawn.Drawer.renderer.graphics.ClearCache();
    //            pawn.Drawer.renderer.graphics.apparelGraphics.Clear();
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(JobDriver_Wear))]
    [HarmonyPatch(nameof(JobDriver_Wear.Notify_Starting))]
    public static class gert
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            __instance.duration = 10;
        }
    }


    [HarmonyPatch(typeof(JobDriver_RemoveApparel))]
    [HarmonyPatch(nameof(JobDriver_RemoveApparel.Notify_Starting))]
    public static class shart
    {
        public static void Postfix(JobDriver_Wear __instance)
        {
            __instance.duration = 10;
        }
    }
}

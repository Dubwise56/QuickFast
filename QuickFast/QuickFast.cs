using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
          new  Harmony("QuickFast").PatchAll();
        }
    }

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

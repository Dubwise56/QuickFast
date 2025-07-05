// // Copyright (C) Imaginary Labs Inc. All rights reserved.

using HarmonyLib;
using RimWorld;
using Verse;

namespace QuickFast.Source
{
    [StaticConstructorOnStartup]
    public static class H_EquipSpeed
    {
        [HarmonyPatch(typeof(JobDriver_RemoveApparel), nameof(JobDriver_RemoveApparel.Notify_Starting))]
        public static class h_JobDriver_RemoveApparel
        {
            public static void Postfix(JobDriver_Wear __instance)
            {
                if (Settings.ChangeEquipSpeed)
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


        [HarmonyPatch(typeof(JobDriver_Wear), nameof(JobDriver_Wear.Notify_Starting))]
        public static class h_JobDriver_Wear
        {
            public static void Postfix(JobDriver_Wear __instance)
            {
                if (Settings.ChangeEquipSpeed)
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
        
    }
}
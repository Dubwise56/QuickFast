using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LudeonTK;
using RimWorld;
using Verse;
using Verse.AI;

namespace QuickFast.Source
{
    [StaticConstructorOnStartup]
    public static class H_MiscPatches
    {
        static H_MiscPatches()
        {
        }
        
                
        public static void ApparelChanged(Pawn pawn)
        {
            if (!UnityData.IsInMainThread) return;
            pawn?.apparel?.Notify_ApparelChanged();
            PortraitsCache.SetDirty(pawn);
        }
        
        
        [HarmonyPatch(typeof(PawnRenderTree), nameof(PawnRenderTree.AdjustParms))]
        static class Patch_PawnRenderNodeWorker_Apparel_Head_ShowHair
        {
            static void Postfix(ref PawnDrawParms parms)
            {
                if (Settings.ShowHairUnderHats)
                {
                    if (ShouldRenderHair(parms))
                    {
                        // always clear the Hair‐hiding bit so hair will draw
                        parms.skipFlags &= ~RenderSkipFlagDefOf.Hair;
                    }
                }
            }
        }


        public static bool ShouldRenderHair(PawnDrawParms pr)
        {
            // Guard against a completely null parameter or null pawn
            if (pr.pawn == null)
                return false;

            var pawn = pr.pawn;

            // Missing story / hairDef → nothing to draw
            if (pawn.story?.hairDef == null)
                return false;

            // If the global filter list exists and contains this hair, suppress it
            if (Settings.hairfilter != null && Settings.hairfilter.Contains(pawn.story.hairDef))
                return false;

            // If the pawn has no apparel component or no worn apparel, we can always draw hair
            if (pawn.apparel?.WornApparel == null || pawn.apparel.WornApparel.Count == 0)
                return true;

            var wornApparel = pawn.apparel.WornApparel;

            // Make sure the combo list exists before continuing
            if (Settings.HatHairCombo == null)
                return true;

            var hairSet = Settings.HatHairCombo.FirstOrDefault(x => x?.Hair == pawn.story.hairDef.defName);

            // If no set or set has no hat entries, hair can be shown
            if (hairSet == null || hairSet.Hats == null || hairSet.Hats.Count == 0)
            {
                return true;
            }

            // Check every overhead item; if it matches the blocked hat list, hide hair
            foreach (var apparel in wornApparel)
            {
                if (apparel == null)
                {
                    continue;
                }

                if (apparel.def?.apparel?.LastLayer != DubsApparelTweaks.Overhead)
                {
                    continue;
                }

                if (apparel.def != null && hairSet.Hats.Contains(apparel.def.defName))
                {
                    return false;
                }
            }

            return true;
        }
        

        [HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.StartPath))]
        public static class H_StartPath
        {
            public static void Postfix(Pawn_PathFollower __instance)
            {
                if (__instance.pawn.Drafted)
                {
                    return;
                }

                PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, true);
            }
        }

        [HarmonyPatch(typeof(Pawn_PathFollower), nameof(Pawn_PathFollower.TryEnterNextPathCell))]
        [StaticConstructorOnStartup]
        public static class H_TryEnterNextPathCell
        {
            public static void Postfix(Pawn_PathFollower __instance)
            {
                if (__instance.pawn.Drafted)
                {
                    return;
                }

                PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, false);
            }
        }

        [HarmonyPatch(typeof(Pawn_DraftController))]
        [HarmonyPatch(nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
        public static class H_Drafted
        {
            public static void Postfix(Pawn_DraftController __instance)
            {
                if (Settings.DraftedHidingMode)
                {
                    ApparelChanged(__instance.pawn);
                }
            }
        }


// Patch the GraphicsFor method to filter out apparel we don't want to render
        [HarmonyPatch(typeof(PawnRenderNode_Apparel), nameof(PawnRenderNode_Apparel.GraphicsFor))]
        public static class H_ApparelGraphicsFor
        {
            public static IEnumerable<Graphic> Postfix(IEnumerable<Graphic> __result, PawnRenderNode_Apparel __instance, Pawn pawn)
            {
                var apparel = __instance.apparel;

                if (!pawn.Spawned || pawn.Map == null || apparel == null || !pawn.RaceProps.Humanlike)
                {
                    // No apparel, return original result
                    foreach (var graphic in __result)
                        yield return graphic;
                    yield break;
                }

                // Check if this apparel should be hidden
                if (ShouldHideApparel(pawn, apparel))
                {
                    // Return empty enumerable to hide this apparel
                    yield break;
                }

                // Return original graphics if not filtered
                foreach (var graphic in __result)
                    yield return graphic;
            }
        }

        [HarmonyPatch(typeof(PawnRenderNode_Apparel), nameof(PawnRenderNode_Apparel.MeshSetFor))]
        public static class H_MeshSetFor
        {
            public static void Postfix(ref GraphicMeshSet __result, PawnRenderNode_Apparel __instance, Pawn pawn)
            {
                if (!Settings.ShowHairUnderHats || Math.Abs(Settings.hairMeshScale - 1f) < 0.001f)
                {
                    return;
                }

                var apparel = __instance.apparel;

                if (pawn == null || apparel == null || !pawn.Spawned || pawn.Map == null || !pawn.RaceProps.Humanlike)
                {
                    return;
                }

                if (ShouldHideApparel(pawn, apparel))
                {
                    return;
                }

                if (!apparel.def.apparel.layers.Contains(DubsApparelTweaks.Overhead))
                {
                    return;
                }

                if (__instance.Props.overrideMeshSize != null)
                {
                    __result = MeshPool.GetMeshSetForSize(__instance.Props.overrideMeshSize.Value.x * Settings.hairMeshScale, __instance.Props.overrideMeshSize.Value.y * Settings.hairMeshScale);
                    return;
                }

                if (__instance.useHeadMesh)
                {
                    __result = HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn, 1f * Settings.hairMeshScale, 1f * Settings.hairMeshScale);
                    return;
                }

                __result = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(pawn, 1f * Settings.hairMeshScale, 1f * Settings.hairMeshScale);
                return;
            }
        }

        private static bool ShouldHideApparel(Pawn pawn, Apparel apparel)
        {
            // Check if pawn should have apparel hidden based on mod settings
            if (Settings.DraftedHidingMode)
            {
                if (pawn.Drafted)
                {
                   return false;
                }
            }
            else
            {
                if (pawn.Position.UsesOutdoorTemperature(pawn.MapHeld))
                {
                    return false;
                }
            }
            
            // Check if this apparel matches your filter criteria
            foreach (var apparelLayerDef in apparel.def.apparel.layers)
            {
                if (Settings.LayerVis.Contains(apparelLayerDef.defName))
                {
                    // Check if it's in the hat filter exception list
                    if (Settings.hatfilter.Contains(apparel.def))
                    {
                        return false; // Don't hide this one
                    }

                    return true; // Hide this apparel
                }
            }

            return false;
        }
        
        public static void PatherCheck(Pawn pawn, IntVec3 nextCell, IntVec3 lastCell, bool startpath)
        {
            if (UnityData.IsInMainThread is false) return;
            
            var map = pawn.MapHeld;

            if (map == null) return;

            if (pawn.NonHumanlikeOrWildMan()) return;

            if (!pawn.IsColonist) return;

            if (!nextCell.InBounds(map) || !lastCell.InBounds(map)) return;

            if (startpath)
            {
                // For start path, always call ApparelChanged regardless of mode/condition
                ApparelChanged(pawn);
                return;
            }
            
            if (Settings.DraftedHidingMode)
            {
                return;
            }

            // Check for indoor/outdoor transitions
            var wasOutdoor = lastCell.UsesOutdoorTemperature(pawn.MapHeld);
            var isOutdoor = nextCell.UsesOutdoorTemperature(pawn.MapHeld);

            if (wasOutdoor != isOutdoor)
            {
                ApparelChanged(pawn);
            }

        }
    }
}
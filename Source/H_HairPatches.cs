using HarmonyLib;
using RimWorld;
using Verse;

namespace QuickFast.Source
{
    public static class H_HairPatches
    {
        [HarmonyPatch(typeof(PawnRenderTree), nameof(PawnRenderTree.AdjustParms))]
        static class Patch_PawnRenderTreeAdjustParms
        {
            static void Postfix(ref PawnDrawParms parms)
            {
                if (Settings.ShowHairUnderHats && ShouldRenderHair(parms))
                {
                    parms.skipFlags &= ~RenderSkipFlagDefOf.Hair;
                }
            }
        }
        
        public static bool ShouldRenderHair(PawnDrawParms drawParams)
        {
            var pawn = drawParams.pawn;
            if (pawn?.story?.hairDef == null) return false;
            
            if (Settings.hairfilter?.Contains(pawn.story.hairDef) == true) return false;

            var wornApparel = pawn.apparel?.WornApparel;
            if (wornApparel == null || wornApparel.Count == 0) return true;

            var hairSet = Settings.HatHairCombo?.FirstOrDefault(x => x?.Hair == pawn.story.hairDef.defName);
            if (hairSet?.Hats == null || hairSet.Hats.Count == 0) return true;

            foreach (var apparel in wornApparel)
            {
                if (apparel?.def?.apparel?.LastLayer != DubsApparelTweaks.Overhead)
                {
                    continue;
                }

                if (apparel?.def != null && hairSet.Hats.Contains(apparel.def.defName))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
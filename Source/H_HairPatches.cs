using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace QuickFast.Source
{
    public static class H_HairPatches
    {
        [HarmonyPatch(typeof(PawnRenderTree), nameof(PawnRenderTree.AdjustParms))]
        static class H_PawnRenderTreeAdjustParms
        {
            static void Postfix(ref PawnDrawParms parms)
            {
                var pawn = parms.pawn;
                if (pawn == null) return;

                var originalSkip = parms.skipFlags;
                bool hairSkippedOrig = (originalSkip & RenderSkipFlagDefOf.Hair) != 0;
                bool beardSkippedOrig = (originalSkip & RenderSkipFlagDefOf.Beard) != 0;
                bool eyesSkippedOrig = (originalSkip & RenderSkipFlagDefOf.Eyes) != 0;

                bool hatHiddenByMod = false;
                bool hiddenIsFullHead = false;
                bool hasAnyHeadGear = false;

                foreach (var app in pawn.apparel?.WornApparel ?? Enumerable.Empty<Apparel>())
                {
                    var def = app.def.apparel;
                    if (def == null) continue;

                    bool coversUpper = def.bodyPartGroups.Contains(BodyPartGroupDefOf.UpperHead);
                    bool coversFull = def.bodyPartGroups.Contains(BodyPartGroupDefOf.FullHead);
                    if (!coversUpper && !coversFull) continue;

                    hasAnyHeadGear = true;

                    if (H_ApparelPatches.ShouldHideApparel(pawn, app))
                    {
                        hatHiddenByMod = true;
                        hiddenIsFullHead = coversFull;
                        break;
                    }
                }

                if (hatHiddenByMod)
                {
                    if (hairSkippedOrig)
                        parms.skipFlags &= ~RenderSkipFlagDefOf.Hair;

                    if (hiddenIsFullHead)
                    {
                        if (beardSkippedOrig)
                            parms.skipFlags &= ~RenderSkipFlagDefOf.Beard;
                        if (eyesSkippedOrig)
                            parms.skipFlags &= ~RenderSkipFlagDefOf.Eyes;
                    }

                    return;
                }

                if (Settings.ShowHairUnderHats && hasAnyHeadGear)
                {
                    bool hairCanShow = ShouldRenderHair(parms);
                    if (hairCanShow && hairSkippedOrig)
                        parms.skipFlags &= ~RenderSkipFlagDefOf.Hair;
                }
            }
        }
        

        public static bool ShouldRenderHair(PawnDrawParms parms)
        {
            var pawn = parms.pawn;
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
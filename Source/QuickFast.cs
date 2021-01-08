using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace QuickFast
{
    [StaticConstructorOnStartup]
    public static class bs
    {
        public static Harmony harmony;

        public static ApparelLayerDef MiddleHead;

        private static readonly MethodInfo RenderPawnInternal = AccessTools.Method(typeof(PawnRenderer),
            nameof(PawnRenderer.RenderPawnInternal),
            new[]
            {
                typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode),
                typeof(bool), typeof(bool), typeof(bool)
            });

        private static readonly HarmonyMethod trans =
            new HarmonyMethod(typeof(H_RenderPawn).GetMethod(nameof(H_RenderPawn.Transpilerino)));

        public static readonly string harmonyID = "Quickfast";

        static bs()
        {
            MiddleHead = DefDatabase<ApparelLayerDef>.GetNamedSilentFail("MiddleHead");

            harmony = new Harmony(harmonyID);
            harmony.PatchAll();

            var CEdrawhair =
                AccessTools.Method("CombatExtended.HarmonyCE.Harmony_PawnRenderer_RenderPawnInternal:DrawHeadApparel");

            if (CEdrawhair != null)
            {
                harmony.Patch(CEdrawhair, null, new HarmonyMethod(typeof(bs).GetMethod(nameof(killme))));
            }

            var b__0 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass2_0:<LayDown>b__0");
            var b__2 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass2_0:<LayDown>b__2");

            if (b__0 != null && b__2 != null)
            {
                var Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_0)));
                harmony.Patch(b__0, null, Prefix);

                Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_2)));
                harmony.Patch(b__2, null, Prefix);
            }
            else
            {
                Log.Warning("Couldn't find the LayDown toils, bad news for hat hiding while sleeping.");
            }

            if (Settings.ShowHairUnderHats)
            {
                ApplyTrans();
            }

            //trick har so it loops the cached visible gear rather than getting all worn apparel
            var meth = AccessTools.Method("AlienRace.HarmonyPatches:DrawAddons");
            if (meth != null)
            {
                // Log.Message("Dubs Apparel Tweaks found HAR");
                var pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Pre)));
                var post = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Post)));
                harmony.Patch(meth, pre, post);

                pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.WornApparelPrefix)));
                meth = AccessTools.Method(typeof(Pawn_ApparelTracker), "get_WornApparel");
                harmony.Patch(meth, pre);
            }
        }

        public static ApparelLayerDef Overhead => MiddleHead ?? ApparelLayerDefOf.Overhead;

        public static void killme(ref bool hideHair)
        {
            if (Settings.ShowHairUnderHats)
            {
                hideHair = H_RenderPawn.HairGotFiltered;
            }
        }

        public static void ApplyTrans()
        {
            harmony.Patch(RenderPawnInternal, transpiler: trans);
            //  Log.Warning("Applied transpiler to RenderPawnInternal to show hair under hats and rescale hats");
        }

        public static void RemoveTrans()
        {
            harmony.Unpatch(RenderPawnInternal, HarmonyPatchType.Transpiler, harmonyID);
        }


        public static void fix_0(object __instance)
        {
            if (!Settings.HatsSleeping)
            {
                return;
            }

            var toil = AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) as Toil;
            var bed = toil?.actor.CurrentBed();
            if (bed != null && toil.actor.RaceProps.Humanlike && !bed.def.building.bed_showSleeperBody)
            {
                toil.actor.Drawer.renderer.graphics.ClearCache();
                toil.actor.Drawer.renderer.graphics.apparelGraphics.Clear();
            }
        }

        public static void fix_2(object __instance)
        {
            if (!Settings.HatsSleeping)
            {
                return;
            }

            if (AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) is Toil toil &&
                toil.actor.RaceProps.Humanlike)
            {
                toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
            }
        }


        public static Mesh MeshHead(Mesh originalMesh, float s)
        {
            var clonedMesh = new Mesh();

            clonedMesh.name = "clone";

            var trash = originalMesh.vertices;

            clonedMesh.vertices = originalMesh.vertices;
            clonedMesh.triangles = originalMesh.triangles;
            clonedMesh.normals = originalMesh.normals;
            clonedMesh.uv = originalMesh.uv;

            for (var index = 0; index < trash.Length; index++)
            {
                var vertex = trash[index];
                vertex.x *= s;
                vertex.y *= s;
                vertex.z *= s;
                trash[index] = vertex;
            }

            clonedMesh.vertices = trash;
            clonedMesh.RecalculateNormals();
            clonedMesh.RecalculateBounds();

            return clonedMesh;
        }


        public static void SwitchIndoors(Pawn pawn)
        {
            var graphics = pawn?.Drawer?.renderer?.graphics;
            if (graphics == null) return;
            if (UnityData.IsInMainThread is false) return;

            if (Settings.HideJackets is true || Settings.JacketsOnlyWhileDrafted is true && pawn.Drafted is false)
            {
                if (graphics.apparelGraphics.Any(x =>
                    x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin))
                {
                    graphics.apparelGraphics.RemoveAll(x =>
                        x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell);
                }
            }

            if (Settings.HideEquipment is true || Settings.EquipmentOnlyWhileDrafted is true && pawn.Drafted is false)
            {
                graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt);
            }

            if (Settings.HideHats is true || Settings.HatsOnlyWhileDrafted is true && pawn.Drafted is false)
            {
                bool Match(ApparelGraphicRecord x)
                {
                    return x.sourceApparel.def.apparel.layers.Any(z => z == Overhead) &&
                           !Settings.hatfilter.Contains(x.sourceApparel.def);
                }

                var hidden = graphics.apparelGraphics.RemoveAll(Match);
            }
        }

        public static void SwitchOutdoors(Pawn pawn)
        {
            var graphics = pawn?.Drawer?.renderer?.graphics;
            if (graphics == null)
            {
                return;
            }

            graphics.ClearCache();
            graphics.apparelGraphics.Clear();
            using (var enumerator = graphics.pawn.apparel.wornApparel.InnerListForReading.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (ApparelGraphicRecordGetter.TryGetGraphicApparel(enumerator.Current,
                        graphics.pawn.story.bodyType, out var item))
                    {
                        graphics.apparelGraphics.Add(item);
                    }
                }
            }
        }

        public static void PatherCheck(Pawn pawn, IntVec3 nextCell, IntVec3 lastCell, bool startpath)
        {
            var map = pawn.MapHeld;

            //   if (Settings.HideHats is false && Settings.HideJackets is false) return true;

            if (UnityData.IsInMainThread is false) return;

            // if (!pawn.RaceProps.Humanlike) return false;

            if (map == null) return;

            if (pawn.NonHumanlikeOrWildMan()) return;

            if (!pawn.IsColonist) return;

            if (!nextCell.InBounds(map) || !lastCell.InBounds(map)) return;

            if (startpath)
            {
                if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
                {
                    SwitchOutdoors(pawn);
                }
                else
                {
                    SwitchIndoors(pawn);
                }

                return;
            }

            if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
            {
                SwitchOutdoors(pawn);
            }
            else
            {
                SwitchIndoors(pawn);
            }

            var last = lastCell.UsesOutdoorTemperature(map);
            var next = nextCell.UsesOutdoorTemperature(map);

            if (last && !next)
            {
                SwitchIndoors(pawn);
            }

            if (!last && next)
            {
                SwitchOutdoors(pawn);
            }
        }
    }


    public static class H_HAR_Workaround
    {
        public static bool run;

        public static void Pre()
        {
            run = true;
        }

        public static void Post()
        {
            run = false;
        }

        public static bool WornApparelPrefix(Pawn_ApparelTracker __instance, ref List<Apparel> __result)
        {
            if (!run) return true;

            var graphics = __instance.pawn?.Drawer?.renderer?.graphics;
            if (graphics != null && !graphics.apparelGraphics.NullOrEmpty())
            {
                __result = new List<Apparel>();
                var coo = graphics.apparelGraphics.Count;
                for (var i = 0; i < coo; i++)
                {
                    var apparel = graphics.apparelGraphics[i];
                    __result.Add(apparel.sourceApparel);
                }
            }

            return false;
        }
    }

    public static class H_RenderPawn
    {
        public static Dictionary<Mesh, Mesh> scalers = new Dictionary<Mesh, Mesh>();

        public static bool HairGotFiltered;

        public static void hairScale_Changed()
        {
            scalers.Clear();
        }

        public static Mesh MeshScaler(PawnRenderer pr, Mesh mesh)
        {
            HairGotFiltered = false;
            if (Settings.hairfilter.Contains(pr.pawn.story.hairDef))
            {
                HairGotFiltered = true;
                return mesh;
            }

            try
            {
                return scalers[mesh];
            }
            catch
            {
                scalers[mesh] = bs.MeshHead(mesh, Settings.hairMeshScale);
                return scalers[mesh];
            }
        }

        public static bool ShouldRenderHair(bool HatDrawn)
        {
            if (HatDrawn is true)
            {
                if (HairGotFiltered)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public static Vector3 offset(Vector3 vec)
        {
            if (HairGotFiltered)
            {
                HairGotFiltered = false;
                return vec;
            }

            vec.y += -0.0036f;
            return vec;
        }

        public static bool oploc(this CodeInstruction obj, OpCode oc, int ind)
        {
            return obj.opcode == oc && obj.operand is LocalBuilder i && i.LocalIndex == ind;
        }

        public static bool loc(this CodeInstruction obj, int ind)
        {
            return obj.operand is LocalBuilder i && i.LocalIndex == ind;
        }

        public static bool op(this CodeInstruction obj, OpCode oc)
        {
            return obj.opcode == oc;
        }

        public static IEnumerable<CodeInstruction> Transpilerino(IEnumerable<CodeInstruction> instructions)
        {
            var f_shouldrender = false;
            var f_offset = false;
            var f_scaler = false;
            var ins_l = instructions.ToList();

            //check its possible before bothering

            for (var i = 0; i < ins_l.Count - 1; i++)
            {
                var ins = ins_l[i];
                if (ins.oploc(OpCodes.Stloc_S, 15) && ins_l[i + 1].op(OpCodes.Ldc_I4_0))
                {
                    f_scaler = true;
                }
                else if (ins.oploc(OpCodes.Ldloc_S, 14))
                {
                    f_shouldrender = true;
                }
                else if (ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
                {
                    f_offset = true;
                }
            }

            if (f_scaler is false)
            {
                Log.Warning("Failed inject m_MeshScaler - hair under hats wont work F");
            }

            if (f_shouldrender is false)
            {
                Log.Warning("Failed inject HairGotFiltered - hair under hats wont work F");
            }

            if (f_offset is false)
            {
                Log.Warning("Failed inject m_offset - hair under hats wont work F");
            }

            if (!f_shouldrender || !f_offset || !f_scaler)
            {
                foreach (var codeInstruction in ins_l) yield return codeInstruction;
            }
            else
            {
                f_shouldrender = false;
                f_offset = false;
                f_scaler = false;

                for (var i = 0; i < ins_l.Count; i++)
                {
                    var ins = ins_l[i];
                    if (f_scaler is false && ins.oploc(OpCodes.Stloc_S, 15) && ins_l[i + 1].op(OpCodes.Ldc_I4_0))
                    {
                        f_scaler = true;
                        yield return ins;
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 15);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(MeshScaler)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 15);
                    }
                    else if (f_shouldrender is false && ins.oploc(OpCodes.Ldloc_S, 14))
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 14);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(ShouldRenderHair)));
                        // yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(H_RenderPawn), nameof(HairGotFiltered)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 14);
                        yield return ins;
                        f_shouldrender = true;
                    }
                    else if (f_offset is false && ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
                    {
                        f_offset = true;
                        yield return ins;
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(offset)));
                        yield return new CodeInstruction(OpCodes.Stloc_S, 13);
                    }
                    else
                    {
                        yield return ins;
                    }
                }
            }
        }
    }


    [HarmonyPatch(typeof(Pawn_DraftController))]
    [HarmonyPatch(nameof(Pawn_DraftController.Drafted), MethodType.Setter)]
    public static class H_Drafted
    {
        public static void Postfix(Pawn_DraftController __instance)
        {
            if (__instance.draftedInt || __instance.pawn.Position.UsesOutdoorTemperature(__instance.pawn.MapHeld))
            {
                bs.SwitchOutdoors(__instance.pawn);
            }
            else
            {
                bs.SwitchIndoors(__instance.pawn);
            }
        }
    }

    internal class GameComponent_quickfast : GameComponent
    {
        private int jankyfix;

        public GameComponent_quickfast(Game game)
        {
        }

        public override void GameComponentTick()
        {
            if (jankyfix < 2)
            {
                jankyfix++;
                if (!Settings.HatsSleeping)
                {
                    return;
                }

                foreach (var map in Find.Maps)
                {
                    foreach (var instanceMapPawn in map.mapPawns.AllPawns)
                    {
                        var bed = instanceMapPawn.CurrentBed();
                        if (instanceMapPawn.InBed() && instanceMapPawn.RaceProps.Humanlike &&
                            !bed.def.building.bed_showSleeperBody)
                        {
                            instanceMapPawn.Drawer.renderer.graphics.ClearCache();
                            instanceMapPawn.Drawer.renderer.graphics.apparelGraphics.Clear();
                        }
                    }
                }
            }
        }
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

            bs.PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, true);
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

            //foreach (var ap in __instance.pawn.drawer.renderer.graphics.apparelGraphics)
            //{
            //    Log.Warning(ap.sourceApparel.def.apparel.LastLayer + "");
            //}

            bs.PatherCheck(__instance.pawn, __instance.nextCell, __instance.lastCell, false);
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
}
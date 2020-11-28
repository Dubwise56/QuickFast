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
    public class QuickFast : Mod
    {
        public static Settings Settings;

        public QuickFast(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Dubs Apparel Tweaks";
        }
    }


    public class Settings : ModSettings
    {
        private static List<ThingDef> _hatfilter;
        private static List<HairDef> _hairfilter;

        public static float hairMeshScale = 1.1f;

        //   public static float hairScaleNarrow = 1.4f;
        public static float EquipModPC = 0.2f;
        public static int EquipModTicks = 10;
        public static bool FlatRate = true;
        public static bool HatsSleeping = true;
        public static bool HideHats = true;
        public static bool HideJackets = true;
        public static bool HatsOnlyWhileDrafted;
        public static bool JacketsOnlyWhileDrafted;
        public static bool ShowHairUnderHats;
        public static bool ChangeEquipSpeed = true;

        public static List<string> HatDefToStrings = new List<string>();

        public static List<string> DefToStrings = new List<string>();
        private string buf;
        private Listing_Standard lis;

        public static List<ThingDef> hatfilter
        {
            get
            {
                if (_hatfilter == null)
                {
                    _hatfilter = new List<ThingDef>();
                    if (HatDefToStrings == null)
                    {
                        HatDefToStrings = new List<string>();
                    }

                    foreach (var HatDefToStrings in HatDefToStrings)
                    {
                        var foo = DefDatabase<ThingDef>.GetNamed(HatDefToStrings);
                        if (foo != null)
                        {
                            _hatfilter.Add(foo);
                        }
                    }
                }

                return _hatfilter;
            }
        }

        public static List<HairDef> hairfilter
        {
            get
            {
                if (_hairfilter == null)
                {
                    _hairfilter = new List<HairDef>();
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

        public void DoWindowContents(Rect canvas)
        {
            var nifta = canvas;
            lis = new Listing_Standard();
            lis.ColumnWidth = (nifta.width - 40f) / 2f;

            lis.Begin(canvas);

            Text.Font = GameFont.Medium;
            lis.Label("Apparel_equip_speed".Translate());
            Text.Font = GameFont.Small;
            lis.CheckboxLabeled("Change_equip_speeds".Translate(), ref ChangeEquipSpeed);
            if (ChangeEquipSpeed)
            {
                lis.CheckboxLabeled("Same_speed_for_all_apparel".Translate(), ref FlatRate);

                if (FlatRate)
                {
                    lis.LabelDouble("Equip_speed_Ticks".Translate(), $"{EquipModTicks} ticks");
                    lis.IntEntry(ref EquipModTicks, ref buf);
                }
                else
                {
                    lis.LabelDouble("Equip_duration".Translate(), $"{EquipModPC.ToStringPercent()}");
                    EquipModPC = lis.Slider(EquipModPC, 0f, 1f);
                }
            }

            lis.GapLine();
            Text.Font = GameFont.Medium;
            lis.Label("Apparel_visibility".Translate());
            Text.Font = GameFont.Small;
            lis.CheckboxLabeled("Hide_hats_when_sleeping".Translate(), ref HatsSleeping);
            lis.CheckboxLabeled("Hide_hats_when_indoors".Translate(), ref HideHats);
            lis.CheckboxLabeled("Hide_jackets_when_indoors".Translate(), ref HideJackets);
            lis.CheckboxLabeled("Hats_only_while_drafted".Translate(), ref HatsOnlyWhileDrafted);
            lis.CheckboxLabeled("Jackets_only_while_drafted".Translate(), ref JacketsOnlyWhileDrafted);

            lis.NewColumn();
            Text.Font = GameFont.Medium;
            lis.Label("Hair_visibility".Translate());
            Text.Font = GameFont.Small;
            var jim = ShowHairUnderHats;
            lis.CheckboxLabeled("Show_hair_under_hats".Translate(), ref ShowHairUnderHats);
            if (jim != ShowHairUnderHats)
            {
                if (ShowHairUnderHats)
                {
                    bs.ApplyTranny();
                }
                else
                {
                    bs.RemoveTranny();
                }
            }

            if (ShowHairUnderHats)
            {
                //  lis.Label("HatScaling".Translate());
                lis.LabelDouble("HatScaling".Translate(), $"{hairMeshScale}");
                var tamw = decimal.Round((decimal)lis.Slider(hairMeshScale, 0.9f, 1.2f), 3);
                if (tamw != (decimal)hairMeshScale)
                {
                    hairMeshScale = (float)tamw;
                    H_RenderPawn.hairScale_Changed();
                }

                //lis.LabelDouble("Narrow_Width".Translate(), $"{hairScaleNarrow}");
                //tamw = decimal.Round((decimal)lis.Slider(hairScaleNarrow, 0f, 2f), 2);
                //if (tamw != (decimal)hairScaleNarrow)
                //{
                //    hairScaleNarrow = (float)tamw;
                //    bs.hairScale_Changed();
                //}

                if (lis.ButtonText("HairScaleReset".Translate()))
                {
                    //  hairScaleNarrow = 1.4f;
                    hairMeshScale = 1.1f;
                    H_RenderPawn.hairScale_Changed();
                }


                GUI.color = Color.green;
                lis.Label("hatFilterTip".Translate());
                GUI.color = Color.white;
                lis.GapLine();
            }


            lis.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref hairMeshScale, "hairMeshScale", 1.06f);
            // Scribe_Values.Look(ref hairScale, "hairScale", 1.6f);
            Scribe_Values.Look(ref ChangeEquipSpeed, "ChangeEquipSpeed");
            Scribe_Values.Look(ref HatsOnlyWhileDrafted, "HatsOnlyWhileDrafted");
            Scribe_Values.Look(ref JacketsOnlyWhileDrafted, "JacketsOnlyWhileDrafted");
            Scribe_Values.Look(ref ShowHairUnderHats, "ShowHairUnderHats");
            Scribe_Values.Look(ref FlatRate, "FlatRate");
            Scribe_Values.Look(ref HideHats, "HatsIndoors");
            Scribe_Values.Look(ref HatsSleeping, "HatsSleeping");
            Scribe_Values.Look(ref EquipModPC, "EquipModPC", 0.2f);
            Scribe_Values.Look(ref EquipModTicks, "EquipModTicks", 10);
            Scribe_Values.Look(ref HideJackets, "HideJackets");
            Scribe_Collections.Look(ref DefToStrings, "hairFilter", LookMode.Value);
            Scribe_Collections.Look(ref HatDefToStrings, "hatFilter", LookMode.Value);
        }
    }

    // TODO maybe let people set the keys
    [HarmonyPatch(typeof(UIRoot_Play))]
    [HarmonyPatch(nameof(UIRoot_Play.UIRootUpdate))]
    public static class h_UIRootOnGUI
    {
        public static void Postfix()
        {
            if (!Input.GetKey(KeyCode.LeftControl))
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                foreach (var pawn in Find.Selector.SelectedObjects.OfType<Pawn>())
                {
                    if (Settings.hairfilter.Contains(pawn.story.hairDef))
                    {
                        Settings.hairfilter.Remove(pawn.story.hairDef);
                        Messages.Message("Hair_Filter_Remove".Translate(pawn.story.hairDef.defName), MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        Settings.hairfilter.Add(pawn.story.hairDef);
                        Messages.Message("Hair_Filter_Add".Translate(pawn.story.hairDef.defName), MessageTypeDefOf.NeutralEvent);
                    }
                }

                Settings.DefToStrings = new List<string>();
                foreach (var s in Settings.hairfilter)
                {
                    Settings.DefToStrings.Add(s.defName);
                }

                QuickFast.Settings.Write();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                foreach (var pawn in Find.Selector.SelectedObjects.OfType<Pawn>())
                {
                    var hat = pawn.apparel.WornApparel.FirstOrDefault(x =>
                        x.def.apparel.LastLayer == ApparelLayerDefOf.Overhead);
                    if (hat == null)
                    {
                        return;
                    }

                    if (Settings.hatfilter.Contains(hat.def))
                    {
                        Settings.hatfilter.Remove(hat.def);
                        Messages.Message("Hat_Filter_Remove".Translate(hat.def.defName), MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        Settings.hatfilter.Add(hat.def);
                        Messages.Message("Hat_Filter_Add".Translate(hat.def.defName), MessageTypeDefOf.NeutralEvent);
                    }
                }

                Settings.HatDefToStrings = new List<string>();
                foreach (var s in Settings.hatfilter)
                {
                    Settings.HatDefToStrings.Add(s.defName);
                }



                QuickFast.Settings.Write();
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

    // [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnInternal), typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool), typeof(bool))]
    public static class H_RenderPawn
    {
        public static Dictionary<Mesh, Mesh> scalers = new Dictionary<Mesh, Mesh>();


        //  public static FieldInfo f_graphics = AccessTools.Field(typeof(PawnRenderer), nameof(PawnRenderer.graphics));

        //  public static FieldInfo ShowHairUnderHats = AccessTools.Field(typeof(Settings), "ShowHairUnderHats");

        public static MethodInfo m_ShouldRenderHair =
            AccessTools.Method(typeof(H_RenderPawn), nameof(ShouldRenderHair));

        public static MethodInfo m_MeshScaler = AccessTools.Method(typeof(H_RenderPawn), nameof(MeshScaler));

        public static MethodInfo m_offset = AccessTools.Method(typeof(H_RenderPawn), nameof(offset));

        //  public static MethodInfo m_get_HairMeshSet = AccessTools.Method(typeof(PawnGraphicSet), "get_HairMeshSet");

        // public static MethodInfo m_MeshAt = AccessTools.Method(typeof(GraphicMeshSet), nameof(GraphicMeshSet.MeshAt));

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

        public static bool ShouldRenderHair(PawnRenderer pr)
        {
            if (HairGotFiltered)
            {
                return true;
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

            for (var i = 0; i < ins_l.Count; i++)
            {
                var ins = ins_l[i];
                if (f_scaler is false && ins.oploc(OpCodes.Stloc_S, 15) && ins_l[i + 1].op(OpCodes.Ldc_I4_0))
                {
                    f_scaler = true;
                }
                else if (f_shouldrender is false && ins.op(OpCodes.Ldc_I4_1) && ins_l[i + 1].oploc(OpCodes.Stloc_S, 14))
                {
                    f_shouldrender = true;
                }
                else if (f_offset is false && ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
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
                Log.Warning("Failed inject m_ShouldRenderHair - hair under hats wont work F");
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
                        yield return new CodeInstruction(OpCodes.Call, m_MeshScaler);
                        yield return new CodeInstruction(OpCodes.Stloc_S, 15);
                    }
                    else if (f_shouldrender is false && ins.op(OpCodes.Ldc_I4_1) && ins_l[i + 1].oploc(OpCodes.Stloc_S, 14))
                    {
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Call, m_ShouldRenderHair);
                        f_shouldrender = true;
                    }
                    else if (f_offset is false && ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
                    {
                        f_offset = true;
                        yield return ins;
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
                        yield return new CodeInstruction(OpCodes.Call, m_offset);
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

    [StaticConstructorOnStartup]
    public static class bs
    {
        public static Harmony harmony;

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
            harmony = new Harmony(harmonyID);
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
                var Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_0)));
                harmony.Patch(b__0, null, Prefix);

                Prefix = new HarmonyMethod(typeof(bs).GetMethod(nameof(fix_2)));
                harmony.Patch(b__2, null, Prefix);
            }


            if (Settings.ShowHairUnderHats)
            {
                ApplyTranny();
            }

            //trick har so it loops the cached visible gear rather than getting all worn apparel
            var meth = AccessTools.Method("AlienRace.HarmonyPatches:DrawAddons");
            if (meth != null)
            {
                Log.Message("Dubs Apparel Tweaks found HAR");
                var pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Pre)));
                var post = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.Post)));
                harmony.Patch(meth, pre, post);

                pre = new HarmonyMethod(typeof(H_HAR_Workaround).GetMethod(nameof(H_HAR_Workaround.WornApparelPrefix)));
                meth = AccessTools.Method(typeof(Pawn_ApparelTracker), "get_WornApparel");
                harmony.Patch(meth, pre);
            }
        }

        public static void ApplyTranny()
        {
            harmony.Patch(RenderPawnInternal, transpiler: trans);
            //  Log.Warning("Applied transpiler to RenderPawnInternal to show hair under hats and rescale hats");
        }

        public static void RemoveTranny()
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

            if (Settings.HideHats is true || Settings.HatsOnlyWhileDrafted is true && pawn.Drafted is false)
            {
                bool Match(ApparelGraphicRecord x) => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead && !Settings.hatfilter.Contains(x.sourceApparel.def);

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

    class GameComponent_quickfast : GameComponent
    {
        public GameComponent_quickfast(Game game)
        {
        }

        private int jankyfix = 0;
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
                        if (instanceMapPawn.InBed() && instanceMapPawn.RaceProps.Humanlike && !bed.def.building.bed_showSleeperBody)
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
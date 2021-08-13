using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;

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

		//[DebugAction("Memory", "Leak test", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.Playing)]
		//public static void LeakTesting()
		//{
		//	void bimleaker(Type allType)
		//	{
		//		if (allType.IsAbstract)
		//		{
		//			return;
		//		}

		//		if (allType.IsGenericType)
		//		{
		//			return;
		//		}


		//		int counts = 0;


		//		try
		//		{
		//			counts = Mesh.FindObjectsOfType(allType).Length;
		//		}
		//		catch (Exception e)
		//		{

		//		}
		//		;
		//		typepcounter.Add(allType, counts);
		//	}
		//	if (typepcounter == null)
		//	{
		//		typepcounter = new Dictionary<Type, int>();
		//		//foreach (var allType in GenTypes.AllTypes.ToList())
		//		//{
		//		//	try
		//		//	{
		//		//		bimleaker(allType);
		//		//	}
		//		//	catch (Exception e)
		//		//	{

		//		//	}
		//		//}

		//		foreach (var allActiveAssembly in ModLister.AllInstalledMods)
		//		{
		//			foreach (var VARIABLE in allActiveAssembly.source.)
		//			{

		//			}
		//			foreach (var type in allActiveAssembly.GetTypes())
		//			{
		//				bimleaker(type);
		//			}
		//		}

		//		bimleaker(typeof(string));
		//		bimleaker(typeof(Mesh));
		//		bimleaker(typeof(Texture));
		//		bimleaker(typeof(Material));
		//		bimleaker(typeof(Texture2D));
		//		Log.Warning(typepcounter.Count + " cached types");
		//	}

		//	var strang = "";


		//	foreach (var allType in typepcounter.Keys.ToList())
		//	{
		//		int counts = 0;

		//		try
		//		{
		//			counts = Mesh.FindObjectsOfType(allType).Length;
		//		}
		//		catch (Exception e)
		//		{

		//		}
		//		;

		//		if (counts > typepcounter[allType])
		//		{
		//			strang += $"\n+{counts - typepcounter[allType]} {allType.Name}";
		//		}

		//		try
		//		{
		//			typepcounter[allType] = counts;
		//		}
		//		catch (Exception e)
		//		{
		//			//typepcounter.Add(allType, counts);
		//		}

		//	}
		//	TextEditor te = new TextEditor();
		//	te.text = strang;
		//	te.OnFocus();
		//	te.Copy();
		//	Log.Warning(strang);

		//}

		//public static Dictionary<Type, int> typepcounter = null;



		//[HarmonyPatch(typeof(PawnCacheRenderer), nameof(PawnCacheRenderer.RenderPawn))]
		//public static class H_PawnCacheRendererRenderPawn
		//{
		//	private static void Prefix(Pawn pawn, ref bool renderBody, ref bool renderHeadgear, ref bool renderClothes, bool portrait)
		//	{
		//		if (portrait)
		//		{
		//			return;
		//		}

		//		if (pawn.CurJob == null)
		//		{
		//			return;
		//		}

		//		if (renderBody == false && !pawn.Awake() && Settings.HatsSleeping)
		//		{
		//			renderHeadgear = false;
		//		}

		//		if (renderBody == true && !pawn.Position.UsesOutdoorTemperature(pawn.Map))
		//		{
		//			//renderHeadgear = false;
		//		}
		//	}
		//}

		//[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.ShellFullyCoversHead))]
		//public static class H_ShellFullyCoversHead
		//{
		//	private static void Postfix(PawnRenderer __instance, PawnRenderFlags flags, ref bool __result)
		//	{
		//		if (Settings.hairfilter.Contains(__instance.pawn.story.hairDef))
		//		{
		//			__result = false;
		//		}
		//		else
		//		{
		//			__result = false;
		//		}
		//	}
		//}

		//[HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawHeadHair))]
		//public static class H_DrawHeadHair
		//{
		//	private static void Prefix(PawnRenderer __instance, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags)
		//	{
		//		if (!Settings.ShowHairUnderHats)
		//		{
		//			return;
		//		}


		//		bool flag11 = bodyDrawType != RotDrawMode.Dessicated && !flags.FlagSet(PawnRenderFlags.HeadStump);
		//		if (flag11)
		//		{
		//			if (Settings.hairfilter.Contains(__instance.pawn.story.hairDef))
		//			{
		//				return;
		//			}

		//			if (__instance.ShellFullyCoversHead(flags))
		//			{
		//				return;
		//			}

		//			Vector3 vector = rootLoc + headOffset;
		//			vector.y += 0.0289575271f;
		//			Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
		//			Mesh mesh4 = __instance.graphics.HairMeshSet.MeshAt(headFacing);
		//			Material material4 = __instance.graphics.HairMatAt(headFacing, flags.FlagSet(PawnRenderFlags.Portrait), flags.FlagSet(PawnRenderFlags.Cache));
		//			bool flag12 = material4 != null;
		//			if (flag12)
		//			{
		//				GenDraw.DrawMeshNowOrLater(mesh4, vector, quat, material4, flags.FlagSet(PawnRenderFlags.DrawNow));
		//			}
		//		}
		//	}
		//}

		//[HarmonyPatch(typeof(PawnRenderer), "<DrawHeadHair>g__DrawApparel|39_0")]
		//public static class H_g__DrawApparel
		//{
		//	private static void Prefix(ApparelGraphicRecord apparelRecord)
		//	{
		//		if (Settings.ShowHairUnderHats && Math.Abs(Settings.hairMeshScale) > 0.001f)
		//		{
		//			if (Settings.hairfilter.Contains(apparelRecord.sourceApparel.Wearer.story.hairDef))
		//			{
		//				//	HairGotFiltered = true;
		//				return;
		//			}
		//			H_g__MeshAt.Dewit = true;
		//		}
		//	}

		//	private static void Postfix(ApparelGraphicRecord apparelRecord)
		//	{
		//		H_g__MeshAt.Dewit = false;
		//	}
		//}

		//[HarmonyPatch(typeof(GenDraw), "DrawMeshNowOrLater", new[] { typeof(Mesh), typeof(Vector3), typeof(Quaternion), typeof(Material), typeof(bool) })]
		//public static class H_g__MeshAt
		//{
		//	public static Dictionary<Mesh, Mesh> scalers = new Dictionary<Mesh, Mesh>();

			//	public static bool Dewit = false;
			//	private static void Prefix(ref Mesh mesh)
			//	{
			//		if (Dewit)
			//		{
			//			try
			//			{
			//				mesh = scalers[mesh];
			//			}
			//			catch
			//			{
			//				scalers[mesh] = bs.MeshHead(mesh, Settings.hairMeshScale);
			//				mesh = scalers[mesh];
			//			}

			//			//mesh = H_RenderPawn.MeshScaler() MeshHead(mesh, Settings.hairMeshScale);
			//		}

			//		H_g__MeshAt.Dewit = false;
			//	}
		//}

		public static Harmony harmony;

		public static ApparelLayerDef MiddleHead;

		private static readonly MethodInfo DrawHeadHair = AccessTools.Method(typeof(PawnRenderer), nameof(PawnRenderer.DrawHeadHair));
		private static readonly MethodInfo g__DrawApparel = AccessTools.Method(typeof(PawnRenderer), "<DrawHeadHair>g__DrawApparel|39_0");


		private static readonly HarmonyMethod DrawHeadHairTranspiler = new HarmonyMethod(typeof(H_RenderPawn).GetMethod(nameof(H_RenderPawn.DrawHeadHairTranspiler)));
		private static readonly HarmonyMethod DrawHeadgearTranspiler = new HarmonyMethod(typeof(H_RenderPawn).GetMethod(nameof(H_RenderPawn.DrawHeadgearTranspiler)));

		public static readonly string harmonyID = "Quickfast";


		public static void hairScale_Changed()
		{
			H_RenderPawn.scalers.Clear();
			if (Find.CurrentMap != null)
			{
				foreach (var p in Find.CurrentMap.mapPawns.FreeColonists)
				{
					p.apparel.Notify_ApparelChanged();
				}
			}
		}

		static bs()
		{



			harmony = new Harmony(harmonyID);
			harmony.PatchAll();

			var b__0 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass3_0:<LayDown>b__0");
			var b__2 = AccessTools.Method("RimWorld.Toils_LayDown+<>c__DisplayClass3_0:<LayDown>b__2");

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


			// fixes for mods

			MiddleHead = DefDatabase<ApparelLayerDef>.GetNamedSilentFail("MiddleHead");

			var CEdrawhair =
				AccessTools.Method("CombatExtended.HarmonyCE.Harmony_PawnRenderer_RenderPawnInternal:DrawHeadApparel");

			if (CEdrawhair != null)
			{
				harmony.Patch(CEdrawhair, null, new HarmonyMethod(typeof(bs).GetMethod(nameof(killme))));
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
			harmony.Patch(DrawHeadHair, transpiler: DrawHeadHairTranspiler);
			harmony.Patch(g__DrawApparel, transpiler: DrawHeadgearTranspiler);
			//Log.Warning("Applied transpiler to RenderPawnInternal to show hair under hats and rescale hats");
		}

		public static void RemoveTrans()
		{
			harmony.Unpatch(DrawHeadHair, HarmonyPatchType.Transpiler, harmonyID);
			harmony.Unpatch(g__DrawApparel, HarmonyPatchType.Transpiler, harmonyID);
		}


		public static void fix_0(object __instance)
		{
			if (!Settings.HatsSleeping)
			{
				return;
			}

			if (AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) is Toil toil)
			{
				toil?.actor?.apparel?.Notify_ApparelChanged();

				var bed = toil?.actor.CurrentBed();
				if (bed != null && toil.actor.RaceProps.Humanlike && !bed.def.building.bed_showSleeperBody)
				{
					toil.actor.Drawer.renderer.graphics.ClearCache();
					toil.actor.Drawer.renderer.graphics.apparelGraphics.Clear();
				}
			}
		}

		public static void fix_2(object __instance)
		{
			if (!Settings.HatsSleeping)
			{
				return;
			}

			if (AccessTools.Field(__instance.GetType(), "layDown").GetValue(__instance) is Toil toil && toil.actor.RaceProps.Humanlike)
			{
				toil?.actor?.apparel?.Notify_ApparelChanged();
				toil.actor.Drawer.renderer.graphics.ResolveApparelGraphics();
				PortraitsCache.SetDirty(toil.actor);
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
			if (UnityData.IsInMainThread is false) return;

			pawn.apparel.Notify_ApparelChanged();
			var graphics = pawn?.Drawer?.renderer?.graphics;
			if (graphics == null) return;

			if ((Settings.DraftedHidingMode && pawn.Drafted is false) || (Settings.DraftedHidingMode is false && !pawn.Position.UsesOutdoorTemperature(pawn.Map)))
			{
				if (Settings.HideJackets is true)
				{
					if (graphics.apparelGraphics.Any(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.OnSkin))
					{
						graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell);
					}
				}

				if (Settings.HideEquipment is true)
				{
					graphics.apparelGraphics.RemoveAll(x => x.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Belt);
				}

				if (Settings.HideHats is true)
				{
					bool Match(ApparelGraphicRecord x)
					{
						return x.sourceApparel.def.apparel.layers.Any(z => z == Overhead) && !Settings.hatfilter.Contains(x.sourceApparel.def);
					}

					var hidden = graphics.apparelGraphics.RemoveAll(Match);
				}
			}
		}

		public static void SwitchOutdoors(Pawn pawn)
		{
			if (UnityData.IsInMainThread is false) return;

			pawn.apparel.Notify_ApparelChanged();
			var graphics = pawn?.Drawer?.renderer?.graphics;
			if (graphics == null)
			{
				return;
			}


			//pawn.apparel.Notify_ApparelChanged();
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
			//PortraitsCache.SetDirty(pawn);
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
				if (Settings.DraftedHidingMode)
				{
					if (pawn.Drafted)
					{
						SwitchOutdoors(pawn);
					}
					else
					{
						SwitchIndoors(pawn);
					}
				}
				else
				{
					if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
					{
						SwitchOutdoors(pawn);
					}
					else
					{
						SwitchIndoors(pawn);
					}
				}

				return;
			}

			if (Settings.DraftedHidingMode)
			{
				return;
			}

			//if (nextCell.UsesOutdoorTemperature(pawn.MapHeld))
			//{
			//    SwitchOutdoors(pawn);
			//}
			//else
			//{
			//    SwitchIndoors(pawn);
			//}

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

		public static bool HairGotFiltered;

		public static Dictionary<Mesh, Mesh> scalers = new Dictionary<Mesh, Mesh>();


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

		//public static Vector3 offset(Vector3 vec)
		//{
		//	if (HairGotFiltered)
		//	{
		//		HairGotFiltered = false;
		//		return vec;
		//	}

		//	vec.y += -0.0036f;
		//	return vec;
		//}

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

		public static IEnumerable<CodeInstruction> DrawHeadgearTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var f_scaler = false;
			var ins_l = instructions.ToList();

			for (var i = 0; i < ins_l.Count - 1; i++)
			{
				var ins = ins_l[i];
				if (ins.op(OpCodes.Stloc_0) && ins_l[i + 1].op(OpCodes.Ldarg_1))
				{
					f_scaler = true;
				}
			}

			if (f_scaler is false)
			{
				Log.Warning("Failed inject m_MeshScaler - hair under hats wont work F");
			}

			if (!f_scaler)
			{
				foreach (var codeInstruction in ins_l) yield return codeInstruction;
			}
			else
			{
				f_scaler = false;
				for (var i = 0; i < ins_l.Count; i++)
				{
					var ins = ins_l[i];
					if (f_scaler is false && ins.op(OpCodes.Stloc_0) && ins_l[i + 1].op(OpCodes.Ldarg_1))
					{
						f_scaler = true;
						yield return ins;
						yield return new CodeInstruction(OpCodes.Ldarg_0);
						yield return new CodeInstruction(OpCodes.Ldloc_0);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(MeshScaler)));
						yield return new CodeInstruction(OpCodes.Stloc_0);
					}
					else
					{
						yield return ins;
					}
				}
			}
		}

		public static IEnumerable<CodeInstruction> DrawHeadHairTranspiler(IEnumerable<CodeInstruction> instructions)
		{
			var f_shouldrender = false;
			//var f_offset = false;

			var ins_l = instructions.ToList();

			//check its possible before bothering

			for (var i = 0; i < ins_l.Count - 1; i++)
			{
				var ins = ins_l[i];
				if (ins.op(OpCodes.Ldloc_2))
				{
					f_shouldrender = true;
				}
				else if (ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
				{
					//f_offset = true;
				}
			}

			if (f_shouldrender is false)
			{
				Log.Warning("Failed inject HairGotFiltered - hair under hats wont work F");
			}

			//if (f_offset is false)
			//{
			//	Log.Warning("Failed inject m_offset - hair under hats wont work F");
			//}

			if (!f_shouldrender)
			{
				foreach (var codeInstruction in ins_l) yield return codeInstruction;
			}
			else
			{
				f_shouldrender = false;
				//f_offset = false;

				for (var i = 0; i < ins_l.Count; i++)
				{
					var ins = ins_l[i];

					if (f_shouldrender is false && ins.op(OpCodes.Ldloc_2))
					{
						yield return new CodeInstruction(OpCodes.Ldloc_2);
						yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(ShouldRenderHair)));
						// yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(H_RenderPawn), nameof(HairGotFiltered)));
						yield return new CodeInstruction(OpCodes.Stloc_2);
						yield return ins;
						f_shouldrender = true;
					}
					//else if (f_offset is false && ins.oploc(OpCodes.Stloc_S, 20) && ins_l[i + 1].oploc(OpCodes.Ldloc_S, 13))
					//{
					//	f_offset = true;
					//	yield return ins;
					//	yield return new CodeInstruction(OpCodes.Ldloc_S, 13);
					//	yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(H_RenderPawn), nameof(offset)));
					//	yield return new CodeInstruction(OpCodes.Stloc_S, 13);
					//}
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
			if (__instance.draftedInt)
			{
				bs.SwitchOutdoors(__instance.pawn);
			}
			else
			{
				if (Settings.DraftedHidingMode || __instance.pawn.Position.UsesOutdoorTemperature(__instance.pawn.MapHeld) is false)
				{
					bs.SwitchIndoors(__instance.pawn);
				}
				else
				{
					bs.SwitchOutdoors(__instance.pawn);
				}
			}
		}
	}

	//internal class GameComponent_quickfast : GameComponent
	//{
	//	private int jankyfix;

	//	public GameComponent_quickfast(Game game)
	//	{
	//	}

	//	public override void GameComponentTick()
	//	{
	//		if (jankyfix < 2)
	//		{
	//			jankyfix++;
	//			if (!Settings.HatsSleeping)
	//			{
	//				return;
	//			}

	//			foreach (var map in Find.Maps)
	//			{
	//				foreach (var instanceMapPawn in map.mapPawns.AllPawns)
	//				{
	//					var bed = instanceMapPawn.CurrentBed();
	//					if (instanceMapPawn.InBed() && instanceMapPawn.RaceProps.Humanlike &&
	//						!bed.def.building.bed_showSleeperBody)
	//					{
	//						instanceMapPawn.Drawer.renderer.graphics.ClearCache();
	//						instanceMapPawn.Drawer.renderer.graphics.apparelGraphics.Clear();
	//						PortraitsCache.SetDirty(instanceMapPawn);
	//					}
	//				}
	//			}
	//		}
	//	}
	//}


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
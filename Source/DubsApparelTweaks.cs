using System.Collections.Generic;

using RimWorld;

using UnityEngine;

using Verse;

namespace QuickFast
{
	public class DubsApparelTweaks : Mod
	{
		public static Settings Settings;

		public DubsApparelTweaks(ModContentPack content) : base(content)
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
		public static float EquipModPC = 0.2f;
		public static int EquipModTicks = 10;
		public static bool FlatRate = true;
		public static bool HatsSleeping = true;
		public static bool HideHats = true;
		public static bool HideEquipment = true;
		public static bool HideJackets = true;
		//public static bool EquipmentOnlyWhileDrafted;
		public static bool HatsOnlyWhileDrafted;
		//public static bool JacketsOnlyWhileDrafted;
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

			lis.GapLine();

			lis.CheckboxLabeled("Hide_hats_when_indoors".Translate(), ref HideHats);
			lis.CheckboxLabeled("Hide_jackets_when_indoors".Translate(), ref HideJackets);
			lis.CheckboxLabeled("Hide_equipment_when_indoors".Translate(), ref HideEquipment);

			if (lis.RadioButton_NewTemp("IndoorHidingMode".Translate(), HatsOnlyWhileDrafted is false, 10))
			{
				HatsOnlyWhileDrafted = false;
			}

			if (lis.RadioButton_NewTemp("DraftedHidingMode".Translate(), HatsOnlyWhileDrafted, 10))
			{
				HatsOnlyWhileDrafted = true;
			}






			//lis.CheckboxLabeled("Hats_only_while_drafted".Translate(), ref HatsOnlyWhileDrafted);
			//lis.CheckboxLabeled("Jackets_only_while_drafted".Translate(), ref JacketsOnlyWhileDrafted);
			//lis.CheckboxLabeled("Equipment_only_while_drafted".Translate(), ref EquipmentOnlyWhileDrafted);

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
					bs.ApplyTrans();
				}
				else
				{
					bs.RemoveTrans();
				}
			}

			if (ShowHairUnderHats)
			{
				lis.LabelDouble("HatScaling".Translate(), $"{hairMeshScale}");
				var tamw = decimal.Round((decimal)lis.Slider(hairMeshScale, 0.9f, 1.2f), 3);
				if (tamw != (decimal)hairMeshScale)
				{
					hairMeshScale = (float)tamw;
					H_RenderPawn.hairScale_Changed();
				}

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
			Scribe_Values.Look(ref ChangeEquipSpeed, "ChangeEquipSpeed");
			Scribe_Values.Look(ref HatsOnlyWhileDrafted, "HatsOnlyWhileDrafted");
			//Scribe_Values.Look(ref EquipmentOnlyWhileDrafted, "EquipmentOnlyWhileDrafted");
			//Scribe_Values.Look(ref JacketsOnlyWhileDrafted, "JacketsOnlyWhileDrafted");
			Scribe_Values.Look(ref ShowHairUnderHats, "ShowHairUnderHats");
			Scribe_Values.Look(ref FlatRate, "FlatRate");
			Scribe_Values.Look(ref HideHats, "HatsIndoors");
			Scribe_Values.Look(ref HatsSleeping, "HatsSleeping");
			Scribe_Values.Look(ref EquipModPC, "EquipModPC", 0.2f);
			Scribe_Values.Look(ref EquipModTicks, "EquipModTicks", 10);
			Scribe_Values.Look(ref HideJackets, "HideJackets");
			Scribe_Values.Look(ref HideEquipment, "HideEquipment");
			Scribe_Collections.Look(ref DefToStrings, "hairFilter", LookMode.Value);
			Scribe_Collections.Look(ref HatDefToStrings, "hatFilter", LookMode.Value);
		}
	}
}
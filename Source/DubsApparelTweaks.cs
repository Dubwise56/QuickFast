using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuickFast.Source
{
    public class DubsApparelTweaks : Mod
    {
        public static Settings Settings;

        public static Harmony harmony;

        public static ApparelLayerDef MiddleHead;
        public static ApparelLayerDef Overhead => MiddleHead ?? ApparelLayerDefOf.Overhead;

        public static readonly string harmonyID = "DubsApparelTweaks.QuickFast.LittleBoy";

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }

        public override string SettingsCategory()
        {
            return "Dubs Apparel Tweaks";
        }

        public DubsApparelTweaks(ModContentPack content) : base(content)
        {
            Settings = GetSettings<Settings>();

            MiddleHead = DefDatabase<ApparelLayerDef>.GetNamedSilentFail("MiddleHead");

            harmony = new Harmony(harmonyID);
            harmony.PatchAll();
        }
    }

    public enum ApparelHidingMode
    {
        Never,
        WhenNotDrafted,
        IndoorsOrNotDrafted
    }


    public class Settings : ModSettings
    {
        private static List<ThingDef> _hatfilter;
        private static List<HairDef> _hairfilter;

        public static bool ShowHairUnderHats = true;
        public static float hairMeshScale = 1.01f;

        public static bool ChangeEquipSpeed = true;
        public static float EquipModPC = 0.2f;
        public static int EquipModTicks = 10;
        public static bool FlatRate = true;

        public static ApparelHidingMode apparelHidingMode;

        public static HashSet<string> LayerVis = new HashSet<string>();

        public class HairHatSet : IExposable
        {
            public string Hair;

            public HashSet<string> Hats = new HashSet<string>();

            public void ExposeData()
            {
                Scribe_Values.Look(ref Hair, "Hair");
                Scribe_Collections.Look(ref Hats, "Hats", LookMode.Value);
                if (Scribe.mode == LoadSaveMode.PostLoadInit)
                {
                    if (Hats == null)
                    {
                        Hats = new HashSet<string>();
                    }
                }
            }
        }

        public static List<HairHatSet> HatHairCombo = new List<HairHatSet>();


        public static List<string> HatDefToStrings = new List<string>();

        public static List<string> DefToStrings = new List<string>();
        private string buf;
        private Listing_Standard Listing;

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
            Listing = new Listing_Standard();
            Listing.ColumnWidth = (nifta.width - 40f) / 2f;

            Listing.Begin(canvas);

            Text.Font = GameFont.Medium;
            Listing.Label("Apparel_equip_speed".Translate());
            Listing.Gap();
            Text.Font = GameFont.Small;
            Listing.CheckboxLabeled("Change_equip_speeds".Translate(), ref ChangeEquipSpeed);
            Listing.Gap();
            if (ChangeEquipSpeed)
            {
                Listing.CheckboxLabeled("Same_speed_for_all_apparel".Translate(), ref FlatRate);

                if (FlatRate)
                {
                    Listing.LabelDouble("Equip_speed_Ticks".Translate(), $"{EquipModTicks} ticks");
                    Listing.IntEntry(ref EquipModTicks, ref buf);
                }
                else
                {
                    Listing.LabelDouble("Equip_duration".Translate(), $"{EquipModPC.ToStringPercent()}");
                    EquipModPC = Listing.Slider(EquipModPC, 0f, 1f);
                }
            }

            Listing.Gap();
            Listing.GapLine();
            Text.Font = GameFont.Medium;
            Listing.Label("Apparel_visibility".Translate());
            Text.Font = GameFont.Small;
            Listing.Gap();
            GUI.color = Color.yellow;
            Listing.Label("HatFiltersTip".Translate());
            GUI.color = Color.white;
            Listing.Gap();
            if (Listing.ButtonText($"Hide apparel: {apparelHidingMode}"))
            {
                Find.WindowStack.Add(new FloatMenu(new List<FloatMenuOption>
                {
                    new FloatMenuOption("Never", () => apparelHidingMode = ApparelHidingMode.Never),
                    new FloatMenuOption("When not drafted", () => apparelHidingMode = ApparelHidingMode.WhenNotDrafted),
                    new FloatMenuOption("When indoors OR not drafted", () => apparelHidingMode = ApparelHidingMode.IndoorsOrNotDrafted)
                }));
            }

            if (apparelHidingMode != ApparelHidingMode.Never)
            {
                Listing.Gap();
                Listing.Label("VisibilityTogglesDesc".Translate());
                DrawLayerTogglesListbox(Listing, ref graoner1, nifta.height - Listing.CurHeight, ref scrollPosition1, DefDatabase<ApparelLayerDef>.AllDefsListForReading);
            }

            Listing.NewColumn();
            Text.Font = GameFont.Medium;
            Listing.Label("Hair_visibility".Translate());
            Text.Font = GameFont.Small;

            Listing.CheckboxLabeled("Show_hair_under_hats".Translate(), ref ShowHairUnderHats);

            if (ShowHairUnderHats)
            {
                Listing.LabelDouble("HatScaling".Translate(), $"{hairMeshScale}");
                var tamw = decimal.Round((decimal)Listing.Slider(hairMeshScale, 0.9f, 1.2f), 3);
                if (tamw != (decimal)hairMeshScale)
                {
                    hairMeshScale = (float)tamw;
                    hairScale_Changed();
                }

                if (Listing.ButtonText("HairScaleReset".Translate()))
                {
                    hairMeshScale = 1.016f;
                    hairScale_Changed();
                }

                GUI.color = Color.yellow;
                Listing.Label("HairFiltersTip".Translate());
                Listing.Label("HatHairFiltersTip".Translate());
                GUI.color = Color.white;
                Listing.GapLine();

                DrawHatHairComboList(Listing, ref graoner2, nifta.height - Listing.CurHeight, ref scrollPosition2, Settings.HatHairCombo);
            }


            Listing.End();
        }


        public static float graoner1 = 50f;
        public Vector2 scrollPosition1;

        public static void DrawHatHairComboList(Listing_Standard listing, ref float groaner, float height, ref Vector2 scrolpos, List<HairHatSet> list)
        {
            var rect = listing.GetRect(height);
            rect.width = 300;
            Text.Font = GameFont.Small;

            var innyrek = rect;
            innyrek.width -= 16f;
            innyrek.height = groaner;

            Widgets.BeginScrollView(rect, ref scrolpos, innyrek);

            GUI.BeginGroup(innyrek);
            var lineHeight = Text.LineHeight;
            float y = 0;
            foreach (var t in list)
            {
                foreach (var tHat in t.Hats)
                {
                    var rec = new Rect(0, y, innyrek.width, lineHeight);
                    Widgets.DrawHighlightIfMouseover(rec);
                    Widgets.Label(rec, $"[{t.Hair}] + {tHat}");

                    y += lineHeight + 3f;
                }
            }

            GUI.EndGroup();
            groaner = y + 25f;

            Widgets.EndScrollView();

            listing.Gap(listing.verticalSpacing);
        }


        public static float graoner2 = 50f;
        public Vector2 scrollPosition2;

        public static void DrawLayerTogglesListbox(Listing_Standard listing, ref float groaner, float height, ref Vector2 scrolpos, List<ApparelLayerDef> list)
        {
            var rect = listing.GetRect(height);
            rect.width = 300;
            Text.Font = GameFont.Small;

            var innyrek = rect;
            innyrek.width -= 16f;
            innyrek.height = groaner;

            Widgets.BeginScrollView(rect, ref scrolpos, innyrek);

            GUI.BeginGroup(innyrek);
            var lineHeight = Text.LineHeight;
            float y = 0;
            foreach (var t in list)
            {
                var biff = LayerVis.Contains(t.defName);
                var jiff = biff;
                var rec = new Rect(0, y, innyrek.width, lineHeight);
                Widgets.DrawHighlightIfMouseover(rec);
                Widgets.CheckboxLabeled(rec, t.LabelCap, ref biff);

                if (jiff != biff)
                {
                    if (biff)
                    {
                        LayerVis.Add(t.defName);
                    }
                    else
                    {
                        LayerVis.Remove(t.defName);
                    }

                    hairScale_Changed();
                }

                y += lineHeight + 3f;
            }

            GUI.EndGroup();
            groaner = y + 25f;

            Widgets.EndScrollView();

            listing.Gap(listing.verticalSpacing);
        }


        public static void hairScale_Changed()
        {
            if (Find.CurrentMap != null)
            {
                foreach (var p in Find.CurrentMap.mapPawns.FreeColonists)
                {
                    p.apparel?.Notify_ApparelChanged();
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref hairMeshScale, "hairMeshScale", 1.06f);
            Scribe_Values.Look(ref ChangeEquipSpeed, "ChangeEquipSpeed");
            Scribe_Values.Look(ref apparelHidingMode, "apparelHidingMode");
            Scribe_Values.Look(ref ShowHairUnderHats, "ShowHairUnderHats", true);
            Scribe_Values.Look(ref FlatRate, "FlatRate");
            Scribe_Values.Look(ref EquipModPC, "EquipModPC", 0.2f);
            Scribe_Values.Look(ref EquipModTicks, "EquipModTicks", 10);
            Scribe_Collections.Look(ref DefToStrings, "hairFilter", LookMode.Value);
            Scribe_Collections.Look(ref HatDefToStrings, "hatFilter", LookMode.Value);
            Scribe_Collections.Look(ref LayerVis, "LayerVis", LookMode.Value);
            Scribe_Collections.Look(ref HatHairCombo, "HatHairCombo", LookMode.Deep);

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (LayerVis == null)
                {
                    LayerVis = new HashSet<string>();
                }

                if (HatHairCombo == null)
                {
                    HatHairCombo = new List<HairHatSet>();
                }
            }

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                HatHairCombo.RemoveAll(x => string.IsNullOrEmpty(x.Hair));
            }
        }
    }
}
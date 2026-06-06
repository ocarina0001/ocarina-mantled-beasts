using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute { }

    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Window_ColorPicker : Window
    {
        private Color colorOne;
        private Color oldColorOne;
        private Color colorTwo;
        private Color oldColorTwo;
        private string hexColorOne, hexColorTwo;
        private bool colorTwoChosen;
        private FurGene furGene;
        private bool hsvColorWheelDragging;
        private string[] textfieldBuffersOne = new string[6];
        private string[] textfieldBuffersTwo = new string[6];
        private Color textfieldColorBufferOne, textfieldColorBufferTwo;
        private string previousFocusedControlName;
        public static Widgets.ColorComponents visibleColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;
        public static Widgets.ColorComponents editableColorTextfields = Widgets.ColorComponents.Hue | Widgets.ColorComponents.Sat;
        public override Vector2 InitialSize => new Vector2(800f, 410f);
        private List<FurColorDef> furColors;
        private Rot4 pawnRot = Rot4.South;
        private List<ColorMaskEntry> colorMasks;
        private int selectedMaskIndex;
        private string selectedMaskName;

        public Window_ColorPicker(FurGene furGene)
        {
            this.doCloseX = true;
            colorOne = furGene.colorOne;
            oldColorOne = colorOne;
            colorTwo = furGene.colorTwo ?? Color.white;
            oldColorTwo = colorTwo;
            hexColorOne = "#" + ColorUtility.ToHtmlStringRGB(colorOne);
            hexColorTwo = "#" + ColorUtility.ToHtmlStringRGB(colorTwo);
            this.furGene = furGene;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
            furColors = new List<FurColorDef>();
            var extension = furGene.def.GetModExtension<FurColors>();
            foreach (var furColorDef in extension.allowedFurColors.OrderBy(x => x.displayOrder))
            {
                furColors.Add(furColorDef);
            }
            var maskExtension = furGene.def.GetModExtension<ColorMasks>();
            colorMasks = maskExtension?.allowedColorMasks?.OrderBy(m => m.maskName).ToList() ?? new List<ColorMaskEntry>();
            selectedMaskName = furGene.selectedMaskName;
            if (colorMasks.Any())
            {
                selectedMaskIndex = colorMasks.FindIndex(m => m.maskName == selectedMaskName);
                if (selectedMaskIndex < 0) selectedMaskIndex = -1;
            }
            else
            {
                selectedMaskIndex = -1;
            }
            //if (colorMasks.Any())
            //{
            //    Log.Message($"[MantledBeasts] Loaded {colorMasks.Count} masks for gene {furGene.def.defName}: " + string.Join(", ", colorMasks.Select(m => m.maskName)));
            //}
            //else
            //{
            //    Log.Message($"[MantledBeasts] No masks defined for gene {furGene.def.defName}. Dropdown will still appear for testing.");
            //}
            //Log.Message($"[MantledBeasts] Current selectedMaskName = {selectedMaskName ?? "null"}");
        }

        private static void HeaderRow(ref RectDivider layout)
        {
            using (new TextBlock(GameFont.Medium))
            {
                TaggedString taggedString = "MantledBeasts_ColorPicker.ChangeFurColors".Translate().CapitalizeFirst();
                RectDivider rectDivider = layout.NewRow(Text.CalcHeight(taggedString, layout.Rect.width));
                GUI.SetNextControlName(Dialog_ColorPickerBase.focusableControlNames[0]);
                Widgets.Label(rectDivider, taggedString);
            }
        }

        private void BottomButtons(ref RectDivider layout)
        {
            RectDivider rectDivider = layout.NewRow(Dialog_ColorPickerBase.ButSize.y, VerticalJustification.Bottom);
            if (Widgets.ButtonText(rectDivider.NewCol(Dialog_ColorPickerBase.ButSize.x), "Cancel".Translate()))
                Close();
            if (Widgets.ButtonText(rectDivider.NewCol(Dialog_ColorPickerBase.ButSize.x, HorizontalJustification.Right), "Accept".Translate()))
            {
                furGene.selectedMaskName = selectedMaskName;
                if (colorOne != oldColorOne)
                    furGene.colorOne = colorOne;
                if (colorTwo != oldColorTwo)
                    furGene.colorTwo = colorTwo;
                Close();
            }
        }

        public override void Close(bool doCloseSound = true)
        {
            base.Close(doCloseSound);
            furGene.ApplyColors();
        }

        private void ColorFields(ref RectDivider layout, ref Color color, ref string hexValue, ref string[] textfieldBuffers, ref Color textfieldColorBuffer, out Vector2 size)
        {
            RectAggregator aggregator = new RectAggregator(new Rect(layout.Rect.position, new Vector2(125f, 0f)), 195906069);
            var editableComponents = Widgets.ColorComponents.Red | Widgets.ColorComponents.Green | Widgets.ColorComponents.Blue;
            var visibleComponents = Widgets.ColorComponents.Red | Widgets.ColorComponents.Green | Widgets.ColorComponents.Blue;
            bool colorChanged = Widgets.ColorTextfields(
                ref aggregator,
                ref color,
                ref textfieldBuffers,
                ref textfieldColorBuffer,
                previousFocusedControlName,
                "colorTextfields",
                editableComponents,
                visibleComponents
            );
            size = aggregator.Rect.size;
            if (colorChanged)
                hexValue = "#" + ColorUtility.ToHtmlStringRGB(color);
            var hexRectLabel = new Rect(layout.Rect.x, aggregator.Rect.yMax + 4, 50, 32);
            using (new TextBlock(TextAnchor.MiddleLeft))
                Widgets.Label(hexRectLabel, "MantledBeasts_ColorPicker.HexCode".Translate());
            var hexRect = new Rect(hexRectLabel.xMax, hexRectLabel.y, 125 - 50, 32);
            string oldHex = hexValue;
            hexValue = Widgets.TextField(hexRect, hexValue).Trim();
            if (hexValue != oldHex && Utils.TryGetColorFromHex(hexValue, out var tempColor))
                color = tempColor;
            if (Event.current.type == EventType.Layout)
                previousFocusedControlName = GUI.GetNameOfFocusedControl();
        }

        private static void ColorReadback(Rect rect, Color color)
        {
            TaggedString label = "MantledBeasts_CurrentColor".Translate().CapitalizeFirst();
            float labelWidth = Mathf.Max(100f, label.GetWidthCached());
            RectDivider rectDivider = new RectDivider(rect, 195906069);
            RectDivider row = rectDivider.NewRow(Text.LineHeight);
            Widgets.Label(row.NewCol(labelWidth), label);
            Widgets.DrawBoxSolid(row.NewCol(125f, HorizontalJustification.Left), color);
        }

        private static void TabControl()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
            {
                bool num = !Event.current.shift;
                Event.current.Use();
                string text = GUI.GetNameOfFocusedControl();
                if (text.NullOrEmpty())
                    text = Dialog_ColorPickerBase.focusableControlNames[0];
                int num2 = Dialog_ColorPickerBase.focusableControlNames.IndexOf(text);
                if (num2 < 0)
                    num2 = Dialog_ColorPickerBase.focusableControlNames.Count;
                num2 = ((!num) ? (num2 - 1) : (num2 + 1));
                if (num2 >= Dialog_ColorPickerBase.focusableControlNames.Count)
                    num2 = 0;
                else if (num2 < 0)
                    num2 = Dialog_ColorPickerBase.focusableControlNames.Count - 1;
                GUI.FocusControl(Dialog_ColorPickerBase.focusableControlNames[num2]);
            }
        }

        //private static readonly Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);
        private static readonly Texture2D RotateButton = ContentFinder<Texture2D>.Get("UI/Widgets/RotRight");
        public override void DoWindowContents(Rect inRect)
        {
            using (TextBlock.Default())
            {
                var activeColor = colorTwoChosen ? colorTwo : colorOne;
                var otherColor = colorTwoChosen ? colorOne : colorTwo;
                var portrait = new Rect(inRect.x, inRect.y, 190, 240);
                Widgets.DrawMenuSection(portrait);
                var oldColors = (furGene.colorOne, furGene.colorTwo);
                string oldMaskName = furGene.selectedMaskName;
                furGene.colorOne = colorTwoChosen ? otherColor : activeColor;
                furGene.colorTwo = colorTwoChosen ? activeColor : otherColor;
                furGene.selectedMaskName = selectedMaskName;
                furGene.ApplyColors();
                RenderTexture image = PortraitsCache.Get(furGene.pawn, new Vector2(200, 200), pawnRot, new Vector3(0, 0, 0.1f), healthStateOverride: PawnHealthState.Mobile, cameraZoom: 1.1f, renderClothes: false, renderHeadgear: false);
                GUI.DrawTexture(portrait, image, ScaleMode.ScaleAndCrop);
                furGene.colorOne = oldColors.colorOne;
                furGene.colorTwo = oldColors.colorTwo;
                furGene.selectedMaskName = oldMaskName;
                furGene.ApplyColors();
                var buttonRotate = new Rect(portrait.xMax - 24, portrait.y, 24, 24);
                if (Widgets.ButtonImage(buttonRotate, RotateButton))
                    pawnRot = pawnRot.Rotated(RotationDirection.Clockwise);
                var layoutRect = new Rect(inRect.x + 200, inRect.y, inRect.width - 200, 240);
                RectDivider layout = new RectDivider(layoutRect, 195906069);
                HeaderRow(ref layout);
                layout.NewRow(0f);
                var color = activeColor;
                var oldColor = color;
                ColorPalette(ref layout, ref color, out var paletteHeight);
                if (oldColor != color)
                    ResetColorValues(color);
                Vector2 size;
                if (colorTwoChosen)
                {
                    ColorFields(ref layout, ref color, ref hexColorTwo, ref textfieldBuffersTwo, ref textfieldColorBufferTwo, out size);
                    colorTwo = color;
                }
                else
                {
                    ColorFields(ref layout, ref color, ref hexColorOne, ref textfieldBuffersOne, ref textfieldColorBufferOne, out size);
                    colorOne = color;
                }
                float height = Mathf.Max(paletteHeight, 128f, size.y);
                RectDivider rectDivider = layout.NewRow(height);
                rectDivider.NewCol(size.x);
                rectDivider.NewCol(250f, HorizontalJustification.Right);
                oldColor = color;
                Widgets.HSVColorWheel(rectDivider.Rect.ContractedBy((rectDivider.Rect.width - 128f) / 2f, (rectDivider.Rect.height - 128f) / 2f), ref color, ref hsvColorWheelDragging, 1f);
                if (oldColor != color)
                    ResetColorValues(color);
                if (colorTwoChosen)
                    colorTwo = color;
                else
                    colorOne = color;
                layout = new RectDivider(new Rect(inRect.x, portrait.yMax + 24 + 15, inRect.width,
                    inRect.height - portrait.height - (24 + 15)), 65436135);
                RectDivider readbackRow = layout.NewRow(Text.LineHeight, VerticalJustification.Top);
                Color readbackColor = colorTwoChosen ? colorTwo : colorOne;
                ColorReadback(readbackRow.Rect, readbackColor);
                {
                    float labelWidth = Mathf.Max(100f, "MantledBeasts_CurrentColor".Translate().CapitalizeFirst().GetWidthCached(), "MantledBeasts_OldColor".Translate().CapitalizeFirst().GetWidthCached());
                    RectDivider maskRow = layout.NewRow(24f, VerticalJustification.Top);
                    Widgets.Label(maskRow.NewCol(labelWidth), "MantledBeasts_Mask".Translate().CapitalizeFirst());
                    Rect maskBtnRect = maskRow.NewCol(140f);
                    string maskBtnText = selectedMaskIndex >= 0 && selectedMaskIndex < colorMasks.Count ? colorMasks[selectedMaskIndex].maskName : (colorMasks.Any() ? colorMasks[0].maskName : "None");
                    if (colorMasks.Any())
                    {
                        if (Widgets.ButtonText(maskBtnRect, maskBtnText))
                        {
                            List<FloatMenuOption> options = new List<FloatMenuOption>();
                            for (int i = 0; i < colorMasks.Count; i++)
                            {
                                int index = i;
                                options.Add(new FloatMenuOption(colorMasks[i].maskName, () =>
                                {
                                    selectedMaskIndex = index;
                                    selectedMaskName = colorMasks[index].maskName;
                                }));
                            }
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                    }
                    else
                    {
                        GUI.enabled = false;
                        Widgets.ButtonText(maskBtnRect, maskBtnText);
                        GUI.enabled = true;
                    }
                }
                BottomButtons(ref layout);
                var buttonsRect = new Rect(inRect.x, portrait.yMax + 10, 117, 24);
                Widgets.Label(buttonsRect, "MantledBeasts_ColorPicker.ColorChannel".Translate());
                buttonsRect = new Rect(buttonsRect.xMax, buttonsRect.y, 50, buttonsRect.width);
                var colorChannel = colorTwoChosen ? "MantledBeasts_ColorPicker.ColorB".Translate() : "MantledBeasts_ColorPicker.ColorA".Translate();
                Widgets.Label(buttonsRect, colorChannel);
                if (Widgets.RadioButton(new Vector2(buttonsRect.xMax, buttonsRect.y), !colorTwoChosen))
                    colorTwoChosen = false;
                if (Widgets.RadioButton(new Vector2(buttonsRect.xMax + 40, buttonsRect.y), colorTwoChosen))
                    colorTwoChosen = true;
                TabControl();
                if (Event.current.type == EventType.Layout)
                    previousFocusedControlName = GUI.GetNameOfFocusedControl();
            }
        }

        private void ResetColorValues(Color color)
        {
            if (colorTwoChosen is false)
                hexColorOne = "#" + ColorUtility.ToHtmlStringRGB(color);
            else
                hexColorTwo = "#" + ColorUtility.ToHtmlStringRGB(color);
        }

        //private string ResetHexValues(Color color)
        //{
        //    if (colorTwoChosen is false)
        //    {
        //        hexColorOne = "#" + ColorUtility.ToHtmlStringRGB(color);
        //        return hexColorOne;
        //    }
        //    else
        //    {
        //        hexColorTwo = "#" + ColorUtility.ToHtmlStringRGB(color);
        //        return hexColorTwo; // was returning hexColorOne; fixed typo
        //    }
        //}

        private void ColorPalette(ref RectDivider layout, ref Color color, out float paletteHeight)
        {
            using (new TextBlock(TextAnchor.MiddleLeft))
            {
                RectDivider rectDivider = layout;
                RectDivider rectDivider2 = rectDivider.NewCol(250f, HorizontalJustification.Right);
                var colors = (colorTwoChosen is false ? furColors.Where(x => x.blacklistPrimary is false).Select(x => x.primaryColor).Distinct().ToList() : furColors.Where(x => x.blacklistSecondary is false && x.secondaryColor.HasValue).Select(x => x.secondaryColor.Value).Distinct().ToList());
                colors.SortByColor(x => x);
                Widgets.ColorSelector(rectDivider2, ref color, colors.ToList(), out paletteHeight,
                    extraOnGUI: ColorSelecterExtraOnGUI);
            }
        }

        private void ColorSelecterExtraOnGUI(Color color, Rect boxRect)
        {
            var firstDef = (colorTwoChosen is false ? furColors.FirstOrDefault(x => x.blacklistPrimary is false && x.primaryColor == color) : furColors.FirstOrDefault(x => x.blacklistSecondary is false && x.secondaryColor.HasValue && x.secondaryColor.Value == color));
            if (firstDef != null)
                TooltipHandler.TipRegion(boxRect, firstDef.LabelCap);
        }
    }
}
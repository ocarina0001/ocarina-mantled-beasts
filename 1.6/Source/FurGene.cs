using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    [HotSwappable]
    public class FurGene : Gene
    {
        public FurColorDef furColor;
        public Color colorOne;
        public Color? colorTwo;
        public string selectedMaskName;

        public override void PostAdd()
        {
            base.PostAdd();
            SetFurColor();
        }
        
        private void SetFurColor()
        {
            var extension = def.GetModExtension<FurColors>();
            if (extension.allowedFurColors.TryRandomElementByWeight(x => x.selectionWeight, out var result))
            {
                SetFurColor(result);
            }
            else
            {
                SetColorMask();
            }
        }
        
        private void SetFurColor(FurColorDef colorDef)
        {
            furColor = colorDef;
            colorOne = furColor.primaryColor;
            colorTwo = furColor.secondaryColor;
            SetColorMask();
            ApplyColors();
        }

        private void SetColorMask()
        {
            var maskExtension = def.GetModExtension<ColorMasks>();
            if (maskExtension?.allowedColorMasks != null && maskExtension.allowedColorMasks.Count > 0)
            {
                maskExtension.allowedColorMasks.TryRandomElementByWeight(x => x.selectionWeight, out var result);
                selectedMaskName = result?.maskName;
            }
        }
        
        public Graphic GetGraphicOverriden(Graphic original, PawnRenderNode source, Pawn pawn)
        {
            if (furColor is null)
            {
                SetFurColor();
            }
            var color1 = GetColorOne(source);
            var color2 = GetColorTwo(source);
            Graphic_Multi maskedGraphic = null;
            if (!string.IsNullOrEmpty(selectedMaskName))
            {
                maskedGraphic = TryApplyMask(original, source, pawn);
            }
            if (maskedGraphic != null)
            {
                return maskedGraphic;
            }
            /*Log.Message("Graphic");
            Log.Message(source.GetType());
            if (source.gene != null)
            {
                Log.Message(source.gene.def.defName);
                try
                {
                    Log.Message(source.Props.shaderTypeDef.shaderPath);
                }
                catch
                {
                    Log.Message("No shaderPath");
                }
            }*/
            if (colorTwo != null && (color1.IndistinguishableFrom(color2) is false)) //are your fur colors even different?
            {
                if (original.Shader == ShaderTypeDefOf.CutoutComplex.Shader //is this a part with a CutoutComplex shader?
                    || (source is PawnRenderNode_Head && pawn.story.headType?.requiredGenes != null 
                    && pawn.story.headType.requiredGenes.Any(x => typeof(FurGene).IsAssignableFrom(x.geneClass)))) //or is this a furgene head?
                {                                                                                                              //I wish heads had a shaderType
                    return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(original.path,
                    ShaderTypeDefOf.CutoutComplex.Shader, Vector2.one, color1, color2);
                }
            }
            if (source.Props.colorType == PawnRenderNodeProperties.AttachmentColorType.Hair)
                return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(original.path, ShaderTypeDefOf.Cutout.Shader, Vector2.one, color1);
            return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(original.path, ShaderTypeDefOf.Cutout.Shader, Vector2.one, color2);
        }

        private Graphic_Multi TryApplyMask(Graphic original, PawnRenderNode source, Pawn pawn)
        {
            if (string.IsNullOrEmpty(selectedMaskName))
                return null;
            var path = original.path;
            var ext = "";
            var lastDot = path.LastIndexOf('.');
            if (lastDot >= 0)
            {
                ext = path.Substring(lastDot);
            }
            var noExtPath = lastDot >= 0 ? path.Substring(0, lastDot) : path;
            string maskedPath = null;
            var directionSuffixes = new[] { "_east", "_north", "_south", "_west"};
            foreach (var suffix in directionSuffixes)
            {
                var index = noExtPath.LastIndexOf(suffix);
                if (index > 0)
                {
                    var before = noExtPath.Substring(0, index);
                    maskedPath = before + "_" + selectedMaskName + suffix + "m" + ext;
                    break;
                }
            }
            if (maskedPath == null)
                return null;
            if (!ContentFinder<Texture2D>.Get(maskedPath, reportFailure: false))
                return null;
            var color1 = GetColorOne(source);
            var color2 = GetColorTwo(source);
            return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(maskedPath, ShaderTypeDefOf.CutoutComplex.Shader, Vector2.one, color1, color2);
        }

        public static Graphic GetGraphicOverridenNoFurgene(Graphic original, PawnRenderNode source, Pawn pawn)
        {
            var color1 = PostProcessNoFurgene(source, pawn, pawn.story.HairColor);
            var color2 = new Color();
            if ((source.gene.def.defName.StartsWith("OCARINA_") && source.gene.def.defName.EndsWith("Ears"))) //oca - i'm not sure what this even does?
                color2 = PostProcessNoFurgene(source, pawn, pawn.story.SkinColor);
            else
            {
                color2.r = ((1f - color1.r) * 0.8f) + color1.r;
                color2.b = ((1f - color1.b) * 0.8f) + color1.b;
                color2.g = ((1f - color1.g) * 0.8f) + color1.g;
                color2.a = 1f;
            }
            return (Graphic_Multi)GraphicDatabase.Get<Graphic_Multi>(original.path, ShaderTypeDefOf.CutoutComplex.Shader, Vector2.one, color1, color2);
        }

        /*public Shader GetShaderOverriden(Shader original, PawnRenderNode source)
        {
            if (furColor is null)
            {
                SetFurColor();
            }
            var color1 = GetColorOne(source);
            var color2 = GetColorTwo(source);
            if (colorTwo != null && (original == ShaderTypeDefOf.CutoutComplex.Shader || source is PawnRenderNode_Body ||
                source is PawnRenderNode_Head) && (color1.IndistinguishableFrom(color2) is false))
            {
                return ShaderTypeDefOf.CutoutComplex.Shader;
            }
            else
            {
                return ShaderTypeDefOf.Cutout.Shader;
            }
        }*/

        public Color GetColorOne(PawnRenderNode source)
        {
            if (ModsConfig.AnomalyActive)
            {
                if (pawn.IsShambler)
                {
                    return MutantUtility.GetShamblerColor(colorOne);
                }
                if (pawn.IsMutant && pawn.mutant.Def.useCorpseGraphics && pawn.mutant.rotStage == RotStage.Rotting)
                {
                    return PawnRenderUtility.GetRottenColor(colorOne);
                }
                if (pawn.IsMutant && pawn.mutant.def.skinColorOverride.HasValue)
                {
                    return PostProcess(source, pawn.mutant.def.skinColorOverride.Value);
                }
            }
            return PostProcess(source, colorOne);
        }

        public Color GetColorTwo(PawnRenderNode source)
        {
            if (ModsConfig.AnomalyActive && colorTwo.HasValue)
            {
                if (pawn.IsShambler)
                {
                    return MutantUtility.GetShamblerColor(colorTwo.Value);
                }
                if (pawn.IsMutant && pawn.mutant.Def.useCorpseGraphics && pawn.mutant.rotStage == RotStage.Rotting)
                {
                    return PawnRenderUtility.GetRottenColor(colorTwo.Value);
                }
            }

            if (pawn.IsMutant && pawn.mutant.def.skinColorOverride.HasValue)
            {
                return PostProcess(source, pawn.mutant.def.skinColorOverride.Value);
            }
            if (colorTwo is null)
            {
                return Color.white;
            }
            var color = colorTwo.Value;
            color = PostProcess(source, color);
            return color;
        }

        private Color PostProcess(PawnRenderNode source, Color color)
        {
            color *= source.props.colorRGBPostFactor;
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
            {
                color = PawnRenderUtility.GetRottenColor(color);
            }
            return color;
        }

        private static Color PostProcessNoFurgene(PawnRenderNode source, Pawn pawn, Color color)
        {
            color *= source.props.colorRGBPostFactor;
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
            {
                color = PawnRenderUtility.GetRottenColor(color);
            }
            return color;
        }

        public void ApplyColors()
        {
            if (pawn != null)
            {
                pawn.story.skinColorOverride = colorTwo;
                pawn.drawer.renderer.SetAllGraphicsDirty();
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (DebugSettings.ShowDevGizmos)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Change fur",
                    action = () =>
                    {
                        Find.WindowStack.Add(new Window_ColorPicker(this));
                    }
                };
            }
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref furColor, "furColor");
            Scribe_Values.Look(ref colorOne, "colorOne");
            Scribe_Values.Look(ref colorTwo, "colorTwo");
            Scribe_Values.Look(ref selectedMaskName, "selectedMaskName");
        }
    }
}

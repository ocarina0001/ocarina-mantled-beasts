using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    [HotSwappable]
    public class FurGene : Gene
    {
        private static readonly int MaskTexID = Shader.PropertyToID("_MaskTex");
        private static readonly int ColorTwoID = Shader.PropertyToID("_ColorTwo");

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
                SetFurColor();
            if (!string.IsNullOrEmpty(selectedMaskName))
            {
                var maskedGraphic = TryApplyMask(original, source, pawn);
                if (maskedGraphic != null)
                {
                    //Log.Message($"FurGene.GetGraphicOverriden: Using masked graphic for {original.path}");
                    return maskedGraphic;
                }
            }
            var color1 = GetColorOne(source);
            var color2 = GetColorTwo(source);
            if (colorTwo != null && !color1.IndistinguishableFrom(color2))
            {
                if (original.Shader == ShaderTypeDefOf.CutoutComplex.Shader
                    || (source is PawnRenderNode_Head && pawn.story.headType?.requiredGenes != null
                        && pawn.story.headType.requiredGenes.Any(x => typeof(FurGene).IsAssignableFrom(x.geneClass))))
                {
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
            string basePath = original.path;
            string maskBasePath = basePath + "_" + selectedMaskName;
            //Log.Message($"FurGene.TryApplyMask: basePath = {basePath}");
            //Log.Message($"FurGene.TryApplyMask: selectedMaskName = {selectedMaskName}");
            //Log.Message($"FurGene.TryApplyMask: maskBasePath = {maskBasePath}");
            var color1 = GetColorOne(source);
            var color2 = GetColorTwo(source);
            //Log.Message($"FurGene.TryApplyMask: color1 = {color1}, color2 = {color2}");
            string[] dirs = { "_north", "_east", "_south", "_west" };
            Material[] materials = new Material[4];
            bool anyLoaded = false;
            var origMulti = original as Graphic_Multi;
            bool origEastFlipped = origMulti?.eastFlipped ?? false;
            bool origWestFlipped = origMulti?.westFlipped ?? false;
            bool westFlipped = false;
            for (int i = 0; i < dirs.Length; i++)
            {
                string baseTexPath = basePath + dirs[i];
                string maskTexPath = maskBasePath + dirs[i] + "m";
                Texture2D baseTex = ContentFinder<Texture2D>.Get(baseTexPath, false);
                Texture2D maskTex = ContentFinder<Texture2D>.Get(maskTexPath, false);
                if (baseTex == null && i == 3)
                {
                    string eastBaseTexPath = basePath + "_east";
                    string eastMaskTexPath = maskBasePath + "_eastm";
                    baseTex = ContentFinder<Texture2D>.Get(eastBaseTexPath, false);
                    maskTex = ContentFinder<Texture2D>.Get(eastMaskTexPath, false);
                    if (baseTex != null)
                    {
                        westFlipped = true;
                        //Log.Message("FurGene.TryApplyMask: Using east texture for west (flipped).");
                    }
                }

                if (baseTex == null)
                {
                    //Log.Warning($"FurGene.TryApplyMask: Base texture missing: {baseTexPath}");
                    continue;
                }
                if (maskTex == null)
                {
                    //Log.Warning($"FurGene.TryApplyMask: Mask texture missing: {maskTexPath}");
                    continue;
                }

                Material mat = MaterialAllocator.Create(ShaderTypeDefOf.CutoutComplex.Shader);
                mat.mainTexture = baseTex;
                mat.SetTexture(MaskTexID, maskTex);
                mat.color = color1;
                mat.SetColor(ColorTwoID, color2);

                materials[i] = mat;
                anyLoaded = true;
            }
            if (!anyLoaded)
            {
                //Log.Error("FurGene.TryApplyMask: No materials could be created for any direction.");
                return null;
            }
            var maskedGraphic = new Graphic_Multi();
            maskedGraphic.mats = materials;
            maskedGraphic.drawSize = original.drawSize;
            maskedGraphic.eastFlipped = origEastFlipped;
            maskedGraphic.westFlipped = westFlipped || origWestFlipped;
            //Log.Message("FurGene.TryApplyMask: Successfully built manual masked graphic.");
            return maskedGraphic;
        }

        public static Graphic GetGraphicOverridenNoFurgene(Graphic original, PawnRenderNode source, Pawn pawn)
        {
            var color1 = PostProcessNoFurgene(source, pawn, pawn.story.HairColor);
            var color2 = new Color();
            if ((source.gene.def.defName.StartsWith("OCARINA_") && source.gene.def.defName.EndsWith("Ears")))
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

        public Color GetColorOne(PawnRenderNode source)
        {
            if (ModsConfig.AnomalyActive)
            {
                if (pawn.IsShambler)
                    return MutantUtility.GetShamblerColor(colorOne);
                if (pawn.IsMutant && pawn.mutant.Def.useCorpseGraphics && pawn.mutant.rotStage == RotStage.Rotting)
                    return PawnRenderUtility.GetRottenColor(colorOne);
                if (pawn.IsMutant && pawn.mutant.def.skinColorOverride.HasValue)
                    return PostProcess(source, pawn.mutant.def.skinColorOverride.Value);
            }
            return PostProcess(source, colorOne);
        }

        public Color GetColorTwo(PawnRenderNode source)
        {
            if (ModsConfig.AnomalyActive && colorTwo.HasValue)
            {
                if (pawn.IsShambler)
                    return MutantUtility.GetShamblerColor(colorTwo.Value);
                if (pawn.IsMutant && pawn.mutant.Def.useCorpseGraphics && pawn.mutant.rotStage == RotStage.Rotting)
                    return PawnRenderUtility.GetRottenColor(colorTwo.Value);
            }
            if (pawn.IsMutant && pawn.mutant.def.skinColorOverride.HasValue)
                return PostProcess(source, pawn.mutant.def.skinColorOverride.Value);
            if (colorTwo is null)
                return Color.white;
            var color = colorTwo.Value;
            return PostProcess(source, color);
        }

        private Color PostProcess(PawnRenderNode source, Color color)
        {
            color *= source.props.colorRGBPostFactor;
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
                color = PawnRenderUtility.GetRottenColor(color);
            return color;
        }

        private static Color PostProcessNoFurgene(PawnRenderNode source, Pawn pawn, Color color)
        {
            color *= source.props.colorRGBPostFactor;
            if (pawn.Drawer.renderer.CurRotDrawMode == RotDrawMode.Rotting)
                color = PawnRenderUtility.GetRottenColor(color);
            return color;
        }

        public void ApplyColors()
        {
            if (pawn != null)
            {
                pawn.story.HairColor = colorOne;
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
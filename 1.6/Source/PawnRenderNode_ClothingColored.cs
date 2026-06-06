using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    public class PawnRenderNode_ClothingColored(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : PawnRenderNode(pawn, props, tree)
    {
        public override Color ColorFor(Pawn pawn)
        {
            Apparel mantlegear = pawn?.apparel?.WornApparel?.FirstOrDefault(a => a.def.defName.Contains("Mantlegear"));
            if (mantlegear == null)
                return Color.white;
            return mantlegear.DrawColor;
        }
    }
}

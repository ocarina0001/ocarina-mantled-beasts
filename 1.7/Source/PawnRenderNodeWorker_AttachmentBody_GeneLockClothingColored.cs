using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    public class PawnRenderNodeWorker_AttachmentBody_GeneLockClothingColored : PawnRenderNodeWorker_AttachmentBody
    {
        public override bool CanDrawNow(PawnRenderNode node, PawnDrawParms parms)
        {
            if (!base.CanDrawNow(node, parms))
                return false;
            Pawn pawn = parms.pawn;
            if (pawn?.genes == null)
                return false;
            if (pawn.genes.CustomXenotype != null)
                return pawn.genes.CustomXenotype.genes.Contains(DefsOf.OCARINA_MantledBeastsTail) == true;
            return pawn.genes.Xenotype?.AllGenes?.Contains(DefsOf.OCARINA_MantledBeastsTail) == true;
        }
    }
}

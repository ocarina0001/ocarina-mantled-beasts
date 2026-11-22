using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace MantledBeasts
{
    public class DefModExtension_ClothingRestriction : DefModExtension
    {
        public List<GeneDef> requiredGenesToEquip = new List<GeneDef>();
        public bool CanEquipGeneCheck(Pawn_GeneTracker tracker, out string reason)
        {
            reason = null;

            if (!requiredGenesToEquip.NullOrEmpty() && !TrackerHasAllOfGenes(tracker, requiredGenesToEquip))
                return false;
            return true;
        }

        private static bool TrackerHasAllOfGenes(Pawn_GeneTracker tracker, List<GeneDef> genes)
        {
            if (genes.NullOrEmpty()) return true;
            foreach (GeneDef gene in genes)
            {
                if (!tracker.HasActiveGene(gene))
                    return false;
            }
            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace MantledBeasts
{
    public class ThoughtWorker_Precept_VajiNotWearingMantle : ThoughtWorker_Precept
    {
        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            return CheckIfVajiAndAnyClothesAreMantle(p);
        }
        public static bool CheckIfVajiAndAnyClothesAreMantle(Pawn p)
        {
            if (p == null || p.apparel == null) return false; // either pawn or their apparel is null, so don't give them the thought
            if (p.genes.HasActiveGene(DefsOf.OCARINA_MantledBeastsFur)) // trues should only be here
                if (p.apparel.WornApparel.Any(apparel => apparel.def.defName.EndsWith("Mantle")))
                    return false; // is wearing mantle, no thought
                else return true; // is not wearing mantle, thought
            return false; // non vaji don't need it
        }
    }

    public class ThoughtWorker_Precept_VajiNotWearingMantleSocial : ThoughtWorker_Precept_Social
    {
        public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            if (otherPawn == null)
                return false;
            return ThoughtWorker_Precept_VajiNotWearingMantle.CheckIfVajiAndAnyClothesAreMantle(otherPawn);
        }
    }

    // yeah yeah whatever let me be lazy
    public class ThoughtWorker_Precept_VajiNotWearingMantlegear : ThoughtWorker_Precept
    {
        public override ThoughtState ShouldHaveThought(Pawn p)
        {
            return CheckIfVajiAndAnyClothesAreMantlegear(p);
        }
        public static bool CheckIfVajiAndAnyClothesAreMantlegear(Pawn p)
        {
            if (p == null || p.apparel == null) return false; // either pawn or their apparel is null, so don't give them the thought
            if (p.genes.HasActiveGene(DefsOf.OCARINA_MantledBeastsFur)) // trues should only be here
                if (p.apparel.WornApparel.Any(apparel => apparel.def.defName.EndsWith("Mantlegear")))
                    return false; // is wearing mantle, no thought
                else return true; // is not wearing mantle, thought
            return false; // non vaji don't need it
        }
    }

    public class ThoughtWorker_Precept_VajiNotWearingMantlegearSocial : ThoughtWorker_Precept_Social
    {
        public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {
            if (otherPawn == null)
                return false;
            return ThoughtWorker_Precept_VajiNotWearingMantlegear.CheckIfVajiAndAnyClothesAreMantlegear(otherPawn);
        }
    }

}

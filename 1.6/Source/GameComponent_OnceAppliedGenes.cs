using System.Collections.Generic;
using Verse;

namespace MantledBeasts
{
    public class GameComponent_OnceAppliedGenes : GameComponent
    {
        private HashSet<string> appliedGenes = new HashSet<string>();

        public GameComponent_OnceAppliedGenes(Game game) { }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref appliedGenes, "appliedGenes", LookMode.Value);
            appliedGenes ??= [];
        }

        private static string Key(Pawn pawn, GeneDef geneDef) => pawn.ThingID + "_" + geneDef.defName;

        public bool HasBeenApplied(Pawn pawn, GeneDef geneDef)
        {
            if (pawn == null || geneDef == null) return false;
            return appliedGenes.Contains(Key(pawn, geneDef));
        }

        public void MarkAsApplied(Pawn pawn, GeneDef geneDef)
        {
            if (pawn == null || geneDef == null) return;
            appliedGenes.Add(Key(pawn, geneDef));
        }
    }
}
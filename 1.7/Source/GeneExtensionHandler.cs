using System;
using RimWorld;
using Verse;

namespace MantledBeasts
{
    public static class GeneExtensionHandler
    {
        public static void TryApplyOnceEffects(Gene gene)
        {
            try
            {
                if (gene?.pawn == null || !gene.Active) return;
                if (Scribe.mode != LoadSaveMode.Inactive) return;
                var ext = gene.def.GetModExtension<GeneExtension>();
                if (ext?.hediffsToBodyPartsOnce == null) return;
                var comp = Current.Game?.GetComponent<GameComponent_OnceAppliedGenes>();
                if (comp == null)
                {
                    Log.Warning("[MantledBeasts] GameComponent_OnceAppliedGenes missing; " + "cannot track once-applied genes.");
                    return;
                }
                if (comp.HasBeenApplied(gene.pawn, gene.def)) return;
                foreach (var item in ext.hediffsToBodyPartsOnce)
                {
                    if (item?.hediff == null || item.bodyparts == null) continue;

                    int num = 0;
                    foreach (var bodyPartDef in item.bodyparts)
                    {
                        var parts = gene.pawn.RaceProps.body.GetPartsWithDef(bodyPartDef);
                        if (parts == null || parts.Count == 0) continue;
                        if (num >= parts.Count) break;
                        var part = parts[num];
                        if (!gene.pawn.health.hediffSet.PartIsMissing(part) && !gene.pawn.health.hediffSet.HasHediff(item.hediff, part))
                            gene.pawn.health.AddHediff(item.hediff, part);
                        num++;
                    }
                }
                comp.MarkAsApplied(gene.pawn, gene.def);
            }
            catch (Exception ex)
            {
                Log.Error("[MantledBeasts] Error in TryApplyOnceEffects for gene " + (gene?.def?.defName ?? "null") + ": " + ex);
            }
        }
    }
}
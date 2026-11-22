using HarmonyLib;
using RimWorld;
using Verse;

namespace MantledBeasts
{
    public static class Patch_CanEquip
    {
        public static void Postfix(Thing thing, Pawn pawn, ref bool __result, ref string cantReason)
        {
            if (!__result) return;
            if (!(thing is Apparel apparel)) return;

            var ext = apparel.def.GetModExtension<DefModExtension_ClothingRestriction>();
            if (ext == null) return;

            if (ext.CanEquipGeneCheck(pawn.genes, out string reason))
                return;

            __result = false;
            cantReason = reason;
        }
    }
}
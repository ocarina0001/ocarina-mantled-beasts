using HarmonyLib;
using Verse;

namespace MantledBeasts
{
    [HarmonyPatch(typeof(Gene), nameof(Gene.PostAdd))]
    public static class Patch_Gene_PostAdd
    {
        [HarmonyPostfix]
        public static void Postfix(Gene __instance)
        {
            GeneExtensionHandler.TryApplyOnceEffects(__instance);
        }
    }
}
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MantledBeasts
{
    [HarmonyPatch(typeof(Apparel), nameof(Apparel.WornGraphicPath), MethodType.Getter)]
    public static class Patch_Apparel_WornGraphicPath
    {
        public static void Postfix(Apparel __instance, ref string __result)
        {
            try
            {
                if (__instance is not Apparel_MoodTexture apparel_MoodTexture)
                    return;
                if (string.IsNullOrEmpty(apparel_MoodTexture.CurrentSuffix))
                    return;
                __result = $"{__result}_{apparel_MoodTexture.CurrentSuffix}";
            }
            catch (System.Exception ex)
            {
                Log.Error($"[MantledBeasts] Exception in apparel texture patch:\n{ex}");
            }
        }

        public static string ResolvePawnState(Pawn pawn)
        {
            //if (pawn.Dead)
            //    return "Dead";
            //if (pawn.Downed)
            //    return "Downed";
            //if (pawn.InMentalState)
            //    return "Mental";
            //if (pawn.Drafted)
            //    return "Combat";
            Need_Mood mood = pawn.needs?.mood;
            float breakThresholdExtreme = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
            if (mood != null)
            {
                float moodLevel = mood.CurLevel;
                if (moodLevel < breakThresholdExtreme)
                    return "AboutToBreak";
                if (moodLevel < breakThresholdExtreme + 0.05f)
                    return "OnEdge";
                if (moodLevel < pawn.mindState.mentalBreaker.BreakThresholdMinor)
                    return "Stressed";
                if (moodLevel < 0.65f)
                    return "Neutral";
                if (moodLevel < 0.9f)
                    return "Content";
                return "Happy";
            }
            return null;
        }
    }
}
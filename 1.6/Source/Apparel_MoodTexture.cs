using RimWorld;
using Verse;
using Verse.AI;

namespace MantledBeasts
{
    public class Apparel_MoodTexture : Apparel
    {
        private string currentSuffix = "";
        private int nextCheckTick;

        public string CurrentSuffix => currentSuffix;

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame < nextCheckTick)
                return;
            nextCheckTick = Find.TickManager.TicksGame + 300;
            Pawn wearer = Wearer;
            if (wearer == null)
                return;
            string newSuffix = Patch_Apparel_WornGraphicPath.ResolvePawnState(wearer) ?? "";
            if (newSuffix == currentSuffix)
                return;
            currentSuffix = newSuffix;
            wearer.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}
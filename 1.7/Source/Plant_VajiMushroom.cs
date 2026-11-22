using RimWorld;
using Verse;

namespace MantledBeasts
{
    public class Plant_VajiMushroom : Plant
    {
        private const int WaterRadius = 2;
        private bool HasNearbyWater
        {
            get
            {
                if (Map == null)
                    return false;
                int num = GenRadial.NumCellsInRadius(WaterRadius);
                for (int i = 0; i < num; i++)
                {
                    IntVec3 cell = Position + GenRadial.RadialPattern[i];
                    if (!cell.InBounds(Map))
                        continue;
                    TerrainDef terrain = cell.GetTerrain(Map);
                    if (terrain != null && terrain.IsWater)
                        return true;
                }
                return false;
            }
        }

        private bool IsRoofed
        {
            get
            {
                return Map?.roofGrid?.Roofed(Position) ?? false;
            }
        }

        public override bool Resting
        {
            get
            {
                return false;
            }
        }

        public override float GrowthRate
        {
            get
            {
                if (Blighted)
                    return 0f;
                if (!Spawned)
                    return 0f;
                if (!IsRoofed)
                    return 0f;
                if (!HasNearbyWater)
                    return 0f;
                return GrowthRateFactor_Temperature;
            }
        }

        public override string GetInspectString()
        {
            string inspect = base.GetInspectString();
            if (!IsRoofed)
                inspect += "\n" + "MantledBeasts_RequiresRoofToGrow".Translate();
            if (!HasNearbyWater)
                inspect += "\n" + "MantledBeasts_RequiresAdjacentWaterToGrow".Translate();
            return inspect;
        }
    }
}
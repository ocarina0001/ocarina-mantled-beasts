using System.Collections.Generic;
using Verse;

namespace MantledBeasts
{
    public class FurColors : DefModExtension
    {
        public List<FurColorDef> allowedFurColors;
    }

    public class ColorMasks : DefModExtension
    {
        public List<ColorMaskEntry> allowedColorMasks;
    }

    public class ColorMaskEntry
    {
        public string maskName;
        public float selectionWeight = 1f;
    }
}

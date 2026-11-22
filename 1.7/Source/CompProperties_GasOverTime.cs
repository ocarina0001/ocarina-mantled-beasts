using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace MantledBeasts
{
    public class CompProperties_GasOverTime : CompProperties
    {
        public GasType type;
        public int gasAmount = 1;
        public int tickInterval = 250;
        public bool useStackCountAsFactor = false;
        public CompProperties_GasOverTime()
        {
            compClass = typeof(CompGasOverTime);
        }
    }
}

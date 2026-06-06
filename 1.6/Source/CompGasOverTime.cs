using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace MantledBeasts
{
    public class CompGasOverTime : ThingComp
    {
        private CompProperties_GasOverTime Props => (CompProperties_GasOverTime)props;
        public override void CompTick()
        {
            base.CompTick();
            if (parent.MapHeld == null || parent.Destroyed)
                return;
            if (parent.IsHashIntervalTick(Props.tickInterval))
            {
                int amount = Props.gasAmount;
                if (Props.useStackCountAsFactor)
                    amount *= parent.stackCount;
                GasUtility.AddGas(parent.PositionHeld, parent.MapHeld, Props.type, amount);
            }
        }
    }
}

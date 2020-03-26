using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lucid_Dream_Explorer
{
    abstract class Range
    {
        public class MaxMin
        {
            public int min, max;
            public MaxMin(int min, int max)
            {
                this.min = min; this.max = max;
            }
        }
        public static readonly MaxMin map = new MaxMin(0, 14);

        public static bool isInsideRange(MaxMin range, int value)
        {
            return range.min <= value || value >= range.max;
        }
    }
}

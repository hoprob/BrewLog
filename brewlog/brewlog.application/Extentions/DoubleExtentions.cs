using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace brewlog.application.Extentions
{
    public static class DoubleExtentions
    {
        public static string ToStringWithDot(this double num)
        {
            return num.ToString().Replace(',', '.');
        }
    }
}

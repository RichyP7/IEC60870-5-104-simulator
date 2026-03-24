using System;
using System.Collections.Generic;
using System.Text;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class Helpers
    {
        public static bool TryParseIntClamped(
            string input,
            int min,
            int max,
            out int value)
                {
                    if (!int.TryParse(input, out int parsed))
                    {
                        value = min;    // or any default you want
                        return false;
                    }

                    if (parsed < min)
                        value = min;
                    else if (parsed > max)
                        value = max;
                    else
                        value = parsed;

                    return true;
                }
    }
}

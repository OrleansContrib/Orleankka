using System;
using System.Globalization;
using System.Linq;

namespace Orleankka
{
    public class AutomaticDeactivationAttribute : Attribute
    {
        public string Idle
        {
            get; set;
        }

        static TimeSpan ParseTimeSpan(string input)
        {
            string str = input.Trim().ToLower(CultureInfo.InvariantCulture);

            int num;
            string s;

            if (str.EndsWith("ms", StringComparison.Ordinal))
            {
                num = 1;
                s = str.Remove(str.Length - 2).Trim();
            }
            else if (str.EndsWith("s", StringComparison.Ordinal))
            {
                num = 1000;
                s = str.Remove(str.Length - 1).Trim();
            }
            else if (str.EndsWith("m", StringComparison.Ordinal))
            {
                num = 60000;
                s = str.Remove(str.Length - 1).Trim();
            }
            else if (str.EndsWith("hr", StringComparison.Ordinal))
            {
                num = 3600000;
                s = str.Remove(str.Length - 2).Trim();
            }
            else
            {
                num = 1000;
                s = str;
            }

            double result;
            if (!double.TryParse(s, out result))
                throw new FormatException("Can't parse time span value: " + input);

            return TimeSpan.FromMilliseconds(result * num);
        }
    }
}

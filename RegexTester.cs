using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchSharp
{
    class RegexTester
    {
        public static bool IsValid(string pattern)
        {
            try
            {
                var regex = new Regex(pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}

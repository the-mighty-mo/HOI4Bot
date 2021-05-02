using System;
using System.Collections.Generic;

namespace HOI4Bot
{
    public static class Constants
    {
        public static readonly List<string> majorAllies = new()
        {
            "USA",
            "UK",
            "USSR"
        };

        public static readonly List<string> majorAxis = new()
        {
            "German Reich",
            "Japan",
            "iTaLY"
        };

        public static readonly List<string> mijorAllies = new()
        {
            "France"
        };

        public static readonly List<string> mijorAxis = new()
        {
        };

        public static readonly List<string> minorAllies = new()
        {
            "Mexico",
            "Poland",
            "China"
        };

        public static readonly List<string> minorAxis = new()
        {
            "Spain",
            "Romania"
        };

        public static readonly Random random = new((int)(DateTime.Now.Ticks % int.MaxValue));
    }
}
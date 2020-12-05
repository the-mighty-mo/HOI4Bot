using System;
using System.Collections.Generic;

namespace HOI4Bot
{
    public static class Constants
    {
        public static readonly List<string> majorAllies = new List<string>()
        {
            "USA",
            "UK",
            "USSR"
        };

        public static readonly List<string> majorAxis = new List<string>()
        {
            "German Reich",
            "Japan",
            "iTaLY"
        };

        public static readonly List<string> mijorAllies = new List<string>()
        {
            "France"
        };

        public static readonly List<string> mijorAxis = new List<string>()
        {
        };

        public static readonly List<string> minorAllies = new List<string>()
        {
            "Mexico",
            "Poland",
            "China"
        };

        public static readonly List<string> minorAxis = new List<string>()
        {
            "Spain",
            "Romania"
        };

        public static readonly Random random = new Random((int)(DateTime.Now.Ticks % int.MaxValue));
    }
}
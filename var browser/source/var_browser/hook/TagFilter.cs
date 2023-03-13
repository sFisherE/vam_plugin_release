using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace var_browser
{
    class TagFilter
    {
        public static List<string> HairRegionTags = new List<string>()
        {
            "head","face","genital","torso","arms","legs","full body",
        };
        public static List<string> HairTypeTags = new List<string>()
        {
            "short","long",
        };
        public static List<string> HairOtherTags = new List<string>()
        {
            "no tag",
            //自定义的一些


            "unknown",//剩下的一些tag
        };

        public static List<string> ClothingRegionTags = new List<string>()
        {
            "head","torso","hip","arms","hands","legs","feet","neck","full body",
        };
        public static List<string> ClothingTypeTags = new List<string>()
        {
            "bra","panties","underwear","shorts","pants","top","shirt","skirt","dress",
            "shoes","socks","stockings","gloves","jewelry","accessory","hat","mask",
            "bodysuit","bottom","glasses","sweater",
        };
        public static List<string> ClothingOtherTags = new List<string>()
        {
            "back","costume","fantasy","heels","jeans","lingerie","sneakers","swimwear",

            //自定义的一些

            "no tag",//特殊
            "unknown",//剩下的一些tag
        };


        public static HashSet<string> AllClothingTags = new HashSet<string>()
        {
            "head","torso","hip","arms","hands","legs","feet","neck","full body",
             "bra","panties","underwear","shorts","pants","top","shirt","skirt","dress",
            "shoes","socks","stockings","gloves","jewelry","accessory","hat","mask",
            "bodysuit","bottom","glasses","sweater",

            "back","costume","fantasy","heels","jeans","lingerie","sneakers","swimwear",
        };

        public static HashSet<string> ClothingUnknownTags = new HashSet<string>()
        {
            "bikini",
        };

        public static HashSet<string> AllHairTags = new HashSet<string>()
        {
            "head","face","genital","torso","arms","legs","full body",
             "short","long",
        };
        public static HashSet<string> HairUnknownTags = new HashSet<string>()
        {
            "tail",
        };
    }
}

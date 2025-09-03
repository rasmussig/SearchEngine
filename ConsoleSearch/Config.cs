using System;

namespace ConsoleSearch
{
    public class Config
    {
        public static bool CaseSensitive 
        { 
            get => Shared.Config.CaseSensitive;
            set => Shared.Config.CaseSensitive = value;
        }

        public static bool ViewTimeStamps 
        { 
            get => Shared.Config.ViewTimeStamps;
            set => Shared.Config.ViewTimeStamps = value;
        }

        public static int? MaxResults 
        { 
            get => Shared.Config.MaxResults;
            set => Shared.Config.MaxResults = value;
        }
    }
}

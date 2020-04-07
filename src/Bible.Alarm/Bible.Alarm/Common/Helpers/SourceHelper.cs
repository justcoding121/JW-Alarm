using Bible.Alarm.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.Common.Helpers
{
    public static class SourceHelper
    {
        public static SourceWebsite GetSourceWebsite(string pubCode, bool isMusic = false)
        {
            if (isMusic)
            {
                return SourceWebsite.JwOrg;
            }

            switch (pubCode)
            {
                case "kjw":
                case "nivuk":
                    return SourceWebsite.BibleGateway;
                default:
                    return SourceWebsite.JwOrg;
            }

        }
    }
}

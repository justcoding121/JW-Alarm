using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.Common.Helpers
{
    public static class JwSourceHelper
    {
        public static Dictionary<string, string> PublicationCodeToNameMappings =
                    new Dictionary<string, string>(new KeyValuePair<string, string>[]{
                                new KeyValuePair<string, string>("nwt","New World Translation (2013)"),
                                new KeyValuePair<string, string>("bi12","New World Translation (1984)"),
                            });



    }
}

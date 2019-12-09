using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.Contracts.Platform
{
    public interface IVersionFinder
    {
        string GetVersionName();
    }
}

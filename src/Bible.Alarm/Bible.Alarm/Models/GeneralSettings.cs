﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.Models
{
    public class GeneralSettings : IEntity
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
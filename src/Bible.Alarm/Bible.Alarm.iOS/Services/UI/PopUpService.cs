﻿using JW.Alarm.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JW.Alarm.Services.iOS
{
    public class iOSPopUpService : ToastService
    {
        public override async Task ShowMessage(string message, int seconds)
        {
            throw new NotImplementedException();
        }
    }
}
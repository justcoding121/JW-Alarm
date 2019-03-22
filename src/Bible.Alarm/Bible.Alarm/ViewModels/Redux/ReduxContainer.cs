﻿using JW.Alarm.ViewModels.Redux.Reducers;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels.Redux
{
    public static class ReduxContainer
    {
        public static IStore<ApplicationState> Store { get; set; }
            = new Store<ApplicationState>(RootReducer.Execute, new ApplicationState());
    }
}
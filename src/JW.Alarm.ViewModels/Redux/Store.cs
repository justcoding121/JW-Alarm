using JW.Alarm.ViewModels.Redux.Reducers;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels.Redux
{
    public static class ReduxStore
    {
        public static IStore<State> Store { get; set; } = new Store<State>(RootReducer.Execute, new State());
    }
}

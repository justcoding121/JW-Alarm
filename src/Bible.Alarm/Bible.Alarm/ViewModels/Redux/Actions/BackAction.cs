using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bible.Alarm.ViewModels.Redux.Actions
{
    public class BackAction : IAction
    {
        public IDisposable CurrentViewModel { get; set; }
        public BackAction(IDisposable currentViewModel)
        {
            CurrentViewModel = currentViewModel;
        }
    }
}

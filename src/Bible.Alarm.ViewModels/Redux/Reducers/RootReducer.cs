using Bible.Alarm.ViewModels.Redux.Actions;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels.Redux.Reducers
{
    public static class RootReducer
    {
        public static ApplicationState Execute(ApplicationState previousState, IAction action)
        {
            if (action is InitializeAction)
            {
                return new ApplicationState()
                {
                    Schedules = (action as InitializeAction).ScheduleList
                };
            }

            if (action is AddScheduleAction)
            {
                var @params = (action as AddScheduleAction);
                previousState.Schedules.Add(@params.ScheduleListItem);
                return new ApplicationState()
                {
                    ScheduleViewModel = previousState.ScheduleViewModel,
                    Schedules = previousState.Schedules
                };
            }

            if (action is RemoveScheduleAction)
            {
                previousState.Schedules.Remove((action as RemoveScheduleAction).ScheduleListItem);
                return new ApplicationState()
                {
                    ScheduleViewModel = previousState.ScheduleViewModel,
                    Schedules = previousState.Schedules
                };
            }

            if (action is UpdateScheduleAction)
            {
                return new ApplicationState()
                {
                    ScheduleViewModel = previousState.ScheduleViewModel,
                    Schedules = previousState.Schedules
                };
            }

            if (action is ViewScheduleAction)
            {
                var @params = (action as ViewScheduleAction);
                return new ApplicationState()
                {
                    ScheduleViewModel = @params.ScheduleViewModel,
                    Schedules = previousState.Schedules,
                    ScheduleListItem = @params.SelectedScheduleListItem
                };
            }

            if (action is BackToHomeAction)
            {
                previousState.ScheduleViewModel.Dispose();

                return new ApplicationState()
                {
                    Schedules = previousState.Schedules
                };
            }

            if (action is MusicSelectionAction)
            {
                var @params = (action as MusicSelectionAction);
                return new ApplicationState()
                {
                    ScheduleViewModel = previousState.ScheduleViewModel,
                    Schedules = previousState.Schedules,
                    ScheduleListItem = previousState.ScheduleListItem,
                    MusicSelectionViewModel = @params.MusicSelectionViewModel,
                    CurrentMusic = @params.CurrentMusic
                };
            }

            return previousState;
        }
    }
}

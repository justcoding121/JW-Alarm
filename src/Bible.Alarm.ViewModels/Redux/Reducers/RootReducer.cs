using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.ViewModels.Redux.Actions.Music;
using Redux;
using System;
using System.Collections.Generic;
using System.Text;

namespace JW.Alarm.ViewModels.Redux.Reducers
{
    public static partial class RootReducer
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
                    Schedules = previousState.Schedules
                };
            }

            if (action is RemoveScheduleAction)
            {
                previousState.Schedules.Remove((action as RemoveScheduleAction).ScheduleListItem);
                return new ApplicationState(); ;
            }

            if (action is UpdateScheduleAction)
            {
                return new ApplicationState()
                {
                    Schedules = previousState.Schedules
                };
            }

            if (action is ViewScheduleAction)
            {
                var @params = (action as ViewScheduleAction);
                return new ApplicationState()
                {
                    Schedules = previousState.Schedules,
                    CurrentScheduleListItem = @params.SelectedScheduleListItem
                };
            }

            if (action is BackAction)
            {
                (action as BackAction).CurrentViewModel.Dispose();

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
                    Schedules = previousState.Schedules,
                    CurrentScheduleListItem = previousState.CurrentScheduleListItem,
                    CurrentMusic = @params.CurrentMusic
                };
            }

            if (action is SongBookSelectionAction)
            {
                var @params = (action as SongBookSelectionAction);
                return new ApplicationState()
                {
                    Schedules = previousState.Schedules,
                    CurrentScheduleListItem = previousState.CurrentScheduleListItem,
                    CurrentMusic = @params.CurrentMusic,
                    TentativeMusic = @params.TentativeMusic 
                };
            }

            if (action is TrackSelectionAction)
            {
                var @params = (action as TrackSelectionAction);
                return new ApplicationState()
                {
                    Schedules = previousState.Schedules,
                    CurrentScheduleListItem = previousState.CurrentScheduleListItem,
                    CurrentMusic = @params.CurrentMusic,
                    TentativeMusic = @params.TentativeMusic
                };
            }

            return previousState;
        }
    }
}

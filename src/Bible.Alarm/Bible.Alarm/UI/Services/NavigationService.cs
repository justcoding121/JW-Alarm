using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.Music;
using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.ViewModels;
using Mvvmicro;
using Xamarin.Forms;
using Bible.Alarm.UI.Views.General;
using Bible.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions;

namespace Bible.Alarm.UI
{
    public class NavigationService : INavigationService
    {
        private readonly INavigation navigater;

        public event Action<object> NavigatedBack;

        public NavigationService(INavigation navigation)
        {
            navigater = navigation;

            var syncContext = TaskScheduler.FromCurrentSynchronizationContext();

            Messenger<object>.Subscribe(Messages.ShowSnoozeDismissModal, async vm =>
            {
                await Task.Delay(0).ContinueWith(async (x) =>
                {
                    await ShowModal("AlarmModal", vm);

                }, syncContext);
            });


            Messenger<object>.Subscribe(Messages.HideSnoozeDismissModal, async vm =>
            {
                await Task.Delay(0).ContinueWith(async (x) =>
                {
                    await CloseModal();

                }, syncContext);
            });
        }

        public async Task ShowModal(string name, object viewModel)
        {
            switch (name)
            {
                case "LanguageModal":
                    {
                        var modal = IocSetup.Container.Resolve<LanguageModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "AlarmModal":
                    {
                        if (navigater.ModalStack.FirstOrDefault()?.GetType() == typeof(AlarmModal))
                        {
                            return;
                        }

                        var modal = IocSetup.Container.Resolve<AlarmModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "BatteryOptimizationExclusionModal":
                    {
                        var modal = IocSetup.Container.Resolve<BatteryOptimizationExclusionModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "NumberOfChaptersModal":
                    {
                        var modal = IocSetup.Container.Resolve<NumberOfChaptersModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                default:
                    throw new ArgumentException("Modal not defined.", name);
            }
        }

        public async Task Navigate(object viewModel)
        {
            var vmName = viewModel.GetType().Name;
            switch (vmName)
            {
                case "ScheduleViewModel":
                    {
                        var view = IocSetup.Container.Resolve<Schedule>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }

                case "MusicSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<MusicSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "SongBookSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<SongBookSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "TrackSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<TrackSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "BibleSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<BibleSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "BookSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<BookSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "ChapterSelectionViewModel":
                    {
                        var view = IocSetup.Container.Resolve<ChapterSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                default:
                    throw new ArgumentException("Invalid View Model name", vmName);
            }
        }

        public async Task GoBack()
        {
            if (navigater.ModalStack.Count > 0)
            {
                await CloseModal();
                return;
            }

            if (navigater.NavigationStack.Count > 1)
            {
                var top = navigater.NavigationStack.Last();
                ReduxContainer.Store.Dispatch(new BackAction((top.BindingContext as IDisposable)));
                await navigater.PopAsync();
            }

            var currentPage = navigater.NavigationStack.LastOrDefault();

            if (currentPage != null)
            {
                NavigatedBack?.Invoke(currentPage.BindingContext);
            }
        }

        public async Task CloseModal()
        {
            if (navigater.ModalStack.Count > 0)
            {
                await navigater.PopModalAsync();

                var currentPage = navigater.NavigationStack.FirstOrDefault();

                if (currentPage != null)
                {
                    NavigatedBack?.Invoke(currentPage.BindingContext);
                }
            }
        }
        public async Task NavigateToHome()
        {
            while (navigater.ModalStack.Count > 0)
            {
                await navigater.PopModalAsync();
            }

            while (navigater.NavigationStack.Count > 1)
            {
                var top = navigater.NavigationStack.Last();
                ReduxContainer.Store.Dispatch(new BackAction((top.BindingContext as IDisposable)));
                await navigater.PopAsync();
            }

            var currentPage = navigater.NavigationStack.FirstOrDefault();

            if (currentPage != null)
            {
                NavigatedBack?.Invoke(currentPage.BindingContext);
            }

        }

    }
}

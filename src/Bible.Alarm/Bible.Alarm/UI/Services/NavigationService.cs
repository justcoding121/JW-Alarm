using Bible.Alarm.Common.Mvvm;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.General;
using Bible.Alarm.UI.Views.Music;
using Bible.Alarm.ViewModels;
using Bible.Alarm.ViewModels.Redux;
using Bible.Alarm.ViewModels.Redux.Actions;
using Bible.Alarm.ViewModels.Shared;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public class NavigationService : INavigationService
    {
        private IContainer container;

        private readonly INavigation navigater;

        public event Action<object> NavigatedBack;

        public NavigationService(IContainer container, INavigation navigater)
        {
            this.container = container;
            this.navigater = navigater;

            var syncContext = this.container.Resolve<TaskScheduler>();

            Messenger<object>.Subscribe(Messages.ShowAlarmModal, async @param =>
            {
                var vm = container.Resolve<AlarmViewModal>();
                await Task.Delay(0).ContinueWith(async (x) =>
                {
                    await ShowModal("AlarmModal", vm);

                }, syncContext);
            });


            Messenger<object>.Subscribe(Messages.HideAlarmModal, async vm =>
            {
                await Task.Delay(0).ContinueWith(async (x) =>
                {
                    await CloseModal();

                }, syncContext);
            });

            Messenger<object>.Subscribe(Messages.ShowMediaProgessModal, async @param =>
            {
                var vm = this.container.Resolve<MediaProgressViewModal>();
                await Task.Delay(0).ContinueWith(async (x) =>
                {
                    await ShowModal("MediaProgressModal", vm);

                }, syncContext);
            });

            Messenger<object>.Subscribe(Messages.HideMediaProgressModal, async vm =>
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
                        var modal = container.Resolve<LanguageModal>();
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

                        var modal = container.Resolve<AlarmModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "BatteryOptimizationExclusionModal":
                    {
                        var modal = container.Resolve<BatteryOptimizationExclusionModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "NumberOfChaptersModal":
                    {
                        var modal = container.Resolve<NumberOfChaptersModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }

                case "MediaProgressModal":
                    {
                        if (navigater.ModalStack.FirstOrDefault()?.GetType() == typeof(MediaProgressModal))
                        {
                            return;
                        }

                        var modal = container.Resolve<MediaProgressModal>();
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
                        var view = container.Resolve<Schedule>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }

                case "MusicSelectionViewModel":
                    {
                        var view = container.Resolve<MusicSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "SongBookSelectionViewModel":
                    {
                        var view = container.Resolve<SongBookSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "TrackSelectionViewModel":
                    {
                        var view = container.Resolve<TrackSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "BibleSelectionViewModel":
                    {
                        var view = container.Resolve<BibleSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "BookSelectionViewModel":
                    {
                        var view = container.Resolve<BookSelection>();
                        view.BindingContext = viewModel;
                        await navigater.PushAsync(view);
                        break;
                    }
                case "ChapterSelectionViewModel":
                    {
                        var view = container.Resolve<ChapterSelection>();
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
                var modal = await navigater.PopModalAsync();
                if (modal.BindingContext is IDisposableModal)
                {
                    (modal.BindingContext as IDisposableModal).Dispose();
                }
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

using Advanced.Algorithms.Distributed;
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Bible.Alarm.UI
{
    public class NavigationService : INavigationService
    {
        private readonly IContainer container;

        private readonly INavigation navigater;

        public event Action<object> NavigatedBack;
        private bool disposed = false;

        private AsyncQueue<(MvvmMessages, object)> queue = new AsyncQueue<(MvvmMessages, object)>();

        public NavigationService(IContainer container, INavigation navigater)
        {
            this.container = container;
            this.navigater = navigater;


            Messenger<object>.Subscribe(MvvmMessages.ShowAlarmModal, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.ShowAlarmModal, @param));
            });

            Messenger<object>.Subscribe(MvvmMessages.HideAlarmModal, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.HideAlarmModal, @param));
            });

            Messenger<object>.Subscribe(MvvmMessages.ShowMediaProgessModal, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.ShowMediaProgessModal, @param));
            });

            Messenger<object>.Subscribe(MvvmMessages.HideMediaProgressModal, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.HideMediaProgressModal, @param));
            });

            Messenger<object>.Subscribe(MvvmMessages.ShowToast, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.ShowToast, @param));
            });

            Messenger<object>.Subscribe(MvvmMessages.ClearToasts, async @param =>
            {
                await queue.EnqueueAsync((MvvmMessages.ClearToasts, @param));
            });

            var syncContext = this.container.Resolve<TaskScheduler>();

            Task.Run(async () =>
            {
                while (!disposed)
                {
                    var item = await queue.DequeueAsync();
                    var message = item.Item1;
                    var @object = item.Item2;

                    switch (message)
                    {
                        case MvvmMessages.ShowAlarmModal:
                            {
                                var vm = container.Resolve<AlarmViewModal>();
                                await Task.Delay(0).ContinueWith(async (x) =>
                                {
                                    await ShowModal("AlarmModal", vm);

                                }, syncContext);
                            }
                            break;

                        case MvvmMessages.HideAlarmModal:
                        case MvvmMessages.HideMediaProgressModal:
                            {
                                await Task.Delay(0).ContinueWith(async (x) =>
                                {
                                    await CloseModal();

                                }, syncContext);
                            }
                            break;
                        case MvvmMessages.ShowToast:
                            {
                                await Task.Delay(0).ContinueWith(async (x) =>
                                {
                                    using var toastService = this.container.Resolve<IToastService>();
                                    await toastService.ShowMessage(@object as string);

                                }, syncContext);
                            }
                            break;

                        case MvvmMessages.ClearToasts:
                            {
                                await Task.Delay(0).ContinueWith(async (x) =>
                                {
                                    using var toastService = this.container.Resolve<IToastService>();
                                    await toastService.Clear();

                                }, syncContext);
                            }
                            break;
                        case MvvmMessages.ShowMediaProgessModal:
                            {
                                var vm = this.container.Resolve<MediaProgressViewModal>();
                                await Task.Delay(0).ContinueWith(async (x) =>
                                {
                                    await ShowModal("MediaProgressModal", vm);

                                }, syncContext);
                            }
                            break;

                    }

                    await Task.Delay(1000);
                }

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
                        if (navigater.ModalStack.LastOrDefault()?.GetType() == typeof(AlarmModal))
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
                        if (navigater.ModalStack.LastOrDefault()?.GetType() == typeof(MediaProgressModal))
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

            var top = navigater.NavigationStack.LastOrDefault();

            if (top != null && top.BindingContext.GetType().Name == vmName)
            {
                var disposable = viewModel as IDisposable;
                disposable?.Dispose();
                return;
            }

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

            var currentPage = navigater.NavigationStack.LastOrDefault();

            if (currentPage != null)
            {
                NavigatedBack?.Invoke(currentPage.BindingContext);
            }

        }

        public void Dispose()
        {
            disposed = true;
        }
    }
}

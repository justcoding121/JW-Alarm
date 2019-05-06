using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bible.Alarm.Services.Contracts;
using Bible.Alarm.UI.Views;
using Bible.Alarm.UI.Views.Bible;
using Bible.Alarm.UI.Views.Music;
using JW.Alarm.Common.Mvvm;
using JW.Alarm.ViewModels;
using Mvvmicro;
using Xamarin.Forms;

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

            Messenger<object>.Subscribe(Messages.SnoozeDismiss, async vm =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    await ShowModal("AlarmModal", vm);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                syncContext
                );

            });
        }

        public async Task CloseModal()
        {
            await navigater.PopModalAsync();

            var currentPage = navigater.NavigationStack.FirstOrDefault();

            if (currentPage != null)
            {
                NavigatedBack?.Invoke(currentPage.BindingContext);
            }

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
                        if(navigater.ModalStack.FirstOrDefault()?.GetType() == typeof(AlarmModal))
                        {
                            return;
                        }

                        var modal = IocSetup.Container.Resolve<AlarmModal>();
                        modal.BindingContext = viewModel;
                        await navigater.PushModalAsync(modal);
                        break;
                    }
                default:
                    throw new ArgumentException("Modal not defined.", name);
            }
        }

        public async Task GoBack()
        {
            await navigater.PopAsync();

            var currentPage = navigater.NavigationStack.LastOrDefault();

            if (currentPage != null)
            {
                NavigatedBack?.Invoke(currentPage.BindingContext);
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

        public async Task NavigateToHome()
        {
            while (navigater.ModalStack.Count > 0)
            {
                await navigater.PopModalAsync();
            }

            while (navigater.NavigationStack.Count > 1)
            {
                await navigater.PopAsync();
            }

            if (navigater.NavigationStack.Count == 0)
            {
                var home = IocSetup.Container.Resolve<Home>();
                await navigater.PushAsync(home);
                return;
            }

        }
    }
}

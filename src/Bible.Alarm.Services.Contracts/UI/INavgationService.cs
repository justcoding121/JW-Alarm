using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bible.Alarm.Services.Contracts
{
    public interface INavigationService
    {
        Task NavigateToHome();
        Task Navigate(object viewModel);
        Task GoBack();
        Task ShowModal(string name, object viewModel);
        Task CloseModal();
    }
}

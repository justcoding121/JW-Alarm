using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JW.Alarm.ViewModels
{
    public class RelayCommandAsync<T> : ICommand
    {
        private readonly Func<T, Task> executedMethod;
        private readonly Func<T, bool> canExecuteMethod;

        public event EventHandler CanExecuteChanged;
        public RelayCommandAsync(Func<T, Task> execute) : this(execute, null) { }

        public RelayCommandAsync(Func<T, Task> execute, Func<T, bool> canExecute)
        {
            this.executedMethod = execute ?? throw new ArgumentNullException("execute");
            this.canExecuteMethod = canExecute;
        }

        public bool CanExecute(object parameter) => this.canExecuteMethod == null || this.canExecuteMethod((T)parameter);
        public async void Execute(object parameter) => await this.executedMethod((T)parameter);
        public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

}

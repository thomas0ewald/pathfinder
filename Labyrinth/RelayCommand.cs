namespace Labyrinth
{
  using System;
  using System.Windows.Input;

  namespace Selecta
  {
    public class RelayCommand : ICommand
    {
      private Action<object> mExecute;

      private Predicate<object> mCanExecute;

      private event EventHandler CanExecuteChangedInternal;

      public RelayCommand(Action<object> aExecute)
        : this(aExecute, DefaultCanExecute)
      {
      }

      public RelayCommand(Action<object> aExecute, Predicate<object> aCanExecute)
      {
        this.mExecute = aExecute ?? throw new ArgumentNullException(nameof(aExecute));
        this.mCanExecute = aCanExecute ?? throw new ArgumentNullException(nameof(aCanExecute));
      }

      public event EventHandler CanExecuteChanged
      {
        add
        {
          CommandManager.RequerySuggested += value;
          this.CanExecuteChangedInternal += value;
        }

        remove
        {
          CommandManager.RequerySuggested -= value;
          this.CanExecuteChangedInternal -= value;
        }
      }

      public bool CanExecute(object aParameter)
      {
        return this.mCanExecute != null && this.mCanExecute(aParameter);
      }

      public void Execute(object aParameter)
      {
        this.mExecute(aParameter);
      }

      public void OnCanExecuteChanged()
      {
        EventHandler handler = this.CanExecuteChangedInternal;
        if (handler != null)
        {
          //DispatcherHelper.BeginInvokeOnUIThread(() => handler.Invoke(this, EventArgs.Empty));
          handler.Invoke(this, EventArgs.Empty);
        }
      }

      public void Destroy()
      {
        this.mCanExecute = a => false;
        this.mExecute = a => { return; };
      }

      private static bool DefaultCanExecute(object parameter)
      {
        return true;
      }
    }
  }
}

using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace System.Windows.Controls
{
    public abstract class Command : ICommand
    {
        #region ICommand Members
        private bool m_LastCanExecuteState;
        protected bool LastCanExecuteState
        {
            get { return m_LastCanExecuteState; }
            set
            {
                if (m_LastCanExecuteState != value)
                {
                    m_LastCanExecuteState = value;
                    OnCanExecuteChanged();
                }
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            bool result = parameter != null;
            LastCanExecuteState = result;
            return result;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                OnExecute(parameter);
        }

        protected abstract void OnExecute(object parameter);

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
        #endregion
    }
}

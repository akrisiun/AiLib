using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ai.Wpf.Models
{
    public class ReportCommand : ICommand
    {
        public ReportCommand()
        {
            CanExecuteChanged = null;
        }

        //     Occurs when changes occur that affect whether or not the command should execute.
        public event EventHandler CanExecuteChanged;
        bool executing = false;

        bool ICommand.CanExecute(object parameter)
        {
            return !executing;
        }

        public void Execute(object parameter)
        {
            executing = true;
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);

            if (parameter != null)
            { 
                if (parameter is Action)
                {
                    var act = parameter as Action;
                    act();
                }
            }

            executing = false;
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

    }
}
using System;
using System.Windows.Input;

namespace Breath_of_the_Wild_Multiplayer.Source_files;

public class RelayCommand : ICommand
{
	private Action<object> _execute;

	private Func<object, bool> _canExecute;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
	{
		_execute = execute;
		_canExecute = canExecute;
	}

	public bool CanExecute(object parameter)
	{
		if (_canExecute != null)
		{
			return _canExecute(parameter);
		}
		return true;
	}

	public void Execute(object parameter)
	{
		_execute(parameter);
	}
}

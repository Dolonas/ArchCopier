using System;
using System.Windows;
using ArchCopier.Infrastructure.Commands.Base;
using ArchCopier.Views;

namespace ArchCopier.Infrastructure.Commands;

public class ManageAboutProgramCommand : Command
{
	private Window? _Window;

	public override bool CanExecute(object? parameter)
	{
		return _Window == null;
	}

	public override void Execute(object? parameter)
	{
		var window = new AboutWindow
		{
			Owner = Application.Current.MainWindow
		};
		_Window = window;
		window.Closed += OnWindowClosed;
		
		window.ShowDialog();
	}

	public void OnWindowClosed(object sender, EventArgs e)
	{
		((Window)sender).Closed -= OnWindowClosed;
		_Window = null;
	}
}
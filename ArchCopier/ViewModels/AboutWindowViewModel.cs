using System.Windows;
using System.Windows.Input;
using ProductComposition.Infrastructure.Commands;
using ProductComposition.ViewModels.Base;

namespace ProductComposition.ViewModels;

public class AboutWindowViewModel : ViewModel
{
	public ICommand CloseWindowCommand { get; }
	public string? ApplicationVersion { get; set; }

	public AboutWindowViewModel()
	{
		ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version + " betta";
		CloseWindowCommand =
			new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
	}

	private bool CanCloseWindowCommandExecute(object p)
	{
		return true;
	}

	private void OnCloseWindowCommandExecuted(object p)
	{
		foreach (Window item in Application.Current.Windows)
			if (item.DataContext == this)
				item.Close();
	}
}
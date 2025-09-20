using System.Windows;
using System.Windows.Threading;
using ArchCopier.Infrastructure.Services;
using ArchCopier.ViewModels;
using Microsoft.Win32;

namespace ArchCopier.Views;

public partial class MainWindow
{
	public MainWindow()
	{
		InitializeComponent();
		
		DataContext = new MainWindowViewModel(new FileService(string.Empty, new string[] { ".spw" }));
		Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(Exception1);
	}

	private static void Exception1(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show(e.Exception.ToString());
	}
}
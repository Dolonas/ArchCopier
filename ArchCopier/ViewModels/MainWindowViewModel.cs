using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProductComposition.Infrastructure.Commands;
using ProductComposition.Infrastructure.Services;
using ProductComposition.ViewModels.Base;
using ProductComposition.Models;
using ProductComposition.Views;
using Serilog;

//TODO: - написать ТЗ
//TODO: - прописывать строчки путей с последними выбранными строчками сборок или папок архива в реестр, сделать сервис для работы с реестром
//TODO: - сделать возможность считывать уже открытую в Компасе активную сборку
//TODO: - прикрутить установщик 
//TODO: - прикрутить окно About program 
//TODO: - прикрутить справочную систему 
//TODO: - копирование файлов в такие же подпапки как в окружении изначальной сборки

namespace ProductComposition.ViewModels;

internal class MainWindowViewModel : ViewModel, INotifyPropertyChanged
{
	public new event PropertyChangedEventHandler? PropertyChanged;

	private string _title = "Product Composition";
	private string? _status;
	private UserControl? _currentControl;
	private readonly IFileService _fileService;
	private readonly IRegistryService _registryService;
	private string _currentDirectory;
	private string _kompasButtonName;
	private string _runButtonName;
	private ComponentCollectionModel _componentCollectionModel;
	private UserControl? _componentListControl;
	private ObservableCollection<ComponentModel>? _componentsList;
	private bool _isKompasButtonEnabled;
	private bool _isReqauringKompasButtonsEnabled;
	private string _fullNameOfCurrentAssembly;
	private string _arhDirectory;
	private readonly string _pathToThisAppDirectory = AppDomain.CurrentDomain.BaseDirectory;
	internal ILogger _logger;
	private Visibility _progressBarVisibility;
	
	private Kompas3D KompasInstance { get; }
	
	private void NotifyPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public string? Status
	{
		get => "Статус: " + _status;
		set
		{
			Set(ref _status, value);
			NotifyPropertyChanged(nameof(Status));
		}
	}
	public string Title
	{
		get => _title;
		set => Set(ref _title, value);
	}
	public string? KompasButtonName
	{
		get => _kompasButtonName;
		private set
		{
			_kompasButtonName = value;
			NotifyPropertyChanged(nameof(KompasButtonName));
		}
	}
	public string? RunButtonName
	{
		get => _runButtonName;
		private set
		{
			_runButtonName = value;
			NotifyPropertyChanged(nameof(RunButtonName));
		}
	}
	public bool IsKompasButtonEnabled
	{
		get => _isKompasButtonEnabled;
		set
		{
			_isKompasButtonEnabled = value;
			NotifyPropertyChanged(nameof(IsKompasButtonEnabled));
		}
	}
	
	public ObservableCollection<ComponentModel>? ComponentsList
	{
		get => _componentsList ?? null;
		set
		{
			_componentCollectionModel.SetComponents(value);
			_componentsList = value;
			OnPropertyChanged();
		}
	}
	
	public string FullNameOfCurrentAssembly
	{
		get => _fullNameOfCurrentAssembly;
		set
		{
			_fullNameOfCurrentAssembly = value;
			NotifyPropertyChanged(nameof(FullNameOfCurrentAssembly));
		}
	}
	
	public string ArhDirectory
	{
		get => _arhDirectory;
		set
		{
			_arhDirectory = value;
			NotifyPropertyChanged(nameof(ArhDirectory));
		}
	}
	public Visibility ProgressBarVisibility
	{
		get => _progressBarVisibility;
		set
		{
			Set(ref _progressBarVisibility, value);
			NotifyPropertyChanged(nameof(ProgressBarVisibility));
		}
	}
	
	private ComponentListViewModel? ComponentListVM { get; }
	
	public UserControl? CurrentControl
	{
		get => _currentControl;
		private set
		{
			_currentControl = value;
			NotifyPropertyChanged(nameof(CurrentControl));
		}
	}
	public UserControl? ComponentListControl
	{
		get => _componentListControl;
		private init
		{
			_componentListControl = value;
			NotifyPropertyChanged(nameof(ComponentListControl));
		}
	}
	
	public bool IsReqauringKompasButtonsEnabled
	{
		get => _isReqauringKompasButtonsEnabled;
		set
		{
			_isReqauringKompasButtonsEnabled = value;
			NotifyPropertyChanged(nameof(IsReqauringKompasButtonsEnabled));
		}
	}
	
	#region Commands
	
	#region StartKompasCommand
	public ICommand StartKompasCommand { get; }

	private bool CanStartKompasCommandExecute(object p)
	{
		return true;
	}

	private  void OnStartKompasCommandExecuted(object p)
	{
		var kompasByteStatus = KompasInstance.TryGetActiveKompas();
		Task.Run(() =>
		{
			if (kompasByteStatus == 0)
			{
				Status = "Компас не установлен";
			}
			if (kompasByteStatus == 1)
			{
				var kompasVisibility = true;
				Status = "Компас запускается...";
				KompasButtonName = "Компас запускается...";
				IsKompasButtonEnabled = false;
				kompasByteStatus = KompasInstance.LoadKompas(kompasVisibility);
				if (!kompasVisibility)
					Status = "Компас запущен в невидимом режиме";
				else
					Status = "Компас запущен";
			}
			SetKompasAndWorkButtonsStatus(kompasByteStatus);
		});
	}

	#endregion
	
	#region OpenAssemblyCommand

	public ICommand OpenAssemblyCommand { get; }

	private bool CanOpenAssemblyExecute(object p)
	{
		return true;
	}

	private void OnOpenAssemblyExecuted(object p)
	{
		FullNameOfCurrentAssembly = _fileService.ChooseFile(_currentDirectory, "a3d");
		KompasInstance.OpenComponent(FullNameOfCurrentAssembly);
		Status = $"Сборка открыта";
	}

	#endregion
	
	#region OpenArchDirectoryCommand

	public ICommand OpenArchDirectoryCommand { get; }

	private bool CanOpenArchDirectoryExecute(object p)
	{
		return true;
	}

	private void OnOpenArchDirectoryExecuted(object p)
	{
		ArhDirectory = _fileService.ChooseDirectory();
		Status = "Папка с архивом назначена";
	}

	#endregion
	
	#region CopyFilesOfComponentsToArchDirectoryCommand

	public ICommand CopyFilesOfComponentsToArchDirectoryCommand { get; }

	private bool CanCopyFilesOfComponentsToArchDirectoryExecute(object p)
	{
		return true;
	}

	private void OnCopyFilesOfComponentsToArchDirectoryExecuted(object p)
	{
		if (_componentCollectionModel.ComponentCollection != null)
		{
			var result = _fileService.CopyFiles(ConvertToListString(_componentCollectionModel.ComponentCollection),
				ArhDirectory);
			if (result == 0)
				Status = "Файлы не скопированы по неизвестной причине";
			else
			{
				Status = $"{result} файлов скопировано в папку архива";
			}
		}
	}

	#endregion
	
	
	#region ReadAssemblyCommand

	public ICommand ReadAssemblyCommand { get; }

	private bool CanReadAssemblyExecute(object p)
	{
		return true;
	}

	private void OnReadAssemblyExecuted(object p)
	{
		var result = _componentCollectionModel.SetComponents(KompasInstance.GetAllPartsOfActiveAssembly());
		Status = result > 0 ? $"Сборка прочитана, в ней найдено {result} оригинальных компонентов" : "Сборка прочитана, но она пуста";
	}

	#endregion
	
	#region GetHelpCommand

	public ICommand GetHelpCommand { get; }

	private bool CanGetHelpExecute(object p)
	{
		return true;
	}

	private void OnGetHelpExecuted(object p)
	{
		var FullNameOfHelpCHMFile = Path.GetFullPath(GetPathToHelpFile() + "\\ProductCompositionHelp.chm");
		
		if (!File.Exists(FullNameOfHelpCHMFile))
		{
			Status = $"Файл справки по пути {FullNameOfHelpCHMFile} не найден";
			return;
		}
		
		var proc = new System.Diagnostics.Process();
		proc.StartInfo.FileName = FullNameOfHelpCHMFile;
		proc.StartInfo.UseShellExecute = true;
		proc.Start ();
	}

	#endregion
	
	#region CloseApplicationCommand
	
	public ICommand CloseApplicationCommand { get; }

	private bool CanCloseApplicationCommandExecute(object p)
	{
		return true;
	}

	private void OnCloseApplicationCommandExecuted(object p)
	{
		Application.Current.Shutdown();
	}

	#endregion
	
	#endregion

	public MainWindowViewModel(IFileService fileService, IRegistryService registryService)
	{
		Title = $"Загрузчик архива v {Assembly.GetExecutingAssembly().GetName().Version} betta";
		_logger = new LoggerConfiguration()  
			.MinimumLevel.Debug() 
			.WriteTo.File("logs\\product_composition.log", rollingInterval: RollingInterval.Day)  
			.CreateLogger();
		WriteEnvironmentVariablesToLog();
		_currentDirectory = GetStartDirectory();
		_fileService = fileService;
		_registryService = registryService;
		ComponentListControl = new ComponentsListView();
		KompasInstance = new Kompas3D(_logger);
		KompasButtonName = "Запустить/Проверить Компас";
		RunButtonName = "Прочитать сборку";
		_componentCollectionModel = new ComponentCollectionModel();
		_componentCollectionModel.PropertyChanged += Model_PropertyChanged;
		ComponentListVM = new ComponentListViewModel(_componentCollectionModel, KompasInstance);
		IsKompasButtonEnabled = true;
		IsReqauringKompasButtonsEnabled = true;
		_fileService.CurrentDirectory = _currentDirectory;
		_fileService.ArrayOfMasks = new[] { ".a3d" };
		FullNameOfCurrentAssembly = "Сборка";
		ArhDirectory = "Путь к архиву";
		Status = GetStatusFromResult(KompasInstance.TryGetActiveKompas());
		CurrentControl =ComponentListControl;
		ProgressBarVisibility = Visibility.Hidden;
		
		#region Commands

		StartKompasCommand =
			new LambdaCommand(OnStartKompasCommandExecuted, CanStartKompasCommandExecute);
		
		OpenAssemblyCommand =
			new LambdaCommand(OnOpenAssemblyExecuted, CanOpenAssemblyExecute);
		
		OpenArchDirectoryCommand =
			new LambdaCommand(OnOpenArchDirectoryExecuted, CanOpenArchDirectoryExecute);
		
		CopyFilesOfComponentsToArchDirectoryCommand =
			new LambdaCommand(OnCopyFilesOfComponentsToArchDirectoryExecuted, CanCopyFilesOfComponentsToArchDirectoryExecute);
		
		ReadAssemblyCommand =
			new LambdaCommand(OnReadAssemblyExecuted, CanReadAssemblyExecute);
		
		GetHelpCommand =
			new LambdaCommand(OnGetHelpExecuted, CanGetHelpExecute);
		
		CloseApplicationCommand =
			new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);

		#endregion
	}

	private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			OnPropertyChanged(e.PropertyName);
	}

	private string GetStatusFromResult(byte kompasByteStatus)
	{
		return kompasByteStatus switch
		{
			0 => "Запустить Компас",
			1 => "Компас не запущен",
			2 => "Компас запущен",
			_ => "Компас не установлен"
		};
	}
	
	private string GetPathToHelpFile()
	{
		var pathToHelpDirectoryVar1 = Path.GetFullPath(_pathToThisAppDirectory + @"\Resources\Help"); // вариант, если приложение запускается у пользователя
		if(Directory.Exists(pathToHelpDirectoryVar1))
			return pathToHelpDirectoryVar1;
		else
			return Path.GetFullPath(_pathToThisAppDirectory + @"..\..\..\..\ProductComposition\Resources\Help"); //вариант номер два, если приложение отлаживается из IDE
	}
	
	private void SetKompasAndWorkButtonsStatus(byte kompasByteStatus)
	{
		switch (kompasByteStatus)
		{
			case 1:
				KompasButtonName =
					"Запустить/Проверить Компас";
				IsKompasButtonEnabled =
					true; // если Компас установлен, но не запущен, то будет гореть кнопка с этой надписью, с предложением запустить таки Компас 
				IsReqauringKompasButtonsEnabled =
					false; // если Компас не запущен, то WorkButtons нет необходимости нажимать
				break;
			case 2:
				KompasButtonName = "Компас запущен";
				IsKompasButtonEnabled = false; // если Компас уже запущен, то кнопка неактивна
				IsReqauringKompasButtonsEnabled = true; // если Компас запущен, то WorkButtons можно и нажать
				break;
			default:
				KompasButtonName = "Проверить Компас";
				IsKompasButtonEnabled = true;
				break;
		}
	}

	private string GetStartDirectory()
	{
		var startDirectoryVar1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Develop_tests_link\компас_чертежи_temp");
		if (Directory.Exists(startDirectoryVar1))
			return startDirectoryVar1;
		else
			return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
	}

	private List<string> ConvertToListString(ObservableCollection<ComponentModel> collection)
	{
		var result = new List<string>();
		foreach (var component in collection)
			if (component.FullFileName != null && !component.IsStandart)
				result.Add(component.FullFileName);
		return result;
	}

	private void WriteEnvironmentVariablesToLog()
	{
		
	}
	
}
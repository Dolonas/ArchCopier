using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArchCopier.Infrastructure.Commands;
using ArchCopier.Infrastructure.Services;
using ArchCopier.Infrastructure.Utilities;
using ArchCopier.ViewModels.Base;
using ArchCopier.Models;
using ArchCopier.Views;
using Serilog;
// ReSharper disable InconsistentNaming

//TODO: - написать ТЗ
//TODO: - прописывать строчки путей с последними выбранными строчками сборок или папок архива в реестр, сделать сервис для работы с реестром
//TODO: - сделать возможность считывать уже открытую в Компасе активную сборку
//TODO: - прикрутить установщик 
//TODO: - копирование файлов в такие же подпапки как в окружении изначальной сборки

namespace ArchCopier.ViewModels;

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
	private ComponentCollectionModel? _componentCollection;
	private UserControl? _componentListControl;
	private bool _isKompasButtonEnabled;
	private bool _isReqauringKompasButtonsEnabled;
	private string _fullNameOfCurrentAssembly;
	private string _arhDirectory;
	private readonly string _nameOfFileSettings;
	private Settings? _currentSettings;
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
	
	public ComponentCollectionModel? ComponentCollection
	{
		get => _componentCollection ?? null;
		set
		{
			_componentCollection = value;
			NotifyPropertyChanged(nameof(ComponentCollection));
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
	
	public Settings? CurrentSettings
	{
		get => _currentSettings;
		set
		{
			_currentSettings = value;
			NotifyPropertyChanged(nameof(CurrentSettings));
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
		private set
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
		ArhDirectory = _fileService.ChooseDirectory(CurrentSettings.ArchDirectoryA);
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
		if (ComponentCollection is not null)
		{
			var result = _fileService.CopyFiles(ConvertToListString(_componentCollection?.ComponentCollection),
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
		var result = _componentCollection?.SetComponents(KompasInstance.GetAllPartsOfActiveAssembly());
		Status = result > 0 ? $"Сборка прочитана, в ней найдено {result} оригинальных компонентов" : "Сборка прочитана, но она пуста";
	}

	#endregion
	
	#region WriteSettingsFileCommand

	public ICommand WriteSettingsFileCommand { get; }

	private bool CanWriteSettingsFileExecute(object p)
	{
		return true;
	}

	private void OnWriteSettingsFileExecuted(object p)
	{
		if (CurrentSettings is null)
		{
			_logger.Debug("Settings is null, невозможно его записать");
			return;
		}
		var jsonTextForWrite = JsonProcessor.SerialiseJSON(CurrentSettings);
		var newFileName = _fileService.WriteFile(_nameOfFileSettings, jsonTextForWrite);
		_logger.Information($"Записан файл настроек в папку {Path.GetDirectoryName(newFileName)}");
	}

	#endregion
	
	#region ChooseDirectoryWithArchACommand

	public ICommand ChooseDirectoryWithArchACommand { get; }

	private bool CanChooseDirectoryWithArchAExecute(object p)
	{
		return true;
	}

	private void OnChooseDirectoryWithArchAExecuted(object p)
	{
		var resultDirectory= _fileService.ChooseDirectory();
		if (string.IsNullOrEmpty(resultDirectory))
		{
			
			Status = "Папка не назначена";
		}
		else
		{
			_logger.Information($"Папка по умолчанию с архивом 'А' назначена {resultDirectory}");
			Status = "Папка по умолчанию с архивом 'А' назначена";
			if (CurrentSettings != null) CurrentSettings.ArchDirectoryA = resultDirectory;
			else _logger.Debug($"Сбой записи в Settings, они null (");
		}
		
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
		var fullNameOfHelpChmFile = Path.GetFullPath(GetPathToHelpFile() + "\\ArchCopierHelp.chm");
		
		if (!File.Exists(fullNameOfHelpChmFile))
		{
			Status = $"Файл справки по пути {fullNameOfHelpChmFile} не найден";
			return;
		}
		
		var proc = new System.Diagnostics.Process();
		proc.StartInfo.FileName = fullNameOfHelpChmFile;
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
		_nameOfFileSettings = Path.Combine(_pathToThisAppDirectory, @"Resources\Settings\settings.json" );
		CurrentSettings = GetSettingsFromFile(_nameOfFileSettings);
		if (CurrentSettings is null)
			_nameOfFileSettings = CreateSettingsFile(_nameOfFileSettings);
		ComponentListControl = new ComponentsListView();
		KompasInstance = new Kompas3D(_logger);
		KompasButtonName = "Запустить/Проверить Компас";
		RunButtonName = "Прочитать сборку";
		ComponentCollection = new ComponentCollectionModel();
		ComponentCollection.PropertyChanged += PropertyChanged;
		ComponentListVM = new ComponentListViewModel(ComponentCollection);
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
		
		ChooseDirectoryWithArchACommand =
			new LambdaCommand(OnChooseDirectoryWithArchAExecuted, CanChooseDirectoryWithArchAExecute);
		
		WriteSettingsFileCommand =
			new LambdaCommand(OnWriteSettingsFileExecuted, CanWriteSettingsFileExecute);
		
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
			return Path.GetFullPath(_pathToThisAppDirectory + @"..\..\..\..\ArchCopier\Resources\Help"); //вариант номер два, если приложение отлаживается из IDE
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

	private Settings? GetSettingsFromFile(string fileName)
	{
		var jsonFileInString = _fileService.ReadFileToString(fileName);
		if(string.IsNullOrEmpty(jsonFileInString))
			return null;
		var result  = JsonProcessor.DeserialiseJSON(jsonFileInString);
		if(result == null)
			_logger.Debug($"Не удалось получить файл настроек по пути {fileName}");
		return result;
	}
	private string CreateSettingsFile(string fileName)
	{
		var jsonFileInString = string.Empty;
		if (CurrentSettings is null)
		{
			_logger.Debug("CurrentSettings is null");
			CurrentSettings = new Settings
			{
				SettingsFileName = fileName,
				ArchDirectoryA = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				ArchDirectoryB = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				ProjectsDirectoryA = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				ProjectsDirectoryB = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
			};
			jsonFileInString = JsonProcessor.SerialiseJSON(CurrentSettings);
		}
		var newFileName = _fileService.WriteFile(fileName, jsonFileInString);
		_logger.Information($"Создан файл настроек по пути {newFileName}");
		return newFileName;
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
		_logger.Information($"MachineName: {Environment.MachineName}");
		_logger.Information($"OS version: {Environment.OSVersion}");
		var drives = Environment.GetLogicalDrives();
		_logger.Information($"Logical drives: {String.Join(", ", drives)}");
		_logger.Information($"UserDomainName: {Environment.UserDomainName}");
		
		_logger.Information($"UserName: {Environment.UserName}");
	}
	
}
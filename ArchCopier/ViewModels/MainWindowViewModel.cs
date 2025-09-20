using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using ArchCopier.Infrastructure.Commands;
using ArchCopier.Infrastructure.Services;
using ArchCopier.Infrastructure.Utilities;
using ArchCopier.ViewModels.Base;
using ArchCopier.Models;
using Serilog;
// ReSharper disable InconsistentNaming

//TODO: - сделать отбор оригинальных компонентов прямо в componentCollection по linq
//TODO: - сделать неактивными кнопки "прочитать сборку" и др., если компас не подключен, чтобы не генерировать исключения
//TODO: - копирование файлов в такие же подпапки как в окружении изначальной сборки, а может быть по какому-то специальному правилу

namespace ArchCopier.ViewModels;

internal class MainWindowViewModel : ViewModel, INotifyPropertyChanged
{
	public new event PropertyChangedEventHandler? PropertyChanged;

	private string _title = "Архив";
	private string? _status;
	private readonly IFileService _fileService;
	private string _kompasButtonName;
	private string _runButtonName;
	private ComponentCollectionModel? _componentCollection;
	private AssemblyTreeModel _assemblyTree;
	private bool _isKompasButtonEnabled;
	private bool _isWorkButtonsEnabled;
	private bool _isButtonOfActiveDocumentEnabled;
	private bool _isArchUploadButtonsEnabled;
	private string _fullNameOfCurrentAssembly;
	private string _arhDirectory;
	private ComponentModel? _selectedComponent;
	private readonly string _nameOfFileSettings;
	private Settings? _currentSettings;
	private readonly string _pathToThisAppDirectory = AppDomain.CurrentDomain.BaseDirectory;
	private readonly ILogger _logger;
	private Kompas3D KompasInstance { get; }
	
	private double _progress;
	private Visibility _progressBarVisibility;
	
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
	
	public double Progress
	{
		get => _progress;
		set
		{
			Set(ref _progress, value);
			NotifyPropertyChanged(nameof(Progress));
		}
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
	public bool IsButtonOfActiveDocumentEnabled
	{
		get => _isButtonOfActiveDocumentEnabled;
		set
		{
			_isButtonOfActiveDocumentEnabled = value;
			NotifyPropertyChanged(nameof(IsButtonOfActiveDocumentEnabled));
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

	private ComponentCollectionModel ComponentCollection
	{
		get
		{
			if (_componentCollection != null) return _componentCollection;
			return _componentCollection = new ComponentCollectionModel(new List<ComponentModel>());
		}
		set
		{
			_componentCollection = value;
			NotifyPropertyChanged(nameof(ListComponents));
		}
	}
	private AssemblyTreeModel AssemblyTree
	{
		get
		{
			if (_assemblyTree != null) return _assemblyTree;
			return _assemblyTree = new AssemblyTreeModel();
		}
		set
		{
			_assemblyTree = value;
			NotifyPropertyChanged(nameof(ListComponents));
		}
	}

	public ObservableCollection<ComponentModel> ListComponents => ComponentCollection.GetComponentList() ?? new ObservableCollection<ComponentModel>();

	public ComponentModel? SelectedComponent
	{
		get => _selectedComponent ?? null;
		set
		{
			_selectedComponent = value;
			NotifyPropertyChanged(nameof(SelectedComponent));
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

	
	
	public bool IsWorkButtonsEnabled
	{
		get => _isWorkButtonsEnabled;
		set
		{
			_isWorkButtonsEnabled = value;
			NotifyPropertyChanged(nameof(IsWorkButtonsEnabled));
		}
	}
	
	public bool IsArchUploadButtonsEnabled
	{
		get => _isArchUploadButtonsEnabled;
		set
		{
			_isArchUploadButtonsEnabled = value;
			NotifyPropertyChanged(nameof(IsArchUploadButtonsEnabled));
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

			if (kompasByteStatus == 2 && KompasInstance.GetActive3DDocument() == 3)
				IsButtonOfActiveDocumentEnabled = true;
			SetKompasButtonStatus(kompasByteStatus);
		});
	}

	#endregion
	
	#region CheckKompasInstallCommand

	public ICommand CheckKompasInstallCommand { get; }

	private bool CanCheckKompasInstallExecute(object p)
	{
		return true;
	}

	private void OnCheckKompasInstallExecuted(object p)
	{
		var kompasByteStatus = KompasInstance.TryGetActiveKompas();
		Status = kompasByteStatus switch
		{
			0 => "Компас не установлен",
			1 => "Компас установлен, но не запущен",
			2 => "Компас установлен и запущен",
			_ => "Компас не установлен"
		};
		SetKompasButtonStatus(
			kompasByteStatus); // установка того, какие кнопки становятся активны, а какие - нет
	}

	#endregion
	
	#region MakeKompasInvisibleCommand

	public ICommand MakeKompasInvisibleCommand { get; }

	private bool CanMakeKompasInvisibleExecute(object p)
	{
		return true;
	}

	private void OnMakeKompasInvisibleExecuted(object p)
	{
		KompasInstance.Visibility = false;
		Status = "Компас переведён в невидимый режим";
	}

	#endregion
	
	#region MakeKompasVisibleCommand

	public ICommand MakeKompasVisibleCommand { get; }

	private bool CanMakeKompasVisibleExecute(object p)
	{
		return true;
	}

	private void OnMakeKompasVisibleExecuted(object p)
	{
		KompasInstance.Visibility = true;
		Status = "Компас переведён в видимый режим";
	}

	#endregion
	
	#region MuteKompasModeWithNoCommand

	public ICommand MuteKompasModeWithYesCommand { get; }

	private bool CanMuteKompasModeWithYesExecute(object p)
	{
		return true;
	}

	private void OnMuteKompasModeWithYesExecuted(object p)
	{
		KompasInstance.HideMassageMode = HideMessageEnum.AlwaysYes;
		Status = "Компас переведён режим игнорирования диалоговых окон с ответом \"Да\"";
	}

	#endregion
	
	#region MuteKompasModeWithNoCommand

	public ICommand MuteKompasModeWithNoCommand { get; }

	private bool CanMuteKompasModeWithNoExecute(object p)
	{
		return true;
	}

	private void OnMuteKompasModeWithNoExecuted(object p)
	{
		KompasInstance.HideMassageMode = HideMessageEnum.AlwaysNo;
		Status = "Компас переведён режим игнорирования диалоговых окон с ответом \"Нет\"";
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
		if (!File.Exists(FullNameOfCurrentAssembly))
		{
			Status = $"Файл или не выбран или не существует";
			return;
		}
		Status = $"Сборка открывается";
		Task.Run(() => { KompasInstance.OpenComponent(FullNameOfCurrentAssembly); });
		IsWorkButtonsEnabled = true;
		Status = $"Сборка открыта";
	}

	#endregion
	
	#region ChooseAssemblyCommand

	public ICommand ChooseAssemblyCommand { get; }

	private bool CanChooseAssemblyExecute(object p)
	{
		return true;
	}

	private void OnChooseAssemblyExecuted(object p)
	{
		FullNameOfCurrentAssembly = _fileService.ChooseFile(_currentSettings?.ProjectsDirectoryA, "a3d");
		Status = $"Сборка выбрана";
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
		ArhDirectory = _fileService.ChooseDirectory(CurrentSettings?.ArchDirectoryA);
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
		ProgressBarVisibility = Visibility.Visible;
		var result = 0;
		Status = "Файлы копируются...";
		Task.Run(() =>
		{
			result = _fileService.CopyFiles(ConvertToListString(_componentCollection?.ComponentCollection),
				ArhDirectory);
			Status = result == 0 ? "Файлы не скопированы по неизвестной причине" : $"{result} файлов скопировано в папку архива";
			ProgressBarVisibility = Visibility.Hidden;
		});
		
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
		var parts = ComponentCollection.ComponentCollection;
		Task.Run(() =>
		{
			Status = "Сборка читается...";
			parts = KompasInstance.GetAllPartsOfActiveAssembly();
			if (parts != null) ComponentCollection = new ComponentCollectionModel(parts.ToList());
			var numOfComponents = ComponentCollection.NumOfOriginalComponents;
			Status = numOfComponents > 0 ? $"Сборка прочитана, в ней найдено {numOfComponents} оригинальных компонентов" : "Сборка прочитана, но она пуста";
			IsArchUploadButtonsEnabled = true;
		});
	}
	
	#endregion
	
	
	#region ReadActiveAssemblyCommand

	public ICommand ReadActiveAssemblyCommand { get; }

	private bool CanReadActiveAssemblyExecute(object p)
	{
		return true;
	}

	private void OnReadActiveAssemblyExecuted(object p)
	{
		
		var resultOfGettingActiveAssembly = KompasInstance.GetActive3DDocument();
		var parts = ComponentCollection.ComponentCollection;
		var treeOfParts = _assemblyTree;
		if (resultOfGettingActiveAssembly == 3)
		{
			Task.Run(() =>
			{
				Status = "Сборка читается...";
				parts = KompasInstance.GetAllPartsOfActiveAssembly();
				treeOfParts = KompasInstance.GetActiveAssemblyTree();
				if (parts != null) ComponentCollection = new ComponentCollectionModel(parts.ToList());
				var numOfComponents = ComponentCollection.NumOfOriginalComponents;
				Status = numOfComponents > 0 ? $"Сборка прочитана, в ней найдено {numOfComponents} оригинальных компонентов" : "Сборка прочитана, но она пуста";
				IsArchUploadButtonsEnabled = true;
			});
			
		}
		else
		{
			Status = $"Активная сборка Компаса не найдена";
		}
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
	
	#region ChooseDirectoryWithProjectsACommand

	public ICommand ChooseDirectoryWithProjectsACommand { get; }

	private bool CanChooseDirectoryWithProjectsAExecute(object p)
	{
		return true;
	}

	private void OnChooseDirectoryWithProjectsAExecuted(object p)
	{
		var resultDirectory= _fileService.ChooseDirectory();
		if (string.IsNullOrEmpty(resultDirectory))
		{
			Status = "Папка не назначена";
		}
		else
		{
			_logger.Information($"Папка по умолчанию с проектами 'А' назначена {resultDirectory}");
			Status = "Папка по умолчанию с проектами 'А'назначена";
			if (CurrentSettings != null) CurrentSettings.ProjectsDirectoryA = resultDirectory;
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

	public MainWindowViewModel(IFileService fileService)
	{
		Title = $"Загрузчик архива v {Assembly.GetExecutingAssembly().GetName().Version} betta";
		_logger = new LoggerConfiguration()  
			.MinimumLevel.Debug() 
			.WriteTo.File("logs\\arch_copier.log", rollingInterval: RollingInterval.Day)  
			.CreateLogger();
		WriteEnvironmentVariablesToLog();
		var currentDirectory = GetStartDirectory();
		_fileService = fileService;
		_nameOfFileSettings = Path.Combine(_pathToThisAppDirectory, @"Resources\Settings\settings.json" );
		CurrentSettings = GetSettingsFromFile(_nameOfFileSettings);
		if (CurrentSettings is null)
			_nameOfFileSettings = CreateSettingsFile(_nameOfFileSettings);
		KompasInstance = new Kompas3D(_logger);
		KompasButtonName = "Запустить/Проверить Компас";
		RunButtonName = "Прочитать сборку";
		_componentCollection = new ComponentCollectionModel
		{
			ComponentCollection = new ObservableCollection<ComponentModel>(
				new ComponentModel[]{new ComponentModel("один", "один"), new ComponentModel("два", "два")})
		};
		SelectedComponent = ComponentCollection?.GetComponentList()?[0];
		_componentCollection.PropertyChanged += Model_PropertyChanged;
		_assemblyTree =  new AssemblyTreeModel();
		_assemblyTree.PropertyChanged += Model_PropertyChanged;
		IsKompasButtonEnabled = true;
		IsWorkButtonsEnabled = false;
		IsArchUploadButtonsEnabled = false;
		IsButtonOfActiveDocumentEnabled = false;
		_fileService.CurrentDirectory = currentDirectory;
		_fileService.ArrayOfMasks = new[] { ".a3d" };
		FullNameOfCurrentAssembly = "Сборка";
		ArhDirectory = "Путь к архиву";
		Status = GetStatusFromResult(KompasInstance.TryGetActiveKompas());
		ProgressBarVisibility = Visibility.Hidden;
		
		#region Commands

		StartKompasCommand =
			new LambdaCommand(OnStartKompasCommandExecuted, CanStartKompasCommandExecute);
		
		CheckKompasInstallCommand =
			new LambdaCommand(OnCheckKompasInstallExecuted, CanCheckKompasInstallExecute);
		
		MakeKompasInvisibleCommand =
			new LambdaCommand(OnMakeKompasInvisibleExecuted, CanMakeKompasInvisibleExecute);
		
		MakeKompasVisibleCommand =
			new LambdaCommand(OnMakeKompasVisibleExecuted, CanMakeKompasVisibleExecute);
		
		MuteKompasModeWithYesCommand =
			new LambdaCommand(OnMuteKompasModeWithYesExecuted, CanMuteKompasModeWithYesExecute);
		
		MuteKompasModeWithNoCommand =
			new LambdaCommand(OnMuteKompasModeWithNoExecuted, CanMuteKompasModeWithNoExecute);
		
		OpenAssemblyCommand =
			new LambdaCommand(OnOpenAssemblyExecuted, CanOpenAssemblyExecute);
		
		ChooseAssemblyCommand =
			new LambdaCommand(OnChooseAssemblyExecuted, CanChooseAssemblyExecute);
		
		OpenArchDirectoryCommand =
			new LambdaCommand(OnOpenArchDirectoryExecuted, CanOpenArchDirectoryExecute);
		
		CopyFilesOfComponentsToArchDirectoryCommand =
			new LambdaCommand(OnCopyFilesOfComponentsToArchDirectoryExecuted, CanCopyFilesOfComponentsToArchDirectoryExecute);
		
		ReadAssemblyCommand =
			new LambdaCommand(OnReadAssemblyExecuted, CanReadAssemblyExecute);
		
		ReadActiveAssemblyCommand =
			new LambdaCommand(OnReadActiveAssemblyExecuted, CanReadActiveAssemblyExecute);
		
		GetHelpCommand =
			new LambdaCommand(OnGetHelpExecuted, CanGetHelpExecute);
		
		ChooseDirectoryWithArchACommand =
			new LambdaCommand(OnChooseDirectoryWithArchAExecuted, CanChooseDirectoryWithArchAExecute);
		
		ChooseDirectoryWithProjectsACommand =
			new LambdaCommand(OnChooseDirectoryWithProjectsAExecuted, CanChooseDirectoryWithProjectsAExecute);
		
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
	
	private void SetKompasButtonStatus(byte kompasByteStatus)
	{
		switch (kompasByteStatus)
		{
			case 1:
				KompasButtonName =
					"Запустить/Проверить Компас";
				IsKompasButtonEnabled =
					true; // если Компас установлен, но не запущен, то будет гореть кнопка с этой надписью, с предложением запустить таки Компас 
				break;
			case 2:
				KompasButtonName = "Компас запущен";
				IsKompasButtonEnabled = false; // если Компас уже запущен, то кнопка неактивна
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

	private List<string> ConvertToListString(ObservableCollection<ComponentModel>? collection)
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

	private void CalculateProgress(int count)
	{
		Progress += 100 / (double)count;
	}
	
}
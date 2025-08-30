using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ArchCopier.Infrastructure.Commands;
using ArchCopier.Infrastructure.Services;
using ArchCopier.Models;
using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;

namespace ArchCopier.ViewModels;

public class ComponentListViewModel : ViewModel, INotifyPropertyChanged, IComponentCollection
{
	public new event PropertyChangedEventHandler? PropertyChanged;

	private ComponentCollectionModel _componentCollection;
	private Kompas3D _kompasInstance;
	private ObservableCollection<ComponentModel>? _componentList;
	private ComponentModel? _selectedComponent;

	public ObservableCollection<ComponentModel>? ComponentList
	{
		get => _componentList ?? null;
		set
		{
			_componentCollection.SetComponents(value);
			_componentList = value;
			OnPropertyChanged();
		}
	}

	public ComponentModel? SelectedComponent
	{
		get => _selectedComponent ?? null;
		set
		{
			_componentCollection.SetSelectedComponent(value);
			_selectedComponent = value;
			OnPropertyChanged();
		}
	}


	#region Commands

	#region OpenSelectedFileCommand

	public ICommand OpenSelectedFileCommand { get; }

	private bool CanOpenSelectedFileExecute(object p)
	{
		return true;
	}
	
	
	
	private void OnOpenSelectedFileExecuted(object obj)
	{
		var result = _kompasInstance.TryGetActiveKompas();
		if (result < 2) return;
		if (_componentCollection.ComponentCollection is not { Count: > 0 }) return;

		if (SelectedComponent?.ComponentDesignation == null) return;
	}
	
	#endregion
	
	#endregion


	public ComponentListViewModel(ComponentCollectionModel fileListModel, Kompas3D kompasInstance)
	{
		_componentCollection = fileListModel;
		_kompasInstance = kompasInstance;
		_componentCollection.PropertyChanged += Model_PropertyChanged;
		if (_componentList is not null)
			SelectedComponent = _componentList[0];
		OpenSelectedFileCommand =
			new LambdaCommand(OnOpenSelectedFileExecuted, CanOpenSelectedFileExecute);
	}

	private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			OnPropertyChanged(e.PropertyName);
	}
}
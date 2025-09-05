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



	public ComponentListViewModel(ObservableCollection<ComponentModel> componentListModel)
	{
		_componentList = componentListModel;
		_componentCollection.PropertyChanged += Model_PropertyChanged;
		if (_componentList is not null)
			SelectedComponent = _componentList[0];
	}

	private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			OnPropertyChanged(e.PropertyName);
	}
}
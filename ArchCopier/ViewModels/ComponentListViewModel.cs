using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchCopier.Models;
using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;

namespace ArchCopier.ViewModels;

public class ComponentListViewModel : ViewModel, INotifyPropertyChanged, IComponentCollection
{
	public new event PropertyChangedEventHandler? PropertyChanged;
	
	private readonly ComponentCollectionModel _componentCollection;
	private ObservableCollection<ComponentModel>? _componentList;
	private ComponentModel? _selectedComponent;

	public ObservableCollection<ComponentModel>? ComponentList
	{
		get => _componentList;
		set
		{
			_componentCollection.SetComponents(value);
			if (value != null) _componentList = new ObservableCollection<ComponentModel>(value);
			OnPropertyChanged();
		}
	}

	public ComponentModel? SelectedComponent
	{
		get => _selectedComponent ?? null;
		set
		{
			_selectedComponent = value;
			OnPropertyChanged();
		}
	}



	public ComponentListViewModel(ComponentCollectionModel componentCollection)
	{
		_componentCollection = componentCollection;
		ComponentList = _componentCollection.GetComponentList() ?? new ObservableCollection<ComponentModel>();
		_componentList = new ObservableCollection<ComponentModel>(_componentCollection.GetComponentList() ?? new ObservableCollection<ComponentModel>());
		_componentCollection.PropertyChanged += Model_PropertyChanged;
		SelectedComponent = _componentList?[0];
	}
	
	private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			OnPropertyChanged(e.PropertyName);
	}

}
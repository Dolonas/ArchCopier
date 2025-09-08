using System.Collections.ObjectModel;
using System.ComponentModel;
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

	public ObservableCollection<ComponentModel>? ComponentCollection
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
			_selectedComponent = value;
			OnPropertyChanged();
		}
	}



	public ComponentListViewModel(ComponentCollectionModel componentCollection)
	{
		_componentCollection = componentCollection;
		SelectedComponent = ComponentCollection?[0];
	}
	
	private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.PropertyName))
			OnPropertyChanged(e.PropertyName);
	}

}
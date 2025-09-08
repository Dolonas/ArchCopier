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
	private ObservableCollection<ComponentModel>? _componentCollection;
	private ComponentModel? _selectedComponent;

	public ObservableCollection<ComponentModel>? ComponentCollection
	{
		get => _componentCollection ?? null;
		set
		{
			_componentCollection = value;
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



	public ComponentListViewModel(ObservableCollection<ComponentModel> componentCollection)
	{
		ComponentCollection = componentCollection;
		if (ComponentCollection.Count == 0)
			ComponentCollection.Add(new ComponentModel("Компонент 1", string.Empty));
		if (_componentCollection is not null)
			SelectedComponent = ComponentCollection[0];
	}

}
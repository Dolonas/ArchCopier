using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProductComposition.Models.Interfaces;

public interface IComponentCollection
{
	public ObservableCollection<ComponentModel>? ComponentList { get; set; }
	public ComponentModel? SelectedComponent { get; set; }
}
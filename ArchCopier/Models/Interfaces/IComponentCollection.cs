using System.Collections.ObjectModel;

namespace ArchCopier.Models.Interfaces;

public interface IComponentCollection
{
	public ObservableCollection<ComponentModel>? ComponentList { get; set; }
	public ComponentModel? SelectedComponent { get; set; }

}
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ArchCopier.Models.Interfaces;

public interface IComponentCollection
{
	public ObservableCollection<ComponentModel>? ComponentCollection { get; set; }
	public ComponentModel? SelectedComponent { get; set; }
}
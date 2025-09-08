using System.Collections.ObjectModel;

namespace ArchCopier.Models.Interfaces;

public interface IComponentCollection
{
	public ObservableCollection<ComponentModel>? ComponentCollection { get; set; }
	public ComponentModel? SelectedComponent { get; set; }

	private void Model_PropertyChanged()
	{
	}
}
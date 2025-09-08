using System.Collections.ObjectModel;
using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;

namespace ArchCopier.Models;

public class ComponentCollectionModel : ViewModel, IEntity
{
    private ObservableCollection<ComponentModel>? _components;
    private ComponentModel? _selectedComponent;

    public int Id { get; set; }


    public ComponentCollectionModel()
    {
        _components = new ObservableCollection<ComponentModel> { new ComponentModel() };
        _selectedComponent = _components[0];
    }

    public ComponentModel? SelectedComponent
    {
        get => _selectedComponent;
        set => _selectedComponent = value;
    }

    public ObservableCollection<ComponentModel>? ComponentCollection
    {
        get => _components;
        set => _components = value;
    }

    public int SetComponents(ObservableCollection<ComponentModel>? components)
    {
        int result = 0;
        if (components!.Count > 0)
        {
            _components = components;
            result = _components.Where(c => !c.IsStandart).Select(c => c.ShortFileName).Distinct().Count();
        }
        else
        {
            _components = new ObservableCollection<ComponentModel> { new("Пусто") };
            result = 0;
        }
        SelectedComponent = _components?[0];
        return result;
    }
	
    public void SetSelectedComponent(ComponentModel? value)
    {
        if (_selectedComponent != null && _selectedComponent != value)
            SelectedComponent = value;
    }

    public void RemoveFile(ComponentModel deletingFile)
    {
        if (_components != null && _selectedComponent != deletingFile)
            _components.Remove(deletingFile);
    }

    public ObservableCollection<ComponentModel>? GetFileList()
    {
        return _components;
    }

    public ComponentModel? GetSelectedFile()
    {
        return SelectedComponent;
    }
}
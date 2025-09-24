using System.Collections.ObjectModel;
using System.ComponentModel;
using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;

namespace ArchCopier.Models;

public class ComponentCollectionModel : ViewModel, IEntity
{
    private ObservableCollection<ComponentModel>? _components;
    private ComponentModel _topComponent;
    private ComponentModel? _selectedComponent;

    public int Id { get; set; }


    public ComponentCollectionModel()
    {
        ComponentCollection = new ObservableCollection<ComponentModel> { new ComponentModel() };
        _selectedComponent = ComponentCollection[0];
    }
    
    public ComponentCollectionModel(List<ComponentModel> components)
    {
        ComponentCollection = new ObservableCollection<ComponentModel> (components);
        _selectedComponent = ComponentCollection[0];
    }

    private ComponentModel? SelectedComponent
    {
        get => _selectedComponent;
        set => _selectedComponent = value;
    } 
    
    public int NumOfOriginalComponents
    {
        get
        {
            if (ComponentCollection != null)
                return ComponentCollection.Where(c => !c.IsStandart).Select(c => c.FullFileName).Distinct().Count();
            return 0;
        }
    }

    public ObservableCollection<ComponentModel>? ComponentCollection
    {
        get => _components;
        set => _components = value;
    }

    public ComponentModel? TopComponent
    {
        get => _components?[0];
        set => _topComponent = value;
    }

    public int SetComponents(ObservableCollection<ComponentModel>? components)
    {
        int result = 0;
        if (components!.Count > 0)
        {
            ComponentCollection = components;
            result = ComponentCollection.Where(c => !c.IsStandart).Select(c => c.ShortFileName).Distinct().Count();
        }
        else
        {
            ComponentCollection = new ObservableCollection<ComponentModel> { new("Пусто") };
            result = 0;
        }
        SelectedComponent = ComponentCollection[0];
        return result;
    }
	
    public void SetSelectedComponent(ComponentModel? value)
    {
        if (_selectedComponent != null && _selectedComponent != value)
            SelectedComponent = value;
    }

    public void RemoveComponent(ComponentModel deletingFile)
    {
        if (ComponentCollection != null && _selectedComponent != deletingFile)
            ComponentCollection.Remove(deletingFile);
    }

    public ObservableCollection<ComponentModel>? GetComponentList()
    {
        return ComponentCollection ?? null;
    }

    public ComponentModel? GetSelectedFile()
    {
        return SelectedComponent;
    }
}
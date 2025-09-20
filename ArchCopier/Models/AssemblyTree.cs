using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;
using Kompas6API5;

namespace ArchCopier.Models;

public class AssemblyTreeModel: ViewModel, IEntity
{
    public int Id { get; set; }
    
    public part Value { get; set; }
    public Node Children { get; set; }

    public AssemblyTreeModel(part value, Node children)
    {
        Value = value;
        Children = children;
    }

    public AssemblyTreeModel() : this(new partClass(), new Node(new partClass(), null))
    {
            
    }
}
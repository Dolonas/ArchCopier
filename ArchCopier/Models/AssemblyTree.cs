using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;
using Kompas6API5;
using KompasAPI7;

namespace ArchCopier.Models;

public class AssemblyTreeModel: Node, IEntity
{
    public AssemblyTreeModel(IPart7 value, Node children) : base(value, null)
    {
        Value = value;
        Children = new List<Node>();
    }

    public AssemblyTreeModel() : this(null, null)
    {
            
    }
}
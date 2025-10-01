using KompasAPI7;

namespace ArchCopier.Models.Interfaces;
public interface INode
{
    public int Id { get; set; }
    public IPart7 Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public void BreadthFirstSearch(Node root) { }
    public void DepthFirstSearch(Node root) { }
    
    public Node InsertNode(IPart7 value, INode parent)
    {
        return new Node(null!, null!);
    }
}
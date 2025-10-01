using ArchCopier.Models.Interfaces;
using KompasAPI7;

namespace ArchCopier.Models;

public class AssemblyTreeModel: INode, IEntity
{
    public int Id { get; set; }
    public IPart7 Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public AssemblyTreeModel(IPart7 value, INode children) 
    {
        Value = value;
        Children = new List<Node>();
    }

    public AssemblyTreeModel() : this(null, null)
    {
            
    }
    
    public void BreadthFirstSearch(INode root) 
    {
        Queue<INode> queue = new Queue<INode>();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            INode current = queue.Dequeue();
            foreach (Node child in current.Children) 
            {
                queue.Enqueue(child);
            }
        }
    }
    public void DepthFirstSearch(INode root)
    {
        Stack<INode> stack = new Stack<INode>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            INode current = stack.Pop();
            foreach (Node child in current.Children)
            {
                stack.Push(child);
            }
        }
    }
    
    public Node InsertNode(IPart7 value, Node parent) 
    {
        Node newNode = new Node(value, parent);
        parent.Children.Add(newNode);
        return newNode;
    }
}
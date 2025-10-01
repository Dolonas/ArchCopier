using ArchCopier.Models.Interfaces;
using ArchCopier.ViewModels.Base;
using KompasAPI7;

namespace ArchCopier.Models;

public class Node : ViewModel, INode
{
    public int Id { get; set; }
    public IPart7 Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public Node(IPart7 value, Node parent)
    {
        Value = value;
        Parent = parent;
        Children = new List<Node>();
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
using ArchCopier.ViewModels.Base;
using Kompas6API3D5COM;
using Kompas6API5;
using KompasAPI7;

namespace ArchCopier.Models;

public class Node : ViewModel
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
    
    public void BreadthFirstSearch(Node root) 
    {
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            Node current = queue.Dequeue();
            Console.WriteLine(current.Value);
            foreach (Node child in current.Children) 
            {
                queue.Enqueue(child);
            }
        }
    }
    public void DepthFirstSearch(Node root)
    {
        Stack<Node> stack = new Stack<Node>();
        stack.Push(root);
        while (stack.Count > 0)
        {
            Node current = stack.Pop();
            Console.WriteLine(current.Value);
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
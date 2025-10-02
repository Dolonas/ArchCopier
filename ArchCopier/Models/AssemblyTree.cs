using KompasAPI7;

namespace ArchCopier.Models;

public class AssemblyTreeModel<Node>: IEnumerable<Node>
{
    public int Id { get; set; }
    private AssemblyTreeModel<Node> _head;

    private int _count;
    public IPart7 Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public AssemblyTreeModel(Node value, Node children) 
    {
        Value = value;
        Children = new List<Node>();
    }
    
    public void BreadthFirstSearch(Node root) 
    {
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            Node current = queue.Dequeue();
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
            foreach (Node child in current.Children)
            {
                stack.Push(child);
            }
        }
    }
    
    public Node InsertNode(Node value, Node parent) 
    {
        Node newNode = new Node(value, parent);
        parent.Children.Add(newNode);
        return newNode;
    }

    public int CompareTo(Node? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Id.CompareTo(other.Id);
    }

    #region Нумератор

    public IEnumerator<Node> GetEnumerator() 
    {
        return PreOrderTraversal(); 
    }
        
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() 
              
    { 
        return GetEnumerator(); 
    } 

    #endregion

    #region Количество узлов в дереве

    public int Count
    {
        get
        {
            return _count;
        }
    }

    #endregion
    
    public IEnumerator<Node> PreOrderTraversal()
    {
        AssemblyTreeModel<Node> current = _head;

        yield return current.Value;
    }
}

namespace TreeLib;

public class Node : IComparable<Node>
{
    public int Id { get; set; }
    private string Value { get; set; }
    private Node? Parent { get; set; }
    internal List<Node> Children { get; set; }
    
    public Node(Node parent, string value)
    {
        Parent = parent;
        Value = value;
        Children = new List<Node>();
    }
    
    public Node(string value)
    {
        Parent = null;
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
    
    public Node InsertNode(string value, Node parent) 
    {
        Node newNode = new Node(parent, value);
        parent.Children.Add(newNode);
        return newNode;
    }

    public int CompareTo(string other)
    {
        return Value.CompareTo(other);
    }

    public int CompareTo(Node? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var idComparison = Id.CompareTo(other.Id);
        if (idComparison != 0) return idComparison;
        var valueComparison = string.Compare(Value, other.Value, StringComparison.Ordinal);
        if (valueComparison != 0) return valueComparison;
        return Parent.CompareTo(other.Parent);
    }
}
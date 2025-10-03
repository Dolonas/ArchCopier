using System.Collections;

namespace TreeLib;


public class Tree : IEnumerable<Node> 
{
    public int Id { get; set; }
    private Node _head;

    private int _count;
    public Node Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public int Count => _count;

    public Tree(Node head, Node parent) 
    {
        _head = head;
        Value = new Node(_head, string.Empty);
        Parent = parent;
        Children = new List<Node>();
    }

    public Tree() : this(null, null)
    {
        
    }
    
    public void Add(string value)
    {
        if (_head == null) // Первый случай: дерево пустое
        {
            _head = new Node(null, value);
        }

        else
        {
            AddTo(_head, value);
        }
        _count++;
    }
    
    private void AddTo(Node node, Node value)
    {
        // Первый случай: значение добавляемого узла меньше чем значение текущего. 

        if (value.CompareTo(node.Value) < 0)
        {
            // если левый потомок отсутствует - добавляем его          

            if (node.Left == null)
            {
                node.Left = new BinaryTreeNode<T>(value);
            }
            else
            {
                // повторная итерация               
                AddTo(node.Left, value);
            }
        }
        // Второй случай: значение добавляемого узла равно или больше текущего значения      
        else
        {
            // Если правый потомок отсутствует - добавлем его.          

            if (node.Right == null)
            {
                node.Right = new BinaryTreeNode<T>(value);
            }
            else
            {
                // повторная итерация

                AddTo(node.Right, value);
            }
        }
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
        Node newNode = new Node(value, parent);
        parent.Children.Add(newNode);
        return newNode;
    }

    public IEnumerator<Node> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
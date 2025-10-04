using KompasAPI7;

namespace ArchCopier.Models;

public class AssemblyTreeModel<Node>
{
    public int Id { get; set; }
    private AssemblyTreeModel<Node> _head;

    private int _count;
    public IPart7 Value { get; set; }
    public Node Parent { get; set; }
    public List<Node> Children { get; set; }
    
    public AssemblyTreeModel(Node value, Node children) 
    {
        
        Children = new List<Node>();
    }
    
    public void BreadthFirstSearch(Node root) 
    {
        Queue<Node> queue = new Queue<Node>();
        queue.Enqueue(root);
        while (queue.Count > 0) 
        {
            Node current = queue.Dequeue();
            // foreach (Node child in current.Children) 
            // {
            //     queue.Enqueue(child);
            // }
        }
    }
   
}

using TreeLib;

namespace TestProjectTree;

public class Tests
{
    Tree tree;
    [SetUp]
    public void Setup()
    {
        tree = new Tree();
        
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}
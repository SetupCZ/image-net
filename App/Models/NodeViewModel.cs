using Library;

namespace App.Models;

// Use primary constructor for simple classes
public class NodeViewModel(NodeEntity node, NodeEntity[] children, int pageIndex = 0, int pages = 0)
{
    public NodeEntity Node { get; } = node;

    public List<NodeViewModel> Children { get; init; } =
        children.Select(node => new NodeViewModel(node, [])).ToList();

    public int PageIndex { get; } = pageIndex;

    public int Pages { get; } = pages;
}

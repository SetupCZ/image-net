using Library;

namespace SourceDataProcessor;

public class Node
{
    public Guid Id { get; init; }

    public required string Name { get; init; }

    public int Count { get; set; }

    public int Lft { get; init; }

    public int Rgt { get; set; }

    public Guid? ParentId { get; init; }

    public Node? Parent { get; init; }

    public NodeEntity ToNodeEntity()
    {
        return new NodeEntity
        {
            Id = Id,
            Name = Name,
            Count = Count,
            LeftIndex = Lft,
            RightIndex = Rgt,
            ParentId = ParentId
        };
    }
}

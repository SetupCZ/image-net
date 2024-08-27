using System.ComponentModel.DataAnnotations.Schema;

namespace Library;

[Table("nodes")]
public class NodeEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required int Count { get; init; }

    public int LeftIndex { get; init; }

    public int RightIndex { get; init; }

    public required Guid? ParentId { get; init; }
}

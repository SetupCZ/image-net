using System.Diagnostics;
using System.Xml;
using Library;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace SourceDataProcessor;

public class TreeReader(AppDbContext context, ILogger<TreeReader> logger)
{
    private int _batchSize;
    private int _batchCount;
    private readonly HashSet<string> _names = [];

    private const string ElementName = "synset";

    // Adjust lower for less db lock, higher for better data consistency
    private const int BatchSize = 5000;

    /*
     * Reads the XML stream and saves the nodes to the database.
     * By streaming the XML, the memory usage is kept low.
     * Still needs to save node names to handle duplicates. Perhaps removing some subset of nodes on batch save.
     * The nodes are saved in batches to limit the slow db access.
     */
    public async Task Read(Stream stream)
    {
        await using var dbContextTransaction = await context.Database.BeginTransactionAsync();

        // increasing node left and right index
        var count = 0;
        // mutable current node being processed
        Node? node = null;

        var timer = new Stopwatch();
        timer.Start();

        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
        while (await reader.ReadAsync())
        {
            switch (reader.NodeType)
            {
                case XmlNodeType.Element:
                    if (reader.Name != ElementName)
                    {
                        break;
                    }

                    var name = GetName(reader, node);

                    var nodeLft = ++count;
                    /*
                     * Going down the tree, create a new node with the current node as parent.
                     * Node holds the parent node reference to be used when the XmlNodeType.EndElement is reached.
                     * If the node is a leaf node, the next reader.NodeType will be XmlNodeType.EndElement. and the node completes.
                     * If the node has children, the next reader.NodeType will be XmlNodeType.Element and the node is set as the parent.
                     */
                    node = new Node
                    {
                        Id = Guid.NewGuid(),
                        Name = name,
                        ParentId = node?.Id, // use node as parent before initializing new one
                        Parent = node,
                        Lft = nodeLft,
                        Rgt = nodeLft + 1
                    };

                    break;
                case XmlNodeType.EndElement:
                    if (node == null || reader.Name != ElementName)
                    {
                        break;
                    }

                    if (!_names.Contains(node.Name))
                    {
                        if (node.Parent != null)
                        {
                            node.Parent.Count += 1;
                        }

                        node.Rgt = ++count;

                        AddNodeToBatch(node);

                        if (_batchSize > BatchSize)
                        {
                            await SaveBatchToDb(dbContextTransaction);
                        }
                    }
                    else
                    {
                        // Omit duplicate nodes
                        logger.LogWarning("Node {name} already exists", node.Name);
                    }

                    logger.LogInformation("Time elapsed: {elapsedTime}", GetFormatedElapsedTime(timer.Elapsed));

                    /*
                     * At the end of the element, set the current node to the parent node for a sibling element to use.
                     */
                    if (node.Parent != null)
                    {
                        node = node.Parent;
                    }

                    break;
            }
        }

        // Save the last batch
        try
        {
            await context.SaveChangesAsync();
            await dbContextTransaction.CommitAsync();
        }
        catch
        {
            await dbContextTransaction.RollbackAsync();
            throw;
        }

        timer.Stop();

        logger.LogInformation("Saved {count} nodes. Elapsed: {elapsedTime}", _batchCount * _batchSize,
            GetFormatedElapsedTime(timer.Elapsed));

        logger.LogInformation("Finished reading");
    }

    private static string GetFormatedElapsedTime(TimeSpan ts)
    {
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";
    }

    private static string GetName(XmlReader reader, Node? node)
    {
        var elementName = reader.GetAttribute("words") ?? "Unknown";
        var name = node != null ? $"{node.Name} > {elementName}" : elementName;
        return name;
    }

    private void AddNodeToBatch(Node node)
    {
        context.Nodes.Add(node.ToNodeEntity());

        _names.Add(node.Name);
        _batchSize += 1;
    }

    private async Task SaveBatchToDb(IDbContextTransaction dbContextTransaction)
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch
        {
            await dbContextTransaction.RollbackAsync();
            throw;
        }

        _batchCount += 1;

        logger.LogInformation("Saved {count} nodes.", _batchCount * _batchSize);

        _batchSize = 0;
    }
}

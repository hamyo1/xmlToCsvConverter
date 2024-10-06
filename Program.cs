using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;


//string input = "C:\\some\\path\\input";
//string output = "C:\\some\\path\\output";

string input = "C:\\Users\\haimh\\Downloads\\xmltocsvconverter\\input";
string output = "C:\\Users\\haimh\\Downloads\\xmltocsvconverter\\output";

XmlToCsvConverterExecute(input, output);

void XmlToCsvConverterExecute(string input_path, string output_path)
{
    XmlToCsvConverter converter = new XmlToCsvConverter(input_path, output_path);
    converter.Convert();
}


class XmlToCsvConverter
{
    private string inputFolderPath;
    private string outputFolderPath;
    private DataTable dataTable;
    private HashSet<string> rowHashes; // Track unique rows with a hash to avoid duplicates
    private bool columnsCreated = false; // Track if columns are created from the first file
    private bool isXmlHaveList = false;
    // Create an instance of the XmlNodeRearranger class
    XmlNodeRearranger rearranger = new XmlNodeRearranger();

    public XmlToCsvConverter(string inputFolderPath, string outputFolderPath)
    {
        this.inputFolderPath = inputFolderPath;
        this.outputFolderPath = outputFolderPath;
        this.dataTable = new DataTable();
        this.rowHashes = new HashSet<string>();
    }

    public void Convert()
    {

        foreach (string file in Directory.GetFiles(inputFolderPath, "*.*", SearchOption.AllDirectories)
                                         .Where(s => s.EndsWith(".DAT", StringComparison.OrdinalIgnoreCase) ||
                                                     s.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)))
        {
            XmlDocument xmlDoc = new XmlDocument();
            // Load and remove comment nodes
            RemoveCommentNodes(xmlDoc);
            xmlDoc.Load(file);
            if (!columnsCreated)
            {
                CreateTableFromXml(xmlDoc);
                columnsCreated = true; // Mark that columns have been created

            }
            AppendDataToTable(xmlDoc);
        }
        string outputFilePath = Path.Combine(outputFolderPath, "output.csv");
        SaveDataTableToCsv(outputFilePath);
    }

    // Remove comment nodes from the XML document
    private void RemoveCommentNodes(XmlDocument doc)
    {
        var commentNodes = doc.SelectNodes("//comment()");
        if (commentNodes != null)
        {
            foreach (XmlNode commentNode in commentNodes)
            {
                commentNode.ParentNode.RemoveChild(commentNode);
            }
        }
    }

    private void CreateTableFromXml(XmlDocument xmlDoc)
    {
        XmlNode root = xmlDoc.DocumentElement;
        AddColumns(root, "");
    }

    private void AddColumns(XmlNode node, string prefix)
    {
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Comment) continue; // Skip comments

            string columnName = string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}";

            if (child.HasChildNodes && child.FirstChild.NodeType == XmlNodeType.Element)
            {
                AddColumns(child, columnName);
            }
            else
            {
                if (!dataTable.Columns.Contains(columnName))
                {
                    dataTable.Columns.Add(columnName);
                }
            }
        }
    }

    private void AppendDataToTable(XmlDocument xmlDoc)
    {
        rearranger.markedNestedListObjects.Clear();
        rearranger.markedListTags.Clear();
        // Step 1: Mark list nodes based on their scope
        rearranger.MarkListNodes(xmlDoc.DocumentElement);
        // Step 2: Rearrange nodes based on the marked list nodes
        rearranger.RearrangeNodes(xmlDoc.DocumentElement);
        XmlNode root = xmlDoc.DocumentElement;
        List<DataRow> rows = new List<DataRow>() { dataTable.NewRow() };
        isXmlHaveList = false;
        ProcessNode(root, rows, "");
        if (isXmlHaveList) rows.Remove(rows[rows.Count - 1]);
        foreach (DataRow row in rows)
        {
            dataTable.Rows.Add(row);
        }
    }

    private void ProcessNode(XmlNode node, List<DataRow> rows, string prefix)
    {
        if (node.NodeType == XmlNodeType.Comment) return; // Skip comments
        foreach (XmlNode child in node.ChildNodes)
        {
            if (child.NodeType == XmlNodeType.Comment) continue; // Skip comments
            string columnName = string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}.{child.Name}";
            if (child.HasChildNodes && child.FirstChild.NodeType == XmlNodeType.Element)
            {
                ProcessNode(child, rows, columnName);
            }
            else
            {
                if (dataTable.Columns.Contains(columnName))
                    rows[rows.Count - 1][columnName] = child.InnerText;

                // Check for last child of a repeating element
                if (rearranger.markedListTags.Contains(node.Name) &&
                    child == node.ChildNodes[node.ChildNodes.Count - 1])
                {
                    DataRow newRow = dataTable.NewRow();
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        newRow[i] = rows[rows.Count - 1][i];
                    }
                    foreach (XmlNode child_1 in node.ChildNodes)
                    {
                        string columnName_1 = string.IsNullOrEmpty(prefix) ? child_1.Name : $"{prefix}.{child_1.Name}";
                        if (dataTable.Columns.Contains(columnName_1))
                            newRow[columnName_1] = string.Empty;
                    }
                    rows.Add(newRow);
                    isXmlHaveList = true;
                }
            }
        }

    }

    private void SaveDataTableToCsv(string filePath)
    {
        // Create a mapping of original column names to simplified tag names
        Dictionary<string, string> columnMapping = new Dictionary<string, string>();
        foreach (DataColumn column in dataTable.Columns)
        {
            string simplifiedName = GetSimplifiedColumnName(column.ColumnName);
            columnMapping[column.ColumnName] = simplifiedName;
        }

        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            // Write the header row with simplified column names
            foreach (var simplifiedName in columnMapping.Values)
            {
                writer.Write(simplifiedName + ",");
            }
            writer.WriteLine();

            // Write the data rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (var columnName in columnMapping.Keys)
                {
                    writer.Write(row[columnName].ToString() + ",");
                }
                writer.WriteLine();
            }
        }
    }

    private string GetSimplifiedColumnName(string columnName)
    {
        // Extract the tag name from the full path
        int lastDotIndex = columnName.LastIndexOf('.');
        return lastDotIndex == -1 ? columnName : columnName.Substring(lastDotIndex + 1);
    }

}


class XmlNodeRearranger
{
    public HashSet<string> markedListTags = new HashSet<string>();
    public HashSet<string> markedNestedListObjects = new HashSet<string>();

    // Step 1: Mark nodes that can appear as lists and their parent objects
    public void MarkListNodes(XmlNode root)
    {
        if (root == null) return;
        // Get child nodes
        var childNodes = root.ChildNodes.Cast<XmlNode>()
                                        .Where(child => child.NodeType != XmlNodeType.Comment)
                                        .ToList();
        // Group by name to find duplicates
        var groupedNodes = childNodes.GroupBy(c => c.Name)
                                     .Where(g => g.Count() > 1) // Only consider groups with duplicates
                                     .ToList();
        // Mark list tags and their parent objects
        foreach (var group in groupedNodes.Distinct())
        {
            markedListTags.Add(group.Key); // Mark as list
        }
        if (groupedNodes.Count > 0)
        {
            foreach (var node in groupedNodes[groupedNodes.Count - 1])
            {
                MarkParentAsNested(node.ParentNode); // Mark parent as nested
                break;
            }
        }
        // Recursively check child nodes
        foreach (var child in childNodes)
        {
            MarkListNodes(child);
        }
    }
    // Mark parent nodes as nested list objects
    private void MarkParentAsNested(XmlNode node)
    {
        while (node != null)
        {
            if (!markedListTags.Contains(node.Name))
                markedNestedListObjects.Add(node.Name);
            node = node.ParentNode;
        }
    }
    // Step 2: Rearrange nodes based on marked lists and nested structures
    public void RearrangeNodes(XmlNode parentNode)
    {
        if (parentNode == null || !parentNode.HasChildNodes) return;
        // Get all child nodes
        var childNodes = parentNode.ChildNodes.Cast<XmlNode>()
                             .Where(child => child.NodeType != XmlNodeType.Comment)
                             .ToList();
        var unmarkedNodes = new List<XmlNode>();
        var nestedListObjects = new List<XmlNode>();
        var markedListNodes = new List<XmlNode>();
        // Categorize nodes
        foreach (var node in childNodes)
        {
            if (markedListTags.Contains(node.Name)) // Marked as list
            {
                markedListNodes.Add(node);
            }
            else if (markedNestedListObjects.Contains(node.Name)) // Marked as nested list object
            {
                nestedListObjects.Add(node);
            }
            else // Unmarked nodes
            {
                unmarkedNodes.Add(node);
            }
        }
        // Clear existing child nodes
        parentNode.RemoveAll();

        // Append nodes in the desired order
        foreach (var node in unmarkedNodes)
        {
            parentNode.AppendChild(node);
        }
        foreach (var node in nestedListObjects)
        {
            parentNode.AppendChild(node);
        }
        foreach (var node in markedListNodes)
        {
            parentNode.AppendChild(node);
        }
        //rearrange deeply into all nodes
        foreach (XmlNode node in parentNode.ChildNodes)
        {
            RearrangeNodes(node);
        }
    }
}
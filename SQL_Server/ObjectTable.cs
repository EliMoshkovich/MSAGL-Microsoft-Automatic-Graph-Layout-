using System.Collections.Generic;

namespace WindowsApplicationSample
{
    // A table together with its primary-key and foreign-key column names
    public class ObjectTable
    {
        public string TableName { get; set; }
        public List<string> PrimaryKeys { get; set; }
        public List<string> ForeignKeys { get; set; }

        public ObjectTable(string tableName)
        {
            TableName = tableName;
            PrimaryKeys = new List<string>();
            ForeignKeys = new List<string>();
        }
    }
}

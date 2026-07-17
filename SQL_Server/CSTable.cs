namespace WindowsApplicationSample
{
    // One row of INFORMATION_SCHEMA.KEY_COLUMN_USAGE: a single key column of a table
    class CSTable
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
    }
}

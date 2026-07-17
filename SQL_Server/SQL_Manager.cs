using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace WindowsApplicationSample
{
    public class SQL_Manager : IDisposable
    {
        // Default connection string: local SQL Server Express with the AdventureWorks2014 sample database
        private const string CONNECTION_STRING = @"Data Source=DESKTOP-20Q82NL\SQLEXPRESS;Initial Catalog=AdventureWorks2014;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        // Connection string actually used by this instance
        private string connectionString;
        // Query that returns every key column in the database together with its constraint name
        private const string QUERY = "SELECT TABLE_NAME, TABLE_SCHEMA, COLUMN_NAME, CONSTRAINT_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE";

        // Command that will be executed against the connection
        public SqlCommand Command { get; private set; }
        private SqlConnection sqlcon;

        // Ctor with a custom connection string
        public SQL_Manager(string connectionString)
        {
            this.connectionString = connectionString;
            OpenConnection();
            SetQuery(QUERY);
        }
        // Default ctor that uses the default connection string
        public SQL_Manager()
        {
            this.connectionString = CONNECTION_STRING;
            OpenConnection();
            SetQuery(QUERY);
        }

        // Open a connection using the connection string
        public bool OpenConnection()
        {
            try
            {
                sqlcon = new SqlConnection(connectionString);
                sqlcon.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Close the SQL connection
        public bool CloseConnection()
        {
            try
            {
                sqlcon.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Build a SqlCommand for a custom query
        public SqlCommand SetQuery(string query)
        {
            Command = new SqlCommand(query, sqlcon);
            return Command;
        }

        // Read the key columns of every table and group them into one ObjectTable per table
        public List<ObjectTable> GetObjectTableList()
        {
            try
            {
                // fetch the query results into ds.Tables[0]
                SqlDataAdapter sda = new SqlDataAdapter();
                DataSet ds = new DataSet();
                sda.SelectCommand = this.Command;
                sda.Fill(ds);

                List<CSTable> listTable = ds.Tables[0].AsEnumerable().Select(dataRow => new CSTable
                {
                    TableName = dataRow.Field<string>("TABLE_SCHEMA") + "." + dataRow.Field<string>("TABLE_NAME"),
                    ColumnName = dataRow.Field<string>("COLUMN_NAME"),
                    IsPrimaryKey = dataRow.Field<string>("CONSTRAINT_NAME").StartsWith("PK", StringComparison.Ordinal)
                }).ToList();

                // build the list of all tables with their primary and foreign keys
                List<ObjectTable> listObject = new List<ObjectTable>();

                for (int i = 0; i < listTable.Count; i++)
                {
                    CSTable oneTable = listTable[i];
                    ObjectTable ot = new ObjectTable(oneTable.TableName);

                    for (int j = 0; j < listTable.Count; j++)
                    {
                        CSTable table = listTable[j];

                        if (oneTable.TableName.Equals(table.TableName))
                        {
                            if (table.IsPrimaryKey)
                            {
                                if (!ot.PrimaryKeys.Contains(table.ColumnName)) // skip columns already in the PK list
                                    ot.PrimaryKeys.Add(table.ColumnName);
                            }
                            else
                            {
                                if (!ot.ForeignKeys.Contains(table.ColumnName)) // skip columns already in the FK list
                                    ot.ForeignKeys.Add(table.ColumnName);
                            }
                        }
                    }

                    // if this table is already in the result, merge any missing keys into it; otherwise add it
                    bool exist = false;
                    foreach (var it in listObject)
                    {
                        if (it.TableName.Equals(ot.TableName))
                        {
                            exist = true;
                        }
                        if (exist)
                        {
                            foreach (var pk in ot.PrimaryKeys)
                            {
                                if (!it.PrimaryKeys.Contains(pk))
                                {
                                    it.PrimaryKeys.Add(pk);
                                }
                            }
                            foreach (var fk in ot.ForeignKeys)
                            {
                                if (!it.ForeignKeys.Contains(fk))
                                {
                                    it.ForeignKeys.Add(fk);
                                }
                            }
                            break;
                        }
                    }
                    if (!exist)
                        listObject.Add(ot);
                }

                return listObject;
            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString());
            }
            return null;
        }

        // Release the underlying SQL connection
        public void Dispose()
        {
            CloseConnection();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace WindowsApplicationSample
{
    public class SQL_Manager
    {
        // the Const Variable connection string to the SQL_SERVER 2014
        private const string CONNECTION_STRING = @"Data Source=DESKTOP-20Q82NL\SQLEXPRESS;Initial Catalog=AdventureWorks2014;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        // the string object of connection string
        private string connetionString;
        // the Const Variable of the query that get data from the database of: Table name, and kinf of the keys
        private const string QUERY = "SELECT TABLE_NAME, TABLE_SCHEMA ,COLUMN_NAME, CONSTRAINT_NAME  FROM  INFORMATION_SCHEMA.KEY_COLUMN_USAGE";
       
        // variable to handler the conect to sql 
        public SqlCommand sqlCommand { get; set; }
        private SqlConnection sqlcon;

        // Ctor with the custom of the connection string 
        public SQL_Manager(string connetionString)
        {
            this.connetionString = connetionString;
            openConnection();
            setQuery(QUERY);
        }
        // Default Ctor that use the default const string of connection string 
        public SQL_Manager()
        {
            this.connetionString = CONNECTION_STRING;
            openConnection();
            setQuery(QUERY);
        }

        // open connection by the connection string to sql object
        public bool openConnection()
        {
            try
            {
                sqlcon = new SqlConnection(connetionString);
                sqlcon.Open();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // close the connection of sql
        public bool closeConnection()
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

        // get SqlCommand from custom query
        public SqlCommand setQuery(string query)
        {
            sqlCommand = new SqlCommand(query, sqlcon);
            return sqlCommand;
        }

        // get the list of all the colums from the tables in database
        public List<ObjectTable> getObjectTableList()
        {
            try
            {
                // get data from database into the ds.Tables[0] object
                SqlDataAdapter sda = new SqlDataAdapter();
                DataSet ds = new DataSet();
                sda.SelectCommand = this.sqlCommand;
                sda.Fill(ds);
                sda.Update(ds);

                List<CSTable> listTable = new List<CSTable>();
                listTable = ds.Tables[0].AsEnumerable().Select(dataRow => new CSTable
                {
                    TableName = dataRow.Field<string>("TABLE_SCHEMA") + "." + dataRow.Field<string>("TABLE_NAME"),
                    ColumnsName = dataRow.Field<string>("COLUMN_NAME"),
                    isPrimaryKey = dataRow.Field<string>("CONSTRAINT_NAME").Substring(0, 2).Equals("PK") ? true : false
                }).ToList();

                // create list of the all tables with there Primay keys, and Forigen keys
                List<ObjectTable> listObject = new List<ObjectTable>();

                for (int i = 0; listTable != null && i < listTable.Count; i++)
                {
                    CSTable OneTable = listTable.ElementAt(i);
                    ObjectTable ot = new ObjectTable(OneTable.TableName);

                    for (int j = 0; j < listTable.Count; j++)
                    {
                        CSTable table = listTable.ElementAt(j);

                        if (OneTable.TableName.Equals(table.TableName))
                        {
                            if (table.isPrimaryKey)
                            {
                                if(!ot.primary_Keys.Contains(table.ColumnsName)) // check if this not allready exist in the list pk
                                    ot.primary_Keys.Add(table.ColumnsName);
                            }
                            else
                            {
                                if (!ot.forigen_Keys.Contains(table.ColumnsName)) // check if this not allready exist in the list fk
                                    ot.forigen_Keys.Add(table.ColumnsName);
                            }
                        }
                    }

                    // check if this table allready exist if so, add the PK's and FK's to the list keys if dont existed, else add it to the list
                    bool exist = false;
                    foreach (var it in listObject)
                    {
                        if (it.TableName.Equals(ot.TableName))
                        {
                            exist = true;
                        }
                        if (exist)
                        {
                            foreach (var pk in ot.primary_Keys)
                            {
                                if (!it.primary_Keys.Contains(pk))
                                {
                                    it.primary_Keys.Add(pk);
                                }
                            }
                            foreach (var fk in ot.forigen_Keys)
                            {
                                if (!it.forigen_Keys.Contains(fk))
                                {
                                    it.forigen_Keys.Add(fk);
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

        // Dtor to handler the close connetion, if there isn't reference to the object
        protected virtual void Finalize()
        {
            closeConnection();
        }
    }
}

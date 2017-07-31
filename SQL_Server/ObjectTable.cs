using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsApplicationSample
{
    public class ObjectTable
    {
        public string TableName { get; set; }
        public List<string> primary_Keys { get; set; }
        public List<string> forigen_Keys { get; set; }
           
        public ObjectTable(string tableName)
        {
            this.TableName = tableName;
            primary_Keys = new List<string>();
            forigen_Keys = new List<string>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Aplikacja_2
{
    [Serializable]
    public class Task
    {
        private string description;
        private bool status;
        private string date;
        private int databaseID;
    
        public Task(string description, bool status, DateOnly date)
        {
            this.description = description;
            this.status = status;
            this.date = date.ToString();
        }

        public Task(string description, bool status)
        {
            this.description = description;
            this.status = status;
        }

        private Task()
        {

        }


        public string Description { get => description; set => description = value; }
        public bool Status { get => status; set => status = value; }
        public string Date { get => date; set => date = value; }
        public int DatabaseID { get => databaseID; set => databaseID = value; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aplikacja_2
{
    [Serializable]
    public class Step
    {
        private string date;
        private int counter;
        public Step(DateOnly date, int steps) 
        { 
            this.date = date.ToString();
            this.counter = steps;
        }

        public Step()
        {

        }

        public string Date { get => date; set => date = value; }
        public int Counter { get => counter; set => counter = value; }
    }
}

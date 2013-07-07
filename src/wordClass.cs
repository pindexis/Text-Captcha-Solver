using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextCaptchaSolver
{
    class wordFunctions
    {
        public List<int> firstClassFunctions;//cannot go without it  (100)
        public List<int> SecondClassFunctions;//2

        public wordFunctions()
        {
            this.firstClassFunctions = new List<int>();
            this.SecondClassFunctions = new List<int>();
        }
        public void AddTofirstClass(int index)
        {
            this.firstClassFunctions.Add(index);
        }
        public void AddToSecondClass(int index)
        {
            this.SecondClassFunctions.Add(index);
        }
        
    }
}

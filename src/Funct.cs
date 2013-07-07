using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextCaptchaSolver
{

    class Funct
    {
        public ExecuteFunctionDelegate executeFunction;


       
        public string[] FirstClassWords;
        public string[] SecondClassWords;
        public delegate string ExecuteFunctionDelegate(string[] keywords, string[] data,string text,string[] words);

        public Funct(ExecuteFunctionDelegate function, string[] first, string[] second)
        {
            this.executeFunction = function;
            FirstClassWords = first;
            SecondClassWords = second;
            
        }
    }
}

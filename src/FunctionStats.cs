using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextCaptchaSolver
{
    class FunctionStats
    {
        public int functionIndex;
        public int Score;
        public List<string> words;
        public List<string> requiredWords;
        public FunctionStats(int funcindex)
        {
            this.functionIndex = funcindex;
            Score = 0;
            words = new List<string>();
        }
    }
}

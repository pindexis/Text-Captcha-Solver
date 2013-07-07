using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
namespace TextCaptchaSolver
{



    class FunctionsDetails
    {
        public FunctionsDetails(List<Funct> functionlist)
        {
            this.FunctionsList = functionlist;
            wordsTable = new Dictionary<string, wordFunctions>();
            FillWordsTable();
        }
        public readonly List<Funct> FunctionsList;
        public Dictionary<string, wordFunctions> wordsTable;
        private void FillWordsTable()
        {
            for (int i = 0; i < FunctionsList.Count; i++)
            {
                Funct f = FunctionsList[i];
                foreach (string word in f.FirstClassWords)
                {
                    if (!wordsTable.Keys.Contains(word))
                        wordsTable.Add(word, new wordFunctions());
                    if (!wordsTable[word].firstClassFunctions.Contains(i))
                        wordsTable[word].AddTofirstClass(i);

                }
                foreach (string word in f.SecondClassWords)
                {
                    if (!wordsTable.Keys.Contains(word))
                        wordsTable.Add(word, new wordFunctions());
                    if (!wordsTable[word].SecondClassFunctions.Contains(i))
                        wordsTable[word].AddToSecondClass(i);
                }
            }
        }

    }

    class LogicCaptchaSolver
    {

        public LogicCaptchaSolver(FunctionsDetails functions)
        {
            this.functionList = functions;
        }

        FunctionsDetails functionList;

        FunctionStats[] FunctionsStats;

        public void InitializeFuncStatsTable()
        {
            this.FunctionsStats = new FunctionStats[functionList.FunctionsList.Count];
            for (int i = 0; i < FunctionsStats.Length; i++)
            {
                FunctionsStats[i] = new FunctionStats(i);
                FunctionsStats[i].requiredWords = functionList.FunctionsList[i].FirstClassWords.ToList();
            }

        }

        public void UpdateStats(wordFunctions wordFunctions, string wordRegex, string word)
        {
            foreach (int index in wordFunctions.firstClassFunctions)
            {
                if (this.FunctionsStats[index].requiredWords.Contains(wordRegex))
                    this.FunctionsStats[index].requiredWords.Remove(wordRegex);

                this.FunctionsStats[index].words.Add(word);
                this.FunctionsStats[index].Score += 4;
            }
            foreach (int index in wordFunctions.SecondClassFunctions)
            {
                this.FunctionsStats[index].Score += 2;
                this.FunctionsStats[index].words.Add(word);
            }
        }
        public string SelectFunction(string[] words, string originalText)
        {
            InitializeFuncStatsTable();


            foreach (string word in words)
            {
                IEnumerable<string> matches = from n in functionList.wordsTable.Keys
                                              where Regex.IsMatch(word, "^" + n + "$")
                                              select n;
                if (matches.Count() > 0)
                {
                    foreach (string wordmatched in matches)
                    {
                        this.UpdateStats(functionList.wordsTable[wordmatched], wordmatched, word);
                    }
                }
            }


            //get statisfied functions;
            IEnumerable<FunctionStats> Appropriatefunctions = from n in FunctionsStats
                                                              where n.requiredWords.Count == 0
                                                              orderby n.Score
                                                              select n;
            if (Appropriatefunctions.Count() < 1)
                throw new Exception("no function to select");



            Appropriatefunctions = Appropriatefunctions.Reverse();
            int max = Appropriatefunctions.ElementAt(0).Score;
            FunctionStats rightFunctionStats = Appropriatefunctions.ElementAt(0);
            int i = 0;
            while ((rightFunctionStats = Appropriatefunctions.ElementAt(i)).Score == max)
            {
                string[] keywords = rightFunctionStats.words.ToArray();
                int functionIndex = rightFunctionStats.functionIndex;
                Funct function = functionList.FunctionsList[functionIndex];
                IEnumerable<string> data = from n in words
                                           where !keywords.Contains(n)
                                           select n;
                try
                {
                    string resultat = function.executeFunction(keywords, data.ToArray(), originalText, words);
                    return resultat;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    i++;
                }
            }
            throw new Exception("no function can be executed");
        }

        public static string NormalizeMathQuestion(string phrase)
        {
            phrase = phrase.Replace(" ", "");
            string operators = @"[\+\-\*\/]";
            string number = @"\-?\d+";
            string term = @"(\(" + number + "(" + operators + number + @")*\)|" + number + ")";

            Match oo;
            string eq = null;

            if ((oo = Regex.Match(phrase, @"(?<eq>" + term + "(" + operators + term + @")+)(\?|$|=|is)(?!\d)")).Success)
            {
                eq = oo.Groups["eq"].Value;
            }
            else if ((oo = Regex.Match(phrase, @"(?<!" + term + ")(?<firstOperand>" + term + ")(?<keyOperator1>" + operators + @")[\*\.]?((?<keyOperator2>" + operators + @")(?<secondOperand>" + term + @"))?=(?<result>"+term+")")).Success)
            {
                string operand1 = oo.Groups["firstOperand"].Value;
                string keyOperator1 = oo.Groups["keyOperator1"].Value;
                string keyOperator2 = oo.Groups["keyOperator2"].Value;
                string operand2 = oo.Groups["secondOperand"].Value;
                string result = oo.Groups["result"].Value;

                if (string.IsNullOrEmpty(operand2))
                {
                    keyOperator2 = "+";
                    operand2 = "0";
                }
                eq = solveEquation(operand1, keyOperator1, operand2, keyOperator2, result);
                eq = Regex.Replace(eq, @"(?<=[\+\-])\(", "1*(");
            }
            else if ((oo = Regex.Match(phrase, @"(?<!" + term + ")(?<KeyOperator>" + operators + ")(?<Operand>" + term + ")=(?<resultat>" + term + ")")).Success)
            {
                string keyOperator = oo.Groups["KeyOperator"].Value;
                string operand1 = oo.Groups["Operand"].Value;
                string result = oo.Groups["resultat"].Value;
                eq = solveEquation("0","+",operand1, keyOperator, result);
            }
            if (eq != null)
            {
                return eq;
            }
            else
                return null;
        }
        public static string GetEquivalentOperator(char theoperator)
        {
            switch (theoperator)
            {
                case '+':
                    return "-";
                case '-':
                    return "+";
                case '/':
                    return "*";
                case '*':
                    return "/";
                default:
                    throw new Exception("no recognized operator");
            }
        }
        public static string solveEquation(string operand1, string operator1, string operand2, string operator2, string result)
        {

            if ((operator1 == "+" || operator1 == "-") && (operator2 == "+" || operator2 == "-"))
            {
                string eqoperand1 = null;
                if (operand1.StartsWith("-"))
                    eqoperand1 = "+" + operand1.Substring(1);
                else
                    eqoperand1 = "-" + operand1;

                string secondSide = result + eqoperand1 + GetEquivalentOperator(char.Parse(operator2)) + operand2;
                return operator1 + "1*(" + secondSide + ")";

            }
            else if ((operator1 == "*" || operator1 == "/") && (operator2 == "*" || operator2 == "/"))
            {
                if (operator1 == "*" && operator2 == "*")
                    return result + "/" + operand1 + "/" + operand2;
                else if (operator1 == "/" && operator2 == "/")
                    return operand1 + "*" + operand2 + "/" + result;
                else throw new Exception("cannot solve");
            }
            else
            {
                if (operator1 == "*" )
                    return "("+result + GetEquivalentOperator(char.Parse(operator2)) + operand2+")" + "/" + operand1;
                else if( operator1 == "/")
                    return operand1 + "/" + "(" + result + GetEquivalentOperator(char.Parse(operator2)) + operand2 + ")";
        
                else
                {
                    string eqoperand1 = null;
                    if (operand1.StartsWith("-"))
                        eqoperand1 = "+" + operand1.Substring(1);
                    else
                        eqoperand1 = "-" + operand1;

                    return operator1 + "1*(" + "("+result + eqoperand1+")" + GetEquivalentOperator(char.Parse(operator2)) + operand2 + ")";
                }
            }
        }

        public static string GetNormalizedText(string phrase, out bool isMathQuestion)
        {
            phrase = NumberConverter.ConvertSpelledNumbersToNumeric(phrase);

            //remove -(if it is not in math operation)

            foreach (KeyValuePair<string, string> p in NormalizedWords)
            {
                Match m = Regex.Match(phrase, p.Value);
                while (m.Success)
                {
                    phrase = phrase.Replace(m.Value.Trim(), p.Key);
                    m = m.NextMatch();

                }
            }
            Match m2;
            if ((m2 = Regex.Match(phrase, @"(?<o1>[0-9]+)\ssubtractedfrom\s(?<o2>[0-9]+)")).Success)
            {
                phrase = phrase.Replace(m2.Groups["o1"].Value + " subtractedfrom " + m2.Groups["o2"].Value, m2.Groups["o2"].Value + "-" + m2.Groups["o1"].Value);
            }
            string MathQuestion = NormalizeMathQuestion(phrase);
            if (MathQuestion != null)
            {
                isMathQuestion = true;
                return MathQuestion;
            }
            else
            {
                isMathQuestion = false;
                phrase = StripExtraCharacters(phrase);
                return Regex.Replace(phrase, @"\s+", @" ");
            }
        }
        public static string StripExtraCharacters(string phrase)
        {
            //replace "a" with A 
            phrase = Regex.Replace(phrase, "\"a\"", "A");
            phrase = Regex.Replace(phrase, "'s\b", " ");
            phrase = Regex.Replace(phrase, @"[^\w ]", " ");
            phrase = phrase.Replace(",", " ");//, would not be removed from first regex i don't know why
            return Regex.Replace(phrase, @"\s+", @" ");
        }
        public static string[] RemoveNonImportantsWords(string[] words)
        {
            List<string> keywords = new List<string>();
            foreach (string word in words)
            {
                if (!StrippedWords.Contains(word))
                    keywords.Add(word);
            }
            return keywords.ToArray();
        }
        public static string[] ExtractWordsFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            //get words within quotes
            Match m = Regex.Match(text,
                @"(?<term>\b[\w\-]+\b)"
                  , RegexOptions.ExplicitCapture);


            List<string> words = new List<string>();
            while (m.Success)
            {
                words.Add(m.Groups["term"].Value);
                m = m.NextMatch();
            }
            return words.ToArray();
        }

        public static string[] ReplaceTwoSeperatedWords(string[] words)
        {
            List<string> result = words.ToList();

            foreach (KeyValuePair<string, string[]> p in TwoSeperatedWordsReplacement)
            {
                if (ContainArray(result, p.Value))
                {
                    int index = IndexOfFirstWord(words, p.Value);
                    for (int i = 0; i < p.Value.Length; i++)
                        result.Remove(p.Value[i]);

                    result.Insert(index, p.Key);
                }

            }
            return result.ToArray();
        }

        public static bool ContainArray(List<string> container, string[] contained)
        {
            bool Iscontained = true;
            int i = 0;
            while (Iscontained && i < contained.Length)
            {
                if (!container.Contains(contained[i]))
                    Iscontained = false;
                i++;
            }
            return Iscontained;
        }
        public static int IndexOfFirstWord(string[] container, string[] contained)
        {

            int findex = Array.IndexOf(container, contained[0]);
            for (int i = 1; i < contained.Length; i++)
            {
                int currentWordIndex = Array.IndexOf(container, contained[i]);
                if (currentWordIndex < findex)
                    findex = currentWordIndex;
            }
            return findex;
        }

        public static string[] StrippedWords = new string[] { ".", "the", "in", "at", "what", "is", "this", "to", "these", "or", "their", "of", "which", "on", "onto", "a", "with", "has", "have", "as", "how", "many", "from", "if", "was", "from", "by", "it", "and", "following", "do", "does" };
        public static Dictionary<string, string> NormalizedWords = new Dictionary<string, string>()
        {
            {"type",@"enter"},
            {"notbelong",@"\bnot belongs?\b"},
            {"howmany",@"\b(number of|how many)\b"},
            {"color",@"\b(colours?|colors)\b"},
            {"contain",@"\b(contain\w*)\b"},
            {"start",@"\b(begin|start)\w*\b"},
            {"-",@"\b(minus|subtracted by)\b"},
            {"subtractedfrom",@"\bsubtracted from\b"},
            {"+",@"\b(plus|add\w*)\b"},
            {"*",@"\b((multiply\w*|times))\b"},
            {"/",@"(\bdivided\b|:)"},
            {"=",@"\bequals?\b"},
            {"biggest",@"\b(bigge|large|highe)\w*\b"},
            {"smallest",@"\b(smalle|lowe)\w*\b"},
            {"1st",@"\b(first|start)\b"},
            {"2nd",@"\b(second)\b"},
            {"3rd" ,@"\b(third)\b"},
            {"4th",@"\b(fourth)\b"},
            {"5th",@"\b(fifth)\b"},
            {"last",@"\bends? with\b"},
            {"part",@"\b(parts)\b"},
            {"person",@"persons"},
            {" personname",@"('s name|'\s+name)\b"},
            {"personname",@"\b(name|firstname|lastname)\b"},
            {"day weekend",@"\bpart (of )?(the )?weekend\b"},
            {"bodypart",@"\b(body parts?|part of (a|the|our|your) (person|body))\b"},
            {"headpart",@"\bpart of (a|the|our|your) head\b"},
            {"list",@"\bphrase\b"},
            {"digit",@"\bdigits\b"},
            {"letter",@"\bletters\b"},
            {"personmorethanone",@"person has more than 1"},
            {"waistabove",@"\babove (the|our|your) waist\b"},
            {"waistbelow",@"\bbelow (the|our|your) waist\b"},
            {"last word",@"\bfinal word\b"},
            {"last letter",@"\b(final letter|letter at the end)\b"},
        };

        public static Dictionary<string, string[]> TwoSeperatedWordsReplacement = new Dictionary<string, string[]>()
        {

        };


    }

}

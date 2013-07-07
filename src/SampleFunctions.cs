using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using MathParser;
namespace TextCaptchaSolver
{
    class SampleFunctions
    {

        static string dictionnaryObjects = "color|animal|bodypart|personname|headpart|day|personmorethanone|waistabove|waistbelow";


        public static string[] f1RegexClues = new string[] { "[a-zA0-9]", "(contain|last|([0-9]+(th|nd|st|rd)))", "word" };
        public static string[] f1simpleClues = new string[] { "list", "letter", "type" };
        public static string ListWordContainLetterOrWhereLetterIsAt(string[] keywords, string[] datas, string originalText, string[] words)
        {
            string letter = null;
            string num = null;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (keywords[i].Length == 1)
                {
                    if (letter != null)
                        throw new Exception("ListWordContainOrStartWithLetter");
                    letter = keywords[i].ToLower();
                }
                else if (Regex.IsMatch(keywords[i], "([0-9])(th|nd|st|rd)"))
                {
                    if (num != null)
                        throw new Exception("ObjectAtPositionInList");
                    num = Regex.Match(keywords[i], "([0-9])(th|nd|st|rd)").Groups[1].Value;
                }
                else if (keywords[i] == "last")
                {
                    if (num != null)
                        throw new Exception("ObjectAtPositionInList");
                    num = "last";
                }

            }

            IEnumerable<string> word;
            if (keywords.Contains("contain"))
                word = from n in datas
                       where n.Contains(letter)
                       select n;
            else
            {
                string[] Verifieddatas;
                string[] tempList;
                if ((tempList = ExtractDatas(originalText,true)) != null)
                    Verifieddatas = tempList;
                else
                    Verifieddatas = datas;

                if (num == "last")
                    word = from n in Verifieddatas
                           where n.EndsWith(letter)
                           select n;
                else
                    word = from n in Verifieddatas
                           where n.Length >= int.Parse(num)
                           where n[int.Parse(num) - 1].ToString() == letter
                           select n;
            }
            if (word.Count() == 1)
                return word.First();
            else
                throw new Exception("ObjectAtPositionInList");

        }

        public static string[] ObjectInSeqReq = new string[] { "(" + dictionnaryObjects + "|word|number)" };
        public static string[] ObjectInSeqOpt = new string[] { "list", "type" };
        public static string ObjectInSeq(string[] keywords, string[] NoNeededdatas, string originalText, string[] words)
        {


            string theObject;

            List<string> containedObjects = GetObjectsInList(keywords);

            if (containedObjects.Count > 1)
            {

                string t1;
                string t2;
                if (containedObjects.Contains(words[words.Length - 1]))
                    t1 = words[words.Length - 1];
                else
                    t1 = GetClosestTo(words[words.Length - 1], keywords, words, -1);

                if (containedObjects.Contains(words[0]))
                    t2 = words[0];
                else
                    t2 = GetClosestTo(words[0], keywords, words, 1);

                int indexOft1 = Array.IndexOf(words, t1);
                int indexOft2 = Array.IndexOf(words, t2);

                int difft1 = words.Length - indexOft1 - 1;
                if (difft1 < 2 || indexOft2 < 2)
                {
                    if (difft1 < indexOft2)
                        theObject = t1;
                    else
                        theObject = t2;
                }
                else
                    throw new Exception("ObjectInSeq");
            }
            else
            {
                theObject = containedObjects[0];
            }

            List<string> VerfiedKeywords = new List<string>() { theObject };
            foreach (string k in keywords)
            {
                if (!containedObjects.Contains(k))
                    VerfiedKeywords.Add(k);
            }

            string[] Verifieddatas;
            string[] tempList;
            if ((tempList = ExtractDatas(originalText,true)) != null)
                Verifieddatas = tempList;
            else
                Verifieddatas = ExtractDataFromList(VerfiedKeywords.ToArray(), words);
            if (Verifieddatas == null || Verifieddatas.Length == 0)
                throw new Exception("ObjectInSeq");
            if (theObject == "word")
            {
                if (Verifieddatas.Length == 1)
                    return Verifieddatas[0];
                else
                    throw new Exception("ObjectInSeq");
            }
            else if (theObject == "number")
            {
                IEnumerable<string> numbers = from n in Verifieddatas
                                              where Regex.IsMatch(n, "^[0-9]+$")
                                              select n;
                if (numbers.Count() == 1)
                    return numbers.First();
                else
                    throw new Exception("ObjectInSeq");
            }
            else
            {
                IEnumerable<string> compatibleWords = from n in Verifieddatas
                                                      where Objects[theObject].Contains(n)
                                                      select n;
                if (compatibleWords.Count() == 1)
                    return compatibleWords.First();
                else
                    throw new Exception("ObjectInSeq");
            }

        }

        public static string[] ObjectAtPositionInListReq = new string[] { "(([0-9](th|nd|st|rd))|last)", "(" + dictionnaryObjects + "|word|number)" };
        public static string[] ObjectAtPositionInListOpt = new string[] { "list", "type" };
        public static string ObjectAtPositionInList(string[] keywords, string[] nondatas, string originalText, string[] words)
        {
            if (!verifyOrder(ObjectAtPositionInListReq, words, true) && !verifyOrder(ObjectAtPositionInListReq.Reverse().ToArray(), words, true))
                throw new Exception("ObjectAtPositionInList");

            string num = null;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (Regex.IsMatch(keywords[i], "([0-9])(th|nd|st|rd)"))
                {
                    if (num != null)
                        throw new Exception("ObjectAtPositionInList");
                    num = Regex.Match(keywords[i], "([0-9])(th|nd|st|rd)").Groups[1].Value;
                }
                else if (keywords[i] == "last")
                {
                    if (num != null)
                        throw new Exception("ObjectAtPositionInList");
                    num = "last";
                }
            }

            List<string> containedObjects = GetObjectsInList(keywords).ToList();


            string theObject = null;
            if (containedObjects.Count > 1)
            {
                theObject = GetClosestTo(num, containedObjects.ToArray(), words, 1);
                if (theObject == null)
                    throw new Exception("ObjectAtPositionInList");
            }
            else
                theObject = containedObjects[0];

            List<string> VerfiedKeywords = new List<string>() { theObject };
            foreach (string k in keywords)
            {
                if (!containedObjects.Contains(k))
                    VerfiedKeywords.Add(k);
            }

            string[] Verifieddatas;
            string[] tempList;
            if ((tempList = ExtractDatas(originalText,true)) != null)
                Verifieddatas = tempList;
            else
                Verifieddatas = ExtractDataFromList(VerfiedKeywords.ToArray(), words);


            if (Verifieddatas == null)
                throw new Exception("ObjectPositionInList");

            if (theObject == "word")
            {
                if (num == "last")
                    return Verifieddatas[Verifieddatas.Length - 1];
                else if (Verifieddatas.Length >= int.Parse(num))
                    return Verifieddatas[int.Parse(num) - 1].ToString();
                else
                    throw new Exception("ObjectPositionInList");
            }
            else if (theObject == "number")
            {
                int result;
                if (num == "last")
                {
                    int j = Verifieddatas.Length - 1;
                    while (j >= 0)
                    {
                        if (int.TryParse(Verifieddatas[j], out result))
                        {
                            return result.ToString();
                        }
                        j--;
                    }
                }
                else if (Verifieddatas.Length >= int.Parse(num))
                {
                    int i = 0;
                    int j = 0;
                    while (j < Verifieddatas.Length)
                    {
                        if (int.TryParse(Verifieddatas[j], out result))
                        {
                            i++;
                            if (i == int.Parse(num))
                                return result.ToString();
                        }
                        j++;
                    }
                }
                else
                    throw new Exception("ObjectAtPositionInList");
            }
            else
            {
                IEnumerable<string> compatibleWords = from n in Verifieddatas
                                                      where Objects[theObject].Contains(n)
                                                      select n;

                if (compatibleWords != null && compatibleWords.Count() > 0)
                {
                    if (num == "last")
                        return compatibleWords.ElementAt(compatibleWords.Count() - 1);
                    else if (compatibleWords.Count() >= int.Parse(num))
                        return compatibleWords.ElementAt(int.Parse(num) - 1);
                }
                else
                    throw new Exception("ObjectPositionInList");
            }
            throw new Exception("ObjectPositionInList");
        }

        public static string[] WeekendDayReq = new string[] { "day", "weekend" };
        public static string[] WeekendDayOpt = new string[] { "list", "type" };
        public static string WeekendDay(string[] keywords, string[] datas, string originalText, string[] words)
        {
            return (from n in datas
                    where isWeekendDay(n)
                    select n).First();
        }



        public static string[] f3RegexClues = new string[] { "howmany", "(" + dictionnaryObjects + "|word|number)" };
        public static string[] f3simpleClues = new string[] { "list", "type" };
        public static string ListHowManyObject(string[] keywords, string[] data, string originalText, string[] words)
        {
            if (!verifyOrder(f3RegexClues, words, true))
                throw new Exception("ObjectAtPositionInList");

            List<string> containedObjects = GetObjectsInList(keywords);
            string theObject = null;
            if (containedObjects.Count > 1)
            {
                if (Array.IndexOf(words, "howmany") < words.Length - 1)
                {
                    int objIndex = Array.IndexOf(words, "howmany") + 1;
                    if (containedObjects.Contains(words[objIndex]))
                        theObject = words[objIndex];
                    else
                        throw new Exception("howmanyobjects");
                }
                else
                {
                    theObject = (from n in words
                                 where containedObjects.Contains(n)
                                 select n).First();
                }

            }
            else
            {
                theObject = containedObjects[0];
            }

            List<string> VerfiedKeywords = new List<string>() { theObject };
            foreach (string k in keywords)
            {
                if (!containedObjects.Contains(k))
                    VerfiedKeywords.Add(k);
            }

            string[] Verifieddatas;
            string[] tempList;
            if ((tempList = ExtractDatas(originalText,true)) != null )
                Verifieddatas = tempList;
            else
                Verifieddatas = ExtractDataFromList(VerfiedKeywords.ToArray(), words);


            if (Verifieddatas == null || Verifieddatas.Length == 0)
                throw new Exception("ObjectInSeq");


            if (theObject == "word")
                return Verifieddatas.Length.ToString();
            else
            {
                IEnumerable<string> compatibleWords = from n in Verifieddatas
                                                      where Objects[theObject].Contains(n)
                                                      select n;

                return compatibleWords.Count().ToString();

            }
        }

        public static string[] f4neededwords = new string[] { "([0-9]+(th|nd|st|rd)|last)", "(digit|letter)" };
        public static string[] f4second = new string[] { "word", "number", "position", "type" };
        public static string LetterAtPosition(string[] keywords, string[] datas, string originalText, string[] words)
        {
            if (!verifyOrder(f4neededwords, words, true) && !verifyOrder(f4neededwords.Reverse().ToArray(), words, true))
                throw new Exception("LetterAtPosition");

            string wordToSearchIn = null;

            if (Regex.IsMatch(originalText, "\"" + @"\w+" + "\""))
            {
                wordToSearchIn = Regex.Match(originalText, "\"" + @"(\w+)" + "\"").Groups[1].Value;
            }
            else if (datas.Length == 1)
            {
                wordToSearchIn = datas[0];
            }
            else
                throw new Exception("LetterAtPosition");

            string th = null;
            bool isLetter = keywords.Contains("letter") ? true : false;
            for (int i = 0; i < keywords.Length; i++)
            {
                if (Regex.IsMatch(keywords[i], "([0-9]+)(th|nd|st|rd)"))
                    th = Regex.Match(keywords[i], "([0-9]+)(th|nd|st|rd)").Groups[1].Value;
            }
            if (th == null)
            {

                for (int i = wordToSearchIn.Length - 1; i >= 0; i--)
                {
                    if (isLetter && char.IsLetter(wordToSearchIn[i]))
                        return wordToSearchIn[i].ToString();
                    if (!isLetter && Char.IsDigit(wordToSearchIn[i]))
                        return wordToSearchIn[i].ToString();
                }
            }
            else
            {
                int num = int.Parse(th);

                int i = 0;
                while (i < datas.Length)
                {
                    string word = datas[i];
                    StringBuilder strippedword = new StringBuilder();
                    for (int j = 0; j < word.Length; j++)
                    {
                        if (isLetter && char.IsLetter(word[j]))
                            strippedword.Append(word[j]);
                        if (!isLetter && Char.IsDigit(word[j]))
                            strippedword.Append(word[j]);
                    }
                    if (strippedword.Length >= num)
                    {
                        return strippedword.ToString()[num - 1].ToString();
                    }
                    i++;
                }


            }
            throw new Exception("cannot execute");
        }


        public static string[] f5neededwords = new string[] { "howmany", "(letter|digit)" };
        public static string[] f5second = new string[] { "word", "type" };
        public static string NumberOfLettersInAWord(string[] keywords, string[] datas, string originalText, string[] words)
        {
            if (!verifyOrder(f5neededwords, words, true))
                throw new Exception("NumberOfLettersInAWord");
            string wordToSearchIn = null;

            if (Regex.IsMatch(originalText, "\"" + @"\w+" + "\""))
            {
                wordToSearchIn = Regex.Match(originalText, "\"" + @"(\w+)" + "\"").Groups[1].Value;
            }
            else if (datas.Length == 1)
            {
                wordToSearchIn = datas[0];
            }
            else
                throw new Exception("LetterAtPosition");


            bool isletter = keywords.Contains("letter") ? true : false;
            if (datas != null && datas.Length > 0)
            {

                int numLetter = 0;
                int numDigit = 0;
                for (int i = 0; i < wordToSearchIn.Length; i++)
                {
                    if (char.IsLetter(wordToSearchIn[i]))
                        numLetter++;
                    else if (char.IsDigit(wordToSearchIn[i]))
                        numDigit++;
                }
                if (isletter)
                    return numLetter.ToString();
                else
                    return numDigit.ToString();
            }
            else throw new Exception("NumberOfLettersInAWord");

        }


        public static string[] f8NeededWords = new string[] { "(biggest|smallest)", "[0-9]+" };
        public static string[] f8OtherWords = new string[] { "list", "number", "type" };
        public static string SmallestNumber(string[] keywords, string[] datas, string originalText, string[] words)
        {

            int min = 1000;
            int max = 0;
            for (int i = 0; i < words.Length; i++)
            {
                int num;
                if (int.TryParse(words[i], out num))
                {
                    if (num < min)
                        min = num;
                    if (num > max)
                        max = num;
                }
            }
            if (keywords.Contains("smallest"))
            {
                return min.ToString();
            }
            else
                return max.ToString();
        }


        public static string[] DetermineDayRequiredWords = new string[] { "tomorrow|yesterday|today", "tomorrow|yesterday|today" };
        public static string[] DetermineDayOtherWords = new string[] { "type" };
        public static string DetermineDay(string[] keywords, string[] datas, string originalText, string[] words)
        {
            string dayname = (from n in datas
                              where Objects["day"].Contains(n)
                              select n).First();




            int firstkeywordPas = 0;
            if (keywords[0] == "tomorrow")
                firstkeywordPas++;
            else if (keywords[0] == "yesterday")
                firstkeywordPas--;

            int secondkeywordPas = 0;
            if (keywords[1] == "tomorrow")
                secondkeywordPas++;
            else if (keywords[1] == "yesterday")
                secondkeywordPas--;

            string[] days = Objects["day"];
            int index;
            for (index = 0; index < days.Length && days[index] != dayname; index++) ;


            int resultDayIndex;
            if (originalText.IndexOf(dayname) > originalText.IndexOf(keywords[1]))
            {
                resultDayIndex = index - secondkeywordPas + firstkeywordPas;
            }
            else
            {
                resultDayIndex = index - firstkeywordPas + secondkeywordPas;
            }

            if (resultDayIndex >= days.Length)
                return days[resultDayIndex - days.Length];
            else if (resultDayIndex < 0)
                return days[days.Length + resultDayIndex];
            else
                return days[resultDayIndex];


        }


        public static string[] NumberAsDigitsRequired = new string[] { "[0-9]+", "digit" };
        public static string[] NumberAsDigitsOther = new string[] { "type" };
        public static string NumberAsDigits(string[] keywords, string[] datas, string originalText, string[] words)
        {

            IEnumerable<string> numbers = from n in keywords
                                          where Regex.IsMatch(n, "[0-9]+")
                                          select n;
            if (numbers.Count() == 1 && !Regex.IsMatch(originalText, "[0-9]+"))
                return numbers.First();
            else
                throw new Exception("NumberAsDigits");

        }

        public static string[] WordInCapitalsRequiredWords = new string[] { "capitals" };
        public static string[] WordInCapitalsAnotherWords = new string[] { "word", "type" };
        public static string GetCapitalWord(string[] keywords, string[] datas, string originalText, string[] words)
        {
            Match m = Regex.Match(originalText, "[A-Z]{2,}");
            if (m.Success && m.Value != "CAPITALS" && !m.NextMatch().Success)
                return m.Value;
            else
                throw new Exception("GetCapitalWord");
        }

        public static string[] wordDoNotBelongREQ = new string[] { "word", "notbelong" };
        public static string[] wordDoNotBelongOPT = new string[] { "list", "type" };
        public static string wordDoNotBelong(string[] keywords, string[] datas, string originalText, string[] words)
        {
            if (!verifyOrder(wordDoNotBelongREQ, words, true))
                throw new Exception("wordDoNotBelong");

            string[] tempList;
            string[] verifiedData;
            if ((tempList = ExtractDatas(originalText,true)) != null)
                verifiedData = tempList;
            else
                verifiedData = ExtractDataFromList(keywords, words);
            Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();
            foreach (string k in verifiedData)
            {
                string obj;
                if ((obj = GetObjectNameWhereBelong(k)) != null)
                {
                    if (dic.ContainsKey(obj))
                        dic[obj].Add(k);
                    else
                    {
                        dic.Add(obj, new List<string>() { k });
                    }

                }
                else
                {
                    if (dic.ContainsKey("undefined"))
                        dic["undefined"].Add(k);
                    else
                    {
                        dic.Add("undefined", new List<string>() { k });
                    }
                }
            }
            if (dic.Count == 2)
            {
                if (dic.ElementAt(0).Value.Count == 1)
                    return dic.ElementAt(0).Value[0];
                else if (dic.ElementAt(1).Value.Count == 1)
                    return dic.ElementAt(1).Value[0];
            }
            throw new Exception("word do not belong");
        }


        public static string[] TypeWordREQ = new string[] { "type" };
        public static string[] TypeWordOPT = new string[] { "word", "letter" };
        public static string TypeWord(string[] keywords, string[] datas, string originalText, string[] words)
        {
            string[] list;
            if (datas.Length == 1)
                return datas[0];
            else if ((list = ExtractDatas(originalText,false)) != null )
                return list[0];
            throw new Exception("wordDoNotBelong");
        }

        public static string[] GetMissingWordReq = new string[] { "missing", "characters?" };
        public static string[] GetMissingWordOPT = new string[] { "word" };
        public static string GetMissingWord(string[] keywords, string[] datas, string originalText, string[] words)
        {
            Match clueWord;
            string joker = null;
            while ((clueWord = Regex.Match(originalText, @"\b[a-z]*([_\.\*][a-z]*)+\b", RegexOptions.IgnoreCase)).Success)
            {
                string tempjoker = Regex.Replace(clueWord.Value, @"[_\.\*]", ".");
                int numOfMissing = 0;
                int numOfC = 0;
                for (int i = 0; i < tempjoker.Length; i++)
                {
                    if (tempjoker[i] == '.')
                        numOfMissing++;
                    else
                        numOfC++;
                }
                if (numOfC > numOfMissing && numOfC >= 2)
                {
                    joker = tempjoker;
                    break;
                }
                clueWord = clueWord.NextMatch();
            }
            if (joker == null)
                throw new Exception("GetMissingWord");
            else
            {
                joker = joker.ToLower();
                string result = null;
                Assembly _assembly = Assembly.GetExecutingAssembly();
                StreamReader sr = new StreamReader(_assembly.GetManifestResourceStream("TextCaptchaSolver.Resources.WordDictionary.txt"));
                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    Match m;
                    if ((m = Regex.Match(line, "^" + joker + "$")).Success)
                    {
                        result = m.Value;
                        break;
                    }
                }
                sr.Close();

                if (result != null)
                    return result;
                else
                    throw new Exception("GetMissingWord");
            }
        }




        public static string Calculate(string expression)
        {


            Parser p = new Parser();
            if (p.Evaluate(expression)) return p.Result.ToString();
            else throw new Exception("calculate");



        }


        public static string[] ExtractDatas(string originaltext, bool extractlist)
        {
            Match m = Regex.Match(originaltext, "\"" + @"([\w\-, ]+)[\?!]?" + "\"");
            string[] datas;
            while (m.Success)
            {
                string group = m.Groups[1].Value;
                group = Regex.Replace(group, ",", " ");
                group = Regex.Replace(group, @"\s+", " ");

                if (group.Contains(' ') && extractlist)
                    datas = group.Split(' ');
                else if (!group.Contains(' ') && !extractlist)
                    datas = new string[] { group };
                else
                {
                    m = m.NextMatch();
                    continue;
                }

                if (!originaltext.EndsWith(m.Value) && ExtractDatas(originaltext.Substring(originaltext.IndexOf(m.Value) + m.Value.Length), extractlist) != null)
                    return null;
                else
                    return datas;
            }

            return null;
        }
        // you need to pass verified keywords here ,after determining the right object
        public static string[] ExtractDataFromList(string[] keywords, string[] words)
        {

            List<string> data = new List<string>();
            int wordslength = words.Length;

            List<List<string>> splittedWords = new List<List<string>>();

            int index = 0;
            splittedWords.Add(new List<string>());

            for (int i = 0; i < wordslength; i++)
            {
                if (Array.IndexOf(keywords, words[i]) > -1)
                {
                    index++;
                    splittedWords.Add(new List<string>());
                }
                else
                    splittedWords[index].Add(words[i]);
            }


            int max = splittedWords.Max(n => n.Count);

            IEnumerable<List<string>> maxParts = from n in splittedWords
                                                 where n.Count == max
                                                 select n;
            if (maxParts.Count() == 1)
                return maxParts.First().ToArray();
            else
                return null;
        }

        //1 after, -1 before ,0 both
        public static string GetClosestTo(string word, string[] theObjects, string[] phrase, int afterOrBeforeOrBoth)
        {
            int Keyindex = Array.IndexOf(phrase, word);

            if (Keyindex == 0 && afterOrBeforeOrBoth == -1)
                return null;
            if (Keyindex == phrase.Length && afterOrBeforeOrBoth == 1)
                return null;

            int closestIndexBefore = -1;
            int closestIndexAfter = -1;
            if (Keyindex > 0)
            {

                closestIndexBefore = Array.LastIndexOf(phrase, theObjects[0], Keyindex - 1, Keyindex);

                for (int i = 1; i < theObjects.Length; i++)
                {
                    int wordIndex = Array.LastIndexOf(phrase, theObjects[i], Keyindex - 1, Keyindex);
                    if (wordIndex > closestIndexBefore)
                        closestIndexBefore = wordIndex;
                }

            }
            if (Keyindex < phrase.Length)
            {
                closestIndexAfter = Array.IndexOf(phrase, theObjects[0], Keyindex + 1);


                for (int i = 1; i < theObjects.Length; i++)
                {
                    int wordIndex = Array.IndexOf(phrase, theObjects[i], Keyindex + 1);
                    if ((wordIndex < closestIndexAfter || closestIndexAfter == -1) && wordIndex != -1)
                        closestIndexAfter = wordIndex;
                }
            }
            if (afterOrBeforeOrBoth == -1)
            {
                if (closestIndexBefore != -1)
                    return phrase[closestIndexBefore];
                else
                    return null;
            }
            else if (afterOrBeforeOrBoth == 1)
            {

                if (closestIndexAfter != -1)
                    return phrase[closestIndexAfter];
                else
                    return null;

            }
            else
            {
                if (closestIndexAfter != -1 && closestIndexBefore != -1)
                {
                    if (Keyindex - closestIndexBefore < closestIndexAfter - Keyindex)
                        return phrase[closestIndexBefore];
                    else
                        return phrase[closestIndexAfter];
                }
                else if (closestIndexBefore == -1 && closestIndexAfter == -1)
                    return null;
                else if (closestIndexBefore != -1)
                {
                    return phrase[closestIndexBefore];
                }
                else
                {
                    return phrase[closestIndexAfter];
                }
            }

        }
        public static bool isWeekendDay(string day)
        {
            if (day == "saturday" || day == "sunday")
                return true;
            return false;
        }
        public static List<string> GetObjectsInList(string[] inputList)
        {
            List<string> presentObjects = new List<string>();
            for (int i = 0; i < inputList.Length; i++)
            {
                if (inputList[i] == "word")
                    presentObjects.Add("word");
                else if (inputList[i] == "number")
                    presentObjects.Add("number");
                else
                    for (int j = 0; j < Objects.Count; j++)
                    {
                        if (Regex.IsMatch(Objects.Keys.ElementAt(j), inputList[i]))
                        {
                            presentObjects.Add(Objects.Keys.ElementAt(j));
                            break;
                        }
                    }

            }

            return presentObjects;
        }

        public static string GetObjectNameWhereBelong(string word)
        {
            IEnumerable<string> matches = from n in Objects
                                          where n.Value.Contains(word)
                                          select n.Key;
            if (matches != null && matches.Count() > 0)
            {
                return matches.First();
            }
            else
                return null;
        }
        public static bool verifyOrder(string[] regexes, string[] words, bool oneExactlyAfterother)
        {
            if (oneExactlyAfterother)
            {
                int j = 0;
                bool Began = false;
                for (int i = 0; i < words.Length; i++)
                {
                    if (Regex.IsMatch(words[i], regexes[j]))
                    {
                        Began = true;
                        j++;
                        if (j == regexes.Length)
                            return true;
                    }
                    else if (Began)
                        return false;
                }

            }
            else
            {
                int j = 0;
                for (int i = 0; i < words.Length; i++)
                {
                    if (Regex.IsMatch(words[i], regexes[j]))
                    {

                        j++;
                        if (j == regexes.Length)
                            break;
                    }
                }
                if (j == regexes.Length)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static Dictionary<string, string[]> Objects = new Dictionary<string, string[]>()
        {
            {"color", new string[] { "red", "blue", "green", "red","magenta", "black", "brown", "pink", "white", "yellow", "orange", "gray", "cyan", "violet","purple" }},
            {"bodypart",new string[]{"heart","face","chin","waist","nose","foot","arm","knee","tongue","chest","ear","ankle","eyes","eye","leg","tooth","hair","hand","toe","foot","stomach","thumb","finger","elbow","tongue","head","brain"}},
            {"personname",new string[]{"richard","james","emily","christopher","chris","linda","ronald","mary","lisa","michelle","john","daniel","anthony","patricia","nancy","laura","robert","paul","kevin","linda","karen","sarah","michael","mark","jason","barbara","betty","kimberly","william","donald","jeff","elizabeth","helen","deborah","david","george","jennifer","sandra","richard","kenneth","maria","donna","charles","steven","susan","carol","joseph","edward","margaret","ruth","thomas","brian","dorothy","sharon"}},
            {"headpart",new string[]{"face","ear","tooth","eye","chin","brain","mouth","nose","hair","head"}},
            {"animal",new string[]{"dog","lion","cat","horse","tiger","shark","donkey","monkey","penguin","snake","fish" ,"frog","bird","lizard","rabbit","sheep","crocodile","chimpanzee","giraffe"}},
            {"day",new string[]{"monday","tuesday","wednesday","thursday","friday","saturday","sunday"}},
            {"personmorethanone",new string[]{"eye","ankle","leg","foot","hand","arm","elbow","tooth","knee","ear","toe","thumb","finger"}},
            {"waistabove",new string[]{"eye","chin","arm","elbow","heart","ear","head","face","thumb","chest","stomach","tooth","finger","hand","heart","tongue","ear","eyes","hair","brain","mouth","nose","ear"}},
            {"waistbelow",new string[]{"ankle","foot","knee","leg","toe"}},
        };

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TextCaptchaSolver
{
    public class CaptchaSolver
    {
        public CaptchaSolver()
        {
            Funct f = new Funct(SampleFunctions.ListWordContainLetterOrWhereLetterIsAt, SampleFunctions.f1RegexClues, SampleFunctions.f1simpleClues);
            Funct f3 = new Funct(SampleFunctions.ListHowManyObject, SampleFunctions.f3RegexClues, SampleFunctions.f3simpleClues);
            Funct f7 = new Funct(SampleFunctions.ObjectInSeq, SampleFunctions.ObjectInSeqReq, SampleFunctions.ObjectInSeqOpt);
            Funct f1 = new Funct(SampleFunctions.ObjectAtPositionInList, SampleFunctions.ObjectAtPositionInListReq, SampleFunctions.ObjectAtPositionInListOpt);
            Funct f2 = new Funct(SampleFunctions.WeekendDay, SampleFunctions.WeekendDayReq, SampleFunctions.WeekendDayOpt);
            Funct f4 = new Funct(SampleFunctions.LetterAtPosition, SampleFunctions.f4neededwords, SampleFunctions.f4second);

            Funct f8 = new Funct(SampleFunctions.SmallestNumber, SampleFunctions.f8NeededWords, SampleFunctions.f8OtherWords);
            Funct f9 = new Funct(SampleFunctions.NumberOfLettersInAWord, SampleFunctions.f5neededwords, SampleFunctions.f5second);
            Funct f10 = new Funct(SampleFunctions.DetermineDay, SampleFunctions.DetermineDayRequiredWords, SampleFunctions.DetermineDayOtherWords);
            Funct f11 = new Funct(SampleFunctions.NumberAsDigits, SampleFunctions.NumberAsDigitsRequired, SampleFunctions.NumberAsDigitsOther);
            Funct f12 = new Funct(SampleFunctions.GetCapitalWord, SampleFunctions.WordInCapitalsRequiredWords, SampleFunctions.WordInCapitalsAnotherWords);
            Funct f13 = new Funct(SampleFunctions.wordDoNotBelong, SampleFunctions.wordDoNotBelongREQ, SampleFunctions.wordDoNotBelongOPT);
            Funct f14 = new Funct(SampleFunctions.TypeWord, SampleFunctions.TypeWordREQ, SampleFunctions.TypeWordOPT);
            Funct f15 = new Funct(SampleFunctions.GetMissingWord, SampleFunctions.GetMissingWordReq, SampleFunctions.GetMissingWordOPT);


            FunctionsDetails functions = new FunctionsDetails(new List<Funct>() { f, f1, f2, f3, f4, f7, f8, f9, f10, f11, f12, f13, f14, f15 });
            l = new LogicCaptchaSolver(functions);
        }
        LogicCaptchaSolver l;
        public string Solve(string question)
        {
            string q = question.ToLower();
            bool isMathQuestion;
            q = LogicCaptchaSolver.GetNormalizedText(q, out isMathQuestion);
            if (isMathQuestion)
            {
                return SampleFunctions.Calculate(q);
            }
            else
            {

                string[] words = LogicCaptchaSolver.ExtractWordsFromText(q);
                string[] strippedwords = LogicCaptchaSolver.RemoveNonImportantsWords(words);
                string[] normalized2 = LogicCaptchaSolver.ReplaceTwoSeperatedWords(strippedwords);
                return l.SelectFunction(normalized2,question);
            }
        }
    }
}

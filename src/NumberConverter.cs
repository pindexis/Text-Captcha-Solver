using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace TextCaptchaSolver
{
    class NumberConverter
    {


        public static string ConvertSpelledNumbersToNumeric(string input)
        {
          

            string firstGroupsNumbers = string.Join("|", Enum.GetNames(typeof(FirstGroupNumbers)));
            string secondGroupsNumbers = string.Join("|", Enum.GetNames(typeof(SecondGroupNumbers)));

            string digit1Regex =
                @"(\b(?<digit1>" + firstGroupsNumbers + @")\b)";   //for digit
            string digit2Regex =
                @"(\b(?<digit2>" + firstGroupsNumbers + @")\b)";  //for ten1
            string digit3Regex =
                @"(\b(?<digit3>" + firstGroupsNumbers + @")\b)"; // for ten2
            string digit4Regex =
                @"(\b(?<digit4>" + firstGroupsNumbers + @")\b)"; //for hundred

            string ten1Regex =
               @"(\b(?<ten1>" + secondGroupsNumbers + ")" +
               @"((\s" + digit2Regex + @")?|\b))";

            string ten2Regex =
               @"(\b(?<ten2>" + secondGroupsNumbers + ")" +
               @"((\s" + digit3Regex + @")?|\b))";



            string hundredRegex =
          digit4Regex +
          @"\shundred" +
          @"((\s?(and\s)?(" + ten1Regex + "|" + digit1Regex + @"))|\b)";


            string thousandRegex = @"(" + ten2Regex + "|" + digit3Regex + @")\s" +
                "thousand"
                + @"((\s?(and\s)?(" + hundredRegex +"|"+ten1Regex+"|"+digit1Regex +@"))|\b)";

            Match thousandMatch = Regex.Match(input, thousandRegex, RegexOptions.ExplicitCapture);

            while (thousandMatch.Success)
            {

                int FirstTentens = (thousandMatch.Groups["ten2"].Success) ? (int)Enum.Parse(typeof(SecondGroupNumbers), thousandMatch.Groups["ten2"].Value) : 0;
                int FirstTenDigit = (thousandMatch.Groups["digit3"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), thousandMatch.Groups["digit3"].Value) : 0;

                int hundreds = (thousandMatch.Groups["digit4"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), thousandMatch.Groups["digit4"].Value) : 0;
               
                int SecondTensTen = (thousandMatch.Groups["ten1"].Success) ? (int)Enum.Parse(typeof(SecondGroupNumbers), thousandMatch.Groups["ten1"].Value) : 0;
                int SecondTensDigit = (thousandMatch.Groups["digit2"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), thousandMatch.Groups["digit2"].Value) : 0;

                int digit = (thousandMatch.Groups["digit1"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), thousandMatch.Groups["digit1"].Value) : 0;

                input = input.Replace(thousandMatch.Value.Trim(), ((FirstTentens + FirstTenDigit) * 1000 + hundreds * 100 + SecondTensTen + SecondTensDigit+digit).ToString());
                thousandMatch = thousandMatch.NextMatch();
            }


            Match hundredsmatch = Regex.Match(input, hundredRegex, RegexOptions.ExplicitCapture);
            while (hundredsmatch.Success)
            {

                int hundreds = (hundredsmatch.Groups["digit4"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), hundredsmatch.Groups["digit4"].Value) : 0;
                int SecondTensTen = (hundredsmatch.Groups["ten1"].Success) ? (int)Enum.Parse(typeof(SecondGroupNumbers), hundredsmatch.Groups["ten1"].Value) : 0;
                int SecondTensDigit = (hundredsmatch.Groups["digit2"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), hundredsmatch.Groups["digit2"].Value) : 0;

                int digit = (thousandMatch.Groups["digit1"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), thousandMatch.Groups["digit1"].Value) : 0;

                input = input.Replace(hundredsmatch.Value.Trim(), (hundreds * 100 + SecondTensTen + SecondTensDigit + digit).ToString());
                hundredsmatch = hundredsmatch.NextMatch();
            }

            Match TensMatch = Regex.Match(input, ten1Regex, RegexOptions.ExplicitCapture);
            while (TensMatch.Success)
            {
                int SecondTensTen = (TensMatch.Groups["ten1"].Success) ? (int)Enum.Parse(typeof(SecondGroupNumbers), TensMatch.Groups["ten1"].Value) : 0;
                int SecondTensDigit = (TensMatch.Groups["digit2"].Success) ? (int)Enum.Parse(typeof(FirstGroupNumbers), TensMatch.Groups["digit2"].Value) : 0;

                input = input.Replace(TensMatch.Value.Trim(), (SecondTensTen + SecondTensDigit).ToString());
                TensMatch = TensMatch.NextMatch();
            }

            Match digitMatch = Regex.Match(input, digit1Regex, RegexOptions.ExplicitCapture);
            while (digitMatch.Success)
            {
                int digit = (int)Enum.Parse(typeof(FirstGroupNumbers), digitMatch.Groups["digit1"].Value);
                input = input.Replace(digitMatch.Value.Trim(), (digit).ToString());
                digitMatch = digitMatch.NextMatch();
            }
            return input;
        }
    }
    public enum FirstGroupNumbers { zero, one, two, three, four, five, six, seven, eight, nine, ten, eleven, twelve, thirteen, fourteen, fifteen, sixteen, seventeen, eighteen, nineteen };
    public enum SecondGroupNumbers { twenty = 20, thirty = 30, forty = 40, fifty = 50, sixty = 60, seventy = 70, eighty = 80, ninety = 90 }

}

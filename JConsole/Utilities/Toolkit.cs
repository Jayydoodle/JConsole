using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JConsole
{
    public class Toolkit
    {
        public static string GetHash(string input)
        {
            uint hash = CreateHash(input.ToLower());
            return hash > 0 ? string.Format("0x{0}", hash.ToString("X8")) : string.Empty;
        }

        public static uint CreateHash(string input)
        {
            uint num = 0;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
            {
                num += bytes[i];
                num += num << 10;
                num ^= num >> 6;
            }
            num += num << 3;
            num ^= num >> 11;
            return num + (num << 15);
        }

        public static List<int> ParseCurrencyFromText(List<string> results)
        {
            IEnumerable<MatchCollection> matches = results.Select(x => Regex.Matches(x, GlobalConstants.RegexPattern.AmericanCurrency, RegexOptions.IgnoreCase));

            List<int> resultGroup = new List<int>();

            foreach (MatchCollection x in matches)
            {
                foreach (Match m in x)
                {
                    string[] values = m.Value.Split(" ");

                    string parsedAmount = new string(values[0].Where(c => char.IsDigit(c) || c == '.').ToArray());

                    if (parsedAmount.EndsWith('.'))
                        parsedAmount = parsedAmount + "0";

                    bool couldParse = double.TryParse(parsedAmount, out double parsedValue);

                    if (!couldParse)
                        continue;

                    if (values.Length == 2)
                    {
                        string modifier = new string(values[1].Where(c => char.IsLetter(c)).ToArray());

                        switch (modifier.ToLower())
                        {
                            case "thousand":
                                parsedValue = parsedValue * 1000;
                                break;

                            case "million":
                                parsedValue = parsedValue * 1000000;
                                break;

                            case "billion":
                                parsedValue = parsedValue * 1000000000;
                                break;
                        }
                    }

                    resultGroup.Add(Convert.ToInt32(parsedValue));
                }
            }

            return resultGroup;
        }
    }
}

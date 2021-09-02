using System.Collections.Generic;
using System.Linq;

namespace TransactionService.Helpers
{
    public static class StringHelpers
    {
        public static string CapitaliseFirstLetter(this string str)
        {
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        public static string LowercaseFirstLetter(this string str)
        {
            return str.First().ToString().ToLower() + str.Substring(1);
        }

        public static IEnumerable<string> GenerateNGrams(string input, int minSize = 3, bool multiCase = false)
        {
            var maxSize = input.Length;
            return input.Split(" ").Aggregate(new List<string>(), (ngrams, token) =>
            {
                if (token.Length > minSize)
                {
                    for (var i = minSize; i <= maxSize && i <= token.Length; i++)
                    {
                        var subString = token.Substring(0, i);
                        if (multiCase)
                        {
                            ngrams.Add(subString.CapitaliseFirstLetter());
                            ngrams.Add(subString.LowercaseFirstLetter());
                        }
                        else
                        {
                            ngrams.Add(subString);
                        }
                    }
                }
                else
                {
                    if (multiCase)
                    {
                        ngrams.Add(token.CapitaliseFirstLetter());
                        ngrams.Add(token.LowercaseFirstLetter());
                    }
                    else
                    {
                        ngrams.Add(token);
                    }
                }
                return ngrams;
            });
        }
    }
}
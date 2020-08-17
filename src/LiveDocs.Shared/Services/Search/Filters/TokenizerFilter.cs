using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LiveDocs.Shared.Services.Search.Filters
{
    public class TokenizerFilter : ISearchFilter
    {
        public Task<string[]> Apply(string[] input)
        {
            return Task.FromResult(input?.Select(s => Split(s).Where(w => !string.IsNullOrWhiteSpace(w))).SelectMany(sm => sm).ToArray());
        }

        private string[] Split(string input)
        {
            List<string> output = new List<string>();

            string temp = "";
            foreach (char c in input)
            {
                if (!char.IsLetterOrDigit(c))
                {
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        output.Add(temp.Trim());
                        temp = "";
                    }
                } else
                    temp += c;
            }
            if (!string.IsNullOrWhiteSpace(temp))
                output.Add(temp.Trim());

            return output.ToArray();
        }
    }
}
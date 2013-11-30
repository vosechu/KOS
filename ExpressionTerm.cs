using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace kOS
{
    public class Term
    {
        public String Text;
        public List<Term> SubTerms;
        public bool TermsAreParameters;

        public enum TermTypes { REGULAR, FINAL, FUNCTION, PARAMETER_LIST, COMPARISON, BOOLEAN, SUFFIX, STRUCTURE, MATH_OPERATOR, COMPARISON_OPERATOR, BOOLEAN_OPERATOR }
        public TermTypes Type;

        private static List<String> mathSymbols;
        private static List<String> comparisonSymbols;
        private static List<String> booleanSymbols;
        private static List<String> allSymbols;
        private static List<String> parameterSeperatorSymbols;
        private static List<String> subaccessSymbols;
        private static List<String> delimeterSymbols;

        static Term()
        {
            mathSymbols = new List<string>();
            mathSymbols.AddRange(new[] { "+", "-", "*", "/", "^" });

            comparisonSymbols = new List<string>();
            comparisonSymbols.AddRange(new[] { "<=", ">=", "!=", "==", "=", "<", ">" });

            booleanSymbols = new List<string>();
            booleanSymbols.AddRange(new[] { " AND ", " OR " });

            parameterSeperatorSymbols = new List<string>();
            parameterSeperatorSymbols.AddRange(new[] { "," });

            subaccessSymbols = new List<string>();
            subaccessSymbols.AddRange(new[] { ":" });

            delimeterSymbols = new List<string>();
            delimeterSymbols.AddRange(new[] { "(", ")", "\"" });

            allSymbols = new List<string>();
            allSymbols.AddRange(mathSymbols.ToArray());
            allSymbols.AddRange(comparisonSymbols.ToArray());
            allSymbols.AddRange(booleanSymbols.ToArray());
            allSymbols.AddRange(parameterSeperatorSymbols.ToArray());
            allSymbols.AddRange(subaccessSymbols.ToArray());
            allSymbols.AddRange(delimeterSymbols.ToArray());
        }

        public Term(String input) : this (input, TermTypes.REGULAR, true) {}
        
        public Term(String input, TermTypes type) : this(input, type, true) { }

        public Term(String input, TermTypes type, bool autoTrim)
        {
            TermsAreParameters = false;
            Text = autoTrim ? input.Trim() : input;
            Type = type;
            SubTerms = new List<Term>();

            if (Type != TermTypes.SUFFIX && type != TermTypes.BOOLEAN_OPERATOR) processSymbols();
        }

        public void CopyFrom(ref Term from)
        {
            Text = from.Text;
            SubTerms = from.SubTerms;
            Type = from.Type;
        }

        public Term Merge(params Term[] terms)
        {
            var output = new Term("");

            foreach (var t in terms)
            {
                output.Text += t.Type == TermTypes.PARAMETER_LIST ? "(" + t.Text + ")" :
                                t.Type == TermTypes.SUFFIX ? ":" + t.Text :
                                t.Text;

                output.SubTerms.Add(t);
            }

            return output;
        }

        public String Demo()
        {
            return Demo(0);
        }

        public String Demo(int tabIndent)
        {
            var retString = new String(' ', tabIndent * 4);

            switch (Type)
            {
                case TermTypes.FUNCTION:
                    retString += "FUNCTION->";
                    break;
                case TermTypes.PARAMETER_LIST:
                    retString += "PARAMS->";
                    break;
                case TermTypes.COMPARISON:
                    retString += "COMPARISON->";
                    break;
                case TermTypes.BOOLEAN:
                    retString += "BOOLEAN->";
                    break;
                case TermTypes.STRUCTURE:
                    retString += "STRUCTURE->";
                    break;
                case TermTypes.MATH_OPERATOR:
                    retString += "MATH ";
                    break;
                case TermTypes.COMPARISON_OPERATOR:
                    retString += "COMP ";
                    break;
                case TermTypes.BOOLEAN_OPERATOR:
                    retString += "BOOL ";
                    break;
                case TermTypes.SUFFIX:
                    retString += ":";
                    break;
            }

            retString += Text + Environment.NewLine;

            return SubTerms.Aggregate(retString, (current, t) => current + t.Demo(tabIndent + 1));
        }

        public override string ToString()
        {
            return Text ?? "Empty Term";
        }

        private void processSymbols()
        {
            // Is the input empty?
            if (String.IsNullOrEmpty(Text)) return;
            
            // HEADING.. BY is now deprecated in favor of HEADING(x,y), but here it is if you're using it still
            Text = Regex.Replace(Text, "HEADING ([ :@A-Za-z0-9\\.\\-\\+\\*/]+) BY ([ :@A-Za-z0-9\\.\\-\\+\\*/]+)", "HEADING($2,$1)", RegexOptions.IgnoreCase);

            // Resource tags are now deprecated in favor of SHIP:ResourceName
            Text = Regex.Replace(Text, "(\\s|^)<([a-zA-Z]+)>(\\s|$)", " SHIP:$2 ", RegexOptions.IgnoreCase);

            // Is this JUST a matched symbol?                
            var s = matchAt(ref Text, 0, ref allSymbols);
            if (s != null && Text.Length == s.Length)
            {
                if (mathSymbols.Contains(s)) Type = TermTypes.MATH_OPERATOR;
                else if (comparisonSymbols.Contains(s)) Type = TermTypes.COMPARISON_OPERATOR;
                else if (booleanSymbols.Contains(s)) Type = TermTypes.BOOLEAN_OPERATOR;

                return;
            }

            SubTerms = new List<Term>();

            // If this is a parameter list, grab the parameters
            if (Type == TermTypes.PARAMETER_LIST)
            {
                var parameterList = parseParameters(Text);
                if (parameterList != null)
                {
                    foreach (var param in parameterList)
                    {
                        SubTerms.Add(new Term(param));
                    }
                }

                return;
            }

            // Does this thing contain a boolean operation?
            var booleanElements = splitByListIgnoreBracket(Text, ref booleanSymbols);
            if (booleanElements != null)
            {
                Type = TermTypes.BOOLEAN;

                foreach (String element in booleanElements)
                {
                    SubTerms.Add(booleanSymbols.Contains(element)
                                     ? new Term(element, TermTypes.BOOLEAN_OPERATOR)
                                     : new Term(element));
                }

                return;
            }

            // Does this thing contain a comparison?
            var comparisonElements = splitByListIgnoreBracket(Text, ref comparisonSymbols);
            if (comparisonElements != null)
            {
                Type = TermTypes.COMPARISON;

                foreach (var element in comparisonElements)
                {
                    SubTerms.Add(new Term(element));
                }

                return;
            }

            // Parse this as a normal term
            var buffer = "";
            for (var i = 0; i < Text.Length; i++)
            {
                s = matchAt(ref Text, i, ref allSymbols);

                switch (s)
                {
                    case null:
                        buffer += Text[i];
                        break;
                    case "(":
                        {
                            int startI = i;
                            Utils.Balance(ref Text, ref i, ')');
                    
                            if (buffer.Trim() != "")
                            {
                                string functionName = buffer.Trim();
                                buffer = "";

                                Term bracketTerm = new Term(Text.Substring(startI + 1, i - startI - 1), TermTypes.PARAMETER_LIST);
                                Term functionTerm = Merge(new Term(functionName), bracketTerm);
                                functionTerm.Type = TermTypes.FUNCTION;

                                SubTerms.Add(functionTerm);
                            }
                            else
                            {
                                SubTerms.Add(new Term(Text.Substring(startI + 1, i - startI - 1)));
                            }
                        }
                        break;
                    case "\"":
                        {
                            int startI = i;
                            i = Utils.FindEndOfString(Text, i + 1);
                            buffer += Text.Substring(startI, i - startI + 1); 
                        }
                        break;
                    case ":":
                        {
                            int end = findEndOfSuffix(Text, i + 1);
                            String suffixName = Text.Substring(i + 1, end - i);
                            i += end - i;

                            if (buffer.Trim() != "")
                            {
                                SubTerms.Add(new Term(buffer.Trim()));
                                buffer = "";
                            }

                            if (SubTerms.Count > 0)
                            {
                                Term last = SubTerms.Last();
                                SubTerms.Remove(last);

                                Term structureTerm = Merge(last, new Term(suffixName, TermTypes.SUFFIX));
                                structureTerm.Type = TermTypes.STRUCTURE;
                                SubTerms.Add(structureTerm);
                            }
                        }
                        break;
                    case "-":
                        if (buffer.Trim() != "" || 
                            (SubTerms.Count > 0 && SubTerms.Last().Type != TermTypes.MATH_OPERATOR 
                             && SubTerms.Last().Type != TermTypes.COMPARISON_OPERATOR))
                        {
                            // Not a sign, treat as operator
                            if (buffer.Trim() != "") SubTerms.Add(new Term(buffer.Trim()));
                            SubTerms.Add(new Term(s));

                            buffer = "";
                            i += s.Length - 1;
                        }
                        else
                        {
                            buffer += Text[i];
                        }
                        break;
                    default:
                        if (buffer.Trim() != "") SubTerms.Add(new Term(buffer.Trim()));
                        SubTerms.Add(new Term(s));
                        buffer = "";
                        i += s.Length - 1;
                        break;
                }
            }

            // If there's only one term, we're done!
            if (SubTerms.Count == 0)
            {
                Type = TermTypes.FINAL;
                return;
            }

            if (buffer.Trim() != "") SubTerms.Add(new Term(buffer));

            // If I end up with exactly one subTerm, then I AM that subterm. Exception: If I already have a special type
            if (SubTerms.Count != 1 || this.Type != TermTypes.REGULAR) return;
            var child = SubTerms[0];
            SubTerms.Clear();

            CopyFrom(ref child);
        }

        private int findEndOfSuffix(String input, int start)
        {
            for (var i = start; i < input.Length; i++)
            {
                var match = Regex.Match(input.Substring(i, 1), "[a-zA-Z0-9_]");
                if (!match.Success)
                {
                    return i == start ? 0 : i - 1;
                }
            }

            return input.Length - 1;
        }

        private IEnumerable<string> splitByListIgnoreBracket(String input, ref List<String> operators)
        {
            return splitByListIgnoreBracket(input, ref operators, false);
        }

        private IEnumerable<string> splitByListIgnoreBracket(String input, ref List<String> operators, bool returnIfOneElement)
        {
            var buffer = "";
            var retList = new List<string>();

            for (var i = 0; i < input.Length; i++)
            {
                switch (input[i])
                {
                    case '(':
                        {
                            int startI = i;
                            Utils.Balance(ref Text, ref i, ')');
                            buffer += Text.Substring(startI, i - startI + 1);
                        }
                        break;
                    case '"':
                        {
                            int startI = i;
                            i = Utils.FindEndOfString(Text, i + 1);
                            buffer += Text.Substring(startI, i - startI + 1);
                        }
                        break;
                    default:
                        {
                            var s = matchAt(ref input, i, ref operators);

                            if (s != null)
                            {
                                // TODO: If buffer empty, syntax error

                                retList.Add(buffer);
                                retList.Add(s);
                                buffer = "";

                                i += s.Length - 1;
                            }
                            else
                            {
                                buffer += input[i];
                            }
                        }
                        break;
                }
            }

            if (buffer.Trim() != "") retList.Add(buffer);

            if (returnIfOneElement)
            {
                return retList;
            }
            return retList.Count > 1 ? retList : null;
        }

        private IEnumerable<string> parseParameters(String input)
        {
            var splitList = splitByListIgnoreBracket(input, ref parameterSeperatorSymbols, true);

            return splitList != null ? splitList.Where(listItem => listItem != ",").ToList() : null;
        }

        private String matchAt(ref String input, int i, ref List<String> matchables)
        {
            foreach (var s in matchables)
            {
                if (s.StartsWith(" "))
                {
                    var r = new Regex("^" + s.Replace(" ", "\\s"), RegexOptions.IgnoreCase);
                    var m = r.Match(input.Substring(i));

                    if (m.Success)
                    {
                        return s;
                    }
                }
                else if (input.Length - i >= s.Length && input.Substring(i, s.Length) == s)
                {
                    return s;
                }
            }

            return null;
        }
    }
    
}

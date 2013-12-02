using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using kOS.Context;
using kOS.Craft;
using kOS.Debug;
using kOS.Values;
using TimeSpan = kOS.Values.TimeSpan;

namespace kOS
{
    public class Expression
    {
        readonly Term rootTerm;
        readonly ExecutionContext executionContext;

        public Expression(Term term, ExecutionContext context)
        {
            rootTerm = term;
            executionContext = context;
        }

        public Expression(String text, ExecutionContext context)
        {
            rootTerm = new Term(text);
            executionContext = context;
        }

        public object GetValue()
        {
            return GetValueOfTerm(rootTerm);
        }

        public object GetValueOfTerm(Term term)
        {
            object output;

            switch (term.Type)
            {
                case Term.TermTypes.FINAL:
                    output = RecognizeConstant(term.Text);
                    if (output != null) return output;
                    output = AttemptGetVariableValue(term.Text);
                    if (output != null) return output;
                    break;
                case Term.TermTypes.REGULAR:
                    output = TryProcessMathStatement(term);
                    if (output != null) return output;
                    break;
                case Term.TermTypes.FUNCTION:
                    output = TryProcessFunction(term);
                    if (output != null) return output;
                    break;
                case Term.TermTypes.STRUCTURE:
                    output = TryProcessStructure(term);
                    if (output != null) return output;
                    break;
                case Term.TermTypes.COMPARISON:
                    output = TryProcessComparison(term);
                    if (output != null) return output;
                    break;
                case Term.TermTypes.BOOLEAN:
                    output = TryProcessBoolean(term);
                    if (output != null) return output;
                    break;
            }
            
            throw new kOSException("Unrecognized term: '" + term.Text + "'", executionContext);
        }

        private object RecognizeConstant(String text)
        {
            text = text.Trim();

            // Numbers
            double testDouble;
            const NumberStyles styles = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
            if (double.TryParse(text, styles, CultureInfo.InvariantCulture, out testDouble)) return testDouble;

            // Booleans
            bool testBool;
            if (bool.TryParse(text, out testBool)) return testBool;

            // Strings
            if (text.StartsWith("\""))
            {
                var end = Utils.FindEndOfString(text, 1);
                if (end == text.Length - 1) return text.Substring(1, text.Length - 2);
            }

            return null;
        }

        private void ReplaceChunkPairAt(ref List<StatementChunk> chunks, int index, StatementChunk replace)
        {
            chunks.RemoveRange(index, 2);
            chunks.Insert(index, replace);
        }

        private object TryProcessMathStatement(Term input)
        {
            var chunks = new List<StatementChunk>();

            for (var i = 0; i < input.SubTerms.Count; i += 2)
            {
                var termValue = GetValueOfTerm(input.SubTerms[i]);

                if (i + 1 < input.SubTerms.Count)
                {
                    var opTerm = input.SubTerms[i + 1];
                    if (opTerm.Type == Term.TermTypes.MATH_OPERATOR)
                    {
                        chunks.Add(new StatementChunk(termValue, opTerm.Text));
                    }
                    else
                    {
                        throw new kOSException("Expression error processing statement '" + input + "'", executionContext);
                    }
                }
                else
                {
                    chunks.Add(new StatementChunk(termValue, ""));
                }
            }

            #region Exponents

            for (var i = 0; i < chunks.Count - 1; i++)
            {
                var c1 = chunks[i];
                var c2 = chunks[i + 1];

                if (c1.Opr != "^") continue;
                    var resultValue = AttemptPow(c1.Value, c2.Value);
                if (resultValue == null) throw new kOSException("Can't use exponents with " + GetFriendlyNameOfItem(c1.Value) + " and " + GetFriendlyNameOfItem(c2.Value), executionContext);

                    ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                    i--;
                }

            #endregion

            #region Multiplication and Division

            for (var i = 0; i < chunks.Count - 1; i ++)
            {
                var c1 = chunks[i];
                var c2 = chunks[i + 1];

                switch (c1.Opr)
                {
                    case "*":
                {
                    var resultValue = AttemptMultiply(c1.Value, c2.Value);
                            if (resultValue == null) throw new kOSException("Can't multiply " + GetFriendlyNameOfItem(c1.Value) + " by " + GetFriendlyNameOfItem(c2.Value), executionContext);

                    ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                    i--;
                }
                        break;
                    case "/":
                {
                    var resultValue = AttemptDivide(c1.Value, c2.Value);
                            if (resultValue == null) throw new kOSException("Can't divide " + GetFriendlyNameOfItem(c1.Value) + " by " + GetFriendlyNameOfItem(c2.Value), executionContext);

                    ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                    i--;
                }
                        break;
                }
            }

            #endregion

            #region Addition and Subtraction

            for (var i = 0; i < chunks.Count - 1; i++)
            {
                var c1 = chunks[i];
                var c2 = chunks[i + 1];

                switch (c1.Opr)
                {
                    case "+":
                {
                    var resultValue = AttemptAdd(c1.Value, c2.Value);
                            if (resultValue == null) throw new kOSException("Can't add " + GetFriendlyNameOfItem(c1.Value) + " and " + GetFriendlyNameOfItem(c2.Value), executionContext);

                    ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                    i--;
                }
                        break;
                    case "-":
                {
                    var resultValue = AttemptSubtract(c1.Value, c2.Value);
                            if (resultValue == null) throw new kOSException("Can't subtract " + GetFriendlyNameOfItem(c2.Value) + " from " + GetFriendlyNameOfItem(c1.Value), executionContext);

                    ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                    i--;
                }
                        break;
                }
            }

            #endregion

            // If everything occurred correctly I should be left with one math chunk containing nothing but the resultant value
            if (chunks.Count == 1)
            {
                return chunks[0].Value;
            }

            return null;
        }

        private object TryProcessFunction(Term input)
        {
            var p = input.SubTerms[1].SubTerms.ToArray();

            object output = TryCreateSpecialValue(input.SubTerms[0].Text, p);
            if (output != null) return output;

            output = TryMathFunction(input.SubTerms[0].Text, p);
            if (output != null) return output;

            output = TryExternalFunction(input.SubTerms[0].Text, p);
            return output;
        }

        private object TryProcessStructure(Term input)
        {   
            var baseTerm = input.SubTerms[0];
            var suffixTerm = input.SubTerms[1];

            if (suffixTerm.Type == Term.TermTypes.SUFFIX)
            {
                // First, see if this is just a variable with a comma in it (old-style structure)
                object output;
                if (Regex.Match(baseTerm.Text, "^[a-zA-Z]+$").Success)
                {
                    output = AttemptGetVariableValue(baseTerm.Text + ":" + suffixTerm.Text);
                    if (output != null) return output;
                }

                var baseTermValue = GetValueOfTerm(baseTerm);
                var value = GetValueOfTerm(baseTerm) as ISpecialValue;
                if (value != null)
                {
                    output = value.GetSuffix(suffixTerm.Text.ToUpper());
                    if (output != null) return output;

                    throw new kOSException("Suffix '" + suffixTerm.Text + "' not found on object", executionContext);
                }
                throw new kOSException("Values of type " + GetFriendlyNameOfItem(baseTermValue) + " cannot have suffixes", executionContext);
            }

            return null;
        }

        private object TryProcessComparison(Term input)
        {
            var chunks = new List<StatementChunk>();

            for (var i = 0; i < input.SubTerms.Count; i += 2)
            {
                var termValue = GetValueOfTerm(input.SubTerms[i]);

                if (i + 1 < input.SubTerms.Count)
                {
                    var opTerm = input.SubTerms[i + 1];
                    if (opTerm.Type == Term.TermTypes.COMPARISON_OPERATOR)
                    {
                        chunks.Add(new StatementChunk(termValue, opTerm.Text));
                    }
                    else
                    {
                        throw new kOSException("Expression error processing comparison '" + input + "'", executionContext);
                    }
                }
                else
                {
                    chunks.Add(new StatementChunk(termValue, ""));
                }
            }

            for (var i = 0; i < chunks.Count - 1; i++)
            {
                var c1 = chunks[i];
                var c2 = chunks[i + 1];
                object resultValue = null;

                switch (c1.Opr)
                {
                    case "=":
                    case "==":
                        resultValue = AttemptEq(c1.Value, c2.Value);
                        break;
                    case "!=":
                        resultValue = AttemptNotEq(c1.Value, c2.Value);
                        break;
                    case "<":
                        resultValue = AttemptLT(c1.Value, c2.Value);
                        break;
                    case ">":
                        resultValue = AttemptGT(c1.Value, c2.Value);
                        break;
                    case "<=":
                        resultValue = AttemptLTE(c1.Value, c2.Value);
                        break;
                    case ">=":
                        resultValue = AttemptGTE(c1.Value, c2.Value);
                        break;
                }

                if (resultValue == null) throw new kOSException("Can't compare " + GetFriendlyNameOfItem(c1.Value) + " to " + GetFriendlyNameOfItem(c2.Value) + " using " + c1.Opr, executionContext);

                ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                i--;
            }
            
            return chunks.Count == 1 ? chunks[0].Value : null;
        }

        private object TryProcessBoolean(Term input)
        {
            var chunks = new List<StatementChunk>();

            for (var i = 0; i < input.SubTerms.Count; i += 2)
            {
                var termValue = GetValueOfTerm(input.SubTerms[i]);

                if (i + 1 < input.SubTerms.Count)
                {
                    var opTerm = input.SubTerms[i + 1];
                    if (opTerm.Type == Term.TermTypes.BOOLEAN_OPERATOR)
                    {
                        chunks.Add(new StatementChunk(termValue, opTerm.Text));
                    }
                    else
                    {
                        throw new kOSException("Expression error processing boolean operation '" + input + "'", executionContext);
                    }
                }
                else
                {
                    chunks.Add(new StatementChunk(termValue, ""));
                }
            }

            for (var i = 0; i < chunks.Count - 1; i++)
            {
                var c1 = chunks[i];
                var c2 = chunks[i + 1];
                object resultValue = null;

                switch (c1.Opr)
                {
                    case "AND":
                        resultValue = AttemptAnd(c1.Value, c2.Value);
                        break;
                    case "OR":
                        resultValue = AttemptOr(c1.Value, c2.Value);
                        break;
                }

                if (resultValue == null) throw new kOSException("Can't compare " + GetFriendlyNameOfItem(c1.Value) + " to " + GetFriendlyNameOfItem(c2.Value) + " using " + c1.Opr, executionContext);

                ReplaceChunkPairAt(ref chunks, i, new StatementChunk(resultValue, c2.Opr));
                i--;
            }

            return chunks.Count == 1 ? chunks[0].Value : null;
            }

        private object TryExternalFunction(String name, IList<Term> p)
        {
            foreach (var f in executionContext.ExternalFunctions.Where(f => f.Name.ToUpper() == name.ToUpper()))
            {
                if (p.Count() != f.ParameterCount) throw new Exception("Wrong number of arguments, expected " + f.ParameterCount);

                var sp = new String[f.ParameterCount];
                for (var i = 0; i < f.ParameterCount; i++)
                {
                    sp[i] = GetValueOfTerm(p[i]).ToString();
                }

                var output = executionContext.CallExternalFunction(f.Name, sp);
                if (output != null) return output;
            }

            return null;
        }

        private object TryMathFunction(String name, IList<Term> p)
        {
            name = name.ToUpper();

            switch (name)
            {
                case "SIN":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Sin(dp[0] * (Math.PI / 180)); }
                case "COS":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Cos(dp[0] * (Math.PI / 180)); }
                case "TAN":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Tan(dp[0] * (Math.PI / 180)); }
                case "ARCSIN":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Asin(dp[0]) * (180 / Math.PI); }
                case "ARCCOS":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Acos(dp[0]) * (180 / Math.PI); }
                case "ARCTAN":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Atan(dp[0]) * (180 / Math.PI); }
                case "ARCTAN2":
                    { var dp = GetParamsAsT<double>(p, 2); return Math.Atan2(dp[0], dp[1]) * (180 / Math.PI); }
                case "ABS":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Abs(dp[0]); }
                case "MOD":
                    { var dp = GetParamsAsT<double>(p, 2); return dp[0] % dp[1]; }
                case "FLOOR":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Floor(dp[0]); }
                case "CEILING":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Ceiling(dp[0]); }
                case "SQRT":
                    { var dp = GetParamsAsT<double>(p, 1); return Math.Sqrt(dp[0]); }
                case "ROUND":
            {
                switch (p.Count())
                {
                    case 1:
                        {
				    var dp = GetParamsAsT<double>(p, 1);
                            return Math.Round(dp[0]);
                        }
                    case 2:
                        {
				    var dp = GetParamsAsT<double>(p, 2);
                            return Math.Round(dp[0], (int)dp[1]);
                        }
                }
            }
		    break;
            }

            return null;
        }

        private ISpecialValue TryCreateSpecialValue(String name, Term[] p)
        {
            name = name.ToUpper();

            switch (name)
            {
                case "NODE":
                    { var dp = GetParamsAsT<double>(p, 4); return new Node(dp[0], dp[1], dp[2], dp[3]); }
                case "V":
                    { var dp = GetParamsAsT<double>(p, 3); return new Vector(dp[0], dp[1], dp[2]); }
                case "R":
                    { var dp = GetParamsAsT<double>(p, 3); return new Direction(new Vector3d(dp[0], dp[1], dp[2]), true); }
                case "Q":
                    { var dp = GetParamsAsT<double>(p, 4); return new Direction(new UnityEngine.Quaternion((float)dp[0], (float)dp[1], (float)dp[2], (float)dp[3])); }
                case "T":
                    { var dp = GetParamsAsT<double>(p, 1); return new TimeSpan(dp[0]); }
                case "LATLNG":
                    { var dp = GetParamsAsT<double>(p, 2); return new GeoCoordinates(executionContext.Vessel, dp[0], dp[1]); }
                case "VESSEL":
                    { var sp = GetParamsAsT<String>(p, 1); return new VesselTarget(executionContext.Vessel.GetVesselByName(sp[0]), executionContext); }
                case "BODY":
                    { var sp = GetParamsAsT<String>(p, 1); return new BodyTarget(sp[0], executionContext); }
            }

            if (name == "HEADING")
            {
                var pCount = p.Count();
                if (pCount < 2 || pCount > 3) throw new kOSException("Wrong number of arguments supplied, expected 2 or 3", executionContext);

                var dp = GetParamsAsT<double>(p, pCount);
                var q = UnityEngine.Quaternion.LookRotation(executionContext.Vessel.GetNorthVector(), executionContext.Vessel.upAxis);
                q *= UnityEngine.Quaternion.Euler(new UnityEngine.Vector3((float)-dp[0], (float)dp[1], (float)(dp.Count() > 2 ? dp[2] : 0)));

                return new Direction(q);
            }

            return null;
        }

        private T GetParamAsT<T>(Term input)
        {
            var value = GetValueOfTerm(input);
            if (value is T) return (T)value;

            if (typeof(T) == typeof(double)) throw new kOSException("Supplied parameter '" + input.Text + "' is not a number", executionContext);
            if (typeof(T) == typeof(String)) throw new kOSException("Supplied parameter '" + input.Text + "' is not a string", executionContext);
            if (typeof(T) == typeof(bool)) throw new kOSException("Supplied parameter '" + input.Text + "' is not a boolean", executionContext);

            throw new kOSException("Supplied parameter '" + input.Text + "' is not of the correct type", executionContext);
        }

        private T[] GetParamsAsT<T>(IList<Term> input, int size)
        {
            if (input.Count() != size)
            {
                throw new kOSException("Wrong number of arguments supplied, expected " + size, executionContext);
            }

            var retVal = new T[size];

            for (var i = 0; i < size; i++)
            {
                retVal[i] = GetParamAsT<T>(input[i]);
            }

            return retVal;
        }

        public static String GetFriendlyNameOfItem(object input)
        {
            if (input is String) return "string";
            if (input is double) return "number";
            if (input is float) return "number";
            if (input is int) return "number";
            if (input is VesselTarget) return "vessel";
            if (input is ISpecialValue) return input.GetType().ToString().Replace("kOS.", "").ToLower();

            return "";
        }

        private object AttemptMultiply(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 * (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("*", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("*", val1, true); }

            return null;
        }

        private object AttemptDivide(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 / (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("/", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("/", val1, true); }

            return null;
        }

        private object AttemptAdd(object val1, object val2)
        {
            if (val1 is String || val2 is String) { return val1.ToString() + val2.ToString(); }

            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 + (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("+", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("+", val1, true); }

            return null;
        }

        private object AttemptSubtract(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 - (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("-", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("-", val1, true); }

            return null;
        }

        private object AttemptPow(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return Math.Pow((double)val1, (double)val2); }

            return null;
        }

        private object AttemptGetVariableValue(string varName)
        {
            executionContext.UpdateLock(varName);
            var v = executionContext.FindVariable(varName);
            
            return v == null ? null : (v.Value is float ? (double)((float)v.Value) : v.Value);
        }

        private object AttemptEq(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return Math.Abs((double)val1 - (double)val2) < 0.00001; }
            if (val1 is String || val2 is String) { return val1.ToString() == val2.ToString(); }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("=", val2, false); }

            return null;
        }

        private object AttemptNotEq(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return Math.Abs((double)val1 - (double)val2) > 0.00001; }
            if (val1 is String || val2 is String) { return val1.ToString() != val2.ToString(); }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("!=", val2, false); }

            return null;
        }

        private object AttemptGT(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 > (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation(">", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation(">", val1, true); }

            return null;
        }

        private object AttemptLT(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 < (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("<", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("<", val1, true); }

            return null;
        }

        private object AttemptGTE(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 >= (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation(">=", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation(">=", val1, true); }

            return null;
        }

        private object AttemptLTE(object val1, object val2)
        {
            if ((val1 is double || val1 is float || val1 is int) && (val2 is double || val2 is float || val2 is int)) { return (double)val1 <= (double)val2; }
            if (val1 is ISpecialValue) { return ((ISpecialValue)val1).TryOperation("<=", val2, false); }
            if (val2 is ISpecialValue) { return ((ISpecialValue)val2).TryOperation("<=", val1, true); }

            return null;
        }

        private bool ObjectToBool(object input, out bool result)
        {
            if (input is bool) { result = (bool)input; return true; }
            if (input is double) { result = (double)input > 0; return true; }

            var s = input as string;
            if (s != null)
            {
                if (bool.TryParse(s, out result)) return true;
                double dblVal;
                if (double.TryParse(s, out dblVal))
                {
                    result = dblVal > 0;
                    return true;
                }
            }

            result = false;
            return false;
        }

        private object AttemptAnd(object val1, object val2)
        {
            bool v1, v2;

            if (!ObjectToBool(val1, out v1)) return null;
            if (!ObjectToBool(val2, out v2)) return null;

            return (v1 && v2);
        }

        private object AttemptOr(object val1, object val2)
        {
            bool v1, v2;

            if (!ObjectToBool(val1, out v1)) return null;
            if (!ObjectToBool(val2, out v2)) return null;

            return (v1 || v2);
        }

        public bool IsNull()
        {
            var value = GetValue();

            return value == null;
        }

        public bool IsTrue()
        {
            var value = GetValue();

            if (value == null) return false;
            if (value is bool) return (bool)value;
            if (value is double) return (double)value > 0;
            var str = value as string;
            if (str != null)
            {
                bool boolVal;
                if (bool.TryParse(str, out boolVal)) return boolVal;

                double numberVal;
                if (double.TryParse(str, out numberVal)) return numberVal > 0;

                return str.Trim() != "";
            }
            return value is ISpecialValue;
        }

        public double Double()
        {
            var value = GetValue();

            if (value == null) return 0;
            if (value is bool) return (bool)value ? 1 : 0;
            if (value is double) return (double)value;
            var str = value as string;
            if (str != null)
            {
                double numberVal;
                return double.TryParse(str, out numberVal) ? numberVal : 0;
            }

            return 0;
        }

        public float Float()
        {
            return (float)Double();
        }

        public override string ToString()
        {
            return GetValue().ToString();
        }

        private struct StatementChunk
        {
            public StatementChunk(object value, String opr)
            {
                Value = value;
                Opr = opr;
            }

            public readonly object Value;
            public readonly String Opr;
        }
    }
}

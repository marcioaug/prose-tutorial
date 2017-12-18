using System;
using System.Text.RegularExpressions;

using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Rules;



namespace ProseTutorial 
{
    public class WitnessFunctions : DomainLearningLogic 
    {
        public WitnessFunctions(Grammar grammar) : base(grammar) { }
        
        public static Regex[] UsefulRegexes = {
            new Regex(@"\w+"),
            new Regex(@"\d+"),
            new Regex(@"\s+"),
            new Regex(@"\.+"),
            new Regex(@"$+")
        };

        [WitnessFunction(nameof(Semantics.Substring), 1)]
        public DisjunctiveExamplesSpec WitnessStartPosition(GrammarRule rule, ExampleSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.Examples) 
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var output = example.Value as string;
                var occurrences = new List<int>();

                for (int i = input.IndexOf(output); i >= 0; i = input.IndexOf(output, i + 1))
                {
                    occurrences.Add(i);
                }
                
                if (occurrences.IsEmpty()) return null;
                result[inputState] = occurrences.Cast<object>();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.Substring), 2, DependsOnParameters = new []{1})]
        public ExampleSpec WitnessEndPosition(GrammarRule rule, ExampleSpec spec, ExampleSpec startSpec)
        {
            var result = new Dictionary<State, object>();

            foreach (var example in spec.Examples) 
            {
                State inputState = example.Key;
                var output = example.Value as string;
                var start = (int) startSpec.Examples[inputState];
                result[inputState] = start + output.Length;
            }

            return new ExampleSpec(result);
        }

        [WitnessFunction(nameof(Semantics.AbsPos), 1)]
        public DisjunctiveExamplesSpec WitnessK(GrammarRule rule, DisjunctiveExamplesSpec spec)
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.DisjunctiveExamples) 
            {
                State inputState = example.Key;
                var v = inputState[rule.Body[0]] as string;
                
                var positions = new List<int>();

                foreach (int pos in example.Value)
                {
                    positions.Add(pos + 1);
                    positions.Add(pos - v.Length - 1);
                }

                if (positions.IsEmpty()) return null;

                result[inputState] = positions.Cast<object>();
            }

            return new DisjunctiveExamplesSpec(result);
        }

        [WitnessFunction(nameof(Semantics.RelPos), 1)]
        public DisjunctiveExamplesSpec WitnessRegexPair(GrammarRule rule, DisjunctiveExamplesSpec spec) 
        {
            var result = new Dictionary<State, IEnumerable<object>>();

            foreach (var example in spec.DisjunctiveExamples)
            {
                State inputState = example.Key;
                var input = inputState[rule.Body[0]] as string;
                var regexes = new List<Tuple<Regex, Regex>>();

                foreach (int output in example.Value)
                {
                    List<Regex>[] leftMatches, rightMatches;
                    BuildStringMatches(input, out leftMatches, out rightMatches);

                    var leftRegex = leftMatches[output];
                    var rightRegex = rightMatches[output];

                    if (leftRegex.IsEmpty() || rightRegex.IsEmpty())
                        return null;

                    regexes.AddRange(from l in leftRegex
                                     from r in rightRegex
                                     select Tuple.Create(l, r));        
                }
                if (regexes.IsEmpty())
                    return null;
                result[inputState] = regexes;
            }

            return DisjunctiveExamplesSpec.From(result);
        }

        static void BuildStringMatches(String inp, out List<Regex>[] leftMatches, out List<Regex>[] rightMatches)
        {
            leftMatches = new List<Regex>[inp.Length + 1];
            rightMatches = new List<Regex>[inp.Length + 1];

            for (int p = 0; p <= inp.Length; ++p)
            {
                leftMatches[p] = new List<Regex>();
                rightMatches[p] = new List<Regex>();
            }

            foreach (Regex r in UsefulRegexes) 
            {
                foreach (Match m in r.Matches(inp))
                {
                    leftMatches[m.Index + m.Length].Add(r);
                    rightMatches[m.Index].Add(r);
                }
            }
        }

    }
}
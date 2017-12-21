using Xunit;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.AST;

namespace ProseTutorial
{
    public class SubstringTest
    {
        private const string grammarPath = "../../../../ProseTutorial/grammar/substring.grammar";
        private Grammar grammar = DSLCompiler.ParseGrammarFromFile(grammarPath).Value;

        [Fact]
        public void TestLearnSubstringPositiveAbsPos()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"19-Feb-1960", "Feb"} 
            };

            var program = GetFirstProgram(examples);

            Assert.Equal("Feb", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "19-Feb-1960")) as string);
        }

        [Fact]
        public void TestLearnSubstringPositiveAbsPosSecOcorrence()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"16-Feb-2016", "16"}, 
                {"14-Jan-2012", "12"},
            };

            var program = GetFirstProgram(examples);

            Assert.Equal("16", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "16-Feb-2016")) as string);
            Assert.Equal("12", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "14-Jan-2012")) as string);
        }

        [Fact]
        public void TestLearnSubstringPositiveAbsPosSecOcorrenceOneExp() 
        {
            var examples = new Dictionary<string, string>
            {
                {"16-Feb-2016", "16"}
            };

            var programs = GetPrograms(examples);
            var program = programs.First();

            Assert.Equal("16", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "16-Feb-2016")) as string);
            Assert.Equal(14, programs.Count()); 
        }

        [Fact]
        public void TestLearnSubstringNegativeAbsPos()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"(Gustavo Soares)", "Gustavo Soares"}, 
                {"(Titus Barik)", "Titus Barik"},
            };

            var program = GetFirstProgram(examples);

            Assert.Equal("Gustavo Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Gustavo Soares)")) as string);
            Assert.Equal("Titus Barik", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Titus Barik)")) as string);
        }

        [Fact]
        public void TestLearnSubstringNegativeAbsPosRanking()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"(Gustavo Soares)", "Gustavo Soares"}
            };

            var program = GetTopKPrograms(examples, 1).First();

            Assert.Equal("Gustavo Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Gustavo Soares)")) as string);
            Assert.Equal("Titus Barik", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Titus Barik)")) as string);
        }

        [Fact]
        public void TestLearnSubstringTwoExamples() 
        {
            var examples = new Dictionary<string, string> 
            { 
                {"Gustavo Soares", "Soares"}, 
                {"Sumit Gulwani", "Gulwani"},
            };

            var program = GetFirstProgram(examples);

            Assert.Equal("Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Gustavo Soares")) as string);
            Assert.Equal("Gulwani", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Sumit Gulwani")) as string);
        }

        [Fact]
        public void TestLearnSubstringOneExample() 
        {
            var examples = new Dictionary<string, string> 
            { 
                {"Gustavo Soares", "Soares"}
            };

            var program =  GetTopKPrograms(examples, 1).First();

            Assert.Equal("Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Gustavo Soares")) as string);
            Assert.Equal("Gulwani", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Sumit Gulwani")) as string);
        }


        public ProgramNode GetFirstProgram(Dictionary<string, string> examples) {
            return GetPrograms(examples).First();
        }

        public IEnumerable<ProgramNode> GetTopKPrograms(Dictionary<string, string> examples, int k) {

            var prose = ConfigureSynthesis(grammar);
            var scoreFeature = new RankingScore(grammar);

            var programs = prose.LearnGrammarTopK(GetSpecification(examples), scoreFeature, k, null);

            return programs;
        }

        public IEnumerable<ProgramNode> GetPrograms(Dictionary<string, string> examples) {

            var prose = ConfigureSynthesis(grammar);
            var learnedSet = prose.LearnGrammar(GetSpecification(examples));

            var programs = learnedSet.RealizedPrograms;

            return programs;
        }

         public ExampleSpec GetSpecification(Dictionary<string, string> examples)
        {
            var examplesState = new Dictionary<State, object>();

            foreach (KeyValuePair<string, string> example in examples) 
            {
                var input = State.CreateForExecution(grammar.InputSymbol, example.Key);
                examplesState[input] = example.Value;
            }

            return new ExampleSpec(examplesState);
        }


        public static SynthesisEngine ConfigureSynthesis(Grammar grammar)
        {
            var witnessFunctions = new WitnessFunctions(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };
            
            return new SynthesisEngine(grammar, synthesisConfig);
        }
    }
}
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.AST;


namespace ProseTutorial
{
    [TestClass]
    public class SubstringTest
    {

        private const string grammarPath = "../../../../ProseTutorial/grammar/substring.grammar";
        private Grammar grammar = DSLCompiler.ParseGrammarFromFile(grammarPath).Value;

        [TestMethod]
        public void TestLearnSubstringPositiveAbsPos()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"19-Feb-1960", "Feb"} 
            };

            var program = GetFirstProgram(examples);

            Assert.AreEqual("Feb", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "19-Feb-1960")) as string);
        }

        [TestMethod]
        public void TestLearnSubstringPositiveAbsPosSecOcorrence()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"16-Feb-2016", "16"}, 
                {"14-Jan-2012", "12"},
            };

            var program = GetFirstProgram(examples);

            Assert.AreEqual("16", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "16-Feb-2016")) as string);
            Assert.AreEqual("12", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "14-Jan-2012")) as string);
        }

        [TestMethod]
        public void TestLearnSubstringPositiveAbsPosSecOcorrenceOneExp() 
        {
            var examples = new Dictionary<string, string>
            {
                {"16-Feb-2016", "16"}
            };

            var programs = GetPrograms(examples);
            var program = programs.First();

            Assert.AreEqual("16", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "16-Feb-2016")) as string);
            //Adicionando o suporte a posições negativas aumenta o número de programas de 2 para 8.
            //Adicionando o suporte a posuções relativas aumenta o número de programas de 8 para 14.
            Assert.AreEqual(14, programs.Count()); 
        }

        [TestMethod]
        public void TestLearnSubstringNegativeAbsPos()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"(Gustavo Soares)", "Gustavo Soares"}, 
                {"(Titus Barik)", "Titus Barik"},
            };

            var program = GetFirstProgram(examples);

            Assert.AreEqual("Gustavo Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Gustavo Soares)")) as string);
            Assert.AreEqual("Titus Barik", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Titus Barik)")) as string);
        }

        [TestMethod]
        public void TestLearnSubstringNegativeAbsPosRanking()
        {
            var examples = new Dictionary<string, string> 
            { 
                {"(Gustavo Soares)", "Gustavo Soares"}
            };

            var program = GetTopKPrograms(examples, 1).First();

            Assert.AreEqual("Gustavo Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Gustavo Soares)")) as string);
            Assert.AreEqual("Titus Barik", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "(Titus Barik)")) as string);
        }

        [TestMethod]
        public void TestLearnSubstringTwoExamples() 
        {
            var examples = new Dictionary<string, string> 
            { 
                {"Gustavo Soares", "Soares"}, 
                {"Sumit Gulwani", "Gulwani"},
            };

            var program = GetFirstProgram(examples);

            Assert.AreEqual("Soares", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Gustavo Soares")) as string);
            Assert.AreEqual("Gulwani", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "Sumit Gulwani")) as string);
        }


        public ProgramNode GetFirstProgram(Dictionary<string, string> examples) {
            return GetPrograms(examples).First();
        }

        public IEnumerable<ProgramNode> GetPrograms(Dictionary<string, string> examples) {

            var prose = ConfigureSynthesis(grammar);
            var learnedSet = prose.LearnGrammar(GetSpecification(examples));

            var programs = learnedSet.RealizedPrograms;

            return programs;
        }

        public IEnumerable<ProgramNode> GetTopKPrograms(Dictionary<string, string> examples, int k) {

            var prose = ConfigureSynthesis(grammar);
            var scoreFeature = new RankingScore(grammar);

            var programs = prose.LearnGrammarTopK(GetSpecification(examples), scoreFeature, k, null);

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

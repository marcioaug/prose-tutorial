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

            var program = GetFirstProgram(examples).First();;

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

            var program = GetFirstProgram(examples).First();;

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

            var programs = GetFirstProgram(examples);
            var program = programs.First();

            Assert.AreEqual("16", program.Invoke(State.CreateForExecution(grammar.InputSymbol, "16-Feb-2016")) as string);
            Assert.AreEqual(2, programs.Count());
        }


        public IEnumerable<ProgramNode> GetFirstProgram(Dictionary<string, string> examples) {

            var prose = ConfigureSynthesis(grammar);

            var examplesState = new Dictionary<State, object>();

            foreach (KeyValuePair<string, string> example in examples) 
            {
                var input = State.CreateForExecution(grammar.InputSymbol, example.Key);
                examplesState[input] = example.Value;
            }

            var spec = new ExampleSpec(examplesState);

            var learnedSet = prose.LearnGrammar(spec);

            var programs = learnedSet.RealizedPrograms;
            return programs;
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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.Learning.Strategies;

namespace ProseTutorial
{
    [TestClass]
    public class SubstringTest
    {

        private const string grammarPath = "../../../../ProseTutorial/grammar/substring.grammar";

        [TestMethod]
        public void TestLearnSubstringPositiveAbsPos()
        {

            var grammar = DSLCompiler.ParseGrammarFromFile(grammarPath);
            var prose = ConfigureSynthesis(grammar.Value);

            var input = State.CreateForExecution(grammar.Value.InputSymbol, "19-Feb-1960");
            var examples = new Dictionary<State, object> { {input, "Feb"} };
            var spec = new ExampleSpec(examples);

            var learnedSet = prose.LearnGrammar(spec);

            Console.WriteLine(learnedSet);

            var programs = learnedSet.RealizedPrograms;
            var output = programs.First().Invoke(input) as string;

            Assert.AreEqual("Feb", output);
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

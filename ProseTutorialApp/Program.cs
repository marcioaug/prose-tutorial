﻿using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;

using ProseTutorial;

namespace ProseTutorialApp
{
    class Program
    {
        static Grammar grammar = DSLCompiler.ParseGrammarFromFile("../ProseTutorial/grammar/substring.grammar").Value;

        static SynthesisEngine prose;

        private static Dictionary<State, object> examples = new Dictionary<State, object>();
        private static ProgramNode topProgram;


        static void Main(string[] args)
        {

            prose = ConfigureSynthesis(grammar);

            string menu = @"
Select one of the options: 

    1 - provide new example
    2 - run io synthesized program on a new input
    3 - exit        
                          ";

            int option = 0;

            while (option != 3) {
                Console.Out.WriteLine(menu);

                try {
                    option = Int16.Parse(Console.ReadLine());
                } 
                catch (Exception) {
                    Console.Out.WriteLine("Invalid option. Try again.");
                    continue;
                }

                try {
                    RunOption(option);
                }
                catch (TypeInitializationException e)
                {
                    Console.Error.WriteLine(e.InnerException.Message);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("Something went wrong...");
                    Console.Error.WriteLine("Exception message: " + e.Message);
                }
            }
        }

        private static void RunOption(int option) {
            switch (option)
            {
                case 1:
                    LearnFromNewExample();
                    break;
                case 2:
                    RunOnNewInput();
                    break;
                default:
                    Console.Out.WriteLine("Invalid option. Try again.");
                    break;
            }
        }

        private static void LearnFromNewExample()
        {
            Console.Out.Write("Provide a new input-output example (e.g., \"(Gustavo Soares)\",\"Gustavo Soares\": ");

            try 
            {
                string input = Console.ReadLine();

                var startFirstExample = input.IndexOf("\"") + 1;
                var endFirstExample = input.IndexOf("\"", startFirstExample) - 1;
                var startSecondExample = input.IndexOf("\"", endFirstExample + 2) + 1;
                var endSecondExample = input.IndexOf("\"", startSecondExample) - 1;

                if ((startFirstExample >= endFirstExample) || (startSecondExample >= endSecondExample)) {
                    throw new Exception("Invalid example format. Please try again. input and output should be between quotes");
                }

                var inputExample = input.Substring(startFirstExample, (endFirstExample - startFirstExample) + 1);
                var outputExample = input.Substring(startSecondExample, (endSecondExample - startSecondExample) + 1);

                var inputState = State.CreateForExecution(grammar.InputSymbol, inputExample);
                examples.Add(inputState, outputExample);
                Console.Out.WriteLine("[OK]");

                LearnPrograms();
            } 
            catch(Exception e)
            {
                throw e;
            }
        }

        private static void LearnPrograms()
        {
            var spec = new ExampleSpec(examples);
            
            Console.Out.WriteLine("Learning a program for examples: ");

            foreach (var example in examples) 
            {
                Console.WriteLine("\"" + example.Key.Bindings.First().Value + "\" -> \"" + example.Value + "\"");
            }

            var scoreFeature = new RankingScore(grammar);
            var topPrograms = prose.LearnGrammarTopK(spec, scoreFeature, 4, null);

            if (topPrograms.IsEmpty())
            {
                throw new Exception("No program was found for this specification.");
            }

            topProgram = topPrograms.First();
            Console.Out.WriteLine("Top 4 learned programs: ");
        
            foreach (var program in topPrograms)
            {
                Console.Out.WriteLine("-----------------------------");
                Console.Out.WriteLine(program.PrintAST(Microsoft.ProgramSynthesis.AST.ASTSerializationFormat.HumanReadable));
            }
        }

        private static void RunOnNewInput()
        {
            if (topProgram == null) 
            {
                throw new Exception("No program wa synthesized. Try to provide new examples first.");
            }

            Console.Out.WriteLine("Top program: " + topProgram);

            try
            {
                Console.Out.Write("Insert a new input: ");
                var newInput = Console.ReadLine();

                var startFirstExample = newInput.IndexOf("\"") + 1;
                var endFirstExample = newInput.IndexOf("\"", startFirstExample) - 1;

                newInput = newInput.Substring(startFirstExample, (endFirstExample - startFirstExample) + 1);

                var newInputState = State.CreateForExecution(grammar.InputSymbol, newInput);

                Console.Out.WriteLine("RESULT: \"" + newInput + "\" -> \"" + topProgram.Invoke(newInputState) + "\"");
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                throw new Exception("The execution of the program on this input thrown an exception.");
            }
        } 

        public static SynthesisEngine ConfigureSynthesis(Grammar grammar) {

            var witnessFunctions = new WitnessFunctions(grammar);
            var deductiveSynthesis = new DeductiveSynthesis(witnessFunctions);
            var synthesisExtrategies = new ISynthesisStrategy[] { deductiveSynthesis };
            var synthesisConfig = new SynthesisEngine.Config { Strategies = synthesisExtrategies };

            return new SynthesisEngine(grammar, synthesisConfig);
        }

    }
}

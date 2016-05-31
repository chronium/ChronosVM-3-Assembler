using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Codeaddicts.libArgument;
using Codeaddicts.libArgument.Attributes;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class Args {
        [Argument ("-i", "--input")]
        public string Input;

        [Argument ("-o", "--output")]
        public string Output;
    }

    class Program {
        static void Main (string[] args) {
            var arguments = ArgumentParser.Parse<Args> (args);
            
            if (!File.Exists (arguments.Input)) {
                Console.WriteLine ($"Input file {arguments.Input} does not exist.");
            } else {
                Lexer lexer = new Lexer (File.ReadAllText (arguments.Input), new List<IMatcher> {
                    new MatchSymbol (new List<string> { ",", "(", ")", ".", ":" }),
                    new MatchNumber (),
                    new MatchRegister (Enum.GetNames (typeof (Registers)).ToList ()),
                    new MatchMnemonic (Enum.GetNames (typeof (Instructions)).ToList ()),
                    new MatchString (),
                    new MatchIdentifier (),
                });

                Parser p = new Parser (lexer);
                try {
                    var nodes = p.Parse ();

                    foreach (var v in nodes)
                        Console.WriteLine ($"{v.GetType ().Name}: {v.Visit ()}");

                    new Assembler (arguments.Output).Assemble (nodes);
                } catch (Exception e) {
                    Console.WriteLine (e.Message);
                    Console.Read ();
                    return;
                }
            }

            Console.Read ();
        }
    }
}

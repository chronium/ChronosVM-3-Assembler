using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class MatchString : MatcherBase {
        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            StringBuilder accum = new StringBuilder ();

            if (tokenizer.Consume () != "\"")
                return null;

            while (tokenizer.current != "\"" || tokenizer.current == "\\")
                if (tokenizer.current == "\\") {
                    tokenizer.Consume ();
                    switch (tokenizer.Consume ()) {
                        case "n":
                            accum.Append ('\n');
                            break;
                        case "b":
                            accum.Append ('\b');
                            break;
                        case "r":
                            accum.Append ('\r');
                            break;
                        case "t":
                            accum.Append ('\t');
                            break;
                        case "\"":
                            accum.Append ('\"');
                            break;
                        case "'":
                            accum.Append ('\'');
                            break;
                        case "\\":
                            accum.Append ('\\');
                            break;
                    }
                } else
                    accum.Append (tokenizer.Consume ());


            if (tokenizer.Consume () != "\"")
                return null;

            return new StringLiteral (accum.ToString (), tokenizer.Line, tokenizer.Column);
        }
    }
}

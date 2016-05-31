using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    class MatchIdentifier : MatcherBase {
        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            StringBuilder accum = new StringBuilder ();

            if (tokenizer.current == "_" || char.IsLetter (tokenizer.current[0]))
                accum.Append (tokenizer.Consume ());
            else
                return null;

            while (!tokenizer.End && char.IsLetterOrDigit (tokenizer.current[0]) || tokenizer.current == "_")
                accum.Append (tokenizer.Consume ());

            if (accum.Length > 0)
                return new Identifier (accum.ToString (), tokenizer.Line, tokenizer.Column);
            return null;
        }
    }
}

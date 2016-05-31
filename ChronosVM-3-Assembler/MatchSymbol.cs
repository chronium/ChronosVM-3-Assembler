using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class MatchSymbol : MatcherBase {
        List<string> symbols;

        public MatchSymbol (List<string> symbols) {
            this.symbols = symbols.OrderBy (s => s.Length).ToList ();
            this.symbols.Reverse ();
        }

        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            foreach (var symbol in this.symbols) {
                var peek = tokenizer.PeekMultiple (symbol.Length);
                if (peek == null)
                    return null;
                var token = string.Join("", peek);
                if (token == symbol) {
                    tokenizer.Advance (symbol.Length);
                    return new Symbol (symbol, tokenizer.Line, tokenizer.Column);
                }
            }

            return null;
        }
    }
}

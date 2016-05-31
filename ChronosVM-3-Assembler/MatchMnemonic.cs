using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class MatchMnemonic : MatcherBase {
        List<string> mnemonics;

        public MatchMnemonic (List<string> mnemonics) {
            this.mnemonics = mnemonics.OrderBy (s => s.Length).ToList ();
            this.mnemonics.Reverse ();
        }

        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            foreach (var symbol in this.mnemonics) {
                var peek = tokenizer.PeekMultiple (symbol.Length);
                if (peek == null)
                    continue;
                var token = string.Join ("", peek).ToUpper ();
                if (token == symbol) {
                    tokenizer.Advance (symbol.Length);
                    if (!char.IsLetterOrDigit(tokenizer.current[0]))
                        return new Mnemonic (symbol, tokenizer.Line, tokenizer.Column);
                }
            }

            return null;
        }
    }
}

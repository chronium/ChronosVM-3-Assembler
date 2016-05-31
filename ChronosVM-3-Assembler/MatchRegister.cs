using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class MatchRegister : MatcherBase {
        List<string> registers;

        public MatchRegister (List<string> registers) {
            this.registers = registers.OrderBy (x => x.Length).Reverse ().ToList ();
        }

        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            foreach (var symbol in this.registers) {
                var peek = tokenizer.PeekMultiple (symbol.Length);
                if (peek == null)
                    continue;
                var token = string.Join ("", peek);
                if (token == symbol) {
                    tokenizer.Advance (symbol.Length);
                    return new Register (symbol.ToUpper ().ToEnum<Registers> (), tokenizer.Line, tokenizer.Column);
                }
            }

            return null;
        }
    }
}

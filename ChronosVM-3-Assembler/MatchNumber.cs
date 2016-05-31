using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class MatchNumber : MatcherBase {
        string decn = "0123456789";
        string hexn = "0123456789ABCDEF";

        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            bool hex = tokenizer.current == "$";
            if (hex)
                tokenizer.Advance ();
            
            StringBuilder accum = new StringBuilder ();
            
            while (!tokenizer.End && (hex ? this.hexn : this.decn).Contains (tokenizer.current.ToUpper ()))
                accum.Append (tokenizer.Consume ());

            if (accum.Length > 0)
                return new Number (Convert.ToUInt32 (accum.ToString (), (hex ? 16 : 10)), tokenizer.Line, tokenizer.Column);

            int? temp = null;
            if (tokenizer.Consume ().ToString () == "'") {
                if (tokenizer.current == "\\") {
                    tokenizer.Consume ();
                    switch (tokenizer.Consume ()) {
                        case "n":
                            temp = '\n';
                            break;
                        case "b":
                            temp = '\b';
                            break;
                        case "r":
                            temp = '\r';
                            break;
                        case "t":
                            temp = '\t';
                            break;
                        case "\"":
                            temp = '\"';
                            break;
                        case "'":
                            temp = '\'';
                            break;
                        case "\\":
                            temp = '\\';
                            break;
                    }
                } else
                    temp = tokenizer.Consume ().ToString ()[0];
                tokenizer.Consume ();
            }

            return temp != null ? new Number((uint) (temp != null ? temp.Value : 0), tokenizer.Line, tokenizer.Column) : null;
        }
    }
}

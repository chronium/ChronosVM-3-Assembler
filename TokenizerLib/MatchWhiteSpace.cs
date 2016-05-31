using System;

namespace TokenizerLib {
    class MatchWhiteSpace : MatcherBase {
        protected override Token IsMatchImpl (Tokenizer tokenizer) {
            bool foundWhiteSpace = false;
            

            while (!tokenizer.End && (string.IsNullOrWhiteSpace (tokenizer.current) || tokenizer.current == "\r")) {
                foundWhiteSpace = true;
                if (tokenizer.current == "\n") {
                    tokenizer.Line++;
                    tokenizer.Column = 0;
                }
                tokenizer.Advance ();
            }

            if (foundWhiteSpace)
                return new WhiteSpace (tokenizer.Line, tokenizer.Column);

            return null;
        }
    }
}

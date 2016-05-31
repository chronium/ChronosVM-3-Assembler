using System;
using System.Linq;

namespace TokenizerLib {
    public class Tokenizer : TokenizableStreamBase<string> {
        public Tokenizer (string source)
            : base (() => source.ToCharArray ().Select (i => i.ToString ()).ToList ()) {
        }
    }
}

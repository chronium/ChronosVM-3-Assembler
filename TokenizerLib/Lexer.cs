using System.Collections.Generic;
using System.Linq;

namespace TokenizerLib {
    public class Lexer {
        public Tokenizer Tokenizer { get; private set; }
        public List<IMatcher> Matchers { get; private set; }

        public Lexer (string source, List<IMatcher> matchers) {
            this.Tokenizer = new Tokenizer (source);
            this.Matchers = new List<IMatcher> ();
            this.Matchers.Add (new MatchWhiteSpace ());
            this.Matchers.AddRange (matchers);
        }

        public IEnumerable<Token> Lex () {
            var current = Next ();

            while (current != null && !(current is EOF)) {
                if (!(current is WhiteSpace))
                    yield return current;
                current = Next ();
            }
            yield return new EOF (Tokenizer.Line, Tokenizer.Column);
        }

        private Token Next () {
            if (this.Tokenizer.End) {
                return new EOF (Tokenizer.Line, Tokenizer.Column);
            }

            return
                      (from match in Matchers
                       let token = match.IsMatch (Tokenizer)
                       where token != null
                       select token).FirstOrDefault ();
        }
    }
}

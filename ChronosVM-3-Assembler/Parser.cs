using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    class Parser {
        private ParseableTokenStream TokenStream { get; }

        public Parser (Lexer lexer) {
            this.TokenStream = new ParseableTokenStream (lexer);
        }
       
        public List<BaseNode> Parse () {
            var nodes = new List<BaseNode> ();

            while (!(this.TokenStream.current is EOF)) {
                BaseNode node = this.TokenStream.CaptureNode ();

                if (node != null)
                    nodes.Add (node);

                else
                    throw new Exception ($"Unexpected token: {this.TokenStream.current} at: {this.TokenStream.current.Line + 1}, {this.TokenStream.current.Column}");
            }

            return nodes;
        }
    }
}

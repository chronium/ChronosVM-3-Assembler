using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class ParseableTokenStream : TokenizableStreamBase<Token> {
        public ParseableTokenStream (Lexer lexer)
            : base (() => lexer.Lex ().ToList ()) {
        }

        public T Take<T> () where T : Token {
            return this.current is T ? (T) this.Consume () : null;
        }

        public bool IsSymbol (string symbol) {
            if (this.current is Symbol)
                return this.Consume ().Value == symbol;

            return false;
        }

        public bool IsMnemonic (Instructions mnemonic) {
            if (this.current is Mnemonic)
                return ((Mnemonic) this.Consume ()).Instruction == mnemonic;

            return false;
        }

        public bool Alt (Func<BaseNode> action) {
            this.TakeSnapshot ();
            BaseNode node = null;

            try {
                var currentIndex = this.Index;
                node = action ();

                if (node != null)
                    this.CachedNodes[currentIndex] = new Memo { Node = node, NextIndex = this.Index };
            } catch { }

            this.RollbackSnapshot ();
            return node != null;
        }

        private Dictionary<int, Memo> CachedNodes = new Dictionary<int, Memo> ();

        internal class Memo {
            public BaseNode Node { get; set; }
            public int NextIndex { get; set; }
        }

        public T Capture<T> (Func<BaseNode> node) where T : BaseNode {
            return this.Alt (node) ? (T) this.Get (node) : null;
        }

        public BaseNode Get (Func<BaseNode> node) {
            Memo memo;
            if (!(this.CachedNodes.TryGetValue (this.Index, out memo)))
                return node ();
            this.Advance (memo.NextIndex - this.Index);

            return memo.Node;
        }

        public BaseNode CaptureNode () {
            BaseNode node = LabelNode.Capture (this);
            if (node == null)
                node = MovNode.Capture (this);
            if (node == null)
                node = HaltNode.Capture (this);
            if (node == null)
                node = InbNode.Capture (this);
            if (node == null)
                node = IncNode.Capture (this);
            if (node == null)
                node = JmpNode.Capture (this);
            if (node == null)
                node = DbNode.Capture (this);
            if (node == null)
                node = XorNode.Capture (this);
            if (node == null)
                node = AddNode.Capture (this);
            if (node == null)
                node = RetNode.Capture (this);
            if (node == null)
                node = CallNode.Capture (this);
            if (node == null)
                node = MulNode.Capture (this);
            if (node == null)
                node = JeNode.Capture (this);
            if (node == null)
                node = CmpNode.Capture (this);
            if (node == null)
                node = JlNode.Capture (this);
            if (node == null)
                node = OutbNode.Capture (this);
            if (node == null)
                node = TimesNode.Capture (this);
            if (node == null)
                node = LocalLabelNode.Capture (this);
            if (node == null)
                node = StiNode.Capture (this);
            if (node == null)
                node = CliNode.Capture (this);
            if (node == null)
                node = LdidtNode.Capture (this);
            if (node == null)
                node = IretNode.Capture (this);
            if (node == null)
                node = CalleNode.Capture (this);
            return node;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public abstract class BaseNode {
        public abstract object Visit ();

        public static BaseNode GetValue (ParseableTokenStream tokenStream) {
            BaseNode dest = ValueNode.Capture (tokenStream);
            if (dest == null)
                dest = RegisterNode.Capture (tokenStream);
            if (dest == null)
                dest = IndirectIdentifier.Capture (tokenStream);
            if (dest == null)
                dest = IndirectRegister.Capture (tokenStream);
            if (dest == null)
                dest = IdentifierNode.Capture (tokenStream);
            if (dest == null)
                dest = IndirectIdentifierNode.Capture (tokenStream);
            if (dest == null)
                dest = IndirectValue.Capture (tokenStream);
            return dest;
        }
    }

    public abstract class Node<T> : BaseNode where T : Node<T> {
        public Node () { }


        public static T Capture (ParseableTokenStream tokenStream) {
            return null;
        }
    }

    public abstract class TerminalNode<T, Y> : Node<Y> where T : Token where Y : Node<Y> {
        public T Value { get; }

        public TerminalNode (T value) {
            this.Value = value;
        }

        new public static Y Capture (ParseableTokenStream tokenStream) {
            return null;
        }
    }

    public class ValueNode : TerminalNode<Number, ValueNode> {
        public ValueNode (Number number)
            : base (number) {

        }

        public override object Visit () {
            return this.Value.Val;
        }

        new public static ValueNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<ValueNode> (() => {
                Number tok = tokenStream.Take<Number> ();
                return tok != null ? new ValueNode (tok) : null;
            });
        }
    }

    public class StringNode : TerminalNode<StringLiteral, StringNode> {
        public StringNode (StringLiteral number)
            : base (number) {

        }

        public override object Visit () {
            return this.Value.Value;
        }

        new public static StringNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<StringNode> (() => {
                StringLiteral tok = tokenStream.Take<StringLiteral> ();
                return tok != null ? new StringNode (tok) : null;
            });
        }
    }

    public class IdentifierNode : TerminalNode<Identifier, IdentifierNode> {
        public IdentifierNode (Identifier number)
            : base (number) {

        }

        public override object Visit () {
            return this.Value.Value;
        }

        new public static IdentifierNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IdentifierNode> (() => {
                bool local = false;
                if (tokenStream.IsSymbol ("."))
                    local = true;
                Identifier ident = tokenStream.Take<Identifier> ();
                if (!ident.Value.StartsWith ("."))
                    ident.Value = (local ? "." : "") + ident.Value;
                return ident != null ? new IdentifierNode (ident) : null;
            });
        }
    }

    public class RegisterNode : TerminalNode<Register, RegisterNode> {
        public RegisterNode (Register number)
            : base (number) {

        }

        public override object Visit () {
            return this.Value.register;
        }

        new public static RegisterNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<RegisterNode> (() => {
                Register tok = tokenStream.Take<Register> ();
                return tok != null ? new RegisterNode (tok) : null;
            });
        }
    }

    public abstract class IndirectNode<T, Y> : Node<Y> where T : Node<T> where Y : Node<Y> {
        public T Value { get; }

        public IndirectNode (T value) {
            this.Value = value;
        }

        new public static Y Capture (ParseableTokenStream tokenStream) {
            return null;
        }
    }

    public class IndirectIdentifier : IndirectNode<IdentifierNode, IndirectIdentifier> {
        public IndirectIdentifier (IdentifierNode val)
            : base (val) {
        }
        public override object Visit () {
            return this.Value.Visit ();
        }

        new public static IndirectIdentifier Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IndirectIdentifier> (() => {
                IdentifierNode term = IdentifierNode.Capture (tokenStream);
                if (!tokenStream.IsSymbol (")"))
                    return null;
                return term != null ? new IndirectIdentifier (term) : null;
            });
        }
    }

    public class IndirectValue : IndirectNode<ValueNode, IndirectValue> {
        public IndirectValue (ValueNode val)
            : base (val) {
        }
        public override object Visit () {
            return this.Value.Visit ();
        }

        new public static IndirectValue Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IndirectValue> (() => {
                if (!tokenStream.IsSymbol ("("))
                    return null;
                ValueNode term = ValueNode.Capture (tokenStream);
                if (!tokenStream.IsSymbol (")"))
                    return null;
                return term != null ? new IndirectValue (term) : null;
            });
        }
    }

    public class IndirectRegister : IndirectNode<RegisterNode, IndirectIdentifier> {
        public IndirectRegister (RegisterNode val)
            : base (val) {
        }
        public override object Visit () {
            return this.Value.Visit ();
        }

        new public static IndirectRegister Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IndirectRegister> (() => {
                if (!tokenStream.IsSymbol ("("))
                    return null;
                RegisterNode term = RegisterNode.Capture (tokenStream);
                if (!tokenStream.IsSymbol (")"))
                    return null;
                return term != null ? new IndirectRegister (term) : null;
            });
        }
    }

    public class LabelNode : Node<LabelNode> {
        public string Name { get; }

        public LabelNode (string name) {
            this.Name = name;
        }

        public override object Visit () {
            return this.Name;
        }

        new public static LabelNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<LabelNode> (() => {
                Identifier ident = tokenStream.Take<Identifier> ();
                if (!tokenStream.IsSymbol (":"))
                    return null;
                return ident != null ? new LabelNode (ident.Value) : null;
            });
        }
    }

    public class LocalLabelNode : Node<LocalLabelNode> {
        public string Name { get; }

        public LocalLabelNode (string name) {
            this.Name = name;
        }

        public override object Visit () {
            return this.Name;
        }

        new public static LocalLabelNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<LocalLabelNode> (() => {
                if (!tokenStream.IsSymbol ("."))
                    return null;
                Identifier ident = tokenStream.Take<Identifier> ();
                if (!tokenStream.IsSymbol (":"))
                    return null;
                return ident != null ? new LocalLabelNode ("." + ident.Value) : null;
            });
        }
    }

    public class IndirectIdentifierNode : Node<IndirectIdentifierNode> {
        public string Name { get; }

        public IndirectIdentifierNode (string name) {
            this.Name = name;
        }

        public override object Visit () {
            return this.Name;
        }

        new public static IndirectIdentifierNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IndirectIdentifierNode> (() => {
                if (!tokenStream.IsSymbol ("("))
                    return null;
                bool local = false;
                if (tokenStream.IsSymbol ("."))
                    local = true;
                Identifier ident = tokenStream.Take<Identifier> ();
                if (!tokenStream.IsSymbol (")"))
                    return null;
                return ident != null ? new IndirectIdentifierNode ((local ? "." : "") + ident.Value) : null;
            });
        }
    }

    public class MovNode : Node<MovNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }
        public Size Size { get; }

        public MovNode (BaseNode dest, BaseNode src, Size size) {
            this.Dest = dest;
            this.Src = src;
            this.Size = size;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static MovNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<MovNode> (() => {
                var size = Size.Unknown;
                if (tokenStream.current.Value.ToLower () == "movw")
                    size = Size.Word;
                else if (tokenStream.current.Value.ToLower () == "movd")
                    size = Size.DWord;
                else if (tokenStream.current.Value.ToLower () == "movq")
                    size = Size.QWord;
                else if (tokenStream.current.Value.ToLower () == "mov")
                    size = Size.Unknown;
                else
                    return null;
                tokenStream.Consume ();
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;

                return new MovNode (dest, src, size);
            });
        }
    }

    public class HaltNode : Node<HaltNode> {
        public HaltNode () {
        }

        public override object Visit () {
            return null;
        }

        new public static HaltNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<HaltNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.HLT))
                    return null;
                return new HaltNode ();
            });
        }
    }

    public class RetNode : Node<HaltNode> {
        public RetNode () {
        }

        public override object Visit () {
            return null;
        }

        new public static RetNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<RetNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.RET))
                    return null;
                return new RetNode ();
            });
        }
    }

    public class IretNode : Node<IretNode> {
        public IretNode () {
        }

        public override object Visit () {
            return null;
        }

        new public static IretNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IretNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.IRET))
                    return null;
                return new IretNode ();
            });
        }
    }

    public class StiNode : Node<StiNode> {
        public StiNode () {
        }

        public override object Visit () {
            return null;
        }

        new public static StiNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<StiNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.STI))
                    return null;
                return new StiNode ();
            });
        }
    }

    public class CliNode : Node<CliNode> {
        public CliNode () {
        }

        public override object Visit () {
            return null;
        }

        new public static CliNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<CliNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.CLI))
                    return null;
                return new CliNode ();
            });
        }
    }

    public class LdidtNode : Node<LdidtNode> {
        public BaseNode Dest { get; }

        public LdidtNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static LdidtNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<LdidtNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.LDIDT))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new LdidtNode (dest);
            });
        }
    }

    public class InbNode : Node<InbNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }

        public InbNode (BaseNode dest, BaseNode src) {
            this.Dest = dest;
            this.Src = src;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static InbNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<InbNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.INB))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;
                return new InbNode (dest, src);
            });
        }
    }

    public class OutbNode : Node<OutbNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }

        public OutbNode (BaseNode dest, BaseNode src) {
            this.Dest = dest;
            this.Src = src;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static OutbNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<OutbNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.OUTB))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;
                return new OutbNode (dest, src);
            });
        }
    }

    public class IncNode : Node<IncNode> {
        public BaseNode Dest { get; }

        public IncNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static IncNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<IncNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.INC))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new IncNode (dest);
            });
        }
    }

    public class JmpNode : Node<JmpNode> {
        public BaseNode Dest { get; }

        public JmpNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static JmpNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<JmpNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.JMP))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new JmpNode (dest);
            });
        }
    }

    public class JeNode : Node<JeNode> {
        public BaseNode Dest { get; }

        public JeNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static JeNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<JeNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.JE))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new JeNode (dest);
            });
        }
    }

    public class JlNode : Node<JlNode> {
        public BaseNode Dest { get; }

        public JlNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static JlNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<JlNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.JL))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new JlNode (dest);
            });
        }
    }

    public class CmpNode : Node<CmpNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }

        public CmpNode (BaseNode dest, BaseNode src) {
            this.Dest = dest;
            this.Src = src;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static CmpNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<CmpNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.CMP))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;

                return new CmpNode (dest, src);
            });
        }
    }

    public class DbNode : Node<DbNode> {
        public List<BaseNode> Values { get; }
        public DbNode (List<BaseNode> values) {
            this.Values = values;               
        }

        public override object Visit () {
            List<string> vals = new List<string> ();
            foreach (BaseNode node in this.Values)
                vals.Add ($"{node.Visit ()}");

            return string.Join (", ", vals);
        }

        new public static DbNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<DbNode> (() => {
                if (tokenStream.Consume().Value.ToLower () != "db")
                    return null;
                List<BaseNode> values = new List<BaseNode> ();
                do {
                    BaseNode val = StringNode.Capture (tokenStream);
                    if (val == null)
                        val = ValueNode.Capture (tokenStream);
                    values.Add (val);
                } while (tokenStream.IsSymbol (","));
                return values.Count > 0 ? new DbNode (values) : null;
            });
        }
    }

    public class XorNode : Node<XorNode> {
        public BaseNode First { get; }
        public BaseNode Second { get; }

        public XorNode (BaseNode first, BaseNode second = null) {
            this.First = first;
            this.Second = second;
        }

        public override object Visit () {
            return $"{this.First.Visit ()}" + (this.Second != null ? $", {this.Second.Visit ()}" : "");
        }

        new public static XorNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<XorNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.XOR))
                    return null;
                BaseNode first = BaseNode.GetValue (tokenStream);
                if (!tokenStream.IsSymbol (","))
                    return new XorNode (first);
                BaseNode second = BaseNode.GetValue (tokenStream);
                return first != null && second != null ? new XorNode (first, second) : null;
            });
        }
    }

    public class AddNode : Node<AddNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }

        public AddNode (BaseNode dest, BaseNode src) {
            this.Dest = dest;
            this.Src = src;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static AddNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<AddNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.ADD))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;

                return new AddNode (dest, src);
            });
        }
    }

    public class MulNode : Node<MulNode> {
        public BaseNode Dest { get; }
        public BaseNode Src { get; }
        public Size Size { get; }

        public MulNode (BaseNode dest, BaseNode src) {
            this.Dest = dest;
            this.Src = src;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}, {this.Src.Visit ()}";
        }

        new public static MulNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<MulNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.MUL))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                if (!tokenStream.IsSymbol (","))
                    return null;
                BaseNode src = BaseNode.GetValue (tokenStream);
                if (src == null)
                    return null;

                return new MulNode (dest, src);
            });
        }
    }

    public class CallNode : Node<CallNode> {
        public BaseNode Dest { get; }

        public CallNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static CallNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<CallNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.CALL))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new CallNode (dest);
            });
        }
    }

    public class CalleNode : Node<CalleNode> {
        public BaseNode Dest { get; }

        public CalleNode (BaseNode dest) {
            this.Dest = dest;
        }

        public override object Visit () {
            return $"{this.Dest.Visit ()}";
        }

        new public static CalleNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<CalleNode> (() => {
                if (!tokenStream.IsMnemonic (Instructions.CALLE))
                    return null;
                BaseNode dest = BaseNode.GetValue (tokenStream);
                if (dest == null)
                    return null;
                return new CalleNode (dest);
            });
        }
    }

    public class TimesNode : Node<TimesNode> {
        public BaseNode Node { get; }
        public int Count { get; }

        public TimesNode (BaseNode node, int count) {
            this.Node = node;
            this.Count = count;
        }
        
        public override object Visit () {
            return $"{this.Count}: {this.Node.GetType ().Name} {this.Node.Visit ()}";
        }

        new public static TimesNode Capture (ParseableTokenStream tokenStream) {
            return tokenStream.Capture<TimesNode> (() => {
                if (tokenStream.Consume ().Value.ToLower() != "times")
                    return null;
                ValueNode value = ValueNode.Capture (tokenStream);
                BaseNode node = tokenStream.CaptureNode ();
                if (node == null)
                    return null;
                return new TimesNode (node, (int) value.Value.Val);
            });
        }
    }
}

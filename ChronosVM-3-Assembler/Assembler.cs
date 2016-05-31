using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronosVM_3_Assembler {
    public class Assembler {
        public BinaryWriter Writer;
        List<Memo> Memos = new List<Memo>();
        public List<byte> Bytes = new List<byte> ();
        public SymbolTable SymbolTable { get; } = new SymbolTable ();

        public long position {
            get {
                return this.Bytes.Count;
            }
        }

        public Assembler (string output) {
            this.Writer = new BinaryWriter (new FileStream (output, FileMode.Create));
            this.SymbolTable.PushScope ();
        }

        public void AssembleNode (BaseNode node) {
            switch (node) {
                case MovNode n:
                    this.Emit (new Mov (this), n);
                    break;
                case HaltNode n:
                    this.Emit (new Hlt (this), n);
                    break;
                case InbNode n:
                    this.Emit (new Inb (this), n);
                    break;
                case IncNode n:
                    this.Emit (new Inc (this), n);
                    break;
                case JmpNode n:
                    this.Emit (new Jmp (this), n);
                    break;
                case LabelNode n:
                    this.TryFillMemos ();
                    this.SymbolTable.ReturnGlobal ();
                    this.SymbolTable[n.Name] = (uint) this.position;
                    break;
                case LocalLabelNode n:
                    this.SymbolTable.PushScope ();
                    this.SymbolTable[n.Name] = (uint) this.position;
                    break;
                case DbNode n:
                    foreach (var v in n.Values)
                        switch (v) {
                            case StringNode str:
                                this.Bytes.AddRange (Encoding.ASCII.GetBytes (str.Value.Value));
                                break;
                            case ValueNode val:
                                this.Bytes.Add ((byte) val.Value.Val);
                                break;
                        }
                    break;
                case XorNode n:
                    this.Emit (new Xor (this), n);
                    break;
                case AddNode n:
                    this.Emit (new Add (this), n);
                    break;
                case MulNode n:
                    this.Emit (new Mul (this), n);
                    break;
                case CallNode n:
                    this.Emit (new Call (this), n);
                    break;
                case RetNode n:
                    this.Emit (new Ret (this), n);
                    break;
                case CmpNode n:
                    this.Emit (new Cmp (this), n);
                    break;
                case JeNode n:
                    this.Emit (new Je (this), n);
                    break;
                case JlNode n:
                    this.Emit (new Jl (this), n);
                    break;
                case OutbNode n:
                    this.Emit (new Outb (this), n);
                    break;
                case CliNode n:
                    this.Emit (new Cli (this), n);
                    break;
                case StiNode n:
                    this.Emit (new Sti (this), n);
                    break;
                case LdidtNode n:
                    this.Emit (new Ldidt (this), n);
                    break;
                case IretNode n:
                    this.Emit (new Iret (this), n);
                    break;
                case CalleNode n:
                    this.Emit (new Calle (this), n);
                    break;
                case TimesNode n:
                    for (int i = 0; i < n.Count; i++)
                        this.AssembleNode (n.Node);
                    break;
                default:
                    Console.WriteLine ("Node: " + node.GetType ().Name + " it ain't implemented");
                    break;
            }
        }

        public void Assemble (List<BaseNode> nodes) {
            foreach (var node in nodes)
                this.AssembleNode (node);

            TryFillMemos ();

            this.Writer.Write (this.Bytes.ToArray ());
        }

        public void TryFillMemos () {
            List<Memo> temp = new List<Memo> ();
            foreach (Memo m in this.Memos) {
                if (this.SymbolTable.Contains (m.label)) {
                    var val = this.SymbolTable[m.label];
                    int i = (int) m.location;
                    var sec = (val & 0xFFFF);
                    this.Bytes[i++] = (byte) (sec & 0x00FF);
                    this.Bytes[i++] = (byte) ((sec & 0xFF00) >> 8);
                    var first = (val & 0xFFFF0000) >> 16;
                    this.Bytes[i++] = (byte) (first & 0x00FF);
                    this.Bytes[i++] = (byte) ((first & 0xFF00) >> 8);
                } else temp.Add (m);
            }
            this.Memos = temp;
        }

        public void Emit (Instruction instruction, BaseNode node) {
            this.Bytes.AddRange (instruction.Emit (node));
        }

        public void AddMemo (int offset, string label) {
            this.Memos.Add (new Memo { label = label, location = this.position + offset });
        }

        internal class Memo {
            public long location;
            public string label;
        }
    }

    public abstract class Instruction {
        public List<byte> Bytes;
        private Assembler assembler;

        public Instruction (Assembler assembler) {
            this.Bytes = new List<byte> ();
            this.assembler = assembler;
        }

        public abstract List<byte> Emit (BaseNode node);

        public void SetWord (byte value) {
            this.Bytes.Add (value);
        }

        public AddressingMode GetMode (BaseNode n1, BaseNode n2) {
            switch (n1) {
                case ValueNode dest:
                    switch (n2) {
                        case ValueNode src:
                            return AddressingMode.ImmediateImmediate;
                    }
                    break;
                case IndirectValue dest:
                    switch (n2) {
                        case ValueNode src:
                            return AddressingMode.IndirectImmediate;
                    }
                    break;
                case RegisterNode dest:
                    switch (n2) {
                        case ValueNode src:
                            return AddressingMode.RegisterImmediate;
                        case RegisterNode src:
                            return AddressingMode.RegisterRegister;
                        case IndirectRegister src:
                            return AddressingMode.RegisterRindirect;
                        case LabelNode src:
                            return AddressingMode.RegisterImmediate;
                        case IdentifierNode src:
                            return AddressingMode.RegisterImmediate;
                        case IndirectIdentifier src:
                            return AddressingMode.RIndirectIndirect;
                    }
                    break;
                case IndirectRegister dest:
                    switch (n2) {
                        case ValueNode src:
                            return AddressingMode.RIndirectImmediate;
                        case IndirectIdentifierNode src:
                            return AddressingMode.RIndirectIndirect;
                        case RegisterNode src:
                            return AddressingMode.RIndirectRegister;
                        case IndirectIdentifier src:
                            return AddressingMode.RIndirectIndirect;
                        case IdentifierNode src:
                            return AddressingMode.RIndirectImmediate;
                    }
                    break;
                case IndirectIdentifier dest:
                    switch (n2) {
                        case ValueNode src:
                            return AddressingMode.IndirectImmediate;
                        case RegisterNode src:
                            return AddressingMode.IndirectRegister;
                    }
                    break;
            }

            return AddressingMode.Unknown;
        }

        public AddressingMode GetMode (BaseNode n) {
            switch (n) {
                case ValueNode node:
                    return AddressingMode.Immediate;
                case RegisterNode node:
                    return AddressingMode.Register;
                case IndirectIdentifier node:
                    return AddressingMode.Indirect;
                case IndirectRegister node:
                    return AddressingMode.RIndirect;
                case IdentifierNode node:
                    return AddressingMode.Immediate;
            }
            return AddressingMode.Unknown;
        }

        public Size GetSize (BaseNode n) {
            switch (n) {
                case ValueNode node: 
                    {
                        var val = node.Value.Val;
                        if (val <= byte.MaxValue)
                            return Size.Word;
                        else if (val <= ushort.MaxValue)
                            return Size.DWord;
                        else if (val <= uint.MaxValue)
                            return Size.QWord;
                        break;
                    }
                case RegisterNode node: 
                    {
                        return Size.QWord;
                    }
                case IndirectRegister reg:
                    return Size.QWord;
                case IdentifierNode label:
                    return Size.QWord;
                case IndirectIdentifierNode label:
                    return Size.QWord;
            }
            return Size.Unknown;
        }

        public void SetValue (ValueNode node) {
            var val = node.Value.Val;
            switch (this.GetSize (node)) {
                case Size.Word:
                    this.Bytes.Add ((byte) val);
                    break;
                case Size.DWord:
                    this.Bytes.Add ((byte) (val & 0x00FF));
                    this.Bytes.Add ((byte) ((val & 0xFF00) >> 8));
                    break;
                case Size.QWord:
                    var sec = (val & 0xFFFF);
                    this.Bytes.Add ((byte) (sec & 0x00FF));
                    this.Bytes.Add ((byte) ((sec & 0xFF00) >> 8));
                    var first = (val & 0xFFFF0000) >> 16;
                    this.Bytes.Add ((byte) (first & 0x00FF));
                    this.Bytes.Add ((byte) ((first & 0xFF00) >> 8));
                    break;
            }
        }

        public void SetArg (BaseNode node) {
            switch (node) {
                case RegisterNode reg:
                    this.SetWord ((byte) reg.Visit ());
                    break;
                case IndirectRegister reg:
                    this.SetWord ((byte) reg.Visit ());
                    break;
                case ValueNode val:
                    this.SetValue ((ValueNode) node);
                    break;
                case IdentifierNode val:
                    this.assembler.AddMemo (this.Bytes.Count, val.Value.Value);
                    this.Bytes.AddRange (new byte[] { 0, 0, 0, 0 });
                    break;
                case IndirectIdentifierNode val:
                    this.assembler.AddMemo (this.Bytes.Count, val.Name);
                    this.Bytes.AddRange (new byte[] { 0, 0, 0, 0 });
                    break;
                case IndirectIdentifier val:
                    this.assembler.AddMemo (this.Bytes.Count, val.Value.Value.Value);
                    this.Bytes.AddRange (new byte[] { 0, 0, 0, 0 });
                    break;
                case IndirectValue val:
                    var value = val.Value.Value.Val;
                    var sec = (value & 0xFFFF);
                    this.Bytes.Add ((byte) (sec & 0x00FF));
                    this.Bytes.Add ((byte) ((sec & 0xFF00) >> 8));
                    var first = (value & 0xFFFF0000) >> 16;
                    this.Bytes.Add ((byte) (first & 0x00FF));
                    this.Bytes.Add ((byte) ((first & 0xFF00) >> 8));
                    break;
                default:
                    Console.WriteLine ("DEAD!");
                    break;
            }
        }
    }

    public class Mov : Instruction {
        public Mov (Assembler assembler)
            : base (assembler) { }
        
        public override List<byte> Emit (BaseNode node) {
            var mov = (MovNode) node;
            this.SetWord ((byte) Instructions.MOVW);
            this.SetWord ((byte) this.GetMode (mov.Dest, mov.Src));
            this.SetWord ((byte) (mov.Size != Size.Unknown ? mov.Size : this.GetSize (mov.Src)));
            this.SetArg (mov.Dest);
            this.SetArg (mov.Src);
            return this.Bytes;
        }
    }

    public class Inb : Instruction {
        public Inb (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (InbNode) node;
            this.SetWord ((byte) Instructions.INB);
            this.SetWord ((byte) this.GetMode (inb.Dest, inb.Src));
            this.SetArg (inb.Dest);
            this.SetArg (inb.Src);

            return this.Bytes;
        }
    }

    public class Outb : Instruction {
        public Outb (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (OutbNode) node;
            this.SetWord ((byte) Instructions.OUTB);
            this.SetWord ((byte) this.GetMode (inb.Dest, inb.Src));
            this.SetArg (inb.Dest);
            this.SetArg (inb.Src);

            return this.Bytes;
        }
    }

    public class Inc : Instruction {
        public Inc (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (IncNode) node;
            this.SetWord ((byte) Instructions.INC);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public class Jmp : Instruction {
        public Jmp (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (JmpNode) node;
            this.SetWord ((byte) Instructions.JMP);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public class Hlt : Instruction {
        public Hlt (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            this.SetWord ((byte) Instructions.HLT);
            return this.Bytes;
        }
    }

    public class Xor : Instruction {
        public Xor (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var xor = (XorNode) node;
            this.SetWord ((byte) Instructions.XOR);
            this.SetWord ((byte) (xor.Second == null ? this.GetMode (xor.First) : this.GetMode (xor.First, xor.Second)));
            this.SetArg (xor.First);
            if (xor.Second != null)
                this.SetArg (xor.Second);
            return this.Bytes;
        }
    }

    public class Add : Instruction {
        public Add (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var mov = (AddNode) node;
            this.SetWord ((byte) Instructions.ADD);
            this.SetWord ((byte) this.GetMode (mov.Dest, mov.Src));
            this.SetWord ((byte) this.GetSize (mov.Src));
            this.SetArg (mov.Dest);
            this.SetArg (mov.Src);
            return this.Bytes;
        }
    }

    public class Mul : Instruction {
        public Mul (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var mov = (MulNode) node;
            this.SetWord ((byte) Instructions.MUL);
            this.SetWord ((byte) this.GetMode (mov.Dest, mov.Src));
            this.SetWord ((byte) this.GetSize (mov.Src));
            this.SetArg (mov.Dest);
            this.SetArg (mov.Src);
            return this.Bytes;
        }
    }

    public class Call : Instruction {
        public Call (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (CallNode) node;
            this.SetWord ((byte) Instructions.CALL);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public class Calle : Instruction {
        public Calle (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (CalleNode) node;
            this.SetWord ((byte) Instructions.CALLE);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public class Ret : Instruction {
        public Ret (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            this.SetWord ((byte) Instructions.RET);
            return this.Bytes;
        }
    }

    public class Iret : Instruction {
        public Iret (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            this.SetWord ((byte) Instructions.IRET);
            return this.Bytes;
        }
    }

    public class Sti : Instruction {
        public Sti (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            this.SetWord ((byte) Instructions.STI);
            return this.Bytes;
        }
    }

    public class Cli : Instruction {
        public Cli (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            this.SetWord ((byte) Instructions.CLI);
            return this.Bytes;
        }
    }

    public class Ldidt : Instruction {
        public Ldidt (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var ldidt = (LdidtNode) node;
            this.SetWord ((byte) Instructions.LDIDT);
            this.SetWord ((byte) this.GetMode (ldidt.Dest));
            this.SetArg (ldidt.Dest);
            return this.Bytes;
        }
    }

    public class Cmp : Instruction {
        public Cmp (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var mov = (CmpNode) node;
            this.SetWord ((byte) Instructions.CMP);
            this.SetWord ((byte) this.GetMode (mov.Dest, mov.Src));
            this.SetWord ((byte) this.GetSize (mov.Src));
            this.SetArg (mov.Dest);
            this.SetArg (mov.Src);
            return this.Bytes;
        }
    }

    public class Je : Instruction {
        public Je (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (JeNode) node;
            this.SetWord ((byte) Instructions.JE);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public class Jl : Instruction {
        public Jl (Assembler assembler)
            : base (assembler) { }

        public override List<byte> Emit (BaseNode node) {
            var inb = (JlNode) node;
            this.SetWord ((byte) Instructions.JL);
            this.SetWord ((byte) this.GetMode (inb.Dest));
            this.SetArg (inb.Dest);

            return this.Bytes;
        }
    }

    public enum AddressingMode : byte {
        ImmediateImmediate,
        ImmediateRegister,
        ImmediateIndirect,
        ImmediateRindirect,
        RegisterImmediate,
        RegisterRegister,
        RegisterIndirect,
        RegisterRindirect,
        IndirectImmediate,
        IndirectRegister,
        IndirectIndirect,
        IndirectRIndirect,
        RIndirectImmediate,
        RIndirectRegister,
        RIndirectIndirect,
        RIndirectRIndirect,
        Immediate,
        Register,
        Indirect,
        RIndirect,
        Unknown
    }

    public enum Size : byte {
        Word = 0b00,
        DWord = 0b01,
        QWord = 0b10,
        Unknown
    }
}

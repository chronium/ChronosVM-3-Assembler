using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TokenizerLib;

namespace ChronosVM_3_Assembler {
    public class Symbol : Token {
        public Symbol (string val, int line, int column) : base (val, line, column) {
        }
    }

    public class Number : Token {
        public uint Val;

        public Number (uint val, int line, int column) : base (val.ToString (), line, column) {
            this.Val = val;
        }
    }

    public class Identifier : Token {
        public Identifier (string val, int line, int column) : base (val, line, column) {

        }
    }

    public class Register : Token {
        public Registers register;

        public Register (Registers register, int line, int column) : base (register.ToString (), line, column) {
            this.register = register;
        }
    }

    public class Mnemonic : Token {
        public Instructions Instruction;

        public Mnemonic (string symbol, int line, int column) : base (symbol, line, column) {
            this.Instruction = symbol.ToUpper ().ToEnum<Instructions> ();
        }
    }

    public class StringLiteral : Token {
        public StringLiteral (string value, int line, int column) : base (value, line, column) {
        }
    }

    public enum Registers : byte {
        A, AH, AL, AHH, AHL, ALH, ALL,
        B, BH, BL, BHH, BHL, BLH, BLL,
        C, CH, CL, CHH, CHL, CLH, CLL,
        D, DH, DL, DHH, DHL, DLH, DLL,
        E, EH, EL, EHH, EHL, ELH, ELL,
        F, FH, FL, FHH, FHL, FLH, FLL,
        W, WH, WL, WHH, WHL, WLH, WLL,
        X, XH, XL, XHH, XHL, XLH, XLL,
        Y, YH, YL, YHH, YHL, YLH, YLL,
        Z, ZH, ZL, ZHH, ZHL, ZLH, ZLL,

        FLAGS, CLOCKS,
        CS, DS, SS,
        PC, SP, BP
    }

    public enum Instructions : byte {
        NOP,
        MOVW = 1,
        MOVD = 1,
        MOVQ = 1,
        CMP,
        CMPS,
        JMP, JE, JNE, JG, JGE, JL, JLE,
        CALL, CALLE, CALLNE, CALLG, CALLGE, CALLL, CALLLE,
        INT,
        RET, IRET,
        CLI, STI,
        INC, DEC,
        ADD, SUB, MUL, DIV, MOD,
        NOT, AND, OR, XOR, SHL, SHR,
        PUSH, POP, PUSHA, POPA,
        ADDS, SUBS, MULS, DIVS, MODS,
        NOTS, ANDS, ORS, XORS, SHLS, SHRS,
        MMSET, MMCPY,
        INB, INW, INQ,
        OUTB, OUTW, OUTQ,
        LDIDT,
        HLT
    }
}

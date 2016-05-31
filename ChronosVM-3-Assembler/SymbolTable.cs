using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronosVM_3_Assembler {
    public class SymbolTable {
        Stack<Scope> Scopes = new Stack<Scope> ();

        public bool IsGlobalScope {
            get {
                return this.Scopes.Count == 1;
            }
        }
        public int MisteryScopeCount { get; set; } = 0;

        public void DeclareSymbol (string name, uint address) {
            this.Scopes.Peek ().Symbols.Add (new Symbol { Address = address, Name = name });
        }

        public void PushScope () {
            this.PushScope (this.MisteryScopeCount.ToString ());
        }

        public void PushScope (string name) {
            this.Scopes.Push (new Scope { Name = name });
        }

        public void PopScope () {
            if (!this.IsGlobalScope)
                this.Scopes.Pop ();
        }

        public void ReturnGlobal () {
            while (!this.IsGlobalScope)
                this.Scopes.Pop ();
        }

        public uint this[string name] {
            get {
                foreach (Scope scope in this.Scopes)
                    foreach (Symbol symbol in scope.Symbols)
                        if (symbol.Name == name)
                            return symbol.Address;
                throw new Exception ($"Symbol {name} not found in the current scope");
            }
            set {
                this.DeclareSymbol (name, value);
            }
        }

        public bool Contains (string name) {
            foreach (Scope scope in this.Scopes)
                foreach (Symbol symbol in scope.Symbols)
                    if (symbol.Name == name)
                        return true;
            return false;
        }

        public string ToScopedName (string name) {
            StringBuilder builder = new StringBuilder ();
            foreach (Scope scope in this.Scopes) {
                builder.Append (scope.Name);
                builder.Append ("_");
            }
            return builder.ToString ();
        }

        internal class Scope {
            public string Name;
            public List<Symbol> Symbols = new List<Symbol> ();

            public override string ToString () {
                return this.Name;
            }
        }
        internal class Symbol {
            public string Name;
            public uint Address;

            public override string ToString () {
                return this.Name;
            }
        }
    }
}

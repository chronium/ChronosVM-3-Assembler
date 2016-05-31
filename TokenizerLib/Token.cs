namespace TokenizerLib {
    public class Token {
        public string Value { get; set; }
        public int Line { get; }
        public int Column { get; }

        public Token (string val, int line, int column) {
            this.Value = val;
            this.Line = line;
            this.Column = column;
        }

        public override string ToString () {
            return this.Value;
        }

        public override bool Equals (object obj) {

            if (obj == null || GetType () != obj.GetType ()) {
                return false;
            }

            if (((Token) obj).Value.GetHashCode () == this.Value.GetHashCode ())
                return true;

            return base.Equals (obj);
        }

        public override int GetHashCode () {
            return $"{this.Value}--{this.Line}--{this.Column}".GetHashCode ();
        }
    }

    public class EOF : Token {
        public EOF (int line, int column)
            : base ("", line, column) {
        }
    }

    public class WhiteSpace : Token {
        public WhiteSpace (int line, int column)
            : base (" ", line, column) {
        }
    }
}

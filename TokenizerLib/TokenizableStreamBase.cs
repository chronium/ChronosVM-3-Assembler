using System;
using System.Collections.Generic;

namespace TokenizerLib {
    public class TokenizableStreamBase<T> where T : class {
        public TokenizableStreamBase (Func<List<T>> extractor) {
            this.Index = 0;
            this.Items = extractor ();
            this.SnapshotIndexes = new Stack<Snapshot> ();
        }

        public int Index { get; private set; }
        private List<T> Items { get; set; }
        private Stack<Snapshot> SnapshotIndexes { get; set; }

        internal class Snapshot {
            public int Index;
            public int Line;
            public int Column;
        }

        public int Line { get; set; }
        public int Column { get; set; }

        public virtual T current {
            get {
                if (EOF (0))
                    return null;

                return this.Items[this.Index];
            }
        }

        public bool End {
            get {
                return EOF (0);
            }
        }

        public T Consume () {
            this.Column++;
            return this.Items[this.Index++];
        }

        public void Advance (int num = 1) {
            this.Column += num;
            this.Index += num;
        }

        private bool EOF (int lookahead) {
            if (this.Index + lookahead >= this.Items.Count)
                return true;
            return false;
        }

        public virtual T Peek (int lookahead) {
            if (EOF (lookahead))
                return null;
            return this.Items[this.Index + lookahead];
        }

        public virtual List<T> PeekMultiple (int len) {
            List<T> items = new List<T> ();
            int i = 0;
            do {
                if (EOF (i))
                    return null;
                items.Add (this.Items[this.Index + i++]);
                len--;
            } while (len > 0);
            return items;
        }

        public void TakeSnapshot () {
            this.SnapshotIndexes.Push (new Snapshot { Index = this.Index, Line = this.Line, Column = this.Column });
        }

        public void RollbackSnapshot () {
            var snapshot = this.SnapshotIndexes.Pop ();
            this.Line = snapshot.Line;
            this.Index = snapshot.Index;
            this.Column = snapshot.Column;
        }

        public void CommitSnapshot () {
            this.SnapshotIndexes.Pop ();
        }
    }
}

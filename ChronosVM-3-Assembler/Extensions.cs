using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChronosVM_3_Assembler {
    public static class Extensions {
        public static T ToEnum<T> (this string value) {
            return (T) Enum.Parse (typeof (T), value, true);
        }
    }
}

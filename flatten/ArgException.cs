using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace flatten {

    class ArgException : Exception {
        public ArgException(string message) : base(message) {
        }
    }
}

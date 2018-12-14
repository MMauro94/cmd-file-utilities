using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace flatten {

    class DupsException: Exception {
        public readonly string filename;
        public DupsException(string filename) {
            this.filename = filename;
        }
    }
}

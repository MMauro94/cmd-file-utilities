using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace flatten {

    public class FileSystemInfoEqualityComparer : IEqualityComparer<FileSystemInfo> {
        public bool Equals(FileSystemInfo x, FileSystemInfo y) {
            return x.FullName.Equals(y.FullName);
        }

        public int GetHashCode(FileSystemInfo obj) {
            return obj.FullName.GetHashCode();
        }
    }
}

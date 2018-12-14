using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace flatten {
    class Plan {

        //Map of old file => new file
        private IDictionary<FileSystemInfo, FileSystemInfo> renamePlan = new Dictionary<FileSystemInfo, FileSystemInfo>(new FileSystemInfoEqualityComparer());
        //Map of items to remove
        private ISet<FileSystemInfo> deletePlan = new HashSet<FileSystemInfo>(new FileSystemInfoEqualityComparer());

        private readonly Duplicates dups;
        private readonly bool keepFolders, recursive;

        public Plan(Duplicates dups, bool keepFolders, bool recursive) {
            this.dups = dups;
            this.keepFolders = keepFolders;
            this.recursive = recursive;
        }

        /// <summary>
        /// Fills the current plan by calculating the actions that need to be performed in the specified path
        /// </summary>
        /// <param name="path">the path to prepare</param>
        public void prepare(DirectoryInfo path) {
            //Initialize temp rename plan
            //Map of new file path => old file path list
            IDictionary<FileSystemInfo, List<FileSystemInfo>> tempRenamePlan = new Dictionary<FileSystemInfo, List<FileSystemInfo>>(new FileSystemInfoEqualityComparer());

            //I need to know if there are already other 
            FileInfo[] initialFiles = path.GetFiles();

            //List and iterate all top level directories and add them as subdir
            DirectoryInfo[] directories = path.GetDirectories();
            foreach (DirectoryInfo dir in directories) {
                addSubDir(delegate (FileSystemInfo flattenedFile, FileSystemInfo originalFile) {
                    //Add to rename plan (duplicates will be accounted for later)
                    if (!tempRenamePlan.ContainsKey(flattenedFile)) {
                        tempRenamePlan.Add(flattenedFile, new List<FileSystemInfo>());
                    }
                    tempRenamePlan[flattenedFile].Add(originalFile);
                }, path, dir);
            }

            //At this point we have a rename plan that COULD contain duplicates
            //Let's detect them and act accordingly

            foreach (KeyValuePair<FileSystemInfo, List<FileSystemInfo>> entry in tempRenamePlan) {
                if (entry.Value.Count > 1 || Array.IndexOf(initialFiles, entry.Value) >= 0) {
                    //If we have duplicates
                    switch (dups) {
                        case Duplicates.Keep:
                            //Just don't add to the real rename plan
                            //Also, we need to cancel the deleting of all above directories of all the dups
                            foreach (FileInfo file in entry.Value) {
                                DirectoryInfo p = file.Directory;
                                while (!path.FullName.Equals(p.FullName)) {
                                    deletePlan.Remove(p);
                                    p = p.Parent;
                                }
                            }
                            break;
                        case Duplicates.Delete:
                            //I need to add to the delete plan all old files
                            deletePlan.UnionWith(entry.Value);
                            break;
                        case Duplicates.Rename:
                            //Add to the rename plan, but with a progressive incrementation
                            int progressive = 1;
                            if (entry.Key is FileInfo) {
                                foreach (FileInfo fsi in entry.Value) {
                                    renamePlan.Add(fsi, new FileInfo(Path.Combine(((FileInfo)entry.Key).Directory.FullName, Path.GetFileNameWithoutExtension(entry.Key.Name) + "_" + progressive + Path.GetExtension(entry.Key.Name))));
                                    progressive++;
                                }
                            } else if (entry.Key is DirectoryInfo) {
                                foreach (DirectoryInfo fsi in entry.Value) {
                                    renamePlan.Add(fsi, new DirectoryInfo(Path.Combine(((DirectoryInfo)entry.Key).Parent.FullName, entry.Key.Name + "_" + progressive)));
                                    progressive++;
                                }
                            } else {
                                throw new Exception("Unknown FileSystemInfo encountered");
                            }
                            break;
                        case Duplicates.Throw:
                            throw new DupsException(entry.Key.Name);
                    }
                } else {
                    //I don't have duplicates, just add to the rename plan
                    renamePlan.Add(entry.Value[0], entry.Key);
                }
            }
        }

        /// <summary>
        /// This method will calculate the actions to perform for this sub dir
        /// </summary>
        /// <param name="subdir">the subdir to add</param>
        private void addSubDir(Action<FileSystemInfo, FileSystemInfo> addFlattened, DirectoryInfo path, DirectoryInfo subdir) {
            //For each file in subdir
            foreach (FileInfo file in subdir.GetFiles()) {
                //Calculate flattened path
                FileInfo flattenedFile = new FileInfo(Path.Combine(path.FullName, file.Name));
                addFlattened(flattenedFile, file);
            }

            //Detect subdirs of this subdir
            DirectoryInfo[] subdirs = subdir.GetDirectories();
            if (recursive) {
                //If recursive is enabled, add each dir as subdir
                foreach (DirectoryInfo dir in subdirs) {
                    addSubDir(addFlattened, path, dir);
                }
            } else if (subdirs.Length > 0) {
                //Also, I need to move this directory to the top level
                foreach (DirectoryInfo dir in subdirs) {
                    //Calculate flattened path
                    DirectoryInfo flattenedDir = new DirectoryInfo(Path.Combine(path.FullName, dir.Name));
                    addFlattened(flattenedDir, dir);
                }
            }
            if (!this.keepFolders) {
                //If don't have to keep the empty folders
                deletePlan.Add(subdir);
            }
        }

        public void execute() {
            //First of all, let's rename the files
            foreach (KeyValuePair<FileSystemInfo, FileSystemInfo> entry in renamePlan) {
                Console.WriteLine("Moving " + entry.Key.FullName + " to " + entry.Value.FullName);
                if (entry.Key is FileInfo && entry.Value is FileInfo) {
                    File.Move(entry.Key.FullName, entry.Value.FullName);
                } else if (entry.Key is DirectoryInfo && entry.Value is DirectoryInfo) {
                    Directory.Move(entry.Key.FullName, entry.Value.FullName);
                } else {
                    throw new Exception("FileSystemInfo mismatch in rename plan");
                }
            }

            //Then let's remove the files
            foreach (FileInfo f in deletePlan.Where(x => x is FileInfo)) {
                Console.WriteLine("Deleting " + f.FullName);
                File.Delete(f.FullName);
            }

            //And finally, the directories
            foreach (DirectoryInfo d in deletePlan.Where(x => x is DirectoryInfo)) {
                Console.WriteLine("Deleting " + d.FullName);
                Directory.Delete(d.FullName);
            }
        }
    }
}

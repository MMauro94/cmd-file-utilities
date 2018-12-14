using System;
using System.Collections.Generic;
using System.IO;

namespace flatten {

    class Program {

        const int EXIT_CODE_OK = 0;
        const int EXIT_CODE_INVALID_ARG = -1;
        const int EXIT_CODE_IO_ERROR = -2;
        const int EXIT_CODE_DUPS = -2;

        public static Duplicates detectDuplicates(List<string> args) {
            if (args.Remove("keepdups")) {
                return Duplicates.Keep;
            } else if (args.Remove("deletedups")) {
                return Duplicates.Delete;
            } else if(args.Remove("renamedups")) {
                return Duplicates.Rename;
            } else {
                return Duplicates.Throw;
            }
        }


        static bool detectKeepFolders(List<string> args) {
            return args.Remove("keepfolders");
        }

        static bool detectRecursive(List<string> args) {
            return args.Remove("recursive");
        }


        static int Main(string[] args) {
            List<string> arguments = new List<string>(args);
            Duplicates dups;
            bool keepFolders, recursive;
            try {
                dups = detectDuplicates(arguments);
                keepFolders = detectKeepFolders(arguments);
                recursive = detectRecursive(arguments);

                if(arguments.Count > 0) {
                    throw new ArgException("Unknown additional arguments: " + String.Join(" ", arguments));
                }
            } catch (ArgException argE) {
                Console.WriteLine("Argument error: " + argE.Message);
                Console.ReadKey();
                return EXIT_CODE_INVALID_ARG;
            }

            int exitCode = flatten(dups, keepFolders, recursive);
            Console.ReadKey();
            return exitCode;
        }

        static int flatten(Duplicates dups, bool keepFolders, bool recursive) {
            Plan plan = new Plan(dups, keepFolders, recursive);

            try {
                Console.WriteLine("Preparing plan...");
                plan.prepare(new DirectoryInfo(Environment.CurrentDirectory));

                plan.execute();

                Console.WriteLine("Completed successfully");
            } catch (DupsException e) {
                Console.WriteLine("Duplicates found for filename " + e.filename + ". Aborting.");
                return EXIT_CODE_DUPS;
            } catch (IOException e) {
                Console.WriteLine("Error during file operation: " + e.Message);
                return EXIT_CODE_IO_ERROR;
            } catch (UnauthorizedAccessException e) {
                Console.WriteLine("Access error during file operation: " + e.Message);
                Console.WriteLine("You could try to execute the program as administrator");
                return EXIT_CODE_IO_ERROR;
            }

            return EXIT_CODE_OK;
        }
    }
}
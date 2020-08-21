
using System;
using System.IO;
using CSScriptLib;

namespace CreateScriptDll
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create Save Directory and Get File Full Name
            var fileFullName = GetFileFullName();

            // Using CS-Script.Core create the DLL of our Script
            CSScript.RoslynEvaluator
                .ReferenceAssemblyOf<Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo>()
                .CompileAssemblyFromCode(
                    @"
                    using System;

                    public class Script
                    {
                        // Noticed the dynamic usage so we dont have to know exactly what is on the arguments.
                        // And we are able to access any of the argument properties, or get an exception is if not found.
                        public ScriptResult Method(dynamic arg1)
                        {
                            return new ScriptResult { Name = $""({arg1.ArgName}) Script Result!"" };
                        }
                    }
                    public class ScriptResult
                    {
                        public string Name { get; set; }
                    }
                ",
                    fileFullName
                );

            // Use the File Full Name to load in the Assembly
            LoadScriptAssemblyAndRunMethod(
                fileFullName
            );
        }

        /// <summary>
        /// Use the File Full Name to load in the Assembly
        /// </summary>
        /// <param name="fileFullName">The fullName of the Assembly File</param>
        private static void LoadScriptAssemblyAndRunMethod(
            string fileFullName
        )
        {
            // Take the file and read the bytes into memory
            var bytes = File.ReadAllBytes(fileFullName);
            // Take the bytes from the file and use Reflection to Load the Assembly
            var assembly = System.Reflection.Assembly.Load(bytes);
            // Create an instance of our Script class located in the scripts
            dynamic t = assembly.CreateInstance("css_root+Script");
            // Create our Data that will be passed to our Script.Method call
            var data = new ScriptArg { ArgName = "Script Argument!" };
            // Place the result into a dynamic object for accessing
            dynamic result = t.Method(data);
            // Display the Name from the result to the console
            Console.WriteLine($"Script.Method(data): {result.Name}");
        }

        private static string GetFileFullName()
        {

            // Create the File Name and Path
            var path = Path.Combine(
                ".",
                "scripts"
            );
            var fileFullName = Path.Combine(
                path,
                "Scripts.dll"
            );
            if (!Directory.Exists(
                path
            ))
            {
                Directory.CreateDirectory(
                    path
                );
            }
            return fileFullName;
        }
    }

    /// <summary>
    /// This class holds our arguments passed into the Script.Method call
    /// </summary>
    public class ScriptArg
    {
        public string ArgName { get; set; }
    }
}
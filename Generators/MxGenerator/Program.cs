using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using VfxEngine.Fix;

namespace MxGenerator
{
    /// <summary>
    /// The MxGenerator application generates the all of the message
    /// classes, and related artifacts, for a specific FIX dictionary.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The MxGenerator application reads in a specific instance of
        /// a FIX dictionary and generates corresponding source code for
        /// the messages, fields, and other entities that are defined in
        /// the dictionary. 
        /// </summary>
        /// <param name="args">
        /// The arguments that were passed to the application from the
        /// command line. These arguments control various aspects of how
        /// the source code is generated.
        /// 
        /// -src (path to FIX dictionary)
        /// The -src argument must be followed by the complete path to
        /// the FIX dictionary (VfxEngine format) that is to be used for
        /// generating the source code files for all of the messages and
        /// related artifacts.
        /// 
        /// -dst (path to output folder)
        /// The -dst argument must be followed by the complete path to
        /// the folder in which the generator will place all of the files
        /// that are generated off of the FIX dictionary.
        /// 
        /// -n (namespace)
        /// The -n argument must be followed by the name of the namespace
        /// that all of the generated code will belong to.
        /// 
        /// </param>
        public void Run(string[] args)
        {
            string srcPath = null;
            string dstPath = null;
            string dstName = null;

            for(int i=0; i!=args.Length; i++)
            {
                string arg = args[i];
                if (arg[0] == '-')
                {
                    string option = arg.Substring(1);
                    switch (option)
                    {
                        case "src":
                            if (i++ < args.Length)
                            {
                                srcPath = args[i];
                            }
                            break;

                        case "dst":
                            if (++i < args.Length)
                            {
                                dstPath = args[i];
                            }
                            break;

                        case "n":
                            if (++i < args.Length)
                            {
                                dstName = args[i];
                            }
                            break;
                    }
                }
            }

            if ((srcPath != null) && (dstPath != null))
            {
                Execute(srcPath, dstPath, dstName);
            }
        }


        /// <summary>
        /// The Execute method is invoked in order to generate the
        /// source code for all of the entities that are defined in
        /// the specified FIX dictionary.
        /// </summary>
        /// <param name="srcPath"></param>
        /// <param name="dstPath"></param>
        private void Execute(string srcPath, string dstPath, string dstNamespace)
        {
            FixDictionary dxInstance = FixDxImporter.Import(srcPath);
            if (dxInstance != null)
            {
                string fixNamespace = "VfxEngine.Fix00";
                if (!string.IsNullOrEmpty(dstNamespace))
                {
                    fixNamespace = dstNamespace;
                }

                // REC: If the destination folder doesn't exist, create it:
                if (!Directory.Exists(dstPath))
                {
                    Directory.CreateDirectory(dstPath);
                }
                
                // REC: Generate the path to the tag definitions file:
                string dstPath_Tags = string.Format("{0}\\{1}.Tags.cs", dstPath, fixNamespace);

                // REC: Generate the tag definitions, write them to the file:
                GenerateTags(dxInstance, dstPath_Tags, fixNamespace);
            }
        }

        /// <summary>
        /// The GenerateTags method is invoked to request that the
        /// system generate a tag definition file, based on the tags
        /// that are discovered in the supplied FIX dictionary.
        /// </summary>
        /// <param name="dxInstance"></param>
        /// <param name="dstPath"></param>
        private void GenerateTags(FixDictionary dxInstance, string dstPath, string dstNamespace)
        {
            StreamWriter mxWriter = new StreamWriter(new FileStream(dstPath, FileMode.Create, FileAccess.Write, FileShare.None));

            mxWriter.WriteLine("//::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
            mxWriter.WriteLine("// VERSAFIX FIX ENGINE - FIX TAG DEFINITIONS FILE");
            mxWriter.WriteLine("//::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::");
            mxWriter.WriteLine("using System;");

            mxWriter.WriteLine(string.Format("namespace {0} {{\r\n\r\n", dstNamespace));

            mxWriter.WriteLine(string.Format("\tpublic class Tags {{"));

            foreach (FixDxField dxField in dxInstance.Fields)
            {
                // REC: Ensure that each field name is valid, in terms of being
                // able to represent it as a .NET variable:
                bool isAcceptable = true;
                foreach (char ch in dxField.Name)
                {
                    if (!char.IsLetterOrDigit(ch) && (ch != '_'))
                    {
                        isAcceptable = false;
                        break;
                    }
                }

                if (isAcceptable)
                {
                    mxWriter.WriteLine(string.Format("\t\tpublic static readonly int {0} = {1};\r\n", dxField.Name, dxField.Tag));
                }
                else
                {
                    mxWriter.WriteLine(string.Format("\t\t// PARSER ERROR: Field name does not look acceptable"));
                    mxWriter.WriteLine(string.Format("\t\t// {0} = {1}\r\n", dxField.Name, dxField.Tag));
                }
            }

            mxWriter.WriteLine(string.Format("\t}}"));

            mxWriter.WriteLine(string.Format("}}"));
            mxWriter.Close();
        }


        static void Main(string[] args)
        {
            Program app = new Program();
            app.Run(args);
        }
    }
}

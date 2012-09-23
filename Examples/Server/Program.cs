using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using VfxEngine.Fix;
using VfxEngine.FixApp;
using VfxEngine.FixEngine;
using VfxEngine.Settings;
using VfxEngine.FixVxRegistry;
using VfxEngine.FixDxRegistry;
using VfxEngine.Services;

namespace Server
{
    class Program
    {
        /// <summary>
        /// All VersaFix applications are driven by an instance
        /// of the VfxFixEngine class, which is responsible for
        /// maintaining the FIX session between the application
        /// and the peer client or server side system.
        /// </summary>
        private VfxFixEngine _fixEngine = new VfxFixEngine();

        /// <summary>
        /// The _fixApplication property maintains a reference to an
        /// instance of the FixApplication, which will handle all of
        /// the callbacks from the VfxEngine instance.
        /// </summary>
        App _fixApplication = new App();

        public void Run(string[] args)
        {
            // REC: Construct the service container that will be used
            // to provide the engine with references to the services it
            // will need to access when it starts:
            IVfxServices services = new VfxServices();

            // REC: Retrieve the current path to the executable:
            string execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // REC: Construct the complete path to the configuration
            // settings file for the application:
            string pathSettings = Path.Combine(execPath, "FixSessions.xml");

            // REC: Attempt to load the application configuration into
            // an instance of a new XML document:
            XmlDocument configuration = new XmlDocument();
            configuration.Load(pathSettings);

            // REC: After the configuration file has been loaded, it must
            // be added to the application's service container so that the
            // engine can access it when it is started.
            services.AddService(typeof(IVfxSettings), new VfxSettings(configuration));

            // REC: The engine requires an IVfxFixApp implementation that
            // it can direct callbacks to as FIX events occur:
            services.AddService(typeof(IVfxFixApp), _fixApplication);

            // REC: Construct the complete path to the versions file that
            // contains the FIX version definitions for the application:
            string pathVersions = Path.Combine(execPath, "FixVersions.xml");

            // REC: Attempt to load the version definitions from the
            // version definitions file associated with the app:
            VfxFixVxRegistry vxRegistry = new VfxFixVxRegistry();
            vxRegistry.Import(pathVersions);


            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            // NOTE: You don't need to do this in your own apps, this is 
            // done here because our example code stores all of the files
            // for the FIX dictionaries in a common directory, so the app
            // walks backwards up the directory tree in order to find the
            // folder that contains those files...
            //-----------------------------------------------------------
            string pathDictionaries = GetPathToExampleDictionaries();

            // REC: Scan the dictionaries directory and import all of
            // the dictionaries into a dictionary registry instance:
            IVfxFixDxRegistry dxRegistry = new VfxFixDxRegistry();
            foreach (string dxFile in Directory.GetFiles(pathDictionaries, "*.xml"))
            {
                // REC: Throughout the VersaFix system, dictionaries are
                // referred to by their file name, sans extension:
                string dxName = Path.GetFileNameWithoutExtension(dxFile);
                // REC: Import the actual dictionary data:
                FixDictionary dxEntry = FixDxImporter.Import(dxFile);
                // REC: Create an entry for the imported dictionary
                // in the dictionary registry:
                if (dxEntry != null)
                {
                    dxRegistry.CreateEntry(dxName, dxEntry);
                }
            }

            // REC: The engine requires IVfxFixVxRegistry so that it can
            // lookup information about configured FIX versions:
            services.AddService(typeof(IVfxFixVxRegistry), vxRegistry);

            // REC: The engine requires IVfxFixDxRegistry so that it can
            // lookup information about configured FIX dictionaries:
            services.AddService(typeof(IVfxFixDxRegistry), dxRegistry);

            // REC: If the initialization completed successfully, the
            // engine can be activated. The application will now start
            // establishing new FIX connections in acccordance with the
            // application's configuration settings:
            this._fixEngine.Activate(services);

            // REC: Sophisticated shutdown logic ;-)
            System.Console.ReadLine();

            
        }


        /// <summary>
        /// This is a utility function that is used in the example code
        /// to locate the folder that all of the example dictionaries are
        /// stored in. You do not need to do this in your own apps, as you
        /// will probably specify a qualified path to the data dictionaries
        /// folder that is relative to your own configuration...
        /// </summary>
        /// <returns></returns>
        private string GetPathToExampleDictionaries()
        {
            // REC: Retrieve the current path to the executable:
            string execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            string pathDictionaries = Path.Combine(execPath, "Data");
            if (!Directory.Exists(pathDictionaries))
            {
                DirectoryInfo parentInfo = Directory.GetParent(execPath);
                while (parentInfo != null)
                {
                    pathDictionaries = Path.Combine(parentInfo.FullName, "Data");
                    if (Directory.Exists(pathDictionaries))
                    {
                        break;
                    }
                    else
                    {
                        pathDictionaries = null;
                        parentInfo = Directory.GetParent(parentInfo.FullName);
                    }
                }
            }

            return pathDictionaries;
        }

        static void Main(string[] args)
        {
            Program app = new Program();
            app.Run(args);
        }
    }
}

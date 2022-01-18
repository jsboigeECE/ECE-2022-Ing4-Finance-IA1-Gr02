using System;
using System.IO;
using System.Runtime.InteropServices;
using Python.Deployment;
using Python.Runtime;

namespace Sudoku.Shared
{
    public abstract class PythonSolverBase : ISolverSudoku
    {

        static PythonSolverBase()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Runtime.PythonDLL = "libpython3.7.dylib";
            }
            else
            {
                Runtime.PythonDLL = "python37.dll";
            }
            InstallPythonComponents();
        }

        public PythonSolverBase()
        {
            InitializePythonComponents();
        }


        protected static void InstallPythonComponents()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                InstallMac();
            }
            else
            {
                InstallEmbedded();
            }
        }

        protected void InstallPipModule(string moduleName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                MacInstaller.PipInstallModule(moduleName);
            }
            else
            {
                Installer.PipInstallModule(moduleName);
            }
        }

        private static void InstallMac()
        {

            

            MacInstaller.LogMessage += Console.WriteLine;
            // Installer.SetupPython().Wait();

            //MacInstaller.InstallPath = "/Library/Frameworks/Python.framework/Versions";
            //MacInstaller.PythonDirectoryName = "3.7/";

            var localInstallPath = MacInstaller.EmbeddedPythonHome;

            var path = $"{localInstallPath};{Path.Combine(localInstallPath, "/lib")};{Environment.GetEnvironmentVariable("Path", EnvironmentVariableTarget.Machine)}";

            Environment.SetEnvironmentVariable("Path", path, EnvironmentVariableTarget.Process);

            Environment.SetEnvironmentVariable("PYTHONHOME", localInstallPath, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PythonPath", path, EnvironmentVariableTarget.Process);

            MacInstaller.RunCommand($@"export DYLD_LIBRARY_PATH={localInstallPath}/lib/:$DYLD_PRINT_LIBRARIES");

            MacInstaller.TryInstallPip();
        }




        private static void InstallEmbedded()
        {

            

            // // set the download source
            // Python.Deployment.Installer.Source = new Installer.DownloadInstallationSource()
            // {
            //     DownloadUrl = @"https://www.python.org/ftp/python/3.7.3/python-3.7.3-embed-amd64.zip",
            // };
            //
            // // install in local directory. if you don't set it will install in local app data of your user account
            // Python.Deployment.Installer.InstallPath = Path.GetFullPath(".");
            //
            // see what the installer is doing

            Installer.LogMessage += Console.WriteLine;

            //
            // install from the given source
            Python.Deployment.Installer.SetupPython().Wait();

            Installer.TryInstallPip();

        }


        protected virtual void InitializePythonComponents()
        {

           
            PythonEngine.Initialize();
            // dynamic sys = PythonEngine.ImportModule("sys");
            // Console.WriteLine("Python version: " + sys.version);
        }

        
        public abstract Shared.GridSudoku Solve(Shared.GridSudoku s);

    }

}
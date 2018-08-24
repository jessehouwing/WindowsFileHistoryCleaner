using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;

namespace FileHistoryCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            int result;
            try
            {
                var commandToExecute = ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
                result = ConsoleCommandDispatcher.DispatchCommand(commandToExecute, args, Console.Out);
            }
            catch (Exception e)
            {
                result = -1;
                Console.Error.WriteLine(e.Message);
                if (e.InnerException != null)
                {
                    Console.Error.WriteLine(e.InnerException.Message);
                }
            }

            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            Environment.Exit(result);
        }
    }
}

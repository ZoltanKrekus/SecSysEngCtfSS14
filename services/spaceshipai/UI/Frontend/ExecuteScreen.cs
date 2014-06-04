using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using SpaceshipAI.Compiler;
using SpaceshipAI.Compiler.Assembler.Lexer;
using SpaceshipAI.UI.Backend;
using SpaceshipAI.VM;

namespace SpaceshipAI.UI.Frontend
{
    public class ExecuteScreen : FrontendScreen
    {
        private readonly string _programName;
        private readonly User _user;

        public ExecuteScreen(StreamReader reader, StreamWriter writer, User user, string programName) : base(reader, writer)
        {
            _user = user;
            _programName = programName;
        }

        private void Execute()
        {
            try
            {
                var vm = new ProgramSession(_user, WriteLine);
                IVirtualMachine program = _user.Programs[_programName];
#if DEBUG
                var sw = new Stopwatch();
                sw.Start();
#endif
                program.Run(vm);
#if DEBUG
                sw.Stop();
                Console.WriteLine("Run time: " + sw.ElapsedMilliseconds);
#endif
                WriteLine("-----");
                WriteLine("Returned: "+vm.Eax.Value.ToString(CultureInfo.InvariantCulture));
            }
            catch (LexicalErrorException e)
            {
                WriteLine("Lexical Error: " + e.Message);
            }
            catch (ParseErrorException e)
            {
                WriteLine("Parse Error: " + e.Message);
            }
            catch (ThreadAbortException)
            {
                WriteLine("Timeout of 5 seconds exceeded...");
            }
            catch (Exception e)
            {
                WriteLine("Error " + e.GetType().Name + ": " + e.Message + Environment.NewLine + "Stacktrace:" +
                          e.StackTrace);
            }
        }

        public override void Start()
        {
            WriteLine("Starting...");
            var workerThread = new Thread(Execute);
            workerThread.Start();
            if (!workerThread.Join(new TimeSpan(0, 0, 5000)))
            {
                workerThread.Abort();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using SpaceshipAI.Compiler;
using SpaceshipAI.Compiler.Assembler.Lexer;
using SpaceshipAI.UI.Backend;
using SpaceshipAI.VM;

namespace SpaceshipAI.UI.Frontend
{
    public class MainMenu : FrontendScreen
    {
        private readonly User _user;

        public MainMenu(StreamReader reader, StreamWriter writer, User user) : base(reader, writer)
        {
            _user = user;
        }


        private void DisplayMenu()
        {
            WriteLine("");
            WriteLine("");
            WriteLine("Select an action:");
            WriteLine("\t\t1 - Execute an AI");
            WriteLine("\t\t2 - Store an AI");
            WriteLine("\t\t3 - Dump an AI");
            WriteLine("\t\t4 - Delete an AI");
            WriteLine("\t\tq - Logout");
        }

        private void ExecuteAi()
        {
            Dictionary<string, string> programs = _user.ProgramSources;
            if (programs.Count == 0)
            {
                WriteLine("There are no stored AI programs.");
                return;
            }
            WriteLine("Stored AI programs:");
            foreach (KeyValuePair<string, string> program in programs)
            {
                WriteLine(program.Key);
            }
            WriteLine("");
            WriteLine("");
            do
            {
                Write("Select a program to execute: ");
                string programName = ReadLine();
                if (programName == "")
                    return;
                if (!programs.ContainsKey(programName))
                {
                    WriteLine("The selected program does not exist.");
                    continue;
                }

                var es = new ExecuteScreen(Reader, Writer, _user, programName);
                es.Start();

                return;
            } while (true);
        }

        private void StoreAi()
        {
            Dictionary<string, string> programSources = _user.ProgramSources;
            if (programSources.Count > 5)
            {
                WriteLine("At most 5 AIs can be stored...");
                return;
            }
            do
            {
                Write("Name: ");
                string programName = ReadLine();
                if (programName == "")
                    return;
                if (programSources.ContainsKey(programName))
                {
                    WriteLine("A program with name '" + programName + "' already exists.");
                    continue;
                }
                WriteLine("AI Program (Max 4096 chars, terminate with EOT. (Hint: Putty - Ctrl+Z, *nix - Ctrl+V Ctrl+Z)):");
                string programCode = ReadUntilEot();
                try
                {
                    IVirtualMachine program = VMCompiler.Compile(programCode);
                    programSources.Add(programName, programCode);
                    _user.Programs.Add(programName, program);
                    WriteLine(programName + " has been stored.");
                }
                catch (LexicalErrorException e)
                {
                    WriteLine("Lexical Error: " + e.Message);
                    return;
                }
                catch (ParseErrorException e)
                {
                    WriteLine("Parse Error: " + e.Message);
                    return;
                }
                catch (Exception e)
                {
                    WriteLine("Error " + e.GetType().Name + ": " + e.Message + Environment.NewLine + "Stacktrace:" +
                              e.StackTrace);
                    return;
                }
                return;
            } while (true);
        }

        private void DeleteAi()
        {
            Dictionary<string, string> programs = _user.ProgramSources;
            if (programs.Count == 0)
            {
                WriteLine("There are no stored AI programs.");
                return;
            }
            WriteLine("Stored AI programs:");
            foreach (KeyValuePair<string, string> program in programs)
            {
                WriteLine(program.Key);
            }
            do
            {
                Write("Select a program to delete: ");
                string programName = ReadLine();
                if (programName == "")
                    return;
                if (!programs.ContainsKey(programName))
                {
                    WriteLine("The selected program does not exist.");
                    continue;
                }

                programs.Remove(programName);
                WriteLine(programName + " has been deleted.");

                return;
            } while (true);
        }

        private void DumpAi()
        {
            Dictionary<string, string> programs = _user.ProgramSources;
            if (programs.Count == 0)
            {
                WriteLine("There are no stored AI programs.");
                return;
            }
            WriteLine("Stored AI programs:");
            foreach (KeyValuePair<string, string> program in programs)
            {
                WriteLine(program.Key);
            }
            WriteLine("");
            WriteLine("");
            do
            {
                Write("Select an AI to list: ");
                string programName = ReadLine();
                if (programName == "")
                    return;
                if (!programs.ContainsKey(programName))
                {
                    WriteLine("The selected program does not exist.");
                    continue;
                }

                WriteLine(programs[programName]);

                return;
            } while (true);
        }

        public override void Start()
        {
            DisplayMenu();

            do
            {
                string input = ReadLine();
                if (input == "1")
                {
                    ExecuteAi();
                    DisplayMenu();
                }
                if (input == "2")
                {
                    StoreAi();
                    DisplayMenu();
                }
                if (input == "3")
                {
                    DumpAi();
                    DisplayMenu();
                }
                if (input == "4")
                {
                    DeleteAi();
                    DisplayMenu();
                }
                if (input == "q" || input == "Q")
                {
                    WriteLine("You have been logged out.");
                    return;
                }
            } while (true);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpaceshipAI.UI.Backend
{
    public class MainScreen : BackendScreen
    {
        private static void DisplayWelcomeMessage()
        {
            WriteLineColor("-------------------------------------", ConsoleColor.Yellow);
            WriteLineColor("-------Spaceship AI Engine 1.0-------", ConsoleColor.Yellow);
            WriteLineColor("---------------Backend---------------", ConsoleColor.Yellow);
            WriteLineColor("-------------------------------------", ConsoleColor.Yellow);
        }

        private static void DisplayMenu()
        {
            ClrScr();
            WriteLineColor("", ConsoleColor.Magenta);
            WriteLineColor("           -----====SpaceShip AI Backend====-----", ConsoleColor.Yellow);
            WriteLineColor("", ConsoleColor.Magenta);
            WriteLineColor("Select an action:", ConsoleColor.Magenta);
            WriteColor("\t1", ConsoleColor.Green);
            WriteLineColor(" - List users", ConsoleColor.Magenta);
            WriteColor("\t2", ConsoleColor.Green);
            WriteLineColor(" - Dump an AI of a user", ConsoleColor.Magenta);
            WriteColor("\t3", ConsoleColor.Green);
            WriteLineColor(" - Display statistics", ConsoleColor.Magenta);
            WriteColor("\tq", ConsoleColor.Green);
            WriteLineColor(" - Quit", ConsoleColor.Magenta);
        }

        private static void ListUsers()
        {
            ClrScr();
            WriteLineColor("Users: ", ConsoleColor.Cyan);
            int i = 1;
            foreach (User user in Users.AsEnumerable)
            {
                WriteLineColor(user.Username + " (" + user.Id + ")", ConsoleColor.Yellow);
                if (i%10 == 0)
                {
                    WriteLineColor("Press a key to continue...", ConsoleColor.Magenta);
                    ReadKey();
                }
                i++;
            }
            WriteLineColor("Press a key to continue...", ConsoleColor.Magenta);
            ReadKey();
        }

        private static void DumpProgram()
        {
            ClrScr();
            string username;
            do
            {
                WriteColor("User: ", ConsoleColor.Magenta);
                username = ReadLine();
                if (string.IsNullOrEmpty(username))
                    return;
                if (!Users.UserRegistered(username))
                {
                    WriteError("User " + username + " does not exist.");
                    continue;
                }
                break;
            } while (true);

            Dictionary<string, string> programs = Users.GetUser(username).ProgramSources;

            WriteLineColor("Programs:", ConsoleColor.Magenta);

            foreach (KeyValuePair<string, string> program in programs)
            {
                WriteLineColor(program.Key, ConsoleColor.Yellow);
            }

            do
            {
                WriteColor("Select an AI to dump:", ConsoleColor.Magenta);
                string programName = ReadLine();
                if (string.IsNullOrEmpty(programName))
                    return;
                if (!programs.ContainsKey(programName))
                {
                    WriteError("The selected program does not exist.");
                    continue;
                }
                ClrScr();
                WriteCode(programs[programName]);
                WriteLineColor("Press a key to continue...", ConsoleColor.Magenta);
                ReadKey();
                return;
            } while (true);
        }

        private static void Statistics()
        {
            ClrScr();
            WriteColor("Total registerered users: ", ConsoleColor.Cyan);
            WriteLineColor(Users.UserCount, ConsoleColor.Yellow);
            WriteColor("Number of AIs: ", ConsoleColor.Cyan);
            WriteLineColor(Users.AsEnumerable.Sum(x => x.ProgramSources.Count), ConsoleColor.Yellow);
            WriteColor("Connected users: ", ConsoleColor.Cyan);
            WriteLineColor(Server.ConnectedUsers, ConsoleColor.Yellow);
            WriteLineColor("Press a key to continue...", ConsoleColor.Magenta);
            ReadKey();
        }

        public override void Start()
        {
            DisplayWelcomeMessage();
            DisplayMenu();
            do
            {
                string input = ReadKey();
                if (input == null)
                    return;
                if (input == "1")
                {
                    ListUsers();
                    DisplayMenu();
                }
                if (input == "2")
                {
                    DumpProgram();
                    DisplayMenu();
                }
                if (input == "3")
                {
                    Statistics();
                    DisplayMenu();
                }
                if (input == "q")
                {
                    WriteError("Are you sure? (Y/n)");
                    var key = ReadKey();
                    if (key == "Y" || key == null)
                        return;
                    DisplayMenu();
                }
            } while (true);
        }
    }
}
using System;
using System.IO;
using SpaceshipAI.UI.Backend;

namespace SpaceshipAI.UI.Frontend
{
    public class WelcomeScreen : FrontendScreen
    {
        public WelcomeScreen(StreamReader reader, StreamWriter writer) : base(reader, writer)
        {
        }

        private void DisplayWelcomeMessage()
        {
            WriteLine("-------------------------------------");
            WriteLine("-------Spaceship AI Engine 1.0-------");
            WriteLine("-------------------------------------");
        }


        private void Login()
        {
            Write("Username: ");
            string username = ReadLine();

            int tries = 0;
            while (tries < 3)
            {
                Write("Password: ");
                string password = ReadLine();
                if (username == "" || password == "")
                    return;
                if (Users.Login(username, password))
                {
                    WriteLine("Login successful.");
                    var mainMenu = new MainMenu(Reader, Writer, Users.GetUser(username));
                    mainMenu.Start();
                    return;
                }
                WriteLine("Wrong username and/or password.");
                tries++;
            }
        }

        private void Register()
        {
            bool usernameOk;
            User user;
            do
            {
                Write("Username: ");
                string username = ReadLine();
                if (username == "")
                    return;
                user = Users.TryRegister(username);
                usernameOk = user != null;
                if (!usernameOk)
                {
                    WriteLine("User " + username + " already exists. Pick another one!");
                }
            } while (!usernameOk);

            try
            {
                do
                {
                    Write("Password: ");
                    string password = ReadLine();
                    if (password == "")
                    {
                        WriteLine("Password cannot be empty!");
                        continue;
                    }
                    user.Password = password;
                    Users.CommitRegister(user);
                    break;
                } while (true);
            }
            catch (Exception)
            {
                Users.DeleteUser(user);
                throw;
            }
        }

        private void DisplayMenu()
        {
            WriteLine("");
            WriteLine("");
            WriteLine("Select an action:");
            WriteLine("\t\t1 - Login");
            WriteLine("\t\t2 - Register");
            WriteLine("\t\tq - Quit");
        }

        public override void Start()
        {
            DisplayWelcomeMessage();
            DisplayMenu();
            do
            {
                string input = ReadLine();
                if (input == "1")
                {
                    Login();
                    DisplayMenu();
                }
                if (input == "2")
                {
                    Register();
                    DisplayMenu();
                }
                if (input == "q" || input == "Q")
                {
                    WriteLine("Goodbye!");
                    return;
                }
            } while (true);
        }
    }
}
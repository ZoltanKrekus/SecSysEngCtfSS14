using System;
using System.Collections.Generic;
using System.Linq;
using SpaceshipAI.VM;

namespace SpaceshipAI.UI.Backend
{
    public class Users
    {
        private static readonly Dictionary<string, User> UserList =
            new Dictionary<string, User>(StringComparer.OrdinalIgnoreCase);

        public static int UserCount;

        public static IEnumerable<User> AsEnumerable
        {
            get { return UserList.Select(x => x.Value); }
        }

        public static bool UserRegistered(string username)
        {
            lock (UserList)
            {
                return UserList.ContainsKey(username);
            }
        }

        public static User GetUser(string username)
        {
            lock (UserList)
            {
                if (UserList.ContainsKey(username))
                    return UserList[username];
                return null;
            }
        }

        public static bool Login(string username, string password)
        {
            lock (UserList)
            {
                User user = GetUser(username);
                if (user == null)
                    return false;
                return user.Password == password;
            }
        }


        public static User TryRegister(string username)
        {
            lock (UserList)
            {
                if (UserList.ContainsKey(username))
                    return null;
                var user = new User(username, "");
                UserList.Add(username, user);
                return user;
            }
        }

        public static void CommitRegister(User user)
        {
            lock (UserList)
            {
                if (!UserList.ContainsKey(user.Username))
                    return;
                user.Id = UserCount;
                UserCount++;
            }
        }

        public static void DeleteUser(User user)
        {
            DeleteUser(user.Username);
        }

        public static void DeleteUser(string username)
        {
            lock (UserList)
            {
                if (UserList.ContainsKey(username))
                {
                    UserList.Remove(username);
                }
            }
        }
    }

    public class User
    {
        public User(string user, string pass)
        {
            Username = user;
            Password = pass;
            ProgramSources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Programs = new Dictionary<string, IVirtualMachine>(StringComparer.OrdinalIgnoreCase);
        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public Dictionary<string, string> ProgramSources { get; private set; }
        public Dictionary<string, IVirtualMachine> Programs { get; private set; }
    }
}
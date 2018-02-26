using MySql.Data.MySqlClient;
using NDesk.Options;
using System;
using System.Collections.Generic;

namespace BackupAndRestoreMySqlServer
{
    class Program
    {
        private static string ServerIPAddress { get; set; }
        private static string User { get; set; }
        private static string Password { get; set; }
        private static string DatabaseName { get; set; }
        private static string FolderToSave { get; set; }
        private static string BackupName { get; set; }

        static void Main(string[] args)
        {
            bool showHelp = false;
            bool restore = false, backup = false;
            var p = new OptionSet()
            {
                { "s|server=",   "MySQL Server address (IP or domain name)", v => ServerIPAddress = v },
                { "u|user=",     "Username to connect to server",            v => User = v },
                { "p|password=", "Password to connect to server",            v => Password = v },
                { "d|database=", "Database name",                            v => DatabaseName = v },
                { "f|folder=",   "Folder where need save backup / where take backup to restore", v => FolderToSave = v },
                { "n|name=",     "Backup name to restore",                   v => BackupName = v },
                { "b|backup",   "Do backup",                                v => backup = v != null },
                { "r|restore",  "Do restore",                               v => restore = v != null },
                { "h|help",     "Show this message",                        v => showHelp = v != null }
            };
            List<string> extra;

            try
            {
                extra = p.Parse(args);
                if (restore && backup)
                {
                    throw new InvalidOperationException("Нельзя одновременно делать backup и restore");
                }
                else if (restore)
                {
                    Restore();
                }
                else if (backup)
                {
                    Backup();
                }
                else if (showHelp)
                {
                    p.WriteOptionDescriptions(Console.Out);
                }
            }
            catch (OptionException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Try type --help for more information");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Backup()
        {
            string connectionString = $"server={ServerIPAddress};user={User};pwd={Password};database={DatabaseName};";
            string file = $@"{FolderToSave}\{DatabaseName}_{DateTime.Now.ToString("ddMMyyyyHHmmss")}.sql";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = connection;
                        connection.Open();
                        mb.ExportToFile(file);
                        connection.Close();
                    }
                }
            }
            Console.WriteLine("Success!");
        }

        private static void Restore()
        {
            string connectionString = $"server={ServerIPAddress};user={User};pwd={Password};database={DatabaseName};CharSet=utf8;";
            string file = $@"{FolderToSave}\{BackupName}";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        cmd.Connection = connection;
                        connection.Open();
                        mb.ImportFromFile(file);
                        connection.Close();
                    }
                }
            }
            Console.WriteLine("Success!");
        }
    }
}
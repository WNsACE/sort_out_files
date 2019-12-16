using System;
using System.Collections.Generic;
using System.IO;
using sort_out_files.tools;

namespace sort_out_files
{
    class Program
    {
        private static readonly string[] command_string_list = new string[] { "-h", "-i", "-o", "-e", "-c", "-f" };
        private static readonly Dictionary<string, string> command_time_string_list = new Dictionary<string, string> { { "y", "yyyy" }, { "m", "yyyyMM" }, { "d", "yyyyMMdd" } };

        private static string in_dir = null;
        private static string out_dir = null;
        private static string time_string = "yyyyMM";
        private static string searchPattern = "*.*";
        private static bool is_delete_same_file = false;

        private static uint move_file_size = 0;
        private static uint dele_file_size = 0;

        static void set_config(string[] args, ref int i)
        {
            string command = args[i];
            bool is_get_data = args.Length > i + 1;
            if (command == "-h")
            {
                Console.WriteLine("command : -h help ");
                Console.WriteLine("          -i in_dir , command : -i in_dir_path ");
                Console.WriteLine("          -o out_dir, command : -o out_dir_path ");
                Console.WriteLine("          -e search_pattern (default : *.*), command : -e *.txt ");
                Console.WriteLine("          -f delete same file (default : not delete file), command : -f ");
                Console.WriteLine("          -c sort for time (default : month), command: year: -c y , month : -c m, day : -c d");
                return;
            }
            switch (command)
            {
                case "-i":
                    if (is_get_data) in_dir = args[++i];
                    break;
                case "-o":
                    if (is_get_data) out_dir = args[++i];
                    break;
                case "-e":
                    if (is_get_data) searchPattern = args[++i];
                    break;
                case "-c":
                    if (is_get_data)
                    {
                        if (command_time_string_list.ContainsKey(args[++i]))
                        {
                            time_string = command_time_string_list[args[i]];
                        }
                    }
                    break;
                case "-f":
                    is_delete_same_file = true;
                    break;
                default:
                    Console.WriteLine(" error commond ");
                    break;
            }
        }

        static bool get_command(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string tmp = Array.Find<string>(command_string_list, (str) => { return str == args[i]; });

                if (!string.IsNullOrEmpty(tmp))
                {
                    set_config(args, ref i);
                }
            }

            return !string.IsNullOrEmpty(in_dir) && !string.IsNullOrEmpty(out_dir);
        }

        static void Main(string[] args)
        {
            if (args.Length <= 0)
            {
                Console.WriteLine("need input commond : -i in_dir -o out_dir ，help for -h ");
                return;
            }

            if (get_command(args))
            {
                move_files();

                Console.WriteLine("move_file_size:{0}, dele_file_size:{1} ", move_file_size, dele_file_size);
            }
        }

        private static void move_files()
        {
            string[] filePaths = Directory.GetFiles(in_dir, searchPattern, SearchOption.AllDirectories);

            foreach (var filePath in filePaths)
            {
                uint number = 1;

                bool is_dele = false;
                DateTime last_time = File.GetLastWriteTime(filePath);

                string file_extension = Path.GetExtension(filePath);
                string file_name = Path.GetFileNameWithoutExtension(filePath);

                string new_dir_name = last_time.ToString(time_string);
                string new_dir_path = Path.Combine(out_dir, new_dir_name);
                string new_file_path = Path.Combine(new_dir_path, file_name + file_extension);

                if (!Directory.Exists(new_dir_path))
                {
                    Directory.CreateDirectory(new_dir_path);
                }

                do
                {
                    if (File.Exists(new_file_path))
                    {
                        string old_file_md5 = MD5_helper.GetFileMD5(new_file_path);
                        string new_file_md5 = MD5_helper.GetFileMD5(filePath);

                        if (old_file_md5 != new_file_md5)
                        {
                            new_file_path = Path.Combine(new_dir_path, string.Format("{0}_{1}{2}", file_name, number, file_extension));
                        }
                        else
                        {
                            if(is_delete_same_file)
                            {
                                Console.WriteLine("delete same file {0} ", filePath);
                                File.Delete(filePath);
                            }
                            is_dele = true;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                } while (true);

                if (!is_dele)
                {
                    Console.WriteLine("---------------- {0} move to {1} ", filePath, new_file_path);
                    File.Move(filePath, new_file_path);
                    move_file_size++;
                }
                else
                {
                    Console.WriteLine("++++++++++++++++ {0} and {1} is same file ", filePath, new_file_path);
                    dele_file_size++;
                }
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void ClearLine(int line)
        {
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, line);
        }
        static void WriteHeader()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                        *************Process Monitor*************");
            Console.WriteLine("                        Author: Chronostasys, ver 1.0.0");
            Console.WriteLine($"{"Name",30}|{"Id",20}|{"Threads",10}|{"Private mem",15}");
        }
        static void Main(string[] args)
        {
            Console.Clear();
            int init = 0;
            int psnum = 0;
            int min = 0;
            bool pause = false;
            string query = "";
            Task.Run(async () =>
            {
                WriteHeader();
                while (true)
                {
                    if (pause)
                    {
                        continue;
                    }
                    var ps = Process.GetProcesses().Where(p=>p.ProcessName.Contains(query)).ToArray();
                    psnum = ps.Length;
                    var ci = init;
                    min = Console.WindowHeight - 10;
                    var top = 3;
                    for (int i = ci; i < ci + min; i++)
                    {
                        ClearLine(i - ci+top);
                        try
                        {
                            var item = ps[i];
                            var name = item.ProcessName.Length < 30 ? item.ProcessName : item.ProcessName.Substring(0, 27) + "...";
                            Console.WriteLine($"{name, 30}|{item.Id,20}|{item.Threads.Count,10}|{item.PrivateMemorySize64,15}");
                        }
                        catch (Exception)
                        {
                        }
                    }
                    ClearLine(min + top);
                    Console.WriteLine($"            total: {ps.Length,15} processes, {ps.Select(p=>p.Threads.Count).Sum()} threads");
                    Console.WriteLine("             Press up/down array to explore other processes, esc for cammnad mod, ctrl + c to shut down");
                    await Task.Delay(50);
                }
            });
            while (true)
            {
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.UpArrow)
                {
                    if (init != 0)
                    {
                        init--;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (init + min < psnum)
                    {
                        init++;
                    }
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    pause = true;
                    var cmd = Console.ReadLine();
                    var cmds = cmd.Split(' ');
                    switch (cmds[0])
                    {
                        case "find":
                            query = cmd.Substring(5, cmd.Length - 5).Trim('"');
                            break;
                        default:
                            query = "";
                            break;
                    }
                    Console.Clear();
                    Console.WriteLine("a");
                    init = 0;
                    WriteHeader();
                    pause = false;
                }
            }


        }
    }
}
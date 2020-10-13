using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void ClearLineAfter(int startPos,int line)
        {
            Console.SetCursorPosition(startPos, line);
            Console.Write(new string(' ', Console.WindowWidth));
        }
        static void WriteHeader()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                        *************Process Monitor*************");
            Console.WriteLine("                        Author: Chronostasys, ver 1.0.0");
            Console.WriteLine($"{"Name",30}|{"Id",10}|{"Threads",10}|{"Ram mem",15}");
        }
        static void Main(string[] args)
        {
            Console.Clear();
            var lkey = new object();
            int init = 0;
            int psnum = 0;
            int min = 0;
            string query = "";
            string orderby = "name";
            bool dec = false;
            Func<Process, object> orderFunc = p=>p.ProcessName;
            Task.Run(async () =>
            {
                WriteHeader();
                while (true)
                {
                    lock (lkey)
                    {
                        var ps = Process.GetProcesses();
                        if (dec)
                        {
                            ps = ps.Where(p => p.ProcessName.ToLower().Contains(query))
                                .OrderByDescending(orderFunc).ToArray();
                        }
                        else
                        {
                            ps = ps.Where(p => p.ProcessName.ToLower().Contains(query))
                                .OrderBy(orderFunc).ToArray();
                        }
                        psnum = ps.Length;
                        var ci = init;
                        min = Console.WindowHeight - 10;
                        var top = 3;
                        for (int i = ci; i < ci + min; i++)
                        {
                            Console.SetCursorPosition(0, i - ci + top);
                            try
                            {
                                var item = ps[i];
                                var name = item.ProcessName.Length < 30 ? item.ProcessName : item.ProcessName.Substring(0, 27) + "...";
                                Console.WriteLine($"{name,30}|{item.Id,10}|{item.Threads.Count,10}|{item.WorkingSet64,15}");
                                ClearLineAfter(Console.CursorLeft, Console.CursorTop);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        Console.SetCursorPosition(0, min+top);
                        //ClearLine(min + top);
                        Console.WriteLine($"            total: {ps.Length,15} processes, {ps.Select(p => p.Threads.Count).Sum(),10} threads");
                        Console.WriteLine($"            query: {(query.Length==0?"null":query),5}, sort: {(orderby.Length == 0 ? "null" : orderby)}, press tab to toggle descending/ascending");
                        Console.WriteLine("            Press up/down array to explore other processes, esc for cammnad mod, ctrl + c to shut down");
                        for (int i = 0; i <Console.WindowHeight -  (min+top+3); i++)
                        {
                            ClearLineAfter(0, min + top + 3 + i);
                        }
                        Console.SetCursorPosition(0, min + top + 3);
                        Console.WindowTop = 0;
                    }
                    await Task.Delay(100);
                }
            });
            while (true)
            {
                var key = Console.ReadKey(true);
                lock (lkey)
                {
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
                        Console.SetCursorPosition(3, Console.CursorTop);
                        Console.Write("Enter Command: ");
                        var cmd = Console.ReadLine();
                        var cmds = cmd.Split(' ');
                        switch (cmds[0])
                        {
                            case "find":
                                query = cmd.Substring(5, cmd.Length - 5).Trim('"').ToLower();
                                break;
                            case "sort":
                                orderby = cmd.Substring(5, cmd.Length - 5).Trim('"').ToLower();
                                switch (orderby)
                                {
                                    case "mem":
                                        orderFunc = p => p.WorkingSet64;
                                        break;
                                    case "thread":
                                        orderFunc = p => p.Threads.Count;
                                        break;
                                    case "id":
                                        orderFunc = p => p.Id;
                                        break;
                                    case "name":
                                        orderFunc = p => p.ProcessName;
                                        break;
                                    default:
                                        orderFunc = p => p.ProcessName;
                                        break;
                                }

                                break;
                            case "kill":
                                var num = int.Parse(cmd.Substring(5, cmd.Length - 5).Trim('"').ToLower());
                                Process.GetProcessById(num).Kill();
                                break;
                            default:
                                query = "";
                                break;
                        }
                        Console.Clear();
                        init = 0;
                        WriteHeader();
                    }
                    else if (key.Key == ConsoleKey.Tab)
                    {
                        dec = !dec;
                    }
                }
            }


        }
    }
}
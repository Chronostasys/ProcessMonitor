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
            Console.SetCursorPosition(0, 6);
        }
        static void ClearLine(int startLine,int endLine)
        {
            for (int line = startLine; line < endLine; line++)
            {
                ClearLine(line);
            }
            
        }
        static void WriteHeader()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine("                        *************Process Monitor*************");
            Console.WriteLine("                        Author: Chronostasys & leezeeyee, ver 1.0.0");
            Console.WriteLine("                        *****************************************");

        }
        static void Main(string[] args)
        {
            Console.Clear();
            var lkey = new object();
            int init = 0;
            int psnum = 0;
            int psLastNum = 0;
            int min = 0;
            int cmdLine = 6;
            string query = "";
            string orderby = "name";
            bool dec = false;
            int prevHeight = Console.WindowHeight;
            Func<Process, object> orderFunc = p => p.ProcessName;
            WriteHeader();
            Task.Run(async () =>
            {           
                while (true)
                {
                    
                    lock (lkey)
                    {
                        if (prevHeight!=Console.WindowHeight)
                        {
                            Console.Clear();
                            WriteHeader();
                        }
                        prevHeight = Console.WindowHeight;
                        Console.WindowTop = 0;
                        Console.CursorVisible = false;
                        Console.SetCursorPosition(0, cmdLine);
                        
                        //recording the num of last ps
                        psLastNum = psnum;

                        var ps = Process.GetProcesses();
                        if(query=="all")
                        {
                            query = "";
                        }
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



                        Console.SetCursorPosition(0, 3);
                        //ClearLine(min + top);
                        Console.WriteLine($"            query: {(query.Length == 0 ? "null" : query),5}, sort: {(orderby.Length == 0 ? "null" : orderby)}, shift + [initial] to change sort mode, press tab to toggle descending/ascending");
                        Console.WriteLine("            Press up/down array to explore other processes, esc for commnad mode, ctrl + c to shut down");
                        Console.WriteLine($"            total: {ps.Length,15} processes, {ps.Select(p => p.Threads.Count).Sum()} threads");
                        Console.WriteLine("                            ");
                        Console.SetCursorPosition(0, cmdLine);
                        Console.WriteLine("                            ");
                        Console.WriteLine($"{"Name",30}|{"Id",20}|{"Threads",10}|{"Ram mem",15}");
                        Console.WriteLine("  _____________________________________________________________________________________");


                        var ci = init;
                        min = Console.WindowHeight - 10;
                        //var top = 3;
                        if(psnum<min&&psnum<psLastNum)
                        {
                            //clear reluctant lines
                            ClearLine(cmdLine + psnum, Console.WindowHeight);
                            psLastNum = psnum;

                        }
                        Console.SetCursorPosition(0, cmdLine+3);
                        for (int i = ci; i < ci + min&&i<ps.Length; i++)
                        {
                            //ClearLine(i - ci + top);

                            try
                            {
                                var item = ps[i];
                                var name = item.ProcessName.Length < 30 ? item.ProcessName : item.ProcessName.Substring(0, 27) + "...";
                                Console.WriteLine($"{name,30}|{item.Id,20}|{item.Threads.Count,10}|{item.WorkingSet64,15}");
                            }
                            catch (Exception)
                            {
                            }
                        }
                        Console.SetCursorPosition(0, cmdLine);
                    }
                    await Task.Delay(100);
                }
            });
            while (true)
            {
                var key = Console.ReadKey(true);

                lock (lkey)
                {

                    if ((key.Modifiers & ConsoleModifiers.Shift) != 0)
                    {
                        //Console.Write("SHIFT+");
                        //Console.WriteLine(key.Key.ToString());
                        if(key.Key==ConsoleKey.M)
                        {
                            orderFunc = p => p.WorkingSet64;
                            orderby = "mem";
                        }
                        else if(key.Key == ConsoleKey.T)
                        {
                            orderFunc = p => p.Threads.Count;
                            orderby = "thread";
                        }
                        else if (key.Key == ConsoleKey.I)
                        {
                            orderFunc = p => p.Id;
                            orderby = "id";
                        }
                        else if (key.Key == ConsoleKey.N)
                        {
                            orderFunc = p => p.ProcessName;
                            orderby = "name";
                        }
                    }

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
                        Console.CursorVisible = true;

                        Console.SetCursorPosition(0, cmdLine);// Console.CursorTop);
                        Console.Write("Enter Command(q to quit): ");

                        var cmd = Console.ReadLine();                        
                        var cmds = cmd.Split(' ');
                        switch (cmds[0])
                        {
                            
                            case "find":

                                try
                                {
                                    query = cmd.Substring(5, cmd.Length - 5).Trim('"').ToLower();
                                }
                                catch (Exception)
                                {
                                    

                                    Console.Write("find [process name]");
                                }


                                break;
                            case "sort":
                                try
                                {
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
                                }
                                catch (Exception)
                                {

                                    Console.Write("sort [prop]:prop is name | id | thread | mem");
                                }                              

                                break;
                            case "kill":
                                try
                                {
                                    var num = int.Parse(cmd.Substring(5, cmd.Length - 5).Trim('"').ToLower());
                                    Process.GetProcessById(num).Kill();
                                }
                                catch (Exception)
                                {                                    
                                    Console.Write("kill [process id]");
                                }
                                
                                break;
                            default:
                                query = "";
                                break;


                        }
                        ClearLine(cmdLine);
                        init = 0;
                        //WriteHeader();
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
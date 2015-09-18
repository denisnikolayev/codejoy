using System.IO;
using System.Linq;
using Nancy.Conventions;

namespace NancySelfHosting
{
    using Nancy;
    using System;
    using System.Diagnostics;

    public class Module : NancyModule
    {
        public Module()
        {
            Get["/"] = parameters => { return answer(parameters); };
        }

        private static DateTime? start;
        
        private String answer(dynamic parameters)
        {
            var board = (string) Request.Query["Board"];
            if (board != null)
            {
                var size = (int) Math.Truncate(Math.Sqrt(board.Length));
                for (var i = 0; i < size - 1; i++)
                {
                    Debug.WriteLine(board.Substring(i*size, size));
                }
            }

            // add "drop" to response when you need to drop a figure
            // for details please check http://codenjoy.com/portal/?p=170#commands			

            var b = new Bot();
            b.Prepare(board);
            var length = b.Scan().Count(a => a.Item2 == CellType.tail);
            if (start == null || b.head.X == b.size / 2 && b.head.Y == b.size / 2 && b.Scan().Count(a=>a.Item2 == CellType.tail) == 0)
            {
                start = DateTime.Now;
            }


            var log = @"c:\temp\bot" + start.Value.ToString("ddhhmmss") + ".log";
            try
            {
                //File.AppendAllText(log, "\r\n\r\n" + board + "\r\n" + length + "\r\n");
                var step = b.Step().ToString().ToUpper();
                //File.AppendAllText(log, step + "\r\n");
                return step;
            }
            catch (Exception)
            {
                //File.AppendAllText(log, "fuck" + "\r\n");
                throw;
            }
            

            
        }
    }
}

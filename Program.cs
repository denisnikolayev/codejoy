using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;

namespace NancySelfHosting
{
    using System;
    using Nancy.Hosting.Self;

    using System.Diagnostics;

    class Program
    {
        static void Main()
        {
            if (!Debugger.IsAttached)
            {
                var imitation = false;

                if (imitation)
                {
                    Imitation();
                }
                else
                {

                    var nancyHost = new NancyHost(new Uri("http://localhost:8888/"));
                    nancyHost.Start();

                    Console.WriteLine("Nancy now listening - navigating to http://localhost:8888/. Press enter to stop");
                    //Process.Start("http://localhost:8888/");
                    Console.ReadKey();

                    nancyHost.Stop();

                    Console.WriteLine("Stopped. Good bye!");
                }
            }
            else
            {
                var bot = new Bot();
                var board = "";
                //    "☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼       ○○○○○○☼☼ ○○○○○○○○○○○○☼☼ ○○○○○○○○○○○○☼☼ ☺  ☻○○○○○○○○☼☼   ◄○○       ☼☼             ☼☼             ☼☼             ☼☼             ☼☼             ☼☼             ☼☼             ☼☼             ☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼☼";
                board += "555555555555555";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222222225";
                board += "522222222242225";
                board += "522222222222  5";
                board += "52      2222215";
                board += "52     3    225";
                board += "52 22        25";
                board += "522222222222225";
                board += "555555555555555";

                bot.Prepare(board);
                bot.Print();
                var step = bot.Step();
                Console.WriteLine(step);
                Console.ReadKey();
            }
        }

        private static void Imitation()
        {
            var rnd = new Random();
            var bot = new Bot
            {
                map = new CellType[15, 15],
                head = new Point(8, 8),
                bomb = new Point(rnd.Next(10) + 3, rnd.Next(10) + 3),
                size = 15
            };

            for (var x = 0; x < 15; x++)
                for (var y = 0; y < 15; y++)
                    bot.map[x, y] = CellType.empty;

            for (var i = 0; i < 15; i++)
                bot.map[0, i] = bot.map[14, i] = bot.map[i, 0] = bot.map[i, 14] = CellType.wall;

            bot[bot.head] = CellType.head;
            bot[bot.bomb] = CellType.bomb;

            Action addApple = () =>
            {
                var space = bot.Scan().Where(a => a.Item2 == CellType.empty).ToArray();
                bot.apple = space[rnd.Next(space.Length - 1)].Item1;
                bot[bot.apple] = CellType.apple;
                bot.Find(CellType.apple);
            };

            var snake = new LinkedList<Point>();
            snake.AddLast(bot.head);
            var tail = new Point(bot.head.X + 1, bot.head.Y);
            snake.AddLast(tail);
            bot[tail] = CellType.tail;

            Func<Point, StepType, Point> move = (current, step) =>
                step == StepType.down ? new Point(current.X, current.Y + 1)
                    : step == StepType.up ? new Point(current.X, current.Y - 1)
                        : step == StepType.left ? new Point(current.X - 1, current.Y)
                            : new Point(current.X + 1, current.Y);

            var log = new StringBuilder();
            Action save = () =>
            {
                Console.WriteLine("проиграл!!");
                log.AppendLine(bot.Scan().Count(a => a.Item2 == CellType.tail).ToString());
                var path = @"c:\temp\" + DateTime.Now.ToString("ddhhmmss") + "log.txt";
                File.WriteAllText(path, log.ToString());
                Process.Start(path);
            };

            addApple();

            var wait = 500;
            var steps = 0;
            while (true)
            {
                log.AppendLine(steps.ToString());
                steps += 1;
                if (steps%100==0)
                {
                    Console.WriteLine(steps + " " + bot.Scan().Count(a => a.Item2 == CellType.tail).ToString());
                }
                bot.PrintTo(log);

                StepType step = StepType.left;
                try
                {
                    step = bot.Step();
                }
                catch (Exception ex)
                {
                    log.AppendLine(ex.ToString());
                    save();
                    return;
                }
                

                log.AppendLine(step.ToString());
               
                var newPos = move(bot.head, step);
                var newCell = bot[newPos];
                switch (newCell)
                {
                    case CellType.empty:
                        bot[bot.head] = CellType.tail;
                        bot.head = newPos;
                        bot[bot.head] = CellType.head;
                        bot[snake.Last.Value] = CellType.empty;
                        snake.RemoveLast();
                        snake.AddFirst(bot.head);

                        break;

                    case CellType.apple:
                        bot[bot.head] = CellType.tail;
                        bot.head = newPos;
                        bot[bot.head] = CellType.head;
                        snake.AddFirst(bot.head);

                        addApple();

                        wait = 500;
                        break;

                    case CellType.head:
                    case CellType.tail:
                    case CellType.bomb:
                    case CellType.wall:
                        save();
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                wait -= 1;
                if (wait == 0)
                {
                    log.AppendLine("цикл");
                    save();
                    return;
                };

            }
        }
    }
}

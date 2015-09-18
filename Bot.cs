using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;

namespace NancySelfHosting
{
    enum CellType 
    {
        empty,
        head,
        tail,
        apple,
        bomb,
        wall
    }

    enum StepType
    {
        up,
        down,
        left,
        right
    }
    

    class Bot
    {
        public CellType[,] map;
        public int size;
        public Point head;
        public Point apple;
        public Point bomb;

        public static Point? lastApple = null;
        public static int stepswithoutapple = 400;

        public CellType this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x <= size - 1 && y <= size - 1)
                {
                    return map[x, y];
                }

                return CellType.wall;
            }
        }

        public CellType this[Point p]
        {
            get { return map[p.X, p.Y]; }
            set { map[p.X, p.Y] = value; }
        }

        public void Print()
        {
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    Console.Write(map[x, y] == CellType.empty ? " " : ((int)map[x, y]).ToString());
                }
                Console.WriteLine();
            }
        }

        public void PrintTo(StringBuilder log)
        {
            for (var y = 0; y < size; y++)
            {
                log.Append("board += \"");
                for (var x = 0; x < size; x++)
                {
                    log.Append(map[x, y] == CellType.empty?" ":((int)map[x, y]).ToString());
                }
                log.Append("\";");
                log.AppendLine();
            }
        }

        public IEnumerable<Func<StepType>> Rules()
        {
            var canReturn = IsEmpty(1, head.Y); //IsEmpty(1, 1) && IsEmpty(2, 1);
            var isOddLine = head.Y%2 == 1;
            var isReturnLine = head.X == 1;
            var isLastHorLine = head.Y == size - 2;
            var isLastVerLine = head.X == size - 2;
            var leftIsReturnLine = head.X - 1 == 1;
            var isOnBegin = head.X == 1 && head.Y == 1;
            var needMoveInRight = isOddLine;
            var needMoveInLeft = !isOddLine;
            



            var isPreLastRow = head.Y == size - 3;
            var canMoveRight = IsEmpty(head.X + 1, head.Y);
            var isEmptyOnLeft = IsEmpty(head.X - 1, head.Y);
            var isEmptyOnUp = IsEmpty(head.X, head.Y - 1);
            var isEmptyDown = IsEmpty(head.X, head.Y + 1);
            var canMoveLeft = isEmptyOnLeft && !leftIsReturnLine;
            var lineUpLeftIsEmpty = Scan().Where(a => a.Item1.Y == head.Y - 1 && a.Item1.X < head.X && a.Item1.X > 0).All(a => IsEmpty(a.Item1.X, a.Item1.Y));
            var lineUpRightIsEmpty = Scan().Where(a => a.Item1.Y == head.Y - 1 && a.Item1.X > head.X && a.Item1.X < size - 1).All(a => IsEmpty(a.Item1.X, a.Item1.Y));
            var upLeftIsNotReturn = head.X - 1 != 1;
            var upRightIsNotLast = head.X + 1 != size - 2;




            var isAppleIsOnReturnLine = apple.X == 1;
            var appleIsOnOurLine = apple.Y == head.Y;
            var appleIsBottonOfUs = apple.Y > head.Y;
            var appleIsOnOurVerLine = apple.X == head.X;
            var appleIsLeftOfUs = apple.X == head.X - 1 && apple.Y == head.Y;
            var appleIsRightOfUs = apple.X == head.X + 1 && apple.Y == head.Y;
            var appleIsLefterOfUs = apple.X < head.X && apple.Y == head.Y;
            var appleIsRighterOfUs = apple.X > head.X && apple.Y == head.Y;
            var appleIsBottomOfUs = apple.X == head.X && apple.Y == head.Y +1;

            var isAppleOnUpOfUs = apple.X == head.X && apple.Y == head.Y - 1;
            var isAppleOnTheLastLine = apple.Y == size - 2;

            var hasAppleUpOfUs = apple.Y == head.Y - 1 && apple.X == head.X;
            var hasBombIsUpperRightCorner1Step = this[head.X + 1, head.Y - 1] == CellType.bomb;
            var hasBombIsUpperRightCorner2Step = this[head.X + 1, head.Y - 2] == CellType.bomb;
            var hasBombIsUpperLeftCorner1Step = this[head.X - 1, head.Y - 1] == CellType.bomb;
            var hasBombIsUpperLeftCorner2Step = this[head.X - 1, head.Y - 2] == CellType.bomb;
            var isBombIsLefterThenMe = bomb.X < head.X;
            var isBombIsRightThenMe = bomb.X > head.X;
            var isBombDownerUs = bomb.X == head.X && bomb.Y == head.Y + 1;
            var isBombIsLeftOnMe = bomb.X == head.X + 1 && bomb.Y == head.Y;
            var isBombIsRightOnMe = bomb.X == head.X + 1 && bomb.Y == head.Y;
            var isBombIsUpOnMe = bomb.X == head.X && bomb.Y == head.Y - 1;


            var tailIsLeftOnMe = this[head.X - 1, head.Y] == CellType.tail;
            var tailIsRightOnMe = this[head.X + 1, head.Y] == CellType.tail;
            var tailIsUpOnMe = this[head.X, head.Y - 1] == CellType.tail;
            var bottomLineDoesNotHaveTail = !Scan().Where(a => a.Item1.Y == head.Y + 1 && a.Item1.X > 1).Any(a => a.Item2 == CellType.tail);
            var bottomLine2DoesNotHaveTail = !Scan().Where(a => a.Item1.Y == head.Y + 2 && a.Item1.X > 1).Any(a => a.Item2 == CellType.tail);

            var length = Scan().Count(a => a.Item2 == CellType.tail);
            

            

            var downsAreEmpty =
                Scan()
                    .Where(a => a.Item1.X == head.X && a.Item1.Y > head.Y && a.Item1.Y < size - 1)
                    .All(a => IsEmpty(a.Item1.X, a.Item1.Y));

            var downIsEmpty = IsEmpty(head.X, head.Y + 1);

           
            Func<StepType> down = () => StepType.down;
            Func<StepType> up = () => StepType.up;
            Func<StepType> right = () => StepType.right;
            Func<StepType> left = () => StepType.left;


            if (length < 70)
            {
                stepswithoutapple = 400;
            }
            else if(lastApple == null || lastApple.Value.X != apple.X || lastApple.Value.Y != apple.Y)
            {
                stepswithoutapple = 400;
                lastApple = apple;
            }
            else
            {
                stepswithoutapple -= 1;
                if (stepswithoutapple == 0)
                {
                    Console.WriteLine("цикл");
                    yield return left;
                }
            }


            // спец правила для ускорения
            // если яблоко на линии возврата, то 
            // хотелось бы сразу спуститься до него и начать возвращаться.
            //yield return new Rule(isAppleIsOnReturnLine && appleIsBottonOfUs && !isReturnLine && downIsEmpty, waiting);
            //yield return new Rule(isAppleIsOnReturnLine && appleIsBottonOfUs && !isReturnLine && !isLastHorLine, down);
            //yield return new Rule(isAppleIsOnReturnLine && appleIsBottonOfUs && !isReturnLine, returning);

            // основные правила
            yield return rule(isReturnLine && !isOnBegin, up);

            // если можем свалиться на яблоко, то падаем
            var haveEnoughSpaceForSnake = (apple.Y - head.Y) + (head.X - 1);
            var notHasTailDowned = Scan().Count(a => a.Item2 == CellType.tail && a.Item1.Y >= head.Y && a.Item1.X > 1) < haveEnoughSpaceForSnake;
            yield return rule(!isReturnLine && appleIsOnOurVerLine && appleIsBottonOfUs && (canReturn || notHasTailDowned) && !isBombDownerUs && downIsEmpty, down);

            // яблоки рядом
            yield return rule(appleIsLeftOfUs, left);
            yield return rule(appleIsRightOfUs && !isLastHorLine && !isPreLastRow, right);
            yield return rule(appleIsBottomOfUs && bottomLineDoesNotHaveTail && bottomLine2DoesNotHaveTail, down);
            yield return rule(isAppleOnUpOfUs && lineUpLeftIsEmpty && isLastHorLine, up);

            // обход мины
            //var bompIsUpOfUs = this[head.X, head.Y - 2] == CellType.bomb;
            //yield return rule(hasAppleUpOfUs && bompIsUpOfUs, up);

            //var needUpperAfterBomb =
            //   needMoveInRight && (hasBombIsUpperRightCorner1Step && lineUpLeftIsEmpty && upLeftIsNotReturn || hasBombIsUpperLeftCorner2Step)
            //   || needMoveInLeft && (hasBombIsUpperLeftCorner1Step && upRightIsNotLast && lineUpRightIsEmpty || hasBombIsUpperRightCorner2Step);
            //yield return rule(needUpperAfterBomb && !isLastVerLine && isEmptyOnUp, up);


            yield return rule(needMoveInLeft && isBombIsLeftOnMe && downIsEmpty, down);
            yield return rule(needMoveInLeft && hasBombIsUpperRightCorner1Step && tailIsUpOnMe, right);
            yield return rule(needMoveInLeft && isBombIsUpOnMe && tailIsLeftOnMe, right);
            yield return rule(needMoveInLeft && hasBombIsUpperLeftCorner1Step && isEmptyOnUp && tailIsLeftOnMe && !isLastVerLine, up);

            yield return rule(needMoveInRight && isBombIsRightOnMe && downIsEmpty, down);
            yield return rule(needMoveInRight && hasBombIsUpperLeftCorner1Step && tailIsUpOnMe, left);
            yield return rule(needMoveInRight && isBombIsUpOnMe && tailIsRightOnMe, left);
            yield return rule(needMoveInRight && hasBombIsUpperRightCorner1Step && isEmptyOnUp && tailIsRightOnMe && !leftIsReturnLine, up);

            
            if (appleIsOnOurLine)
            {
                yield return rule(needMoveInLeft && appleIsRighterOfUs && canMoveRight ,right);
                yield return rule(needMoveInRight && appleIsLefterOfUs && canMoveLeft, left);
            }

            // waiting
            yield return rule(isLastHorLine && isAppleOnUpOfUs && lineUpLeftIsEmpty, left);
            yield return rule(isLastHorLine, left);
            yield return rule(needMoveInRight && canMoveRight && !isLastVerLine, right);
            yield return rule(needMoveInRight && !canMoveRight, down);
            //yield return rule(isPreLastRow && leftIsReturnLine && appleIsBottonOfUs, down);
            yield return rule(isPreLastRow && canMoveRight && downIsEmpty && canMoveRight, right);
            yield return rule(isPreLastRow && !canMoveRight && downIsEmpty, down);
            yield return rule(isPreLastRow && (!canMoveRight || !downIsEmpty), left);

            yield return rule(needMoveInLeft && isEmptyOnLeft && (!leftIsReturnLine || isPreLastRow), left);
            yield return rule(needMoveInLeft && !canMoveLeft && isEmptyDown, down);

            yield return rule(canMoveRight, right);
            yield return rule(canMoveLeft, left);
        }

        public Func<StepType> rule(bool check, Func<StepType> r)
        {
            if (check)
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Check: " + r().ToString());
                }
                return r;
            }
            return null;
        } 

        public StepType Step()
        {
            var rule = Rules().ToArray().First(a => a != null);
           
            return rule();
        }

        private bool IsEmpty(int x, int y) => this[x, y] == CellType.empty || this[x, y] == CellType.apple;
              

        public void Prepare(string board)
        {
            size = (int) Math.Truncate(Math.Sqrt(board.Length));
            map = new CellType[size, size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    map[x, y] = parserMap[board[y*size + x]];
                }
            }

            head = Find(CellType.head);
            apple = Find(CellType.apple);
            bomb = Find(CellType.bomb);
        }

        public Point Find(CellType t) => Scan().First(a => a.Item2 == t).Item1;

        public IEnumerable<Tuple<Point, CellType>> Scan()
        {
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    yield return Tuple.Create(new Point(x, y), map[x, y]);
                }
            }
        } 

        public Dictionary<char, CellType> parserMap = new Dictionary<char, CellType>()
        {
            [' '] = CellType.empty,
            ['☼'] = CellType.wall,
            ['☺'] = CellType.apple,
            ['○'] = CellType.tail,
            ['☻'] = CellType.bomb,
            ['◄'] = CellType.head,
            ['►'] = CellType.head,
            ['▲'] = CellType.head,
            ['▼'] = CellType.head,


            [((int)CellType.empty).ToString()[0]] = CellType.empty,
            [((int)CellType.head).ToString()[0]] = CellType.head,
            [((int)CellType.tail).ToString()[0]] = CellType.tail,
            [((int)CellType.apple).ToString()[0]] = CellType.apple,
            [((int)CellType.bomb).ToString()[0]] = CellType.bomb,
            [((int)CellType.wall).ToString()[0]] = CellType.wall
        };

    }
}

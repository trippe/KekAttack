using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KekAttack
{

    class Program
    {
        public const int _frameswindow = 5;


        private static void Controller()
        {
            while (true)
            {
                ConsoleKeyInfo input = Console.ReadKey(true);
                if (input.Key == ConsoleKey.UpArrow) CommandQueue.Enqueue(Directions.Up);
                if (input.Key == ConsoleKey.RightArrow) CommandQueue.Enqueue(Directions.Right);
                if (input.Key == ConsoleKey.LeftArrow) CommandQueue.Enqueue(Directions.Left);

                if (CommandQueue.Count > 2) CommandQueue.Dequeue();
            }
        }

        private static void Draw()
        {
            Console.SetCursorPosition(0, 0);
            for (int i = 0; i < Field.GetLength(1); i++)
            {
                for (int j = 0; j < Field.GetLength(0); j++)
                {
                    if (Field[j, i] == null)
                        Console.Write(" ");
                    else Console.Write(Field[j, i].Sprite);
                }
                Console.WriteLine();
            }
        }

        private static void Update()
        {
            foreach (BaseObject obj in Field)
            {
                //наличие объекта
                if (obj != null)
                {
                    //-Игрок
                    if (obj.Type == Types.Player)
                    {
                        //--в покое
                        if (obj.DroppedFrames >= 0)
                        {
                            if (obj.Raycast(Directions.Down).Item1.Type == Types.Empty) //проверка на падение
                                obj.DroppedFrames = (int)eDroppedFrames.PlayerJump;
                            //есть действие
                            else if (CommandQueue.Count != 0)
                            {
                                if ((obj as Player).lastAction == Directions.Up)   //восстановление после прыжка
                                {
                                    obj.DroppedFrames = (int)eDroppedFrames.PlayerRecover;
                                    (obj as Player).lastAction = Directions.Null;
                                }
                                else
                                {
                                    Directions command = CommandQueue.Dequeue();  //действие
                                    Types commandResult = obj.Push(command);
                                    if (commandResult == Types.Empty)
                                    {
                                        (obj as Player).lastAction = command;
                                        command = Directions.Null;
                                    }
                                    else if (commandResult == Types.Box) //смещение ящика
                                    {
                                        //проверка блокировки ящика


                                        //проверка блокировки ящика
                                        Box box = (Box)obj.Raycast(command).Item1;
                                        if ((box.Raycast(Directions.Up).Item1.Type == Types.Empty) && (box.Raycast(command).Item1.Type == Types.Empty))
                                            if (box.Raycast(Directions.Down).Item1.Type != Types.Empty)
                                                if (box.Push(command) == Types.Empty)
                                                    if (obj.Push(command) == Types.Empty)
                                                    {
                                                        (obj as Player).lastAction = command;
                                                        command = Directions.Null;
                                                    }
                                    }
                                }
                            }
                        }
                        //--в задержке
                        else if (obj.DroppedFrames != 0)
                        {
                            obj.DroppedFrames += _frameswindow;
                            if (obj.DroppedFrames == 0) //падение в конце задержки
                            {
                                obj.Push(Directions.Down);
                            }
                            else if (CommandQueue.Count != 0) //управление в прыжке
                            {
                                Directions command = CommandQueue.Dequeue();
                                //прыжок вбок
                                if ((obj as Player).lastAction == Directions.Up && (command == Directions.Right || command == Directions.Left))
                                {
                                    //возможность прыжка вбок
                                    bool raycast_success = false;
                                    if (command == Directions.Right && obj.Raycast(Directions.LowerRight).Item1.Type == Types.Box) raycast_success = true;
                                    else if (command == Directions.Left && obj.Raycast(Directions.LowerLeft).Item1.Type == Types.Box) raycast_success = true;

                                    if (raycast_success)
                                    {
                                        Types commandResult = obj.Push(command);
                                        if (commandResult == Types.Empty)
                                        {
                                            obj.DroppedFrames = 0;
                                            (obj as Player).lastAction = command;
                                        }
                                        else if (commandResult == Types.Box)
                                        {
                                            //проверка блокировки ящика
                                            Box box = (Box)obj.Raycast(command).Item1;
                                            if ((box.Raycast(Directions.Up).Item1.Type == Types.Empty) && (box.Raycast(command).Item1.Type == Types.Empty))
                                                if (box.Raycast(Directions.Down).Item1.Type != Types.Empty)
                                                    if (box.Push(command) == Types.Empty)
                                                        if (obj.Push(command) == Types.Empty)
                                                        {
                                                            (obj as Player).lastAction = command;
                                                            command = Directions.Null;
                                                        }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    else if (obj.Type == Types.Box)
                    {
                        //в покое
                        if (obj.DroppedFrames >= 0)
                        {
                            if (obj.Raycast(Directions.Down).Item1.Type == Types.Empty) //проверка на падение
                                obj.DroppedFrames = (int)eDroppedFrames.Box;
                        }
                        //в задержке
                        else if (obj.DroppedFrames != 0)
                        {
                            obj.DroppedFrames += _frameswindow;
                            if (obj.DroppedFrames == 0) //падение в конце задержки
                            {
                                obj.Push(Directions.Down);
                            }
                        }
                    }
                }
            }
        }


        public static BaseObject[,] Field;
        public static Queue<Directions> CommandQueue;

        public static int _boxGenerationCounterConst = -1000;
        public static int boxGenerationCounter = _boxGenerationCounterConst;
        //public static Directions command = Directions.Null;
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Field = new BaseObject[22, 10];
            CommandQueue = new Queue<Directions>();

            CreateFieldBorders();

            Thread controllerThread = new Thread(new ThreadStart(Controller));

            controllerThread.Start();

            Player player = new Player(Field, 4, 2);

            GenerateLayout();


            while (true)
            {
                CheckRowCompletion();

                GenerateBox();
                Update();
                Draw();
                
                Thread.Sleep(_frameswindow);
            }
        }

        private static bool CheckRowCompletion()
        {
            bool result = true;
            for (int i = 1; i < Field.GetLength(0) - 1; i++)
            {
                if (Field[i, Field.GetLength(1) - 2] == null)
                {
                    result = false;
                    break;
                }
                else if (Field[i, Field.GetLength(1) - 2].Type != Types.Box)
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                for (int i = 1; i < Field.GetLength(0) - 1; i++)
                {
                    Field[i, Field.GetLength(1) - 2] = null;
                }
            }

            return result;

        }

        private static void GenerateBox()
        {
            if (boxGenerationCounter == 0)
            {
                Random rnd = new Random();
                new Box(Field, rnd.Next(1, Field.GetLength(0) - 1), 1);
                boxGenerationCounter = _boxGenerationCounterConst;
            }
            else
            {
                boxGenerationCounter += _frameswindow;
            }


        }

        private static void GenerateLayout()
        {
            Random rnd = new Random();
            int j = Field.GetLength(1) - 2;
            for (int i = 1; i < Field.GetLength(0) - 1; i++)
            {
                Thread.Sleep(10);
                try
                {
                    if (rnd.Next(1, 3) == 1)
                        new Box(Field, i, j);
                }
                catch (Exception)
                { }
            }
            for (int i = 1; i < Field.GetLength(0) - 1; i++)
            {
                Thread.Sleep(10);

                try
                {
                    if (rnd.Next(1, 6) == 1)
                        new Box(Field, i, j - 1);
                }
                catch (Exception)
                { }
            }

        }

        private static void CreateFieldBorders()
        {
            for (int i = 0; i < Field.GetLength(0); i++)
            {
                for (int j = 0; j < Field.GetLength(1); j++)
                {
                    if ((i == 0 && j == 0) || (i == (Field.GetLength(0) - 1) && j == 0) || (i == 0 && (j == Field.GetLength(1) - 1)) || ((i == Field.GetLength(0) - 1) && (j == Field.GetLength(1) - 1)))
                        new Border(Field, Sprites.CBorder, i, j);
                    else if (i == 0 || (i == (Field.GetLength(0) - 1)))
                        new Border(Field, Sprites.VBorder, i, j);
                    else if (j == 0 || (j == (Field.GetLength(1) - 1)))
                        new Border(Field, Sprites.HBorder, i, j);

                }
            }
        }
    }
}

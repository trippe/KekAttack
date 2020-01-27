using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static KekAttack.UI;

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

        private static void DrawUI(Layout ui)
        {
            foreach (UIElement elem in ui.Elements)
            {
                Console.SetCursorPosition(elem.x, elem.y);
                Console.Write(elem.text);
            }
            Button actb = null;
            if ((actb = ui.GetActive()) != null)
            {
                Console.SetCursorPosition(actb.x, actb.y);
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.Gray;
                Console.Write(actb.text);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
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

            Layout MainMenu = new Layout();
            MainMenu.Elements.Add(new Label("label  1", 12, 3));
            MainMenu.Elements.Add(new Label("label  2", 12, 7));
            MainMenu.CreateButton("button 1", 3, 3, 0, 0, null);
            MainMenu.CreateButton("button 2", 3, 5, 0, 1, null);
            MainMenu.CreateButton("button 3", 22, 3, 1, 0, null);
            MainMenu.CreateButton("button 4", 12, 5, 1, 1, null);
            MainMenu.CreateButton("button 5", 3, 7, 0, 2, null);



            //while (true) //main menu cycle
            //{
            //    DrawUI(MainMenu);
            //                    ConsoleKeyInfo input = Console.ReadKey(true);
            //    if (input.Key == ConsoleKey.UpArrow) MainMenu.ActivateButton(Directions.Up);
            //    else if (input.Key == ConsoleKey.RightArrow) MainMenu.ActivateButton(Directions.Right);
            //    else if (input.Key == ConsoleKey.LeftArrow) MainMenu.ActivateButton(Directions.Left);
            //    else if (input.Key == ConsoleKey.DownArrow) MainMenu.ActivateButton(Directions.Down);
            //}

            CreateFieldBorders();

            Thread controllerThread = new Thread(new ThreadStart(Controller));

            controllerThread.Start();

            Player player = new Player(Field, 4, 2);

            GenerateLayout();

            while (true) //main cycle
            {
                GenerateBox();
                Update();
                Draw();

                Thread.Sleep(_frameswindow);
                CheckRowCompletion();
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

            if (result) //очистка ряда
            {
                Thread.Sleep(100);
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
                try
                {
                    Random rnd = new Random();
                    new Box(Field, rnd.Next(1, Field.GetLength(0) - 1), 1);
                    boxGenerationCounter = _boxGenerationCounterConst;
                }
                catch (Box.CreateBoxException)
                {
                    GameOver();
                }
            }
            else
            {
                boxGenerationCounter += _frameswindow;
            }


        }

        private static void GameOver()
        {
            throw new NotImplementedException();
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

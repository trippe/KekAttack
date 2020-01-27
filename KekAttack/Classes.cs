using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KekAttack
{
    class UI
    {
        public abstract class UIElement
        {
            public string text;
            public int x;
            public int y;
        }

        public class Label : UIElement
        {
            public Label(string Text, int X, int Y)
            {
                text = Text;
                x = X;
                y = Y;
            }
        }

        public class Button : UIElement
        {
            public bool Active = false;

            public int xRelative;
            public int yRelative;

            public delegate object Action();
            private Action action;

            public void Highlight()
            {
                if (Active == false) Active = true;
                else Active = false;
            }

            public Button(string Text, int X, int Y, int XRelative, int YRelative, Action act)
            {
                text = Text;
                x = X;
                y = Y;
                xRelative = XRelative;
                yRelative = YRelative;
                action = act;
            }
        }

        public class Layout
        {
            public List<UIElement> Elements = new List<UIElement>();

            private bool HasActiveButton()
            {
                if (Elements.Where(x => (x.GetType() == typeof(Button)) && ((x as Button).Active == true)).Count() != 0) return true;
                else return false;
            }

            public Button GetActive()
            {
                return Elements.FirstOrDefault(x => (x.GetType() == typeof(Button)) && ((x as Button).Active)) as Button;
            }

            public void CreateButton(string Text, int X, int Y, int XRelative, int YRelative, UI.Button.Action act)
            {
                Button button = new Button(Text, X, Y, XRelative, YRelative, act);
                if (!HasActiveButton()) button.Highlight();
                Elements.Add(button);
            }

            public void ActivateButton(Directions direction)
            {
                Button cActive = null;
                Button target = null;
                if ((cActive = GetActive()) != null)
                {
                    switch (direction)
                    {
                        case Directions.Left:
                            target = (Elements.Where(x => (x.GetType() == typeof(Button)) && ((x as Button).Active == false))
                                .Where(x => (x as Button).yRelative == cActive.yRelative && (x as Button).xRelative < cActive.xRelative)
                                .OrderBy(x => (x as Button).xRelative)
                                .LastOrDefault() as Button);
                            break;
                        case Directions.Right:
                            target = (Elements.Where(x => (x.GetType() == typeof(Button)) && ((x as Button).Active == false))
                                .Where(x => (x as Button).yRelative == cActive.yRelative && (x as Button).xRelative > cActive.xRelative)
                                .OrderBy(x => (x as Button).xRelative)
                                .FirstOrDefault() as Button);
                            break;
                        case Directions.Down:
                            target = (Elements.Where(x => (x.GetType() == typeof(Button)) && ((x as Button).Active == false))
                                .Where(x => (x as Button).xRelative == cActive.xRelative && (x as Button).yRelative > cActive.yRelative)
                                .OrderBy(x => (x as Button).xRelative)
                                .FirstOrDefault() as Button);
                            break;
                        case Directions.Up:
                            target = (Elements.Where(x => (x.GetType() == typeof(Button)) && ((x as Button).Active == false))
                                .Where(x => (x as Button).xRelative == cActive.xRelative && (x as Button).yRelative < cActive.yRelative)
                                .OrderBy(x => (x as Button).xRelative)
                                .LastOrDefault() as Button);
                            break;
                    }
                    if (target != null)
                    {
                        target.Active = true;
                        cActive.Active = false;
                    }
                }
            }
        }
    }


    class Border : BaseObject
    {
        public Border(BaseObject[,] parentField, Sprites objectSprite, int posX, int posY)
        {
            Type = Types.Border;
            Field = parentField;
            Sprite = (char)objectSprite;

            if (this.Create(posX, posY) != Types.Empty)
            {
                throw new Exception("Failed to create border");
            }
        }
    }

    class Box : BaseObject
    {
        public Box(BaseObject[,] parentField, int posX, int posY)
        {
            Random rnd = new Random();
            switch (rnd.Next(1, 3))
            {
                case 1:
                    Sprite = (char)Sprites.Box1;
                    break;
                case 2:
                    Sprite = (char)Sprites.Box2;
                    break;
                case 3:
                    Sprite = (char)Sprites.Box3;
                    break;
            }
            Type = Types.Box;
            Field = parentField;

            if (this.Create(posX, posY) != Types.Empty)
            {
                throw new CreateBoxException();
            }
        }

        public Box(BaseObject[,] parentField, Sprites objectSprite, int posX, int posY)
        {
            Type = Types.Box;
            Field = parentField;
            Sprite = (char)objectSprite;

            if (this.Create(posX, posY) != Types.Empty)
            {
                throw new CreateBoxException();
            }
        }

        public class CreateBoxException : Exception
        {
            public CreateBoxException()
            {
            }
        }
    }

    class Player : BaseObject
    {
        private static Player instance = null;

        public Directions lastAction = Directions.Null;

        public Player(BaseObject[,] parentField, int posX, int posY)
        {
            if (instance == null)
            {
                Type = Types.Player;
                Field = parentField;
                Sprite = (char)Sprites.Player;

                if (this.Create(posX, posY) != Types.Empty)
                {
                    throw new Exception("Failed to create Player");
                }

                instance = this;
            }
            else
            {
                throw new Exception("Player already exists");
            }
        }
    }

    class Object : BaseObject
    {
        public Object(Types objectType, BaseObject[,] parentField)
        {
            Type = objectType;
            Field = parentField;
            switch (Type)
            {
                case Types.Border:
                    Sprite = (char)Sprites.CBorder;
                    break;
                case Types.Box:
                    Sprite = (char)Sprites.Box1;
                    break;
                case Types.Player:
                    Sprite = (char)Sprites.Player;
                    break;
                default:
                    Sprite = (char)Sprites.Unident;
                    break;
            }
        }

        public Object(Types objectType, BaseObject[,] parentField, int posX, int posY)
        {
            Type = objectType;
            Field = parentField;
            switch (Type)
            {
                case Types.Border:
                    Sprite = (char)Sprites.CBorder;
                    break;
                case Types.Box:
                    Sprite = (char)Sprites.Box1;
                    break;
                case Types.Player:
                    Sprite = (char)Sprites.Player;
                    break;
                default:
                    Sprite = (char)Sprites.Unident;
                    break;
            }

            if (this.Create(posX, posY) != Types.Empty)
            {
                throw new Exception("Failed to create object");
            }
        }

        public Object(Types objectType, BaseObject[,] parentField, Sprites objectSprite)
        {
            Type = objectType;
            Field = parentField;
            Sprite = (char)objectSprite;
        }

        public Object(Types objectType, BaseObject[,] parentField, Sprites objectSprite, int posX, int posY)
        {
            Type = objectType;
            Field = parentField;
            Sprite = (char)objectSprite;

            if (this.Create(posX, posY) != Types.Empty)
            {
                throw new Exception("Failed to create object");
            }
        }
    }

    public abstract class BaseObject
    {
        private int x = -1;
        private int y = -1;

        public BaseObject[,] Field;
        public Types Type = Types.Unident;
        public char Sprite = (char)Sprites.Unident;
        public int DroppedFrames = 0;

        public Types Push(Directions command)
        {
            int destination_x = x;
            int destination_y = y;

            switch (command)
            {
                case Directions.Left:
                    destination_x = x - 1;
                    destination_y = y;
                    break;
                case Directions.Right:
                    destination_x = x + 1;
                    destination_y = y;
                    break;
                case Directions.Up:
                    destination_x = x;
                    destination_y = y - 1;
                    break;
                case Directions.Down:
                    destination_x = x;
                    destination_y = y + 1;
                    break;
            }

            if (Field[destination_x, destination_y] == null)
            {
                Field[x, y] = null;
                x = destination_x;
                y = destination_y;
                Field[x, y] = this;
                return Types.Empty;
            }
            else
            {
                return Field[destination_x, destination_y].Type;
            }
        }
        public Types Move(int destination_x, int destination_y)
        {
            if (Field[destination_x, destination_y] == null)
            {
                Field[x, y] = null;
                x = destination_x;
                y = destination_y;
                Field[x, y] = this;
                return Types.Empty;
            }
            else
            {
                return Field[destination_x, destination_y].Type;
            }
        }

        public Types Create(int destination_x, int destination_y)
        {
            if (Field != null)
            {
                if (Field[destination_x, destination_y] == null)
                {
                    x = destination_x;
                    y = destination_y;
                    Field[x, y] = (BaseObject)this;
                    return Types.Empty;
                }
                else
                {
                    return Field[destination_x, destination_y].Type;
                }
            }
            else
            {
                return Types.Empty;
            }
        }

        public Tuple<BaseObject, int> Raycast(Directions direction) //type, range
        {
            BaseObject castObject = null;

            switch (direction)
            {
                case Directions.Left:
                    if (Field[x - 1, y] == null) castObject = new Object(Types.Empty, null, x - 1, y);
                    else
                        castObject = Field[x - 1, y];
                    break;
                case Directions.Right:
                    if (Field[x + 1, y] == null) castObject = new Object(Types.Empty, null, x + 1, y);
                    else
                        castObject = Field[x + 1, y];
                    break;
                case Directions.Up:
                    if (Field[x, y - 1] == null) castObject = new Object(Types.Empty, null, x, y - 1);
                    else
                        castObject = Field[x, y - 1];
                    break;
                case Directions.Down:
                    if (Field[x, y + 1] == null) castObject = new Object(Types.Empty, null, x, y + 1);
                    else
                        castObject = Field[x, y + 1];
                    break;
                case Directions.LowerLeft:
                    if (Field[x - 1, y + 1] == null) castObject = new Object(Types.Empty, null, x - 1, y + 1);
                    else
                        castObject = Field[x - 1, y + 1];
                    break;
                case Directions.LowerRight:
                    if (Field[x + 1, y + 1] == null) castObject = new Object(Types.Empty, null, x + 1, y + 1);
                    else
                        castObject = Field[x + 1, y + 1];
                    break;

            }

            return new Tuple<BaseObject, int>(castObject, 1);
        }

    }

}

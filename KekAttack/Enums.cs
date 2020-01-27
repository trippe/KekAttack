using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KekAttack
{
    public enum Sprites
    {
        Empty   =   ' ',

        Box1    =   '▓',

        Box2    =   '▒',

        Box3    =   '░',
        VBorder =   '║',
        HBorder =   '═',
        CBorder =   '╬',
        Player  =   'T',
        Unident =   '?'
    };

    public enum Types
    {
        Empty,
        Player,
        Box,
        Border,
        Subsprite,
        Unident
    }

    public enum Directions
    {
        Null,
        Up,
        UpperRight,
        Right,
        LowerRight,
        Down,
        LowerLeft,
        Left,
        UpperLeft
    }

    public enum eDroppedFrames
    {
        Player = 0,
        PlayerJump = -150,
        PlayerRecover = -50,
        Box = -300
    }
}

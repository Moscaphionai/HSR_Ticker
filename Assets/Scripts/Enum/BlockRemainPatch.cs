using System;

[Flags]
public enum BlockRemainPatch
{
    /*
     *               upLeft Upright
     *
     *         LeftUp            RightUp
     *                    orig
     *         LeftDown          RightDown
     *
     */
    None = 0,
    UpLeft = 1 << 0,
    UpRight = 1 << 1,
    LeftUp = 1 << 2,
    LeftDown = 1 << 3,
    RightUp = 1 << 4,
    RightDown = 1 << 5,

    Walkable = UpLeft | UpRight,
    Full = LeftUp | LeftDown | RightUp | RightDown | UpLeft | UpRight
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbis
{
    public enum Dificuty : byte
    {
        Easy,
        Normal,
        Hard
    }

    [Flags]
    public enum Note : byte
    {
        Up    = 1 << 0,
        Down  = 1 << 1,
        Left  = 1 << 2,
        Right = 1 << 3
    }

    [Flags]
    public enum NoteState : byte
    {
        Static = 1 << 0,
        Miss = 1 << 1,
        Hit = 1 << 2
    }

    public enum Map : byte
    {
        Stage,
        Philly,
        Limo,
        Halloween
    }

    public enum EventTarget : byte
    {
        Speaker,
        Player1,
        Player2
    }
}

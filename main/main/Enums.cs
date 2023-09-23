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

    public struct SFXScheme
    {
        public SFXType Countdown1;
        public SFXType Countdown2;
        public SFXType Countdown3;
        public SFXType CountdownGo;

        public SFXType Dies;
        public SFXType Win;
    }
    public enum SFXType
    {
        NONE,
        Countdown1,
        Countdown2,
        Countdown3,
        CountdownGo,
        Countdown1Pixel,
        Countdown2Pixel,
        Countdown3Pixel,
        CountdownGoPixel,
        CarPass0,
        CarPass1,
        NoteMiss1,
        NoteMiss2,
        NoteMiss3,
        Dies,
        DiesPixel,
        DeadLoop,
        DeadRetry,
        MenuChoice,
        MenuConfirm,
        MenuBack,
        Girlfriend1,
        Girlfriend2,
        Girlfriend3,
        Girlfriend4,
        ThunderA,
        ThunderB,
        Train
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
        Halloween,
        Christmas,
        ChristmasEvil
    }

    public enum EventTarget : byte
    {
        Speaker,
        Player1,
        Player2
    }
}

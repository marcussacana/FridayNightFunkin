using Orbis.Game;
using OrbisGL.GL2D;
using System.Collections.Generic;

namespace Orbis
{
    public struct NewStatusEvent
    {
        public EventTarget Target;
        public string AnimationSufix;
        public string AnimationPrefix;
        public string NewAnimation;

        public SongNoteEntry NoteInfo;
    }

    public struct SongInfo
    {
        public string Name;
        public Dificuty Dificuty;

        public string Speaker;
        public string Player1;
        public string Player2;

        public Map BG;

        public SongData song;

        public SongInfo()
        {
            Name = string.Empty;
            Dificuty = Dificuty.Normal;
            Speaker = "gf";
            Player1 = "bf";
            Player2 = null;
            BG = Map.Stage;
            song = new SongData();
        }
    }

    public struct SongData
    {
        public float bpm;
        public int sections;
        public bool needsVoices;
        public string player1;
        public string player2;
        public string song;
        public List<object> sectionLengths;
        public float speed;
        public bool validScore;

        public List<NoteInfo> notes;
    }
    public struct NoteInfo
    {
        public float lengthInSteps;
        public float bmp;
        public bool changeBpm;
        public bool mustHitSection;

        public List<List<float>> sectionNotes;

        public int typeOfSection;

        public bool altAnim;
    }
}

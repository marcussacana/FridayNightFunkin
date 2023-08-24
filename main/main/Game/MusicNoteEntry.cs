using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Orbis.Game
{
    public class SongNoteEntry : GLObject2D
    {
        NoteMenu Player;
        public Blank2D BackLayer { get; private set; } = new Blank2D();
        public SpriteAtlas2D Render { get; private set; }

        public Note Type { get; private set; }

        public bool CanBeHit { get; private set; }

        long TargetTick;
        float TargetMS;
        float DurationMS;
        float YPerMS;
        long SongBeginTick;

        Vector2 InitialPos;

        public SongNoteEntry(SpriteAtlas2D Render, NoteMenu Player, Note Type, float TargetMS, float DurationMS, float YPerMS)
        {
            this.Render = Render;
            this.Type = Type;
            this.TargetMS = TargetMS;
            this.DurationMS = DurationMS;
            this.YPerMS = YPerMS;
            this.Player = Player;
            this.TargetTick = (long)TargetMS * Constants.ORBIS_MILISECOND;

            AddChild(BackLayer);
            AddChild(Render);
            SetupNote();
        }

        private void SetupNote()
        {
            switch (Type)
            {
                case Note.Up:
                    Render.SetActiveAnimation(NotesNames.UP_NOTE);
                    Position = new Vector2(NoteMenu.UpX, TargetMS * YPerMS);
                    break;
                case Note.Left:
                    Render.SetActiveAnimation(NotesNames.LEFT_NOTE);
                    Position = new Vector2(NoteMenu.LeftX, TargetMS * YPerMS);
                    break;
                case Note.Right:
                    Render.SetActiveAnimation(NotesNames.RIGHT_NOTE);
                    Position = new Vector2(NoteMenu.RightX, TargetMS * YPerMS);
                    break;
                case Note.Down:
                    Render.SetActiveAnimation(NotesNames.DOWN_NOTE);
                    Position = new Vector2(NoteMenu.DownX, TargetMS * YPerMS);
                    break;
            }

            Height = Render.Height;
            Width = Render.Width;

            List<SpriteAtlas2D> SustainNotes = new List<SpriteAtlas2D>();

            float SustainY = DurationMS*YPerMS;
            for (int i = 0, x = 0; i < SustainY; x++)
            {
                var Sustain = Render.Clone(false);
                Sustain.Opacity = 150;
                Sustain.Position = new Vector2(Width / 2, i - (x * 1.5f));
                
                switch (Type)
                {
                    case Note.Up:
                        Sustain.SetActiveAnimation(NotesNames.UP_BAR);
                        break;
                    case Note.Down:
                        Sustain.SetActiveAnimation(NotesNames.DOWN_BAR);
                        break;
                    case Note.Left:
                        Sustain.SetActiveAnimation(NotesNames.LEFT_BAR);
                        break;
                    case Note.Right:
                        Sustain.SetActiveAnimation(NotesNames.RIGHT_BAR);
                        break;
                }

                i += Sustain.Height;

                Sustain.Position += new Vector2(-(Sustain.Width / 2), Height / 2);

                SustainNotes.Add(Sustain);
            }

            if (SustainNotes.Any())
            {
                var LastNote = SustainNotes.Last();

                switch (Type)
                {
                    case Note.Up:
                        LastNote.SetActiveAnimation(NotesNames.UP_BAR_END);
                        break;
                    case Note.Down:
                        LastNote.SetActiveAnimation(NotesNames.DOWN_BAR_END);
                        break;
                    case Note.Left:
                        LastNote.SetActiveAnimation(NotesNames.LEFT_BAR_END);
                        break;
                    case Note.Right:
                        LastNote.SetActiveAnimation(NotesNames.RIGHT_BAR_END);
                        break;
                }
            }

            foreach (var Note in SustainNotes)
                BackLayer.AddChild(Note);
        }

        public void SetSongBegin(long Tick)
        {
            SongBeginTick = Tick;
        }

        public override void Draw(long Tick)
        {
            if (SongBeginTick == 0)
                return;

            long ElapsedTick = Tick - SongBeginTick;
            if (ElapsedTick > Constants.ORBIS_MILISECOND)
            {
                long ReamingMS = (TargetTick - ElapsedTick) / Constants.ORBIS_MILISECOND;

                if (ReamingMS < 0)
                {
                    CanBeHit = true;
                    Render.Opacity = 0;
                }

                if (ReamingMS + DurationMS < -100)
                {
                    Render.Opacity = 0;
                    CanBeHit = false;
                }

                float CurrentY = YPerMS * ReamingMS;

                foreach (var Sustain in BackLayer.Childs)
                {
                    float SustainDistance = ((-Sustain.Position.Y) + Height / 2);
                    if (Sustain.Visible && SustainDistance > CurrentY)
                        Sustain.Visible = false;
                }

                Position = new Vector2(Position.X, CurrentY);
            }
            base.Draw(Tick);
        }
    }
}

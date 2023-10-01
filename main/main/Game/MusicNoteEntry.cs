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
        public Blank2D BackLayer { get; private set; } = new Blank2D();
        public SpriteAtlas2D Render { get; private set; }

        public Note Type { get; private set; }

        public bool CanBeHit { get; private set; }

        public bool Hitted { get; set; }

        public bool AltAnimation { get; set; }

        const float HitZoneOffset = ((10f / 60) * 1000);

        const float GoodDistance = HitZoneOffset * 0.2f;
        const float BadDistance = HitZoneOffset * 0.75f;
        const float ShitDistance = HitZoneOffset * 0.9f;

        float SustainPoints = 0;

        public float Score = 0;

        float TargetMS;
        float DurationMS;
        float YPerMS;
        long SongBeginTick;

        public event EventHandler OnNoteElapsed;
        public event EventHandler OnNoteHeaderElapsed;
        public event EventHandler OnNoteReached;

        public SongNoteEntry(GLObject2D Render, Note Type, float TargetMS, float DurationMS, float YPerMS)
        {
            this.Render = (SpriteAtlas2D)Render;
            this.Type = Type;
            this.TargetMS = TargetMS;
            this.DurationMS = DurationMS;
            this.YPerMS = YPerMS;

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

            SetupSustain();
        }

        private void SetupSustain()
        {
            List<SpriteAtlas2D> SustainNotes = new List<SpriteAtlas2D>();

            float SustainY = DurationMS * YPerMS;
            for (int i = 0, x = 0; i < SustainY; x++)
            {
                var Sustain = (SpriteAtlas2D)Render.Clone(false);
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
            if (SongBeginTick == 0 || Tick < SongBeginTick)
                return;

            long ElapsedTick = Tick - SongBeginTick;
            if (ElapsedTick > Constants.ORBIS_MILISECOND)
            {
                long ElapsedMS = ElapsedTick / Constants.ORBIS_MILISECOND;
                long DistanceMS = (long)TargetMS - ElapsedMS;

                //Original game tries to make easy to hit a
                //note after the proper time than before.
                if (DistanceMS < HitZoneOffset * 0.5 && !CanBeHit)
                {
                    CanBeHit = true;
                    OnNoteReached?.Invoke(this, EventArgs.Empty);
                }

                if (DistanceMS < -HitZoneOffset && Render.Opacity != 0)
                {
                    Render.Opacity = 0;
                    OnNoteHeaderElapsed?.Invoke(this, EventArgs.Empty);
                }

                if (DistanceMS + DurationMS < -HitZoneOffset)
                {
                    EndNote();
                    return;
                }

                if (Render.Visible && CanBeHit && Hitted)
                {
                    Render.Visible = false;
                    ComputeScore(DistanceMS);
                    
                    // if we dispose the note too fast, no animation will be saw
                    // so, let's keep the note active while it still in HitZoneOffset

                    SustainPoints = Math.Max(Score * 0.2f, 5);
                }

                float CurrentY = YPerMS * DistanceMS;
                UpdateSustainVisibility(CurrentY);

                Position = new Vector2(Position.X, CurrentY);
            }
            base.Draw(Tick);
        }

        private void UpdateSustainVisibility(float CurrentY)
        {
            foreach (var Sustain in BackLayer.Childs)
            {
                if (!Sustain.Visible)
                    continue;

                float SustainDistance = ((-Sustain.ZoomPosition.Y) + Height / 2);
                if (SustainDistance > CurrentY)
                {
                    if (CanBeHit && Hitted)
                        Score += SustainPoints;

                    if (Hitted && Sustain.Opacity == 150)
                        Sustain.Visible = false;
                    else
                        Sustain.Opacity = 140;
                }
            }
        }

        private void ComputeScore(long DistanceMS)
        {
            var AbsReamingMS = Math.Abs(DistanceMS);
            if (AbsReamingMS < GoodDistance)
            {
                Score = 200;
            }
            else if (AbsReamingMS < BadDistance)
            {
                Score = 100;
            }
            else if (AbsReamingMS < ShitDistance)
            {
                Score = 50;
            }
        }

        private void EndNote()
        {
            CanBeHit = false;
            SongBeginTick = 0;
            OnNoteElapsed?.Invoke(this, EventArgs.Empty);
        }
    }
}

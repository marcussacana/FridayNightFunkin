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

        public SpriteAtlas2D SustainRender { get; private set; }
        public SpriteAtlas2D SustainEndRender { get; private set; }

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

        private float SustainBegin;
        private float SustainLength;
        private float SustainHeight;

        public event EventHandler OnSustainMissed;
        public event EventHandler OnSustainHit;
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
            SustainLength = DurationMS * YPerMS;
            SustainRender = (SpriteAtlas2D)Render.Clone(false);
            SustainRender.Opacity = 150;

            SustainBegin = SustainRender.ZoomPosition.Y;
        }
        private void SetSustainFrame()
        {
            switch (Type)
            {
                case Note.Up:
                    SustainRender.SetActiveAnimation(NotesNames.UP_BAR);
                    break;
                case Note.Down:
                    SustainRender.SetActiveAnimation(NotesNames.DOWN_BAR);
                    break;
                case Note.Left:
                    SustainRender.SetActiveAnimation(NotesNames.LEFT_BAR);
                    break;
                case Note.Right:
                    SustainRender.SetActiveAnimation(NotesNames.RIGHT_BAR);
                    break;
            }
        }

        private void SetSustainEndFrame()
        {
            switch (Type)
            {
                case Note.Up:
                    SustainRender.SetActiveAnimation(NotesNames.UP_BAR_END);
                    break;
                case Note.Down:
                    SustainRender.SetActiveAnimation(NotesNames.DOWN_BAR_END);
                    break;
                case Note.Left:
                    SustainRender.SetActiveAnimation(NotesNames.LEFT_BAR_END);
                    break;
                case Note.Right:
                    SustainRender.SetActiveAnimation(NotesNames.RIGHT_BAR_END);
                    break;
            }
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
                Position = new Vector2(Position.X, CurrentY);

                UpdateSustainVisibility(ZoomPosition.Y);
            }

            DrawSustain(Tick);
            base.Draw(Tick);
        }
        private void DrawSustain(long Tick)
        {
            if (!Visible || SustainLength <= 0 || ZoomPosition.Y > 2000)
                return;
            
            SetSustainFrame();
            SustainRender.Position = AbsolutePosition;
            SustainRender.ZoomPosition += new Vector2(ZoomWidth / 2 - (SustainRender.ZoomWidth / 2), ZoomHeight / 2);
            SustainHeight = SustainRender.ZoomHeight;

            for (float y = SustainBegin; y < SustainLength; y += SustainRender.ZoomHeight)
            {
                bool Last = y + SustainRender.ZoomHeight > SustainLength;

                if (Last)
                {
                    SetSustainEndFrame();

                    SustainRender.ZoomPosition -= new Vector2(0, 20);
                }

                SustainRender.ZoomPosition += new Vector2(0, SustainRender.ZoomHeight);
                SustainRender.Draw(Tick);
                
                if (Last)
                    break;
            }
        }

        private void UpdateSustainVisibility(float CurrentY)
        {
            if (SustainLength <= 0 && SustainBegin < SustainLength)
                return;

            while (CurrentY + (SustainBegin + SustainHeight / 2) <= 0)
            {
                SustainBegin += SustainRender.ZoomHeight;

                if (CanBeHit && Hitted)
                {
                    Score += SustainPoints;
                    OnSustainHit?.Invoke(this, null);
                    continue;
                }

                OnSustainMissed?.Invoke(this, null);
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

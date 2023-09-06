using OrbisGL;
using OrbisGL.GL2D;
using System;
using System.Numerics;

namespace Orbis.Game
{
    public class PlayerNoteEntry : IDisposable
    {
        public SpriteAtlas2D Render { get; private set;}
        public SpriteAtlas2D Overlay { get; set; }

        int StaticID;
        string PressAnim;

        public Note Type { get; private set; }

        public NoteState State { get; private set; }

        public PlayerNoteEntry(GLObject2D Render, Note Type)
        {
            this.Render = (SpriteAtlas2D)Render;
            this.Type = Type;
            
            Overlay = (SpriteAtlas2D)this.Render.Clone(false);
            Overlay.Position -= new Vector2(40, 40);

            switch (Type)
            {
                case Note.Up:
                    StaticID = NotesNames.UP_FRAME_ID;
                    PressAnim = NotesNames.UP_PRESS;
                    Overlay.SetActiveAnimation(NotesNames.UP_HIT);
                    break;
                case Note.Down:
                    StaticID = NotesNames.DOWN_FRAME_ID;
                    PressAnim = NotesNames.DOWN_PRESS;
                    Overlay.SetActiveAnimation(NotesNames.DOWN_HIT);
                    break;
                case Note.Left:
                    StaticID = NotesNames.LEFT_FRAME_ID;
                    PressAnim = NotesNames.LEFT_PRESS;
                    Overlay.SetActiveAnimation(NotesNames.LEFT_HIT);
                    break;
                case Note.Right:
                    StaticID = NotesNames.RIGHT_FRAME_ID;
                    PressAnim = NotesNames.RIGHT_PRESS;
                    Overlay.SetActiveAnimation(NotesNames.RIGHT_HIT);
                    break;
            }

            Render.AddChild(Overlay);

            SetState(NoteState.Static);
        }

        public void SetState(NoteState NewState)
        {
            if (NewState == State)
                return;

            State = NewState;

            if (State == NoteState.Static)
            {
                Render.SetActiveAnimation(NotesNames.STATIC_ARROW);
                Render.SetCurrentFrame(StaticID);
                Overlay.Visible = false;
            }
            else
            {
                Render.SetActiveAnimation(PressAnim);
                Overlay.Visible = true;
            }
        }

        const int HalfSecond = Constants.ORBIS_SECOND / 2;
        long LastUpdateTick = 0;
        public void Update(long Tick)
        {
            if (State == NoteState.Static)
                return;

            if (Tick - LastUpdateTick > HalfSecond)
            {
                if (Overlay.Visible)
                    Overlay.NextFrame();

                LastUpdateTick = Tick;
            }
        }

        public void Dispose()
        {
            Render.Dispose();
            Overlay.Dispose();
        }
    }
}

using OrbisGL;
using OrbisGL.GL2D;
using System;

namespace Orbis.Game
{
    public class PlayerNoteEntry
    {
        public SpriteAtlas2D Render { get; private set;}

        public Note Type { get; private set; }

        public NoteState State { get; private set; }

        public PlayerNoteEntry(SpriteAtlas2D Render, Note Type)
        {
            this.Render = Render;
            this.Type = Type;

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

                switch (Type)
                {
                    case Note.Up:
                        Render.SetCurrentFrame(NotesNames.UP_FRAME_ID);
                        break;
                    case Note.Down:
                        Render.SetCurrentFrame(NotesNames.DOWN_FRAME_ID);
                        break;
                    case Note.Left:
                        Render.SetCurrentFrame(NotesNames.LEFT_FRAME_ID);
                        break;
                    case Note.Right:
                        Render.SetCurrentFrame(NotesNames.RIGHT_FRAME_ID);
                        break;
                }
            }
            else
            {
                switch (Type)
                {
                    case Note.Up:
                        Render.SetActiveAnimation(State == NoteState.Hit ? NotesNames.UP_HIT : NotesNames.UP_MISS);
                        break;
                    case Note.Down:
                        Render.SetActiveAnimation(State == NoteState.Hit ? NotesNames.DOWN_HIT : NotesNames.DOWN_MISS);
                        break;
                    case Note.Left:
                        Render.SetActiveAnimation(State == NoteState.Hit ? NotesNames.LEFT_HIT : NotesNames.LEFT_MISS);
                        break;
                    case Note.Right:
                        Render.SetActiveAnimation(State == NoteState.Hit ? NotesNames.RIGHT_HIT : NotesNames.RIGHT_MISS);
                        break;
                }
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
                Render.NextFrame();
                LastUpdateTick = Tick;
            }
        }
    }
}

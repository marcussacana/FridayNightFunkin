using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Orbis.Game
{
    public class NoteMenu : GLObject2D
    {
        public bool CPU { get; set; }

        CharacterAnim Character;
        public int Missed { get; private set; } = 0;
        public int Score { get; private set; } = 0;
        public SongPlayer Game { get; private set; }

        public event EventHandler<NewStatusEvent> OnNoteElapsed;

        public event EventHandler OnNoteMissed;
        public event EventHandler OnNoteHit;

        public const int NoteOffset = 155;

        public const int LeftX = NoteOffset * 0;
        public const int DownX = NoteOffset * 1;
        public const int UpX = NoteOffset * 2;
        public const int RightX = NoteOffset * 3;

        public bool IsPlayer1 { get; private set; }

        SpriteAtlas2D NoteSprite;

        bool UpCanPress => UpNotes.Any(x => x.CanBeHit);
        bool DownCanPress => DownNotes.Any(x => x.CanBeHit);
        bool LeftCanPress => LeftNotes.Any(x => x.CanBeHit);
        bool RightCanPress => RightNotes.Any(x => x.CanBeHit);

        PlayerNoteEntry Up;
        PlayerNoteEntry Down;
        PlayerNoteEntry Left;
        PlayerNoteEntry Right;


        List<SongNoteEntry> SongNotes = new List<SongNoteEntry>();

        SongNoteEntry[] UpNotes;
        SongNoteEntry[] DownNotes;
        SongNoteEntry[] LeftNotes;
        SongNoteEntry[] RightNotes;

        public NoteMenu(SpriteAtlas2D NoteSprite, SongPlayer Game, bool Player1, bool CPU)
        {
            this.NoteSprite = NoteSprite;
            this.Game = Game;
            this.CPU = CPU;

            Up = new PlayerNoteEntry(NoteSprite.Clone(false), Note.Up);
            Down = new PlayerNoteEntry(NoteSprite.Clone(false), Note.Down);
            Left = new PlayerNoteEntry(NoteSprite.Clone(false), Note.Left);
            Right = new PlayerNoteEntry(NoteSprite.Clone(false), Note.Right);

            AddChild(Up.Render);
            AddChild(Down.Render);
            AddChild(Left.Render);
            AddChild(Right.Render);

            Left.Render.Position = new Vector2(LeftX, 0);
            Down.Render.Position = new Vector2(DownX, 0);
            Up.Render.Position = new Vector2(UpX, 0);
            Right.Render.Position = new Vector2(RightX, 0);

            SetupSong(Game.SongInfo.song, IsPlayer1 = Player1);
        }

        public void SetPress(Note Target)
        {
            switch (Target)
            {
                case Note.Up:
                    Up.SetState(UpCanPress ? NoteState.Hit : NoteState.Miss);
                    if (UpCanPress)
                        HitNote(Target);
                    break;
                case Note.Left:
                    Left.SetState(LeftCanPress ? NoteState.Hit : NoteState.Miss);
                    if (LeftCanPress)
                        HitNote(Target);
                    break;
                case Note.Right:
                    Right.SetState(RightCanPress ? NoteState.Hit : NoteState.Miss);
                    if (RightCanPress)
                        HitNote(Target);
                    break;
                case Note.Down:
                    Down.SetState(DownCanPress ? NoteState.Hit : NoteState.Miss);
                    if (DownCanPress)
                        HitNote(Target);
                    break;
            }
        }
        public void UnsetPress(Note Target)
        {
            switch (Target)
            {
                case Note.Up:
                    Up.SetState(NoteState.Static);
                    break;
                case Note.Left:
                    Left.SetState(NoteState.Static);
                    break;
                case Note.Right:
                    Right.SetState(NoteState.Static);
                    break;
                case Note.Down:
                    Down.SetState(NoteState.Static);
                    break;
            }
        }

        private void HitNote(Note target)
        {
            SongNoteEntry HittedNote;
            switch (target)
            {
                case Note.Left:
                    HittedNote = LeftNotes.First(x => x.CanBeHit);
                    break;
                case Note.Right:
                    HittedNote = RightNotes.First(x => x.CanBeHit);
                    break;
                case Note.Down:
                    HittedNote = DownNotes.First(x => x.CanBeHit);
                    break;
                case Note.Up:
                    HittedNote = UpNotes.First(x => x.CanBeHit);
                    break;
                default:
                    throw new Exception("Invalid Note Type");
            }

            HittedNote.Hitted = true;
        }

        public void SetSongBegin(long Tick)
        {
            foreach (var Note in SongNotes)
                Note.SetSongBegin(Tick);
        }

        public void SetupSong(SongData Song, bool IsPlayer1)
        {
            Character = new CharacterAnim(IsPlayer1 ? Song.player1 : Song.player2 ?? "gf");

            SongNotes.Clear();
            Game.ComputeStep(out int BPMS, out int SPMS);

            //near but not accurate with the original game
            var YPerMS = (SPMS / 100f * 1.5f * Song.speed) / 4;
            //var YPerMS = (SPMS / 100f * 1.5f * Song.speed) / 6; //slower, for debugging


            foreach (var Section in Song.notes)
            {
                bool OwnNote = (Section.mustHitSection && IsPlayer1) || (!Section.mustHitSection && !IsPlayer1);

                if (!OwnNote)
                    continue;

                foreach (var NoteData in Section.sectionNotes)
                {
                    float Milisecond = NoteData[0];
                    float Type = NoteData[1];
                    float Duration = NoteData[2];

                    Note NType;
                    switch (Type % 4)
                    {
                        case 0:
                            NType = Note.Left;
                            break;
                        case 1:
                            NType = Note.Down;
                            break;
                        case 2:
                            NType = Note.Up;
                            break;
                        case 3:
                            NType = Note.Right;
                            break;

                        default:
                            throw new Exception("Invalid Note Type");
                    }

                    var CurNote = new SongNoteEntry(NoteSprite.Clone(false), NType, Milisecond, Duration, YPerMS, CPU);
                    CurNote.OnNoteElapsed += NoteEntryElapsed;
                    SongNotes.Add(CurNote);
                }
            }

            foreach (var Note in SongNotes)
                AddChild(Note);

            UpNotes = SongNotes.Where(x => x.Type == Note.Up).ToArray();
            DownNotes = SongNotes.Where(x => x.Type == Note.Down).ToArray();
            LeftNotes = SongNotes.Where(x => x.Type == Note.Left).ToArray();
            RightNotes = SongNotes.Where(x => x.Type == Note.Right).ToArray();
        }

        private void NoteEntryElapsed(object sender, EventArgs e)
        {
            var ElapsedNote = ((SongNoteEntry)sender);
            var NoteScore = (int)ElapsedNote.Score;

            if (NoteScore == 0)
            {
                Missed++;
                Score -= 100;

                if (IsPlayer1)
                {
                    var NewState = new NewStatusEvent();
                    NewState.Target = EventTarget.Player1;

                    SetPlayerMiss(ElapsedNote, NewState);
                }
                else
                {
                    var NewState = new NewStatusEvent();
                    NewState.Target = EventTarget.Player2;

                    SetPlayerMiss(ElapsedNote, NewState);
                }
            }
            else
            {
                Score += NoteScore;

                if (IsPlayer1)
                {
                    var NewState = new NewStatusEvent();
                    NewState.Target = EventTarget.Player1;

                    SetPlayerHit(ElapsedNote, NewState);
                }
                else
                {
                    var NewState = new NewStatusEvent();
                    NewState.Target = EventTarget.Player2;

                    SetPlayerHit(ElapsedNote, NewState);
                }
            }

            ElapsedNote.Dispose();
        }

        private void SetPlayerHit(SongNoteEntry ElapsedNote, NewStatusEvent NewState)
        {
            switch (ElapsedNote.Type)
            {
                case Note.Up:
                    NewState.NewAnimation = Character.UP;
                    break;
                case Note.Down:
                    NewState.NewAnimation = Character.DOWN;
                    break;
                case Note.Left:
                    NewState.NewAnimation = Character.LEFT;
                    break;
                case Note.Right:
                    NewState.NewAnimation = Character.RIGHT;
                    break;
            }

            OnNoteElapsed?.Invoke(this, NewState);
            OnNoteHit?.Invoke(this, EventArgs.Empty);
        }
        private void SetPlayerMiss(SongNoteEntry ElapsedNote, NewStatusEvent NewState)
        {
            switch (ElapsedNote.Type)
            {
                case Note.Up:
                    NewState.NewAnimation = Character.UP_MISS;
                    break;
                case Note.Down:
                    NewState.NewAnimation = Character.DOWN_MISS;
                    break;
                case Note.Left:
                    NewState.NewAnimation = Character.LEFT_MISS;
                    break;
                case Note.Right:
                    break;
            }

            OnNoteElapsed?.Invoke(this, NewState);
            OnNoteMissed?.Invoke(this, EventArgs.Empty);
        }

        public override void Draw(long Tick)
        {
            Up.Update(Tick);
            Down.Update(Tick);
            Left.Update(Tick);
            Right.Update(Tick);

            base.Draw(Tick);
        }

        public override void Dispose()
        {
            NoteSprite.Dispose();
            base.Dispose();
        }
    }
}

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

        private long SongStartTick;

        public const int StartDelayMS = 3000;

        IEnumerator<SongNoteEntry> NoteCreator;

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


        private IEnumerable<SongNoteEntry> SongNotes => UpNotes.Concat(DownNotes).Concat(LeftNotes).Concat(RightNotes); 

        List<SongNoteEntry> UpNotes = new List<SongNoteEntry>();
        List<SongNoteEntry> DownNotes = new List<SongNoteEntry>();
        List<SongNoteEntry> LeftNotes = new List<SongNoteEntry>();
        List<SongNoteEntry> RightNotes = new List<SongNoteEntry>();

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
            this.SongStartTick = Tick;
            
            foreach (var Note in SongNotes)
                Note.SetSongBegin(Tick);
        }

        public void SetupSong(SongData Song, bool IsPlayer1)
        {
            Character = new CharacterAnim(IsPlayer1 ? Song.player1 : Song.player2 ?? "gf");
            
            Game.ComputeStep(out int BPMS, out int SPMS);

            NoteCreator = CreateNotes(Song, IsPlayer1, SPMS);

            EnsureEnoughtNotes();
        }

        private void EnsureEnoughtNotes()
        {
            while (SongNotes.Count() < 20)
            {
                if (!AddNextNote())
                    break;
            }
        }

        private bool AddNextNote()
        {
            if (NoteCreator.MoveNext())
            {
                var CurNote = NoteCreator.Current;

                switch (CurNote.Type)
                {
                    case Note.Up:
                        UpNotes.Add(CurNote);
                        break;
                    case Note.Down:
                        DownNotes.Add(CurNote);
                        break;
                    case Note.Left:
                        LeftNotes.Add(CurNote);
                        break;
                    case Note.Right:
                        RightNotes.Add(CurNote);
                        break;
                }

                if (SongStartTick != 0)
                    CurNote.SetSongBegin(SongStartTick);
                
                AddChild(CurNote);
                return true;
            }

            return false;
        }

        private IEnumerator<SongNoteEntry> CreateNotes(SongData Song, bool IsPlayer1, int SPMS)
        {
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
                    float Milisecond = NoteData[0] + StartDelayMS;
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

                    var CurNote = new SongNoteEntry(NoteSprite.Clone(false), NType, Milisecond, Duration, YPerMS);
                    CurNote.OnNoteReached += NoteEntryReached;
                    CurNote.OnNoteElapsed += NoteEntryElapsed;

                    yield return CurNote;
                }
            }
        }

        private void NoteEntryReached(object sender, EventArgs e)
        {
            var ReachedNote = ((SongNoteEntry)sender);

            if (CPU)
                SetPress(ReachedNote.Type);
        }
        private void NoteEntryElapsed(object sender, EventArgs e)
        {
            var ElapsedNote = ((SongNoteEntry)sender);
            var NoteScore = (int)ElapsedNote.Score;


            var NewState = new NewStatusEvent();
            NewState.Target = IsPlayer1 ? EventTarget.Player1 : EventTarget.Player2;
            NewState.NewAnimation = Character.DANCING;

            if (NoteScore == 0)
            {
                Missed++;
                Score -= 100;

                SetPlayerMiss(ElapsedNote, NewState);
            }
            else
            {
                Score += NoteScore;

                SetPlayerHit(ElapsedNote, NewState);
            }

            if (CPU)
            {
                UnsetPress(ElapsedNote.Type);
            }

            DeleteNote(ElapsedNote);
            EnsureEnoughtNotes();
        }

        private void DeleteNote(SongNoteEntry Target)
        {
            switch (Target.Type)
            {
                case Note.Down:
                    DownNotes.Remove(Target);
                    break;
                case Note.Up:
                    UpNotes.Remove(Target);
                    break;
                case Note.Left:
                    LeftNotes.Remove(Target);
                    break;
                case Note.Right:
                    RightNotes.Remove(Target);
                    break;
            }

            Target.Dispose();
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
            NoteCreator?.Dispose();
            NoteSprite.Dispose();
            base.Dispose();
        }
    }
}

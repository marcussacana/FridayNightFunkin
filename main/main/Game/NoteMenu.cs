using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Orbis.Game
{
    public class NoteMenu : GLObject2D
    {
        public bool CPU { get; set; }

        CharacterAnim CharAnim => IsPlayer1 ? Game.Player1Anim : Game.Player2Anim;

        public int Missed { get; private set; } = 0;
        public int Score { get; private set; } = 0;
        public SongPlayer Game { get; private set; }

        public event EventHandler<NewStatusEvent> OnNoteElapsed;

        public event EventHandler<SongNoteEntry> OnNoteMissed;
        public event EventHandler<SongNoteEntry> OnNoteHit;

        public event EventHandler OnSustainHit;
        public event EventHandler OnSustainMissed;

        public event EventHandler OnNoteSectionEnd;

        public const int NoteOffset = 155;

        public const int LeftX = NoteOffset * 0;
        public const int DownX = NoteOffset * 1;
        public const int UpX = NoteOffset * 2;
        public const int RightX = NoteOffset * 3;

        private long SongStartTick;
        private long FrozenTick = -1;
        
        public const int StartDelayMS = 5000;

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

        private int LoadedSections;

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

            Left.Render.ZoomPosition = new Vector2(LeftX, 0);
            Down.Render.ZoomPosition = new Vector2(DownX, 0);
            Up.Render.ZoomPosition = new Vector2(UpX, 0);
            Right.Render.ZoomPosition = new Vector2(RightX, 0);

            SetupSong(Game.SongInfo.song, IsPlayer1 = Player1);
        }

        public void SetPress(Note Target)
        {
            bool CanBeHit = false;

            switch (Target)
            {
                case Note.Up:
                    Up.SetState(UpCanPress ? NoteState.Hit : NoteState.Miss);
                    CanBeHit = UpCanPress;
                    break;
                case Note.Left:
                    Left.SetState(LeftCanPress ? NoteState.Hit : NoteState.Miss);
                    CanBeHit = LeftCanPress;
                    break;
                case Note.Right:
                    Right.SetState(RightCanPress ? NoteState.Hit : NoteState.Miss);
                    CanBeHit = RightCanPress;
                    break;
                case Note.Down:
                    Down.SetState(DownCanPress ? NoteState.Hit : NoteState.Miss);
                    CanBeHit = DownCanPress;
                    break;
            }

            if (CanBeHit)
                HitNote(Target);
            else
                ComputeMiss(Target);
        }
        public void UnsetPress(Note Target)
        {
            IEnumerable<SongNoteEntry> Lookup;

            switch (Target)
            {
                case Note.Up:
                    Up.SetState(NoteState.Static);
                    Lookup = UpNotes.Where(x => x.Hitted);
                    break;
                case Note.Left:
                    Left.SetState(NoteState.Static);
                    Lookup = LeftNotes.Where(x => x.Hitted);
                    break;
                case Note.Right:
                    Right.SetState(NoteState.Static);
                    Lookup = RightNotes.Where(x => x.Hitted);
                    break;
                case Note.Down:
                    Down.SetState(NoteState.Static);
                    Lookup = DownNotes.Where(x => x.Hitted);
                    break;
                default:
                    return;
            }

            foreach (var Note in Lookup)
                Note.Hitted = false;
        }
        
        private void HitNote(Note target)
        {
            IEnumerable<SongNoteEntry> Lookup;
            switch (target)
            {
                case Note.Left:
                    Lookup = LeftNotes.Where(x => x.CanBeHit);
                    break;
                case Note.Right:
                    Lookup = RightNotes.Where(x => x.CanBeHit);
                    break;
                case Note.Down:
                    Lookup = DownNotes.Where(x => x.CanBeHit);
                    break;
                case Note.Up:
                    Lookup = UpNotes.Where(x => x.CanBeHit);
                    break;
                default:
                    throw new Exception("Invalid Note Type");
            }

            foreach (var Note in Lookup)
                Note.Hitted = true;
        }

        public void SetSongBegin(long Tick)
        {
            this.SongStartTick = Tick;
            
            foreach (var Note in SongNotes)
                Note.SetSongBegin(Tick);
        }

        public void SetupSong(SongData Song, bool IsPlayer1)
        {            
            Game.ComputeStep(out int BPMS, out int SPMS);

            NoteCreator = CreateNotes(Song, IsPlayer1, SPMS);

            EnsureEnoughtNotes();
        }

        private bool FirstSection = true;
        private void EnsureEnoughtNotes()
        {
            //Ensure it was 5 sections loaded
            while (LoadedSections < 5)
            {
                var Rst = AddNextNote();
                
                if (Rst == null)
                    LoadedSections++;
                
                if (Rst is false)
                {
                    break;
                }
            }
        }

        private bool? AddNextNote()
        {
            if (NoteCreator.MoveNext())
            {
                var CurNote = NoteCreator.Current;

                if (CurNote == null)
                    return null;

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


            var OwnedSections = Song.notes.Where(Section => (Section.mustHitSection && IsPlayer1) || (!Section.mustHitSection && !IsPlayer1));

            foreach (var Section in OwnedSections)
            {
                var LastNote = Section.sectionNotes.Count > 0 ? Section.sectionNotes.Last() : null;
                
                if (LastNote == null)
                    continue;

                foreach (var NoteData in Section.sectionNotes)
                {
                    bool LastSectionNote = LastNote == NoteData;

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
                    CurNote.OnNoteHeaderElapsed += NoteEntryHeaderElapsed;
                    CurNote.OnNoteElapsed += NoteEntryElapsed;
                    CurNote.OnSustainHit += (sender, e) => OnSustainHit?.Invoke(this, null);
                    CurNote.OnSustainMissed += (sender, e) => OnSustainMissed?.Invoke(this, null);
                    CurNote.AltAnimation = Section.altAnim;

                    if (LastSectionNote)
                    {
                        CurNote.OnNoteElapsed += (sender, e) => EnsureEnoughtNotes();
                        CurNote.OnNoteElapsed += (sender, e) =>
                        {
                            LoadedSections--;
                            OnNoteSectionEnd?.Invoke(this, e);
                        };
                    }

                    yield return CurNote;
                }

                yield return null;
            }
        }

        private void NoteEntryReached(object sender, EventArgs e)
        {
            var ReachedNote = ((SongNoteEntry)sender);

            if (CPU)
                SetPress(ReachedNote.Type);
        }

        private void NoteEntryHeaderElapsed(object sender, EventArgs e)
        {
            var ElapsedNote = ((SongNoteEntry)sender);
            var NoteScore = (int)ElapsedNote.Score;

            if (NoteScore == 0)
            {
                ComputeMiss(ElapsedNote);
            }
            else
            {
                Score += NoteScore;

                SetPlayerHit(ElapsedNote);
            }

            //Reset Score count because the NoteEntryElapsed should sum only the sustain note;
            ElapsedNote.Score = 0;
        }

        private void ComputeMiss(SongNoteEntry ElapsedNote)
        {
            Missed++;
            Score -= 100;

            var NewState = CreateNewState(ElapsedNote.AltAnimation);
            SetPlayerMiss(ElapsedNote.Type, ref NewState);

            OnNoteElapsed?.Invoke(this, NewState);
            OnNoteMissed?.Invoke(this, ElapsedNote);
        }
        private void ComputeMiss(Note Type)
        {
            Score -= 100;

            OnNoteMissed?.Invoke(this, null);
        }

        private void NoteEntryElapsed(object sender, EventArgs e)
        {
            var ElapsedNote = ((SongNoteEntry)sender);
            var NoteScore = (int)ElapsedNote.Score;

            Score += NoteScore;

            if (CPU)
            {
                UnsetPress(ElapsedNote.Type);
            }

            DeleteNote(ElapsedNote);
        }

        private NewStatusEvent CreateNewState(bool AltAnimation)
        {
            var NewState = new NewStatusEvent();
            NewState.AltAnimation = Game.AltPlayer2 = AltAnimation;
            NewState.Target = IsPlayer1 ? EventTarget.Player1 : EventTarget.Player2;
            NewState.NewAnimation = CharAnim.DANCING;
            return NewState;
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

        private void SetPlayerHit(SongNoteEntry ElapsedNote)
        {
            var NewState = CreateNewState(ElapsedNote.AltAnimation);

            switch (ElapsedNote.Type)
            {
                case Note.Up:
                    NewState.NewAnimation = CharAnim.UP;
                    break;
                case Note.Down:
                    NewState.NewAnimation = CharAnim.DOWN;
                    break;
                case Note.Left:
                    NewState.NewAnimation = CharAnim.LEFT;
                    break;
                case Note.Right:
                    NewState.NewAnimation = CharAnim.RIGHT;
                    break;
            }

            OnNoteElapsed?.Invoke(this, NewState);
            OnNoteHit?.Invoke(this, ElapsedNote);
        }
        private void SetPlayerMiss(Note Type, ref NewStatusEvent NewState)
        {
            switch (Type)
            {
                case Note.Up:
                    NewState.NewAnimation = CharAnim.UP_MISS;
                    break;
                case Note.Down:
                    NewState.NewAnimation = CharAnim.DOWN_MISS;
                    break;
                case Note.Left:
                    NewState.NewAnimation = CharAnim.LEFT_MISS;
                    break;
                case Note.Right:
                    NewState.NewAnimation = CharAnim.RIGHT_MISS;
                    break;
            }
        }

        public void Freeze()
        {
            FrozenTick = 0;
        }

        public void Unfreeze()
        {
            FrozenTick = -1;
        }

        public override void Draw(long Tick)
        {
            if (FrozenTick >= 0)
            {
                if (FrozenTick == 0)
                    FrozenTick = Tick;

                Tick = FrozenTick;
            }

            Up.Update(Tick);
            Down.Update(Tick);
            Left.Update(Tick);
            Right.Update(Tick);

            base.Draw(Tick);
        }

        public override void Dispose()
        {
            Up?.Dispose();
            Down?.Dispose();
            Left?.Dispose();
            Right?.Dispose();
            
            NoteCreator?.Dispose();
            NoteSprite.Dispose();
            
            base.Dispose();
        }
    }
}

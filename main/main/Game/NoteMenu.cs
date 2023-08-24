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

        public SongPlayer Player { get; private set; }

        public const int NoteOffset = 155;

        public const int LeftX = NoteOffset * 0;
        public const int DownX = NoteOffset * 1;
        public const int UpX = NoteOffset * 2;
        public const int RightX = NoteOffset * 3;

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

        public NoteMenu(SpriteAtlas2D NoteSprite, SongPlayer Player, bool Player1)
        {
            this.NoteSprite = NoteSprite;
            this.Player = Player;

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

            SetupSong(Player.SongInfo.song, Player1);
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
            throw new NotImplementedException();
        }

        public void SetSongBegin(long Tick)
        {
            foreach (var Note in SongNotes)
                Note.SetSongBegin(Tick);
        }

        public void SetupSong(SongData Song, bool IsPlayer1)
        {
            SongNotes.Clear();
            Player.ComputeStep(out int BPMS, out int SPMS);

            //near but not accurate with the original game
            var YPerMS = (SPMS / 100f * 1.5f * Song.speed) / 4;


            foreach (var Section in Song.notes)
            {
                if (Section.mustHitSection == IsPlayer1)
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

                    SongNotes.Add(new SongNoteEntry(NoteSprite.Clone(false), this, NType, Milisecond, Duration, YPerMS));
                }
            }

            foreach (var Note in SongNotes)
                AddChild(Note);

            UpNotes = SongNotes.Where(x => x.Type == Note.Up).ToArray();
            DownNotes = SongNotes.Where(x => x.Type == Note.Down).ToArray();
            LeftNotes = SongNotes.Where(x => x.Type == Note.Left).ToArray();
            RightNotes = SongNotes.Where(x => x.Type == Note.Right).ToArray();
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

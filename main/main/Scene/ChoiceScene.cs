using Orbis.Audio;
using OrbisGL;
using OrbisGL.Audio;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Scene
{
    public class ChoiceScene : GLObject2D
    {
        private WavePlayer SFXAPlayer;
        private OrbisAudioOut SFXADriver;

        private WavePlayer SFXBPlayer;
        private OrbisAudioOut SFXBDriver;

        private WavePlayer SFXCPlayer;
        private OrbisAudioOut SFXCDriver;

        private Blank2D ChoiceList = new Blank2D();

        public ChoiceScene(Vector2 Position, int Width, int Heigth, string[] Choices, SpriteAtlas2D Atlas) : this(Choices, Atlas)
        {
            this.Position = Position;
            this.Width = Width;
            this.Height = Heigth;

#if ORBIS
            SFXAPlayer = new WavePlayer();
            SFXADriver = new OrbisAudioOut();
            SFXADriver.SetVolume(0);

            SFXBPlayer = new WavePlayer();
            SFXBDriver = new OrbisAudioOut();
            SFXBDriver.SetVolume(0);

            SFXCPlayer = new WavePlayer();
            SFXCDriver = new OrbisAudioOut();
            SFXCDriver.SetVolume(0);
            
            SFXAPlayer.SetAudioDriver(SFXADriver);
            SFXBPlayer.SetAudioDriver(SFXBDriver);
            SFXCPlayer.SetAudioDriver(SFXCDriver);
#endif

            if (SFXAPlayer != null)
                SFXAPlayer.OnTrackEnd += TrackEnd;
            if (SFXBPlayer != null)
                SFXBPlayer.OnTrackEnd += TrackEnd;
            if (SFXCPlayer != null)
                SFXCPlayer.OnTrackEnd += TrackEnd;

            UpdateSelection(true);
        }

        private ChoiceScene(string[] Choices, SpriteAtlas2D Atlas)
        {
            Vector2 CurrentPos = Vector2.Zero;
            foreach (var Choice in Choices)
            {
                var Text = new AtlasText2D(Atlas, Util.FontBoldMap);
                Text.SetText(Choice.ToUpper());
                Text.Position = CurrentPos;
                Text.Opacity = 255 - SecondaryTranparency;
                Text.StaticText = true;
                Text.SetZoom(0.8f);
                ChoiceList.AddChild(Text);

                CurrentPos += new Vector2(Text.Width, Text.Height + 50);
            }

            AddChild(ChoiceList);
        }


        /// <summary>
        /// Trigger just right after the selection is done
        /// </summary>
        public event EventHandler OnChoiceDoneBegin;

        /// <summary>
        /// Trigger after the selection animation is done
        /// </summary>
        public event EventHandler OnChoiceDoneEnd;

        public event EventHandler OnSelectionChanged;

        private void TrackEnd(object sender, EventArgs e)
        {
            if (!WaitingSongEnd)
                return;

            WaitingSongEnd = false;

            OnChoiceDoneEnd?.Invoke(this, e);
        }

        const int SecondaryTranparency = 80;
        const int XSelectionDistance = 50;
        const int AnimSelectionDuration = 250;

        GLObject2D LastSelection;
        AtlasText2D Selection;
        float MoveInitialY;
        float MoveDeltaY;
        long AnimTick = -1;

        public void MoveUp()
        {
            if (SelectedIndex <= 0 || AnimTick != -1)
                return;

            SelectedIndex--;
            UpdateSelection(false);
        }

        public void MoveDown()
        {
            if (SelectedIndex >= ChoiceList.Childs.Count() - 1 || AnimTick != -1)
                return;

            SelectedIndex++;
            UpdateSelection(false);
        }

        bool WaitingSongEnd = false;
        public void DoChoice()
        {
            if (WaitingSongEnd)
                return;

            if (SFXAPlayer != null)
            {
                WaitingSongEnd = true;

                PlayAudio(SFXHelper.Default.GetSFX(SFXType.MenuConfirm));
                OnChoiceDoneBegin?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnChoiceDoneEnd?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SelectedIndex { get; private set; }

        private void UpdateSelection(bool Instant)
        {
            bool Changed = LastSelection != Selection;

            LastSelection = Selection;
            Selection = (AtlasText2D)ChoiceList.Childs.ElementAt(SelectedIndex);

            var ListBasePos = Height / 2f;
            MoveInitialY = ChoiceList.Position.Y;

            var CenterSelectionY = Selection.ZoomPosition.Y + (Selection.ZoomHeight / 2);
            var TargetListY = ListBasePos - CenterSelectionY;

            MoveDeltaY = TargetListY - MoveInitialY;

            if (Instant)
            {
                ChoiceList.Position = new Vector2(Position.X, Position.Y + MoveDeltaY);

                foreach (var Child in ChoiceList.Childs)
                    Child.Position = new Vector2(0, Child.Position.Y);

                Selection.Position = new Vector2(XSelectionDistance, Selection.Position.X);
                Selection.Opacity = 255;
            }
            else
            {
                AnimTick = 0;

                PlayAudio(SFXHelper.Default.GetSFX(SFXType.MenuChoice));
            }

            if (Changed)
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        int LastTrack = 0;
        private void PlayAudio(MemoryStream Audio)
        {
            if (Audio == null)
                return;

            Audio.Position = 0;

            switch (LastTrack++)
            {
                case 0:
                    SFXCDriver.SetVolume(0);
                    SFXBDriver.SetVolume(0);

                    SFXADriver?.SetVolume(80);
                    SFXAPlayer?.Open(Audio);
                    SFXAPlayer?.Restart();
                    break;
                case 1:
                    SFXCDriver.SetVolume(0);
                    SFXADriver.SetVolume(0);

                    SFXBDriver?.SetVolume(80);
                    SFXBPlayer?.Open(Audio);
                    SFXBPlayer?.Restart();
                    break;
                default:
                    SFXADriver.SetVolume(0);
                    SFXBDriver.SetVolume(0);

                    SFXCDriver?.SetVolume(80);
                    SFXCPlayer?.Open(Audio);
                    SFXCPlayer?.Restart();

                    LastTrack = 0;
                    break;

            }
        }

        private void DoSelection(long Tick)
        {
            if (AnimTick >= 0)
            {
                if (AnimTick == 0)
                {
                    AnimTick = Tick;
                }

                var DeltaTick = Tick - AnimTick;

                var ElapsedMS = (float)(DeltaTick / Constants.ORBIS_MILISECOND);

                float Progress = ElapsedMS / AnimSelectionDuration;

                if (Progress >= 1)
                {
                    Progress = 1;
                    AnimTick = -1;
                }

                if (LastSelection != null)
                {
                    LastSelection.Position = new Vector2(XSelectionDistance - (XSelectionDistance * Progress), LastSelection.Position.Y);
                    LastSelection.Opacity = (byte)(255 - (SecondaryTranparency * Progress));
                }

                Selection.Opacity = (byte)(255 - (SecondaryTranparency * (1 - Progress)));
                Selection.Position = new Vector2(XSelectionDistance * Progress, Selection.Position.Y);

                Progress = Geometry.CubicBezier(new Vector2(0.5f, 0), new Vector2(0.5f, 0), Progress);

                var CurrentY = MoveInitialY + (MoveDeltaY * Progress);

                ChoiceList.Position = new Vector2(Position.X, CurrentY);
            }
        }

        long LastFrameTick;
        public override void Draw(long Tick)
        {
            DoSelection(Tick);
            DoSelectedAnim(Tick);
            base.Draw(Tick);
        }

        private void DoSelectedAnim(long Tick)
        {
            if (Selection != null)
            {
                var ElapsedTicks = Tick - LastFrameTick;

                if (ElapsedTicks > Constants.ORBIS_MILISECOND * 100)
                {
                    LastFrameTick = Tick;
                    foreach (var Child in Selection.Childs.Cast<SpriteAtlas2D>())
                    {
                        Child.NextFrame();
                    }
                }
            }
        }
    }
}

using Orbis.Audio;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.Audio;
using OrbisGL.Controls.Events;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Orbis.Scene
{
    internal class FreestyleScene : GLObject2D, ILoadable
    {

        bool Active = false;

        static readonly List<string> Songs = new List<string>() {
            "tutorial",
            "bopeebo",
            "fresh",
            "dadbattle",
            "spookeez",
            "south",
            "pico",
            "philly",
            "blammed",
            "satin-panties",
            "high",
            "milf",
            "cocoa",
            "eggnog",
            "winter-horrorland",
            "senpai",
            "roses",
            "thorns",
            "guns",
            "monster",
            "stress",
            "test",
            "ugh"
        };


        static Texture2D TexView;
        static SpriteAtlas2D Atlas;

        Blank2D SongList = new Blank2D();

        private WavePlayer Theme;
        private OrbisAudioOut ThemeDriver;
        private WavePlayer SFXPlayer;
        private OrbisAudioOut SFXDriver;

        private AtlasText2D DifficultyView;

        private Rectangle2D Fade = new Rectangle2D(1920, 1080, true);

        LoadingScene LoadScreen;

        Dificuty CurrentDifficulty = Dificuty.Normal;

        int SelectedIndex = 0;

        public FreestyleScene(WavePlayer Theme, OrbisAudioOut ThemeDriver, WavePlayer SFXPlayer, OrbisAudioOut SFXDriver, LoadingScene LoadScreen)
        {
            this.Theme = Theme;
            this.ThemeDriver = ThemeDriver;
            this.SFXPlayer = SFXPlayer;
            this.SFXDriver = SFXDriver;
            this.LoadScreen = LoadScreen;
        }

        public bool Loaded { get; set; }

        public int TotalProgress => 2;

        public void Load(Action<int> OnProgressChanged)
        {
            if (Loaded)
                return;

            if (TexView == null)
            {
                using (var Stream = Util.CopyFileToMemory("menuBGBlue.dds"))
                {
                    TexView = new Texture2D();
                    TexView.Texture = new Texture(true);
                    TexView.Texture.SetDDS(Stream, true);
                    TexView.AutoSize = false;
                    TexView.Width = 1920;
                    TexView.Height = 1080;
                    AddChild(TexView);
                }
            }

            OnProgressChanged?.Invoke(1);

            if (Atlas == null)
            {
                var XML = Util.GetXML("alphabet.xml");
                Atlas = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);
            }


            Vector2 CurrentPos = Vector2.Zero;
            foreach (var Song in Songs)
            {
                var Text = new AtlasText2D(Atlas, Util.FontBoldMap);
                Text.SetText(GetFirendlyName(Song));
                Text.Position = CurrentPos;
                Text.SetZoom(0.8f);
                SongList.AddChild(Text);

                CurrentPos += new Vector2(Text.Width, Text.Height + 20);
            }

            SongList.Position = new Vector2(50, 0);
            AddChild(SongList);

            DifficultyView = new AtlasText2D(Atlas, Util.FontBoldMap);
            DifficultyView.SetText("NORMAL");
            DifficultyView.SetZoom(1.2f);
            DifficultyView.ZoomPosition = new Vector2(Application.Default.Width - 300, 50);
            AddChild(DifficultyView);

            Loaded = true;

#if ORBIS
            Application.Default.Gamepad.OnButtonUp += GamepadOnOnButtonUp;
#endif
            Application.Default.KeyboardDriver.OnKeyUp += OnKeyUp;

            Fade.Color = RGBColor.Black;
            Fade.Opacity = 0;

            AddChild(Fade);


            UpdateSelection(true);

            OnProgressChanged?.Invoke(2);

        }

        private void OnKeyUp(object Sender, KeyboardEventArgs Args)
        {
            if (Started)
                return;

            if (Args.Keycode == IME_KeyCode.DOWNARROW)
                MoveDown();
            if (Args.Keycode == IME_KeyCode.UPARROW)
                MoveUp();

            if (Args.Keycode == IME_KeyCode.RIGHTARROW)
                IncreaseDifficulty();
            if (Args.Keycode == IME_KeyCode.LEFTARROW)
                DecreaseDifficulty();

            if (Args.Keycode == IME_KeyCode.KEYPAD_ENTER)
                SongConfirm();
        }

        private void GamepadOnOnButtonUp(object sender, ButtonEventArgs args)
        {
            if (Started)
                return;

            if (args.Button.HasFlag(OrbisPadButton.Down))
                MoveDown();

            if (args.Button.HasFlag(OrbisPadButton.Up))
                MoveUp();

            if (args.Button.HasFlag(OrbisPadButton.Left))
                DecreaseDifficulty();

            if (args.Button.HasFlag(OrbisPadButton.Right))
                IncreaseDifficulty();
            
            if (args.Button.HasFlag(OrbisPadButton.Options) || args.Button.HasFlag(OrbisPadButton.Cross))
                SongConfirm();
        }

        private void MoveUp()
        {
            if (SelectedIndex <= 0 || AnimTick != -1)
                return;

            SelectedIndex--;
            UpdateSelection(false);
        }

        bool MusicConfirmed = false;
        private void SongConfirm()
        {
            if (SFXPlayer != null)
            {
                BeginFadeOut = 0;

                SFXPlayer.Open(SFXHelper.Default.GetSFX(SFXType.MenuConfirm));
                SFXPlayer.OnTrackEnd += ConfirmTrackEnd;
                ThemeDriver.SetVolume(0);
                SFXDriver.SetVolume(100);
                SFXPlayer.Restart();
            }
            else
            {
                MusicConfirmed = true;
            }
        }

        private void ConfirmTrackEnd(object sender, EventArgs e)
        {
            SFXPlayer.OnTrackEnd -= ConfirmTrackEnd;


            //Again, the OnTrackEnd event runs in a secondary thread, we can't touch in the OpenGL objects.
            //So, let's just set an flag and let the main draw loop detect it
            MusicConfirmed = true;
        }

        const int XSelectionDistance = 50;

        GLObject2D LastSelection;
        AtlasText2D Selection;
        float MoveInitialY;
        float MoveDeltaY;
        long AnimTick = -1;
        private void MoveDown()
        {
            if (SelectedIndex >= Songs.Count - 1 || AnimTick != -1)
                return;

            SelectedIndex++;
            UpdateSelection(false);
        }

        private void UpdateSelection(bool Instant)
        {
            LastSelection = Selection;
            Selection = (AtlasText2D)SongList.Childs.ElementAt(SelectedIndex);

            var ListBasePos = 1080f / 2f;
            var SelectionY = Selection.ZoomPosition.Y;

            var TargetYPos = ListBasePos - SelectionY;

            MoveDeltaY = TargetYPos - SongList.Position.Y;
            MoveInitialY = SongList.Position.Y;

            if (Instant)
            {
                SongList.Position = new Vector2(SongList.Position.X, SongList.Position.Y + MoveDeltaY);
                
                foreach (var Child in SongList.Childs)
                    Child.Position = new Vector2(0, Child.Position.Y);

                Selection.Position = new Vector2(XSelectionDistance, Selection.Position.X);
            }
            else
            {
                AnimTick = 0;
                SFXPlayer.Open(SFXHelper.Default.GetSFX(SFXType.MenuChoice));
            }
        }

        private void DecreaseDifficulty()
        {
            switch (CurrentDifficulty)
            {
                case Dificuty.Normal:
                    CurrentDifficulty = Dificuty.Easy;
                    DifficultyView.SetText("EASY");
                    break;
                case Dificuty.Hard:
                    CurrentDifficulty = Dificuty.Normal;
                    DifficultyView.SetText("NORMAL");
                    break;
            }
        }

        private void IncreaseDifficulty()
        {
            switch (CurrentDifficulty)
            {
                case Dificuty.Easy:
                    CurrentDifficulty = Dificuty.Normal;
                    DifficultyView.SetText("NORMAL");
                    break;
                case Dificuty.Normal:
                    CurrentDifficulty = Dificuty.Hard;
                    DifficultyView.SetText("HARD");
                    break;
            }
        }

        private bool Started = false;
        private string GetFirendlyName(string Song)
        {
            return Song.Replace("-", " ").ToUpper();
        }

        long BeginFadeOut = -1;
        long BeginFadeIn = -1;

        long LastFrameTick = 0;
        public override void Draw(long Tick)
        {
            if (LastFrameTick == 0)
            {
                Active = true;
                ThemeDriver?.SetVolume(80);
                Theme?.Resume();
            }

            DoSongSelection(Tick);

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

            DoFade(Tick);

            if (MusicConfirmed)
            {
                MusicConfirmed = false;

                SongPlayer SP = new SongPlayer(Util.GetSongByName(Songs[SelectedIndex], CurrentDifficulty));
                SP.OnSongEnd += SongEnd;
                
                Application.Default.RemoveObjects();
                Application.Default.AddObject(LoadScreen);

                LoadScreen.Load(SP, () =>
                {
                    Application.Default.RemoveObjects();
                    Application.Default.AddObject(SP);
                    LastFrameTick = 0;
                    Started = true;
                    SP.Begin();
                });

                return;
            }

            base.Draw(Tick);
        }

        private void DoSongSelection(long Tick)
        {
            if (AnimTick >= 0)
            {
                if (AnimTick == 0)
                {
                    AnimTick = Tick;
                    SFXPlayer.Restart();
                }

                var DeltaTick = Tick - AnimTick;

                var ElapsedMS = (float)(DeltaTick / Constants.ORBIS_MILISECOND);

                const int AnimDuration = 150;

                float Progress = ElapsedMS / AnimDuration;

                if (Progress > 1)
                {
                    Progress = 1;
                    AnimTick = -1;
                }

                if (LastSelection != null)
                    LastSelection.Position = new Vector2(XSelectionDistance - (XSelectionDistance * Progress), LastSelection.Position.Y);

                Selection.Position = new Vector2(XSelectionDistance * Progress, Selection.Position.Y);

                Progress = Geometry.CubicBezier(new Vector2(0.5f, 0), new Vector2(0.5f, 0), Progress);

                var CurrentY = MoveInitialY + (MoveDeltaY * Progress);

                SongList.Position = new Vector2(SongList.Position.X, CurrentY);
            }
        }

        private void DoFade(long Tick)
        {
            if (BeginFadeOut >= 0)
            {
                if (BeginFadeOut == 0)
                {
                    BeginFadeOut = Tick;
                }

                var DeltaTick = Tick - BeginFadeOut;

                var ElapsedMS = (float)(DeltaTick / Constants.ORBIS_MILISECOND);

                const int FadeDuration = 500;

                float Progress = ElapsedMS / FadeDuration;

                if (Progress > 1)
                {
                    BeginFadeOut = -1;
                    Progress = 1;
                }

                Fade.Opacity = (byte)(255 * Progress);
            }

            if (BeginFadeIn >= 0)
            {
                if (BeginFadeIn == 0)
                {
                    BeginFadeIn = Tick;
                }

                var DeltaTick = Tick - BeginFadeIn;

                var ElapsedMS = (float)(DeltaTick / Constants.ORBIS_MILISECOND);

                const int FadeDuration = 500;

                float Progress = ElapsedMS / FadeDuration;

                if (Progress > 1)
                {
                    BeginFadeIn = -1;
                    Application.Default.RemoveObjects();
                    Application.Default.AddObject(this);
                    Progress = 1;
                }

                Fade.Opacity = (byte)(255 - (255 * Progress));
            }
        }

        private void SongEnd(object sender, EventArgs e)
        {
            var Player = (SongPlayer)sender;
            Player.Dispose();

            Application.Default.AddObject(this);

            BeginFadeIn = 0;
            Started = false;
        }
    }
}

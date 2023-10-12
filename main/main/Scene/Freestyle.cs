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

        ChoiceScene Menu;

        static Texture2D TexView;
        static SpriteAtlas2D Atlas;

        private Vector2 SongListPos = new Vector2(50f, 0);

        private WavePlayer Theme;
        private OrbisAudioOut ThemeDriver;

        private AtlasText2D DifficultyView;

        private Rectangle2D Fade = new Rectangle2D(1920, 1080, true);

        LoadingScene LoadScreen;

        Dificuty CurrentDifficulty = Dificuty.Normal;

        public FreestyleScene(WavePlayer Theme, OrbisAudioOut ThemeDriver, LoadingScene LoadScreen)
        {
            this.Theme = Theme;
            this.ThemeDriver = ThemeDriver;
            this.LoadScreen = LoadScreen;
        }

        public bool Loaded { get; set; }

        public int TotalProgress => 2;

        public bool ChoiceDone { get; private set; }

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

            Menu = new ChoiceScene(SongListPos, 1920, 1080, Songs.Select(GetFirendlyName).ToArray(), Atlas);
            Menu.OnChoiceDoneBegin += Menu_OnChoiceDoneBegin;
            Menu.OnChoiceDoneEnd += Menu_OnChoiceDoneEnd;

            AddChild(Menu);

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
            

            OnProgressChanged?.Invoke(2);

        }

        private void Menu_OnChoiceDoneEnd(object sender, EventArgs e)
        {
            ChoiceDone = true;
        }

        private void Menu_OnChoiceDoneBegin(object sender, EventArgs e)
        {
            ThemeDriver.SetVolume(0);
            BeginFadeOut = 0;
        }

        private void OnKeyUp(object Sender, KeyboardEventArgs Args)
        {
            if (Started)
                return;

            if (Args.Keycode == IME_KeyCode.DOWNARROW)
                Menu.MoveDown();
            if (Args.Keycode == IME_KeyCode.UPARROW)
                Menu.MoveUp();

            if (Args.Keycode == IME_KeyCode.RIGHTARROW)
                IncreaseDifficulty();
            if (Args.Keycode == IME_KeyCode.LEFTARROW)
                DecreaseDifficulty();

            if (Args.Keycode == IME_KeyCode.KEYPAD_ENTER || Args.Keycode == IME_KeyCode.RETURN)
                Menu.DoChoice();
        }

        private void GamepadOnOnButtonUp(object sender, ButtonEventArgs args)
        {
            if (Started)
                return;

            if (args.Button.HasFlag(OrbisPadButton.Down))
                Menu.MoveDown();

            if (args.Button.HasFlag(OrbisPadButton.Up))
                Menu.MoveUp();

            if (args.Button.HasFlag(OrbisPadButton.Left))
                DecreaseDifficulty();

            if (args.Button.HasFlag(OrbisPadButton.Right))
                IncreaseDifficulty();
            
            if (args.Button.HasFlag(OrbisPadButton.Options) || args.Button.HasFlag(OrbisPadButton.Cross))
                Menu.DoChoice();
        }

        int SFXTrack = 1;

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
                ThemeDriver?.SetVolume(80);
                Theme?.Resume();
            }

            DoFade(Tick);

            if (ChoiceDone && BeginFadeOut == -1)
            {
                ChoiceDone = false;

                StartSong();

                return;
            }

            LastFrameTick = Tick;
            
            base.Draw(Tick);
        }

        private void StartSong() => StartSong(Util.GetSongByName(Songs[Menu.SelectedIndex], CurrentDifficulty));
        private void StartSong(SongInfo Song)
        {
            SongPlayer SP = new SongPlayer(Song);
            SP.OnSongEnd += SongEnd;
            SP.OnSongRestart += SongRestart;

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

                const int FadeDuration = 2000;

                float Progress = ElapsedMS / FadeDuration;

                if (Progress >= 1)
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
                    Application.Default.RemoveObjects();
                    Application.Default.AddObject(this);
                    BeginFadeIn = Tick;
                }

                var DeltaTick = Tick - BeginFadeIn;

                var ElapsedMS = (float)(DeltaTick / Constants.ORBIS_MILISECOND);

                const int FadeDuration = 1000;

                float Progress = ElapsedMS / FadeDuration;

                if (Progress >= 1)
                {
                    BeginFadeIn = -1;
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
        
        private void SongRestart(object sender, EventArgs e)
        {
            var Player = (SongPlayer)sender;
            var Info = Player.SongInfo; 
            Player.Dispose();

            StartSong(Info);
        }
    }
}

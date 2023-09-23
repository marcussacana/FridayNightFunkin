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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Orbis.Scene
{
    internal class FreestyleScene : GLObject2D, ILoadable
    {
        private MemoryStream SFXA;
        private MemoryStream SFXB;
        private MemoryStream SFXC;
        
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
        private Vector2 SongListPos = new Vector2(50f, 0);

        private WavePlayer Theme;
        private OrbisAudioOut ThemeDriver;
        private WavePlayer SFXPlayer;
        private OrbisAudioOut SFXDriver;
        private WavePlayer SFXBPlayer;
        private OrbisAudioOut SFXBDriver;
        private WavePlayer SFXCPlayer;
        private OrbisAudioOut SFXCDriver;

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

#if ORBIS
            SFXPlayer.Close();
            SFXDriver.SetVolume(0);
            SFXDriver.Stop();
            
            SFXBPlayer = new WavePlayer();
            SFXBDriver = new OrbisAudioOut();

            SFXBPlayer.SetAudioDriver(SFXBDriver);
            
            SFXCPlayer = new WavePlayer();
            SFXCDriver = new OrbisAudioOut();

            SFXCPlayer.SetAudioDriver(SFXCDriver);
            

            SFXA = SFXHelper.Default.GetSFX(SFXType.MenuChoice);
            SFXB = new MemoryStream();
            SFXA.CopyTo(SFXB);

            SFXA.Position = 0;
            SFXC = new MemoryStream();
            SFXA.CopyTo(SFXC);
#endif

            if (SFXPlayer != null)
                SFXPlayer.OnTrackEnd += ConfirmTrackEnd;

            if (SFXBPlayer != null)
                SFXBPlayer.OnTrackEnd += ConfirmTrackEnd;
            
            if (SFXCPlayer != null)
                SFXCPlayer.OnTrackEnd += ConfirmTrackEnd;
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

                CurrentPos += new Vector2(Text.Width, Text.Height + 50);
            }

            SongList.Position = SongListPos;
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

            if (Args.Keycode == IME_KeyCode.KEYPAD_ENTER || Args.Keycode == IME_KeyCode.RETURN)
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

        bool WaitingSongEnd = false;
        bool MusicConfirmed = false;
        private void SongConfirm()
        {
            if (WaitingSongEnd)
                return;
            
            if (SFXPlayer != null)
            {
                WaitingSongEnd = true;
                MusicConfirmed = false;
                BeginFadeOut = 0;
                
                PlayAudioSwap(SFXHelper.Default.GetSFX(SFXType.MenuConfirm));
                
                ThemeDriver.SetVolume(0);
            }
            else
            {
                MusicConfirmed = true;
            }
        }
        private void ConfirmTrackEnd(object sender, EventArgs e)
        {
            //Prevents the SFX from the Main Menu trigger this method in the initialization
            if (!WaitingSongEnd)
                return;
            
            WaitingSongEnd = false;

            //Again, the OnTrackEnd event runs in a secondary thread, we can't touch in the OpenGL objects.
            //So, let's just set an flag and let the main draw loop detect it
            MusicConfirmed = true;
        }

        int SFXTrack = 1;

        const int XSelectionDistance = 50;
        const int AnimSelectionDuration = 250;

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
            MoveInitialY = SongList.Position.Y;

            var CenterSelectionY = Selection.ZoomPosition.Y + (Selection.ZoomHeight / 2);
            var TargetListY = ListBasePos - CenterSelectionY;

            MoveDeltaY = TargetListY - MoveInitialY;

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
                
                MemoryStream SelSFX;
                switch (SFXTrack)
                {
                    case 0:
                        SelSFX = SFXA;
                        break;
                    case 1:
                        SelSFX = SFXB;
                        break;
                    default:
                        SelSFX = SFXC;
                        break;
                }
                PlayAudioSwap(SelSFX);
            }
        }

        /// <summary>
        /// Our Audio Player can't instant supend the current track and swap to a new one,
        /// due the buffering, if we want interrupt the audio we may swap to a new instance
        /// and mute the old one, this class has 3 instances and the audio duration is 500ms
        /// with two instances we can asume that changing the bettewen both instances with the 
        /// max of 250ms of delay the last audio will be finished, then 3 should be safe.
        /// </summary>
        private void PlayAudioSwap(MemoryStream Audio)
        {
            if (Audio == null)
                return;

            Audio.Position = 0;

            switch (SFXTrack++)
            {
                case 0:
                    SFXBDriver?.SetVolume(0);
                    SFXCDriver?.SetVolume(0);
                    SFXBPlayer?.Close();
                    SFXCPlayer?.Close();
                        
                    SFXDriver?.SetVolume(80);
                    SFXPlayer?.Open(Audio);
                    SFXPlayer?.Restart();
                    break;
                case 1:
                    SFXDriver?.SetVolume(0);
                    SFXCDriver?.SetVolume(0);
                    SFXPlayer?.Close();
                    SFXCPlayer?.Close();
                    
                    SFXBDriver?.SetVolume(80);
                    SFXBPlayer?.Open(Audio);
                    SFXBPlayer?.Restart();
                    break;
                default:
                    SFXDriver?.SetVolume(0);
                    SFXBDriver?.SetVolume(0);
                    SFXPlayer?.Close();
                    SFXBPlayer?.Close();
                    
                    SFXCDriver?.SetVolume(80);
                    SFXCPlayer?.Open(Audio);
                    SFXCPlayer?.Restart();
                    break;
            }

            if (SFXTrack > 2)
                SFXTrack = 0;
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

            if (MusicConfirmed && BeginFadeOut == -1)
            {
                MusicConfirmed = false;

                StartSong();

                return;
            }

            base.Draw(Tick);
        }

        private void StartSong() => StartSong(Util.GetSongByName(Songs[SelectedIndex], CurrentDifficulty));
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

        private void DoSongSelection(long Tick)
        {
            if (AnimTick >= 0)
            {
                if (AnimTick == 0)
                {
                    AnimTick = Tick;
                    SFXPlayer?.Restart();
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

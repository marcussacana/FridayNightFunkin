﻿using Orbis.Audio;
using Orbis.BG;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.Controls.Events;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.IO;
using System.Numerics;

namespace Orbis.Game
{
    public class SongPlayer : GLObject2D, ILoadable
    {
        SFXHelper SFX;
        
        MusicPlayer MusicPlayer;

        HealthBar Health;

        Blank2D BackLayer = new Blank2D();

        Blank2D FrontLayer = new Blank2D();


        float CoordinatesScale = 1.5f;//1.5 for 1080p

        bool IsStoryMode;

        bool LastNoteIsFromPlayer1;

        public SongInfo SongInfo { get; private set; }

        IBG BG;

        GLObject2D BGObject;

        TiledSpriteAtlas2D Speaker;
        TiledSpriteAtlas2D Player1;
        TiledSpriteAtlas2D Player2;

        CharacterAnim Player1Anim;
        CharacterAnim Player2Anim;
        CharacterAnim SpeakerAnim;

        Vector2 Player2CamPos;
        Vector2 Player1CamPos;

        string Player1AnimPrefix = string.Empty;
        string Player2AnimPrefix = string.Empty;
        string SpeakerAnimPrefix = string.Empty;
        string Player1AnimSufix = string.Empty;
        string Player2AnimSufix = string.Empty;
        string SpeakerAnimSufix = string.Empty;

        string Player1CurrentAnim = string.Empty;
        string Player2CurrentAnim = string.Empty;
        string SpeakerCurrentAnim = string.Empty;

        NoteMenu Player1Menu;
        NoteMenu Player2Menu;

        Vector2 DiePlayerPos;
        long DieTime;
        Rectangle2D DieBG = null;
        bool Player1Dead;


        bool AnimationChanged = true;

        public SongPlayer(SongInfo SongInfo)
        {
            switch (SongInfo.BG) {
                case Map.Stage:
                    BG = (IBG)(BGObject = new StageBG());
                    break;
                default:
                    throw new NotImplementedException();
            };

            SFX = new SFXHelper();

            if (SongInfo.Speaker == null)
                SongInfo.Speaker = "gf";

            SongInfo.Player1 = SongInfo.song.player1;

            switch (SongInfo.song.player2)
            {
                case "gf":
                    SongInfo.Player2 = null;
                    break;
                default:
                    SongInfo.Player2 = SongInfo.song.player2;
                    break;
            }

            SongInfo.Name = SongInfo.song.song;

            SetBPM(SongInfo.song.bpm);

            this.SongInfo = SongInfo;
        }

        public int TotalProgress => 11 + BG.TotalProgress + SFX.TotalProgress;

        public bool Loaded { get; private set; }

        public void Load(Action<int> OnProgressChanged)
        {
            BG.Load(OnProgressChanged);
            SFX.Load((i) => { OnProgressChanged(BG.TotalProgress + i); });

            int BaseProgress = BG.TotalProgress + SFX.TotalProgress;

            //1 - Load Player1 Sprite Info
            var P1Asset = Character.AssetsMap[SongInfo.Player1];
            var P1AssetData = Util.GetXML(P1Asset);

            OnProgressChanged?.Invoke(BaseProgress + 1);

            //2 - Load Player1 Sprite
            Player1 = new TiledSpriteAtlas2D();
            Player1.LoadSprite(P1AssetData, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(BaseProgress + 2);

            if (SongInfo.Player2 != null)
            {
                //3 - Load Player2 Sprite Info
                var P2Asset = Character.AssetsMap[SongInfo.Player2];
                var P2AssetData = Util.GetXML(P2Asset);

                OnProgressChanged?.Invoke(BaseProgress + 3);

                //4 - Load Player2 Sprite
                Player2 = new TiledSpriteAtlas2D();
                Player2.LoadSprite(P2AssetData, Util.CopyFileToMemory, true);

                OnProgressChanged?.Invoke(BaseProgress + 4);

            }
            else
            {
                Player2?.Dispose();
                Player2 = null;
            }

            //5 - Load Speaker Sprite Info
            var SpeakerAsset = Orbis.Speaker.AssetsMap[SongInfo.Speaker];
            var SpeakerAssetData = Util.GetXML(SpeakerAsset);

            OnProgressChanged?.Invoke(BaseProgress + 5);

            //6 - Load Speaker Sprite
            Speaker = new TiledSpriteAtlas2D();
            Speaker.LoadSprite(SpeakerAssetData, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(BaseProgress + 6);

            //7 - Load Notes Sprite Info
            var NotesAssetData = Util.GetXML(NotesNames.NotesAssets);

            OnProgressChanged?.Invoke(BaseProgress + 7);

            //8 - Load Notes
            var Notes = new SpriteAtlas2D(NotesAssetData, Util.CopyFileToMemory, true);

            Player1Menu = new NoteMenu(Notes, this, true, false);
            Player2Menu = new NoteMenu(Notes, this, false, true);

            Player1.OnAnimationEnd += OnAnimEnd;
            Player2.OnAnimationEnd += OnAnimEnd;

            Notes.Texture = null;//Prevent Texture disposal
            Notes.Dispose();

            OnProgressChanged?.Invoke(BaseProgress + 8);

            BG.OnMapStatusChanged += StatusChanged;
            Player1Menu.OnNoteElapsed += StatusChanged;
            Player2Menu.OnNoteElapsed += StatusChanged;

            //9 - Load Health Bar
            using (var HealthBarData = Util.CopyFileToMemory("healthBar.dds"))
            using (var P1Icon = Util.CopyFileToMemory(Character.IconMap[SongInfo.Player1]))
            using (var P2Icon = Util.CopyFileToMemory(Character.IconMap[SongInfo.Player2 ?? "gf"]))
            {
                var HealthTex = new Texture(true);
                HealthTex.SetDDS(HealthBarData, false);


                var P1Tex = new Texture(true);
                var P2Tex = new Texture(true);

                P1Tex.SetDDS(P1Icon, false);
                P2Tex.SetDDS(P2Icon, false);

                Health = new HealthBar(Player1Menu, Player2Menu, HealthTex, P1Tex, P2Tex);
            }

            OnProgressChanged?.Invoke(BaseProgress + 9);

            //10 - Load Music
#if ORBIS
            var Instrumental = Util.CopyFileToMemory($"songs/{SongInfo.Name}/Inst_48khz.wav");
            var Voices = Util.CopyFileToMemory($"songs/{SongInfo.Name}/Voices_48khz.wav");

            MusicPlayer = new MusicPlayer(Instrumental, Voices, false);
#endif
            OnProgressChanged?.Invoke(BaseProgress + 10);

            //11 - Setup Env
            SetupDisplay();
            SetupInput();
            SetupEvents();

            Loaded = true;

            OnProgressChanged?.Invoke(BaseProgress + 11);
        }

        private void SetupEvents()
        {
            Health.OnPlayerDies += OnPlayerDies;
        }

        private void OnPlayerDies(object sender, NoteMenu e)
        {
            if (DieTime != 0)
                return;

            bool IsPlayer1 = e == Player1Menu;

            if (!IsPlayer1 && Player2Menu.CPU)
                return;
            
            MusicPlayer.Pause();
            MusicPlayer.PlayActiveSFX(SFX.GetSFX(SFXType.Dies));

            Player1Dead = IsPlayer1;

            DieBG = new Rectangle2D(Application.Default.Width, Application.Default.Height, true);
            DieBG.Color = RGBColor.Black;
            DieBG.Opacity = 0;
            DieBG.RefreshVertex();

            if (IsPlayer1)
            {
                Player1CurrentAnim = Player1Anim.DIES;
                AnimationChanged = true;
                UpdateAnimations();
            }
            else
            {
                Player2CurrentAnim = Player2Anim.DIES;
                AnimationChanged = true;
                UpdateAnimations();
            }

            DieTime = LastDrawTick;

            AddChild(DieBG);
        }

        private void OnAnimEnd(object sender, EventArgs e)
        {
            if (sender == Player1)
            {
                if (Player1.CurrentSprite == Player1Anim.HEY ||
                    Player1.CurrentSprite == Player1Anim.LEFT_MISS ||
                    Player1.CurrentSprite == Player1Anim.UP_MISS ||
                    Player1.CurrentSprite == Player1Anim.RIGHT_MISS ||
                    Player1.CurrentSprite == Player1Anim.DOWN_MISS)
                {
                    Player1CurrentAnim = Player1Anim.DANCING;
                    AnimationChanged = true;
                    UpdateAnimations();
                    return;
                }

                if (Player1.CurrentSprite == Player1Anim.DIES)
                {
                    DiePlayerPos += Player1Anim.GetAnimOffset(Player1Anim.DIES);
                    DiePlayerPos -= Player1Anim.GetAnimOffset(Player1Anim.DEAD);

                    Player1CurrentAnim = Player1Anim.DEAD;
                    AnimationChanged = true;
                    UpdateAnimations();
                    return;
                }
            }

            if (sender == Player2)
            {
                if (Player2.CurrentSprite == Player2Anim.HEY ||
                    Player2.CurrentSprite == Player2Anim.LEFT_MISS ||
                    Player2.CurrentSprite == Player2Anim.UP_MISS ||
                    Player2.CurrentSprite == Player2Anim.RIGHT_MISS ||
                    Player2.CurrentSprite == Player2Anim.DOWN_MISS)
                {
                    Player2CurrentAnim = Player2Anim.DANCING;
                    AnimationChanged = true;
                    UpdateAnimations();
                    return;
                }

                if (Player2.CurrentSprite == Player2Anim.DIES)
                {
                    DiePlayerPos += Player2Anim.GetAnimOffset(Player1Anim.DIES);
                    DiePlayerPos -= Player2Anim.GetAnimOffset(Player1Anim.DEAD);

                    Player2CurrentAnim = Player2Anim.DEAD;
                    AnimationChanged = true;
                    UpdateAnimations();
                    return;
                }
            }
        }

        bool CanBegin = true;
        bool BeginRequested = false;
        long BeginAudioTick = 0;
        public void Begin()
        {
            if (!CanBegin)
                return;

            CanBegin = false;
            BeginRequested = true;
        }

        private void SetupInput()
        {
#if ORBIS
            Application.Default.Gamepad.OnButtonDown += Gamepad_OnButtonDown;
            Application.Default.Gamepad.OnButtonUp += Gamepad_OnButtonUp;
#endif

            Application.Default.KeyboardDriver.OnKeyDown += KeyboardDriver_OnKeyDown;
            Application.Default.KeyboardDriver.OnKeyUp += KeyboardDriver_OnKeyUp;
        }

        private void KeyboardDriver_OnKeyUp(object Sender, KeyboardEventArgs Args)
        {
            if (DieTime == 0)
            {
                switch (Args.Keycode)
                {
                    case IME_KeyCode.W:
                    case IME_KeyCode.UPARROW:
                        Player1Menu.UnsetPress(Note.Up);
                        break;
                    case IME_KeyCode.S:
                    case IME_KeyCode.DOWNARROW:
                        Player1Menu.UnsetPress(Note.Down);
                        break;
                    case IME_KeyCode.A:
                    case IME_KeyCode.LEFTARROW:
                        Player1Menu.UnsetPress(Note.Left);
                        break;
                    case IME_KeyCode.D:
                    case IME_KeyCode.RIGHTARROW:
                        Player1Menu.UnsetPress(Note.Right);
                        break;
                }
            }
        }

        private void KeyboardDriver_OnKeyDown(object Sender, KeyboardEventArgs Args)
        {
            if (DieTime == 0)
            {
                switch (Args.Keycode)
                {
                    case IME_KeyCode.W:
                    case IME_KeyCode.UPARROW:
                        Player1Menu.SetPress(Note.Up);
                        break;
                    case IME_KeyCode.S:
                    case IME_KeyCode.DOWNARROW:
                        Player1Menu.SetPress(Note.Down);
                        break;
                    case IME_KeyCode.A:
                    case IME_KeyCode.LEFTARROW:
                        Player1Menu.SetPress(Note.Left);
                        break;
                    case IME_KeyCode.D:
                    case IME_KeyCode.RIGHTARROW:
                        Player1Menu.SetPress(Note.Right);
                        break;
                }
            }
        }

        private void Gamepad_OnButtonDown(object Sender, ButtonEventArgs Args)
        {
            if (DieTime == 0)
            {
                switch (Args.Button)
                {
                    case OrbisPadButton.Triangle:
                    case OrbisPadButton.Up:
                        Player1Menu.SetPress(Note.Up);
                        break;
                    case OrbisPadButton.Square:
                    case OrbisPadButton.Left:
                        Player1Menu.SetPress(Note.Left);
                        break;
                    case OrbisPadButton.Circle:
                    case OrbisPadButton.Right:
                        Player1Menu.SetPress(Note.Right);
                        break;
                    case OrbisPadButton.Cross:
                    case OrbisPadButton.Down:
                        Player1Menu.SetPress(Note.Down);
                        break;
                }
            }
        }
        private void Gamepad_OnButtonUp(object Sender, ButtonEventArgs Args)
        {
            if (DieTime == 0)
            {
                switch (Args.Button)
                {
                    case OrbisPadButton.Triangle:
                    case OrbisPadButton.Up:
                        Player1Menu.UnsetPress(Note.Up);
                        break;
                    case OrbisPadButton.Square:
                    case OrbisPadButton.Left:
                        Player1Menu.UnsetPress(Note.Left);
                        break;
                    case OrbisPadButton.Circle:
                    case OrbisPadButton.Right:
                        Player1Menu.UnsetPress(Note.Right);
                        break;
                    case OrbisPadButton.Cross:
                    case OrbisPadButton.Down:
                        Player1Menu.UnsetPress(Note.Down);
                        break;
                }
            }
        }

        private void SetupDisplay()
        {
            Speaker.Position = new Vector2(400, 120) * CoordinatesScale;

            switch (SongInfo.Speaker)
            {
                case "pico-speaker":
                case "pico":
                    Speaker.Position -= new Vector2(50, 200) * CoordinatesScale;
                    break;
            }

            if (Player2 != null)
            {
                Player2.Position = new Vector2(100, 100) * CoordinatesScale;
                Player2CamPos = Player2.GetMiddle();
            }

            ComputePlayer2Position();

            Player1.Position = new Vector2(640, 350) * CoordinatesScale;

            ComputePlayer1Position();

            BG.SetCharacterPosition(Player1, Player2, Speaker);

            BackLayer.AddChild(BGObject);
            BackLayer.AddChild(Speaker);
            BackLayer.AddChild(Player1);
            BackLayer.AddChild(Player2);
            AddChild(BackLayer);

            FrontLayer.AddChild(Player1Menu);
            FrontLayer.AddChild(Player2Menu);
            FrontLayer.AddChild(Health);
            AddChild(FrontLayer);

            Player1Menu.Position = new Vector2(1210, 50);
            Player2Menu.Position = new Vector2(50, 50);

            EnableAnimations();
        }

        private void EnableAnimations()
        {
            if (Player1Anim == null)
            {
                Player1Anim = new CharacterAnim(SongInfo.Player1);
                Player2Anim = new CharacterAnim(SongInfo.Player2);
                SpeakerAnim = new CharacterAnim(SongInfo.Speaker);
            }

            Player1CurrentAnim = Player1Anim.DANCING;
            Player2CurrentAnim = Player2Anim.DANCING;
            SpeakerCurrentAnim = SpeakerAnim.DANCING;

            AnimationChanged = true;

            UpdateAnimations();
        }

        private void UpdateAnimations()
        {
            if (!AnimationChanged)
                return;

            var P1OldOffset = Player1Anim.GetAnimOffset(Player1.CurrentSprite);
            var P2OldOffset = Player2Anim.GetAnimOffset(Player2.CurrentSprite);

            AnimationChanged = false;

            string FullAnimationName = $"{Player1AnimPrefix}{Player1CurrentAnim}{Player1AnimSufix}";
            string PrefixOnlyAnimation = $"{Player1AnimPrefix}{Player1CurrentAnim}";
            string SuffixOnlyAnimation = $"{Player1CurrentAnim}{Player1AnimSufix}";

            if (!Player1.SetActiveAnimation(FullAnimationName) &&
                !Player1.SetActiveAnimation(PrefixOnlyAnimation) &&
                !Player1.SetActiveAnimation(SuffixOnlyAnimation))
            {
                Player1.SetActiveAnimation(Player1CurrentAnim);
            }

            if (Player2 != null)
            {
                FullAnimationName = $"{Player2AnimPrefix}{Player2CurrentAnim}{Player2AnimSufix}";
                PrefixOnlyAnimation = $"{Player2AnimPrefix}{Player2CurrentAnim}";
                SuffixOnlyAnimation = $"{Player2CurrentAnim}{Player2AnimSufix}";

                if (!Player2.SetActiveAnimation(FullAnimationName) &&
                    !Player2.SetActiveAnimation(PrefixOnlyAnimation) &&
                    !Player2.SetActiveAnimation(SuffixOnlyAnimation))
                {
                    Player2.SetActiveAnimation(Player2CurrentAnim);
                }
            }

            FullAnimationName = $"{SpeakerAnimPrefix}{SpeakerCurrentAnim}{SpeakerAnimSufix}";
            PrefixOnlyAnimation = $"{SpeakerAnimPrefix}{SpeakerCurrentAnim}";
            SuffixOnlyAnimation = $"{SpeakerCurrentAnim}{SpeakerAnimSufix}";

            if (!Speaker.SetActiveAnimation(FullAnimationName) &&
                !Speaker.SetActiveAnimation(PrefixOnlyAnimation) &&
                !Speaker.SetActiveAnimation(SuffixOnlyAnimation))
            {
                Speaker.SetActiveAnimation(SpeakerCurrentAnim);
            }

            var P1Offset = Player1Anim.GetAnimOffset(Player1.CurrentSprite);
            var P2Offset = Player2Anim.GetAnimOffset(Player2.CurrentSprite);

            if (P1Offset != P1OldOffset)
            {
                Player1.Position += P1OldOffset;
                Player1.Position -= P1Offset;
            }

            if (P2Offset != P2OldOffset)
            {
                Player2.Position += P2OldOffset;
                Player2.Position -= P2Offset;
            }
        }
        public void ComputeStep(out int BeatPerMS, out int StepPerMS)
        {
            BeatPerMS = (int)((60 / SongInfo.song.bpm) * 1000);
            StepPerMS = BeatPerMS / 4;
        }
        private void ComputePlayer1Position()
        {
            switch (SongInfo.BG)
            {
                case Map.Limo:
                    Player1.Position += new Vector2(-220, 260) * CoordinatesScale;
                    break;
            }
        }

        private void ComputePlayer2Position()
        {
            switch (SongInfo.Player2)
            {
                case null:
                case "gf":
                    Player2CamPos = Speaker.GetMiddle();
                    break;
                case "spooky":
                    Player2.Position += new Vector2(0, 200) * CoordinatesScale;
                    break;
                case "monster":
                    Player2.Position += new Vector2(0, 100) * CoordinatesScale;
                    break;
                case "monster-christmas":
                    Player2.Position += new Vector2(0, 130) * CoordinatesScale;
                    break;
                case "dad":
                    Player2.Position += new Vector2(170, -25);
                    break;
                case "pico":
                    Player2CamPos += new Vector2(600, 0) * CoordinatesScale;
                    Player2.Position += new Vector2(300, 0) * CoordinatesScale;
                    break;
                case "parents-christmas":
                    Player2.Position += new Vector2(-500, 0) * CoordinatesScale;
                    break;
                case "senpai-angry":
                case "senpai":
                    Player2.Position += new Vector2(150, 360) * CoordinatesScale;
                    Player2CamPos = (Player2.GetMiddle() + new Vector2(300, 0)) * CoordinatesScale;
                    break;
                case "spirit":
                    Player2.Position += new Vector2(-150, 100) * CoordinatesScale;
                    Player2CamPos = (Player2.GetMiddle() + new Vector2(300, 0)) * CoordinatesScale;
                    break;
                case "tankman":
                    Player2.Position += new Vector2(0, 180) * CoordinatesScale;
                    break;
            }
        }
        private void StatusChanged(object sender, NewStatusEvent Status)
        {
            if (DieTime != 0)
                return;

            if (SongInfo.Player2 == null)
                Status.Target = EventTarget.Speaker;

            switch (Status.Target)
            {
                case EventTarget.Player1:
                    Player1AnimPrefix = Status.AnimationPrefix;
                    Player1AnimSufix = Status.AnimationSufix;
                    if (!string.IsNullOrWhiteSpace(Status.NewAnimation))
                        Player1CurrentAnim = Status.NewAnimation;
                    break;
                case EventTarget.Player2:
                    Player2AnimPrefix = Status.AnimationPrefix;
                    Player2AnimSufix = Status.AnimationSufix;
                    if (!string.IsNullOrWhiteSpace(Status.NewAnimation))
                        Player2CurrentAnim = Status.NewAnimation;
                    break;
                case EventTarget.Speaker:
                    SpeakerAnimPrefix = Status.AnimationPrefix;
                    SpeakerAnimSufix = Status.AnimationSufix;
                    if (!string.IsNullOrWhiteSpace(Status.NewAnimation))
                        SpeakerCurrentAnim = Status.NewAnimation;
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (sender is NoteMenu Menu)
            {
                bool IsPlayer1 = Menu == Player1Menu;

                if (LastNoteIsFromPlayer1 && !IsPlayer1)
                    Player1CurrentAnim = Player1Anim.HEY;

                if (!LastNoteIsFromPlayer1 && IsPlayer1)
                    Player2CurrentAnim = Player2Anim.HEY ?? Player2Anim.DANCING ?? Player2CurrentAnim;

                LastNoteIsFromPlayer1 = IsPlayer1;
            }

            AnimationChanged = true;
            UpdateAnimations();
        }

        public void SetBPM(float BPM)
        {
#if ORBIS
            BPMTicks = (int)((BPM / 60) * Constants.ORBIS_MILISECOND) * 10;
#else
            BPMTicks = (int)((BPM / 60) * Constants.ORBIS_MILISECOND);
#endif
        }

        int BPMTicks;

        long LastDrawTick;
        long NextUpdateFrame = 0;
        public override void Draw(long Tick)
        {
            if (!Loaded)
                return;

            bool FirstFrame = false;

            ExecuteDeadAnim(Tick);

            if (Tick > NextUpdateFrame)
            {
                if (NextUpdateFrame == 0)
                    FirstFrame = true;
                
                NextUpdateFrame = Tick + BPMTicks;

                Player1?.NextFrame();
                Player2?.NextFrame();
                Speaker?.NextFrame();
            }

            //Firt frame may have an delayed tick due the loading,
            //Let's skip the timmer related events to the next one,
            //that will have an updated tick.
            if (!FirstFrame)
            {
                if (BeginRequested)
                {
                    BeginRequested = false;
                    BeginAudioTick = Tick + (Constants.ORBIS_MILISECOND * NoteMenu.StartDelayMS);

                    //Preload Audio
                    MusicPlayer?.Resume();
                    MusicPlayer?.Pause();

                    Player1Menu.SetSongBegin(Tick);
                    Player2Menu.SetSongBegin(Tick);
                }

                if (BeginAudioTick != 0 && Tick >= BeginAudioTick)
                {
                    MusicPlayer?.Resume();
                    BeginAudioTick = 0;
                }
            }

            LastDrawTick = Tick;
            base.Draw(Tick);
        }

        private void ExecuteDeadAnim(long Tick)
        {
            if (DieBG != null && DieBG.Opacity != 255)
            {
                long ElapsedTick = Tick - DieTime;
                int ElapsedMS = (int)(ElapsedTick / Constants.ORBIS_MILISECOND);
                DieBG.Opacity = (byte)(Math.Min(ElapsedMS / 500f, 1f) * 255);

                if (DieBG.Opacity == 255)
                {
                    BG.Dispose();


                    if (Player1Dead)
                    {
                        DieBG.AddChild(Player1);
                        Player2.Dispose();
                    }
                    else
                    {
                        DieBG.AddChild(Player2);
                        Player1.Dispose();
                    }

                    Player1Menu.Dispose();
                    Player2Menu.Dispose();
                    Health.Dispose();
                }
            }
            else if (DieBG != null)
            {
                var CenterBG = new Vector2(DieBG.Width, DieBG.Height) / 2;
                var PlayerCenter = (Player1Dead ? new Vector2(Player1.Width, Player1.Height) : new Vector2(Player2.Width, Player2.Height)) / 2;

                var CenteredPoint = CenterBG - PlayerCenter;

                var Distance = CenteredPoint - DiePlayerPos;

                if (Distance == Vector2.Zero)
                    return;

                long ElapsedTick = Tick - DieTime;
                int ElapsedMS = (int)(ElapsedTick / Constants.ORBIS_MILISECOND);
                var MoveProgress = Math.Min(ElapsedMS / 1500f, 1f);


                if (Player1Dead)
                    Player1.Position = (Distance * MoveProgress) + DiePlayerPos;
                else
                    Player2.Position = (Distance * MoveProgress) + DiePlayerPos;
            }
        }

        public override void Dispose()
        {
            if (Loaded)
            {
#if ORBIS
                Application.Default.Gamepad.OnButtonDown -= Gamepad_OnButtonDown;
                Application.Default.Gamepad.OnButtonUp -= Gamepad_OnButtonUp;
#endif
            }

            base.Dispose();
        }
    }
}
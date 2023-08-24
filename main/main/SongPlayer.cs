using Orbis.BG;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.Controls.Events;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Xml;

namespace Orbis
{
    public class SongPlayer : GLObject2D, ILoadable
    {

        Blank2D BackLayer = new Blank2D();

        Blank2D FrontLayer = new Blank2D();


        float CoordinatesScale = 1.5f;//1.5 for 1080p

        bool IsStoryMode;

        public SongInfo SongInfo { get; private set; }

        IBG BG;

        GLObject2D BGObject;

        SpriteAtlas2D Speaker;
        SpriteAtlas2D Player1;
        SpriteAtlas2D Player2;

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

            SetBPM(SongInfo.song.bpm);

            this.SongInfo = SongInfo;
        }

        public int TotalProgress => 9 + BG.TotalProgress;

        public bool Loaded { get; private set; }

        public void Load(Action<int> OnProgressChanged)
        {
            BG.Load(OnProgressChanged);

            //1 - Load Player1 Sprite Info
            var P1Asset = Character.AssetsMap[SongInfo.Player1];
            var P1AssetData = Util.GetXML(P1Asset);

            OnProgressChanged?.Invoke(BG.TotalProgress + 1);

            //2 - Load Player1 Sprite
            Player1 = new SpriteAtlas2D();
            Player1.LoadSprite(P1AssetData, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(BG.TotalProgress + 2);

            if (SongInfo.Player2 != null)
            {
                //3 - Load Player2 Sprite Info
                var P2Asset = Character.AssetsMap[SongInfo.Player2];
                var P2AssetData = Util.GetXML(P2Asset);

                OnProgressChanged?.Invoke(BG.TotalProgress + 3);

                //4 - Load Player2 Sprite
                Player2 = new SpriteAtlas2D();
                Player2.LoadSprite(P2AssetData, Util.CopyFileToMemory, true);

                OnProgressChanged?.Invoke(BG.TotalProgress + 4);

            }
            else
            {
                Player2?.Dispose();
                Player2 = null;
            }

            //5 - Load Speaker Sprite Info
            var SpeakerAsset = Orbis.Speaker.AssetsMap[SongInfo.Speaker];
            var SpeakerAssetData = Util.GetXML(SpeakerAsset);

            OnProgressChanged?.Invoke(BG.TotalProgress + 5);

            //6 - Load Speaker Sprite
            Speaker = new SpriteAtlas2D();
            Speaker.LoadSprite(SpeakerAssetData, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(BG.TotalProgress + 6);

            //7 - Load Notes Sprite Info
            var NotesAssetData = Util.GetXML(NotesNames.NotesAssets);

            OnProgressChanged?.Invoke(BG.TotalProgress + 7);

            //8 - Load Notes
            var Notes = new SpriteAtlas2D(NotesAssetData, Util.CopyFileToMemory, true);

            Player1Menu = new NoteMenu(Notes, this, true);
            Player2Menu = new NoteMenu(Notes, this, false);

            Notes.Texture = null;//Prevent Texture disposal
            Notes.Dispose();

            OnProgressChanged?.Invoke(BG.TotalProgress + 8);

            BG.OnMapStatusChanged += BG_MapStatusChanged;

            //9
            SetupDisplay();
#if ORBIS
            SetupInput();
#endif

            Loaded = true;

            OnProgressChanged?.Invoke(BG.TotalProgress + 9);
        }

        bool CanBegin = true;
        bool BeginRequested = false;
        public void Begin()
        {
            if (!CanBegin)
                return;

            CanBegin = false;
            BeginRequested = true;
        }

        private void SetupInput()
        {
            Application.Default.Gamepad.OnButtonDown += Gamepad_OnButtonDown;
            Application.Default.Gamepad.OnButtonUp += Gamepad_OnButtonUp;
        }

        private void Gamepad_OnButtonDown(object Sender, ButtonEventArgs Args)
        {
            switch (Args.Button)
            {
                case OrbisPadButton.Triangle:
                case OrbisPadButton.Up:
                    Player1Menu.SetPress(Note.Up);
                    break;
            }
        }
        private void Gamepad_OnButtonUp(object Sender, ButtonEventArgs Args)
        {
            switch (Args.Button)
            {
                case OrbisPadButton.Triangle:
                case OrbisPadButton.Up:
                    Player1Menu.UnsetPress(Note.Up);
                    break;
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
            AddChild(FrontLayer);

            Player1Menu.Position = new Vector2(50, 50);
            Player2Menu.Position = new Vector2(1210, 50);

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

        private void BG_MapStatusChanged(object sender, NewStatusEvent Status)
        {
            switch (Status.Target)
            {
                case "player1":
                    Player1AnimPrefix = Status.AnimationPrefix;
                    Player1AnimSufix = Status.AnimationSufix;
                    break;
                case "player2":
                    Player2AnimPrefix = Status.AnimationPrefix;
                    Player2AnimSufix = Status.AnimationSufix;
                    break;
                case "speaker":
                    SpeakerAnimPrefix = Status.AnimationPrefix;
                    SpeakerAnimSufix = Status.AnimationSufix;
                    break;
                default:
                    throw new NotImplementedException();
            }

            AnimationChanged = true;
            UpdateAnimations();
        }

        public void SetBPM(float BPM)
        {
            BPMTicks = (int)((BPM / 60) * Constants.ORBIS_MILISECOND);
        }

        int BPMTicks;

        long NextUpdateFrame = 0;
        public override void Draw(long Tick)
        {
            if (Tick > NextUpdateFrame)
            {
                NextUpdateFrame = Tick + BPMTicks;

                Player1?.NextFrame();
                Player2?.NextFrame();
                Speaker?.NextFrame();
            }

            if (BeginRequested)
            {
                BeginRequested = false;
                Player1Menu.SetSongBegin(Tick);
                Player2Menu.SetSongBegin(Tick);
            }

            base.Draw(Tick);
        }

        public override void Dispose()
        {
            if (Loaded)
            {
                Application.Default.Gamepad.OnButtonDown -= Gamepad_OnButtonDown;
                Application.Default.Gamepad.OnButtonUp -= Gamepad_OnButtonUp;
            }

            base.Dispose();
        }
    }
}

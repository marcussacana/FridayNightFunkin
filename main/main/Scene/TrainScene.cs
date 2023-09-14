using Orbis.Audio;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.IO;
using System.Numerics;

namespace Orbis.Scene
{
    internal class TrainScene : GLObject2D, IScene
    {
        Texture2D Sky;
        Texture2D City;
        Texture2D Train;
        Texture2D BehindTrain;
        Texture2D Street;

        Texture2D WindowA;
        Texture2D WindowB;
        Texture2D WindowC;
        Texture2D WindowD;
        Texture2D WindowE;

        SongPlayer Game;

        MusicPlayer Player => Game.MusicPlayer;

        MemoryStream TrainSFX;

        int NextWindow;

        public TrainScene(SongPlayer Player)
        {
            Game = Player;
        }

        public bool Loaded { get; private set; }

        public int TotalProgress => 9;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;

        public void Load(Action<int> OnProgressChanged)
        {
            if (Loaded)
                return;

            using (var Data = Util.CopyFileToMemory("sky.dds"))
            {
                Sky = new Texture2D();
                Sky.Texture = new Texture(true);
                Sky.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(1);

            using (var Data = Util.CopyFileToMemory("city.dds"))
            {
                City = new Texture2D();
                City.Texture = new Texture(true);
                City.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(2);

            using (var Data = Util.CopyFileToMemory("behindTrain.dds"))
            {
                BehindTrain = new Texture2D();
                BehindTrain.Texture = new Texture(true);
                BehindTrain.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(3);

            using (var Data = Util.CopyFileToMemory("train.dds"))
            {
                Train = new Texture2D();
                Train.Texture = new Texture(true);
                Train.Texture.SetDDS(Data, true);
            }

            using (var Data = Util.CopyFileToMemory("street.dds"))
            {
                Street = new Texture2D();
                Street.Texture = new Texture(true);
                Street.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(4);

            using (var Data = Util.CopyFileToMemory("win0.dds"))
            {
                WindowA = new Texture2D();
                WindowA.Texture = new Texture(true);
                WindowA.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(5);

            using (var Data = Util.CopyFileToMemory("win1.dds"))
            {
                WindowB = new Texture2D();
                WindowB.Texture = new Texture(true);
                WindowB.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(6);

            using (var Data = Util.CopyFileToMemory("win2.dds"))
            {
                WindowC = new Texture2D();
                WindowC.Texture = new Texture(true);
                WindowC.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(7);

            using (var Data = Util.CopyFileToMemory("win3.dds"))
            {
                WindowD = new Texture2D();
                WindowD.Texture = new Texture(true);
                WindowD.Texture.SetDDS(Data, true);
            }

            OnProgressChanged?.Invoke(8);

            using (var Data = Util.CopyFileToMemory("win4.dds"))
            {
                WindowE = new Texture2D();
                WindowE.Texture = new Texture(true);
                WindowE.Texture.SetDDS(Data, true);
            }

            AddChild(Sky);
            AddChild(City);
            AddChild(BehindTrain);
            AddChild(Train);
            AddChild(Street);

            City.AddChild(WindowA);
            City.AddChild(WindowB);
            City.AddChild(WindowC);
            City.AddChild(WindowD);
            City.AddChild(WindowE);

            City.RefreshVertex();
            Train.RefreshVertex();

            Train.Position = new Vector2(TraingBeginX, City.Height - Train.Height - 190);

            TrainSFX = SFXHelper.Default.GetSFX(SFXType.Train);

            UpdateVisibleWindow();

            Width = City.Width;
            Height = City.Height;

            Position = this.GetMiddle() + (new Vector2(1920, 1080) / 2);

            Loaded = true;
            OnProgressChanged?.Invoke(9);
        }

        public override void SetZoom(float Value)
        {
            var ZoomVal = Coordinates2D.ParseZoomFactor(Value);
            var AditionalZoom = ZoomVal + 30;
            base.SetZoom(Coordinates2D.ParseZoomFactor(AditionalZoom));
        }

        private void UpdateVisibleWindow()
        {
            WindowA.Visible = false;
            WindowB.Visible = false;
            WindowC.Visible = false;
            WindowD.Visible = false;
            WindowE.Visible = false;

            switch (NextWindow)
            {
                case 0:
                    WindowA.Visible = true;
                    break;
                case 1:
                    WindowB.Visible = true;
                    break;
                case 2:
                    WindowC.Visible = true;
                    break;
                case 3:
                    WindowD.Visible = true;
                    break;
                default:
                    WindowE.Visible = true;
                    NextWindow = -1;
                    break;
            }

            NextWindow++;
        }

        const string HairLifitingAnim = "Hair Lifiting";

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            Game.SpeakerAnim.AddOffsetAlias(Game.SpeakerAnim.HAIR_LANDING, HairLifitingAnim);

            Speaker.CreateAnimationByIndex(Game.SpeakerAnim.HAIR_LANDING, HairLifitingAnim, false, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1, 0);
            Speaker.CreateAnimationByIndex(Game.SpeakerAnim.HAIR_LANDING, Game.SpeakerAnim.HAIR_LANDING, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            Speaker.CreateAnimationByIndex(Game.SpeakerAnim.HAIR_BLOWING, Game.SpeakerAnim.HAIR_BLOWING, true, 0, 1, 2, 3);

            Speaker.OnAnimationEnd += Speaker_OnAnimationEnd;
        }

        private void Speaker_OnAnimationEnd(object sender, EventArgs e)
        {
            SpriteAtlas2D Target = (SpriteAtlas2D)sender;

            if (Target.CurrentSprite == HairLifitingAnim)
            {
                OnMapStatusChanged?.Invoke(this, new NewStatusEvent()
                {
                    NewAnimation = Game.SpeakerAnim.HAIR_BLOWING
                });
                return;
            }

            if (Target.CurrentSprite == Game.SpeakerAnim.HAIR_BLOWING && !InTrainAnim)
            {
                OnMapStatusChanged?.Invoke(this, new NewStatusEvent()
                {
                    NewAnimation = Game.SpeakerAnim.HAIR_LANDING
                });
                return;
            }

            if (Target.CurrentSprite == Game.SpeakerAnim.HAIR_LANDING)
            {
                OnMapStatusChanged?.Invoke(this, new NewStatusEvent()
                {
                    NewAnimation = Game.SpeakerAnim.DANCING
                });
                return;
            }
        }

        Random Rnd = new Random();

        bool FirstTrain = true;
        bool SFXStarted = false;
        bool InTrainAnim = false;
        long LastTrainTick;
        long LastWindowUpdate;
        int TrainDelayMS;
        int SoundTrainDelayMS;

        const int TrainPassSFXDelay = 4500;
        const int TrainDurationMS = 1000;
        const int TrainTargetX = -4000;
        const int TraingBeginX = 1920;

        const int WindowDelayMS = 1500;


        public override void Draw(long Tick)
        {
            DoTrainAnim(Tick);

            int ElapsedWindowUpdateMS = (int)((Tick - LastWindowUpdate) / Constants.ORBIS_MILISECOND);

            if (ElapsedWindowUpdateMS > WindowDelayMS)
            {
                LastWindowUpdate = Tick;
                UpdateVisibleWindow();
            }


            base.Draw(Tick);
        }

        private void DoTrainAnim(long Tick)
        {
            if (LastTrainTick == 0)
            {
                if (!FirstTrain)
                    TrainDelayMS = Rnd.Next(10000, 15000);
                else
                    TrainDelayMS = Rnd.Next(5000, 9000);

                SoundTrainDelayMS = TrainDelayMS - TrainPassSFXDelay;

                LastTrainTick = Tick;
                InTrainAnim = false;
                SFXStarted = false;
                FirstTrain = false;
            }

            float ElapsedMS = (Tick - LastTrainTick) / Constants.ORBIS_MILISECOND;

            if (!SFXStarted && ElapsedMS >= SoundTrainDelayMS)
            {
                SFXStarted = true;

                if (TrainSFX != null)
                {
                    TrainSFX.Position = 0;
                    Player.SetPassiveSFXVol(0.40f);
                    Player.PlayPassiveSFX(TrainSFX);
                }
            }

            if (ElapsedMS > TrainDelayMS)
            {
                if (!InTrainAnim)
                {
                    InTrainAnim = true;
                    OnMapStatusChanged?.Invoke(this, new NewStatusEvent()
                    {
                        Target = EventTarget.Speaker,
                        NewAnimation = HairLifitingAnim
                    });
                }

                //Because we want measure the time after the train begin
                ElapsedMS -= TrainDelayMS;

                var TrainProgress = Math.Min(ElapsedMS / TrainDurationMS, 1f);

                var TrainXDistance = TraingBeginX - TrainTargetX;

                Train.ZoomPosition = new Vector2(TraingBeginX - (TrainXDistance * TrainProgress), Train.ZoomPosition.Y);

                if (TrainProgress == 1)
                {
                    InTrainAnim = false;
                    LastTrainTick = 0;
                }
            }
        }

        public override void Dispose()
        {
            Player.MutePassiveSFX();
            base.Dispose();
        }
    }
}

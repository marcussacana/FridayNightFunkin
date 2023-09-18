using ICSharpCode.SharpZipLib.Core;
using Orbis.Audio;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Scene
{
    internal class LimoScene : GLObject2D, IScene
    {
        SongPlayer Game;

        MemoryStream SFXA;
        MemoryStream SFXB;

        MemoryStream GameoverTheme;
        MemoryStream GameoverEnd;
        public LimoScene(SongPlayer Player)
        {
            Game = Player;
        }

        const int DancerDistance = 350;

        const int FPS30 = (int)(1000f/20);

        SpriteAtlas2D LimoBG;
        SpriteAtlas2D LimoDancerA;
        SpriteAtlas2D LimoDancerB;
        SpriteAtlas2D LimoDancerC;
        SpriteAtlas2D LimoDancerD;
        SpriteAtlas2D LimoDriver;

        Texture2D Car;
        Texture2D SunBG;
        public bool Loaded { get; private set; }

        public int TotalProgress => 5;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;
        public void Load(Action<int> OnProgressChanged)
        {
            var XML = Util.GetXML("bgLimo.xml");
            LimoBG = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(1);

            XML = Util.GetXML("limoDancer.xml");
            LimoDancerA = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);
            LimoDancerB = (SpriteAtlas2D)LimoDancerA.Clone(true);
            LimoDancerC = (SpriteAtlas2D)LimoDancerA.Clone(true);
            LimoDancerD = (SpriteAtlas2D)LimoDancerA.Clone(true);

            OnProgressChanged?.Invoke(2);


            using (var Stream = Util.CopyFileToMemory("fastCarLol.dds"))
            {
                Car = new Texture2D(Stream, true);
                Car.RefreshVertex();
            }

            OnProgressChanged?.Invoke(3);

            using (var Stream = Util.CopyFileToMemory("limoSunset.dds"))
            {
                SunBG = new Texture2D(Stream, true);
                SunBG.RefreshVertex();
            }

            OnProgressChanged?.Invoke(4);

            XML = Util.GetXML("limoDrive.xml");
            LimoDriver = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);

            LimoBG.SetActiveAnimation("background limo pink");

            LimoDancerA.SetActiveAnimation("bg dancer sketch PINK");
            LimoDancerB.SetActiveAnimation("bg dancer sketch PINK");
            LimoDancerC.SetActiveAnimation("bg dancer sketch PINK");
            LimoDancerD.SetActiveAnimation("bg dancer sketch PINK");

            LimoDriver.SetActiveAnimation("Limo stage");

            LimoBG.FrameDelay = FPS30;
            LimoDriver.FrameDelay = FPS30;

            LimoDancerA.Position = new Vector2(200, 150);
            LimoDancerB.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 1), LimoDancerA.Position.Y);
            LimoDancerC.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 2), LimoDancerA.Position.Y);
            LimoDancerD.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 3), LimoDancerA.Position.Y);


            LimoBG.Position = new Vector2(-100, LimoDancerA.Rectangle.Bottom - 30);

            LimoDriver.Position = new Vector2(-500, 320);

            LimoBG.SetZoom(0.8f);
            LimoDancerA.SetZoom(0.8f);
            LimoDancerB.SetZoom(0.8f);
            LimoDancerC.SetZoom(0.8f);
            LimoDancerD.SetZoom(0.8f);

            AddChild(SunBG);
            AddChild(LimoBG);

            AddChild(LimoDancerA);
            AddChild(LimoDancerB);
            AddChild(LimoDancerC);
            AddChild(LimoDancerD);

            AddChild(Car);

            Width = SunBG.Width;
            Height = SunBG.Height;

            Position = this.GetMiddle() + (new Vector2(1920, 1080) / 2);

            Car.Position = new Vector2(1920, 500);

            SFXA = SFXHelper.Default.GetSFX(SFXType.CarPass0);
            SFXB = SFXHelper.Default.GetSFX(SFXType.CarPass1);

            Loaded = true;
            OnProgressChanged?.Invoke(5);
        }

        public override void SetZoom(float Value = 1)
        {
            if (Disposed)
                return;
            
            SunBG.SetZoom(Value);
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            Speaker.Position += new Vector2(-50, 100);
            Player2.Position += new Vector2(200, -50);
            Player1.Position += new Vector2(300, -250);
            Speaker.AddChild(LimoDriver);
            LimoDriver.SetZoom(1.0f);
        }

        long LastCarPassTick;
        int CarPassDelayMS;
        const int CarPassDuration = 500;
        const int TotalDeltaCarX = 3200;
        const int CarPassSoundDelay = 1000;
        bool CarPassSoundStarted = false;

        Random Rand = new Random();

        public override void Draw(long Tick)
        {
            if (Disposed)
                return;
            
            DoDancerAnim(Tick);

            if (CarPassDelayMS == 0)
            {
                CarPassDelayMS = Rand.Next(5000, 15000) + CarPassSoundDelay;
                LastCarPassTick = Tick;
                CarPassSoundStarted = false;
            }

            int CarPassElapsedMS = (int)((Tick - LastCarPassTick) / Constants.ORBIS_MILISECOND);

            if (!CarPassSoundStarted && CarPassElapsedMS > CarPassDelayMS - CarPassSoundDelay)
            {
                CarPassSoundStarted = true;

                if (Game.MusicPlayer != null)
                {
                    var Sound = Rand.NextDouble() > 0.5f ? SFXA : SFXB;
                    Sound.Position = 0;

                    Game.MusicPlayer.PlayPassiveSFX(Sound);
                    Game.MusicPlayer.SetPassiveSFXVol(0.40f);
                }
            }

            if (CarPassElapsedMS > CarPassDelayMS)
            {
                CarPassElapsedMS -= CarPassDelayMS;

                float Progress = ((float)CarPassElapsedMS) / CarPassDuration;

                if (Progress >= 1f)
                {
                    Progress = 1;
                    CarPassDelayMS = 0;
                }

                Car.Position = new Vector2(-1280 + (TotalDeltaCarX * Progress), Car.Position.Y);
            }

            base.Draw(Tick);
        }

        long LastDancerUpdateTick = 0;
        private void DoDancerAnim(long Tick)
        {
            long ElapsedTick = Tick - LastDancerUpdateTick;
            if (ElapsedTick > Game.BPMTicks)
            {
                LastDancerUpdateTick = Tick;
                LimoDancerA?.NextFrame();
                LimoDancerB?.NextFrame();
                LimoDancerC?.NextFrame();
                LimoDancerD?.NextFrame();
            }
        }
    }
}

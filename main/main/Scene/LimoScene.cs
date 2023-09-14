using ICSharpCode.SharpZipLib.Core;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
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

        public LimoScene(SongPlayer Player)
        {
            Game = Player;
        }

        const int DancerDistance = 300;

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
                Car = new Texture2D();
                Car.Texture = new Texture(true);
                Car.Texture.SetDDS(Stream, true);
                Car.RefreshVertex();
            }

            OnProgressChanged?.Invoke(3);

            using (var Stream = Util.CopyFileToMemory("limoSunset.dds"))
            {
                SunBG = new Texture2D();
                SunBG.Texture = new Texture(true);
                SunBG.Texture.SetDDS(Stream, true);
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

            LimoDancerA.Position = new Vector2(300, 50);
            LimoDancerB.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 1), LimoDancerA.Position.Y);
            LimoDancerC.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 2), LimoDancerA.Position.Y);
            LimoDancerD.Position = new Vector2(LimoDancerA.Position.X + (DancerDistance * 3), LimoDancerA.Position.Y);


            LimoBG.Position = new Vector2(0, LimoDancerA.Rectangle.Bottom - 30);

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

            Loaded = true;
            OnProgressChanged?.Invoke(5);
        }

        public override void SetZoom(float Value = 1)
        {
            SunBG.SetZoom(Value);
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            Speaker.Position += new Vector2(-50, 100);
            Player2.Position += new Vector2(250, -50);
            Player1.Position += new Vector2(300, -250);
            Speaker.AddChild(LimoDriver);
            LimoDriver.SetZoom(1.0f);
        }

        long LastCarPassTick;
        int CarPassDelayMS;
        const int CarPassDuration = 600;
        const int TotalDeltaCarX = -3500;

        Random Rand = new Random();

        public override void Draw(long Tick)
        {
            DoDancerAnim(Tick);

            if (CarPassDelayMS == 0)
            {
                CarPassDelayMS = Rand.Next(5000, 15000);
                LastCarPassTick = Tick;
            }

            int CarPassElapsedMS = (int)((Tick - LastCarPassTick) / Constants.ORBIS_MILISECOND);

            if (CarPassElapsedMS > CarPassDelayMS)
            {
                CarPassElapsedMS -= CarPassDelayMS;

                float Progress = ((float)CarPassElapsedMS) / CarPassDuration;

                if (Progress >= 1f)
                {
                    Progress = 1;
                    CarPassDelayMS = 0;
                }

                Car.Position = new Vector2(1920 + (TotalDeltaCarX * Progress), Car.Position.Y);
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

        public void Dispose()
        {

        }
    }
}

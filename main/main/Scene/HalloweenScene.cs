using Orbis.Audio;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Orbis.Scene
{
    internal class HalloweenScene : GLObject2D, IScene
    {
        SongPlayer Game;

        MusicPlayer Player => Game.MusicPlayer;

        public HalloweenScene(SongPlayer Player)
        {
            Game = Player;
        }

        TiledSpriteAtlas2D BG;
        public bool Loaded { get; private set; }

        public int TotalProgress => 3;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;

        public void Load(Action<int> OnProgressChanged)
        {
            if (Loaded)
                return;

            var XML = Util.GetXML("halloween_bg.xml");
            OnProgressChanged?.Invoke(1);

            BG = new TiledSpriteAtlas2D(XML, Util.CopyFileToMemory, true);
            BG.SetActiveAnimation("halloweem bg");
            BG.RefreshVertex();
            OnProgressChanged?.Invoke(2);


            AddChild(new Rectangle2D(BG.Rectangle, true) { Color = RGBColor.Black });
            AddChild(BG);

            Width = Childs.Max(x => x.Width);
            Height = Childs.Max(y => y.Height);

            Position = this.GetMiddle() + (new Vector2(1920, 1080) / 2);

            BG.FrameDelay = 1000 / 60;
            BG.OnAnimationEnd += AnimEnd;

            Loaded = true;
            OnProgressChanged?.Invoke(3);
        }

        public override void SetZoom(float Factor)
        {
            if (Disposed)
                return;

            var ZoomLevel = Coordinates2D.ParseZoomFactor(Factor);
            var Plus5Percent = Coordinates2D.ParseZoomFactor(ZoomLevel + 5);

            base.SetZoom(Factor);

            BG.SetZoom(Plus5Percent);
        }

        private void DoThunder()
        {
            var SFX = SFXHelper.Default.GetSFX(Rnd.NextDouble() > 0.5f ? SFXType.ThunderA : SFXType.ThunderB);
            
            Player?.SetPassiveSFXVol(0.6f);
            Player?.PlayPassiveSFX(SFX);
            
            BG.SetActiveAnimation("halloweem bg lightning strike");
            
            OnMapStatusChanged?.Invoke(this, new NewStatusEvent()
            {
                Target = EventTarget.Speaker,
                NewAnimation = Game.SpeakerAnim.HIT
            });
        }

        private void AnimEnd(object sender, EventArgs e)
        {
            BG.SetActiveAnimation("halloweem bg");
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            var DeltaPos = new Vector2(20, 60);
            Player1.Position += DeltaPos;
            Player2.Position += DeltaPos + new Vector2(10, -20);
            Speaker.Position += DeltaPos;

            int[] AnimIndex = new int[] { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3 };
            Speaker.CreateAnimationByIndex(Game.SpeakerAnim.HIT, Game.SpeakerAnim.HIT, true, AnimIndex);
        }

        Random Rnd = new Random();

        int CurrentDelay = 0;
        long LastThunder = 0;

        public override void Draw(long Tick)
        {
            if (Disposed)
                return;

            if (Game.Started)
            {
                if (CurrentDelay == 0)
                    CurrentDelay = Rnd.Next(5000, 24000);

                if (LastThunder == 0)
                    LastThunder = Tick;

                long ThunderElapsedMs = (Tick - LastThunder) / Constants.ORBIS_MILISECOND;
                if (ThunderElapsedMs > CurrentDelay)
                {
                    CurrentDelay = 0;
                    LastThunder = Tick;
                    DoThunder();
                }
            }

            base.Draw(Tick);
        }
    }
}

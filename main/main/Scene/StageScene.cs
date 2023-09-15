using Orbis.Interfaces;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Linq;
using System.Numerics;

namespace Orbis.Scene
{
    internal class StageScene : GLObject2D, IScene
    {
        Texture2D StageBackTex;
        Texture2D StageFrontTex;
        Texture2D StageCurtainsTex;

        public bool Loaded { get; private set; }

        public int TotalProgress => 3;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;

        public void Load(Action<int> OnProgressChanged)
        {

            //1
            using (var ImgData = Util.CopyFileToMemory("assets/shared/images/stageback.dds"))
            {
                StageBackTex = new Texture2D();
                StageBackTex.Texture = new Texture(true);
                StageBackTex.Texture.SetDDS(ImgData, true);
            }

            OnProgressChanged?.Invoke(1);

            //2
            using (var ImgData = Util.CopyFileToMemory("assets/shared/images/stagefront.dds"))
            {
                StageFrontTex = new Texture2D();
                StageFrontTex.Texture = new Texture(true);
                StageFrontTex.Texture.SetDDS(ImgData, true);
            }

            OnProgressChanged?.Invoke(2);

            //3
            using (var ImgData = Util.CopyFileToMemory("assets/shared/images/stagecurtains.dds"))
            {
                StageCurtainsTex = new Texture2D();
                StageCurtainsTex.Texture = new Texture(true);
                StageCurtainsTex.Texture.SetDDS(ImgData, true);
            }


            StageFrontTex.Position = new Vector2(0, StageBackTex.Texture.Height - StageFrontTex.Texture.Height);

            AddChild(StageBackTex);
            AddChild(StageFrontTex);
            AddChild(StageCurtainsTex);

            Width = Childs.Max(x => x.Width);
            Height = Childs.Max(y => y.Height);

            Position = this.GetMiddle() + (new Vector2(1920, 1080)/2);

            Loaded = true;

            OnProgressChanged?.Invoke(3);
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            //default map, no changes
        }

        public override void Draw(long Tick)
        {
            if (Disposed)
                return;

            base.Draw(Tick);
        }

        public override void Dispose()
        {
            StageBackTex?.Dispose();
            StageFrontTex?.Dispose();
            StageCurtainsTex?.Dispose();
            base.Dispose();
        }
    }
}

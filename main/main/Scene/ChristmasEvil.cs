using Orbis.Interfaces;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Numerics;

namespace Orbis.Scene
{
    internal class ChristmasEvil : GLObject2D, IScene
    {
        Texture2D BG;
        Texture2D Snow;
        Texture2D Tree;
        public bool Loaded { get; private set; }

        public int TotalProgress => 3;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;


        public void Load(Action<int> OnProgressChanged)
        {
            using (var Stream = Util.CopyFileToMemory("evilBG.dds"))
                BG = new Texture2D(Stream, true);

            OnProgressChanged?.Invoke(1);

            using (var Stream = Util.CopyFileToMemory("evilSnow.dds"))
                Snow = new Texture2D(Stream, true);

            OnProgressChanged?.Invoke(2);

            using (var Stream = Util.CopyFileToMemory("evilTree.dds"))
                Tree = new Texture2D(Stream, true);

            BG.RefreshVertex();

            AddChild(BG);
            AddChild(Tree);
            AddChild(Snow);

            Width = BG.Width;
            Height = BG.Height;

            Snow.Position = new Vector2(0, 1150);

            Tree.Position = new Vector2(800, 200);

            Position = this.GetMiddle() + (new Vector2(1920, 1080) / 2);

            Loaded = true;

            OnProgressChanged?.Invoke(3);
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            Speaker.Position += new Vector2(50, 150);
            Player1.Position += new Vector2(200, 150);
            Player2.Position += new Vector2(100, 000);
        }
    }
}
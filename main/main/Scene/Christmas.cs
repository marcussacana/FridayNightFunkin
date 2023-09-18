using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Orbis.Scene
{
    internal class Christmas : GLObject2D, IScene
    {
        SongPlayer Game;

        SpriteAtlas2D BackMobs;
        SpriteAtlas2D MiddleMobs;
        SpriteAtlas2D Santa;

        Texture2D Background;
        Texture2D BackgroundEscalator;

        Texture2D Tree;

        Texture2D Snow;

        public bool Loaded { get; private set; }

        public int TotalProgress => 0;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;

        public Christmas(SongPlayer Player)
        {
            Game = Player;
        }

        public void Load(Action<int> OnProgressChanged)
        {
            var XML = Util.GetXML("bottomBop.xml");
            MiddleMobs = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(1);

            XML = Util.GetXML("santa.xml");
            Santa = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(2);

            XML = Util.GetXML("upperBop.xml");
            BackMobs = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);

            OnProgressChanged?.Invoke(3);

            using (var Stream = Util.CopyFileToMemory("bgWalls.dds"))
                Background = new Texture2D(Stream, true);

            OnProgressChanged?.Invoke(4);

            using (var Stream = Util.CopyFileToMemory("bgEscalator.dds"))
                BackgroundEscalator = new Texture2D(Stream, true);

            OnProgressChanged?.Invoke(5);

            using (var Stream = Util.CopyFileToMemory("christmasTree.dds"))
                Tree = new Texture2D(Stream, true);

            OnProgressChanged?.Invoke(6);

            using (var Stream = Util.CopyFileToMemory("fgSnow.dds"))
                Snow = new Texture2D(Stream, true);


            AddChild(Background);
            Background.AddChild(BackMobs);
            AddChild(BackgroundEscalator);

            AddChild(Tree);
            AddChild(MiddleMobs);

            AddChild(Snow);

            AddChild(Santa);

            BackMobs.Position = new Vector2(900, 550);
            MiddleMobs.Position = new Vector2(1200, 700);
            Snow.Position = new Vector2(0, 1200);

            Tree.Position = new Vector2(1650, 500);

            Santa.Position = new Vector2(650, 700);

            Santa.SetActiveAnimation("santa idle in fear");
            BackMobs.SetActiveAnimation("Upper Crowd Bob");
            MiddleMobs.SetActiveAnimation("Bottom Level Boppers");

            Santa.FrameDelay = 1000 / 60;
            BackMobs.FrameDelay = Santa.FrameDelay;
            MiddleMobs.FrameDelay = Santa.FrameDelay;

            Background.RefreshVertex();

            Width = Background.Width;
            Height = Background.Height;

            Position = this.GetMiddle() + (new Vector2(1920, 1080) / 2); 

            Loaded = true;

            OnProgressChanged?.Invoke(7);
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            Speaker.Position += new Vector2(400, 100);
            Player1.Position += new Vector2(400, 100);
            Player2.Position += new Vector2(800, 100);
        }
    }
}

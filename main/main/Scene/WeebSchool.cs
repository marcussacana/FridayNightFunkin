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
    internal class WeebSchool : GLObject2D, IScene
    {
        Texture2D Sky;
        Texture2D School;
        Texture2D Street;

        SpriteAtlas2D Freaks;

        Texture2D TreeBack;
        SpriteAtlas2D Tree;


        SongPlayer Game;

        public WeebSchool(SongPlayer Player)
        {
            Game = Player;
        }

        public bool Loaded { get; private set; }

        public int TotalProgress => 5;

        public event EventHandler<NewStatusEvent> OnMapStatusChanged;

        public void Load(Action<int> OnProgressChanged)
        {
            using (var Stream = Util.CopyFileToMemory("weebSky.dds"))
                Sky = new Texture2D(Stream, false);

            OnProgressChanged?.Invoke(1);

            using (var Stream = Util.CopyFileToMemory("weebSchool.dds"))
                School = new Texture2D(Stream, false);

            OnProgressChanged?.Invoke(2);

            using (var Stream = Util.CopyFileToMemory("weebTreesBack.dds"))
                TreeBack = new Texture2D(Stream, false);

            using (var TxtStream = Util.CopyFileToMemory("weebTrees.txt"))
            using (var Stream = Util.CopyFileToMemory("weebTrees.dds"))
            {
                var Lines = Encoding.UTF8.GetString(TxtStream.ToArray()).Replace("\r\n", "\n").Split('\n').Where(x => x.Contains("="));

                Tree = new SpriteAtlas2D();
                Tree.Texture = new Texture(true);
                Tree.Texture.SetDDS(Stream, false);

                var FrameRects = Lines.Select(x =>
                {
                    var Coord = x.Split('=').Last().Trim().Split(' ', '\t');
                    return new Rectangle(float.Parse(Coord[0]), float.Parse(Coord[1]), float.Parse(Coord[2]), float.Parse(Coord[3]));
                }).ToArray();

                Tree.CreateAnimation("Tree", FrameRects);
            }

            OnProgressChanged?.Invoke(3);

            using (var Stream = Util.CopyFileToMemory("weebStreet.dds"))
                Street = new Texture2D(Stream, false);

            OnProgressChanged?.Invoke(4);

            var XML = Util.GetXML("bgFreaks.xml");
            Freaks = new SpriteAtlas2D(XML, Util.CopyFileToMemory, false);

            Sky.RefreshVertex();
            Tree.RefreshVertex();
            TreeBack.RefreshVertex();
            School.RefreshVertex();
            Freaks.RefreshVertex();
            Street.RefreshVertex();

            AddChild(Sky);
            AddChild(School);
            AddChild(TreeBack);
            AddChild(Tree);
            AddChild(Street);
            AddChild(Freaks);

            Tree.SetActiveAnimation("Tree");
            Tree.FrameDelay = 30;

            SetZoom(1);

            Width = Sky.Width;
            Height = Sky.Height;

            ZoomPosition = ZoomPosition.GetMiddle(ZoomWidth, ZoomHeight) + (new Vector2(1920, 1080) / 2);

            Tree.ZoomPosition = new Vector2(-600, -1200);

            Freaks.SetActiveAnimation(Game.SongInfo.Name == "roses" ? "BG fangirls dissuaded" : "BG girls group");
            Freaks.FrameDelay = 16;
            Freaks.ZoomPosition = new Vector2(0, 300);

            Loaded = true;

            OnProgressChanged?.Invoke(5);
        }

        public override void SetZoom(float Value = 1)
        {
            var FriendlyZoom = Coordinates2D.ParseZoomFactor(Value);
            FriendlyZoom += 600;
            base.SetZoom(Coordinates2D.ParseZoomFactor(FriendlyZoom));
        }

        public void SetCharacterPosition(TiledSpriteAtlas2D Player1, TiledSpriteAtlas2D Player2, TiledSpriteAtlas2D Speaker)
        {
            var Zoom = Coordinates2D.ParseZoomFactor(700);
            Player1.SetZoom(Zoom);
            Player2.SetZoom(Zoom);
            Speaker.SetZoom(Zoom);

            foreach (var Texture in Player1.Textures)
            {
                Texture?.AntiAliasing(false);
            }

            foreach (var Texture in Player2.Textures)
            {
                Texture?.AntiAliasing(false);
            }

            foreach (var Texture in Speaker.Textures)
            {
                Texture?.AntiAliasing(false);
            }

            Player1.ZoomPosition = new Vector2(1000, 550);
            Speaker.ZoomPosition = new Vector2(500, 200);
            Player2.ZoomPosition = new Vector2(40, 100);
        }

        public void Draw(long Tick)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }
    }
}

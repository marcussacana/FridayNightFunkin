using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public int TotalProgress => 6;

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

            Game.NoteSprite = CreatePixelNote();
            Game.NoteSprite.SetZoom(1);

            OnProgressChanged?.Invoke(5);

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

            Game.PopupHelper.Sufix = "-pixel";
            Game.PopupHelper.ZoomFactor -= 1f;
            Game.PopupOffset -= new Vector2(90, 60);

            Loaded = true;

            OnProgressChanged?.Invoke(6);
        }

        public class ZoomNote : SpriteAtlas2D
        {
            public override void SetZoom(float Value = 1)
            {
                var FriendlyZoom = Coordinates2D.ParseZoomFactor(Value);
                FriendlyZoom += 600;
                base.SetZoom(Coordinates2D.ParseZoomFactor(FriendlyZoom));
            }

            public override GLObject2D Clone(bool AllowDisposal)
            {
                var Note = new ZoomNote()
                {
                    FrameOffsets = FrameOffsets,
                    Sprites = Sprites,
                    AllowTexDisposal = AllowDisposal,
                    Width = Width,
                    Height = Height,
                    Texture = Texture
                };
                
                if (!AllowDisposal)
                    ((Texture2D)Note.SpriteView.Target).SharedTexture = true;
                
                Note.SetZoom(1);
                return Note;
            }
        }

        private SpriteAtlas2D CreatePixelNote()
        {
            using (var Stream = Util.CopyFileToMemory("arrows-pixels-mod.dds"))
            {
                var PixelNotes = new ZoomNote();
                PixelNotes.Texture = new Texture(true);
                PixelNotes.Texture.SetDDS(Stream, false);

                PixelNotes.CreateAnimation(NotesNames.STATIC_ARROW, new Rectangle[] {
                    new Rectangle(0, 0, 17, 16),
                    new Rectangle(17, 0, 17, 16),
                    new Rectangle(52, 0, 17, 16),
                    new Rectangle(34, 0, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.LEFT_NOTE, new Rectangle[] {
                    new Rectangle(0, 16, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.DOWN_NOTE, new Rectangle[] {
                    new Rectangle(17, 16, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.UP_NOTE, new Rectangle[] {
                    new Rectangle(34, 16, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.RIGHT_NOTE, new Rectangle[] {
                    new Rectangle(52, 16, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.LEFT_PRESS, new Rectangle[] {
                    new Rectangle(0, 34, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.DOWN_PRESS, new Rectangle[] {
                    new Rectangle(17, 34, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.UP_PRESS, new Rectangle[] {
                    new Rectangle(34, 34, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.RIGHT_PRESS, new Rectangle[] {
                    new Rectangle(52, 34, 17, 16)
                });

                PixelNotes.CreateAnimation(NotesNames.LEFT_HIT, new Rectangle[] {
                    new Rectangle(0, 16, 17, 16),
                    new Rectangle(0, 68, 17, 16),
                    new Rectangle(0, 51, 17, 16),
                    new Rectangle(0, 68, 17, 16),
                }, new Vector2[] {
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f)
                });

                PixelNotes.CreateAnimation(NotesNames.DOWN_HIT, new Rectangle[] {
                    new Rectangle(17, 16, 17, 16),
                    new Rectangle(17, 68, 17, 16),
                    new Rectangle(17, 51, 17, 16),
                    new Rectangle(17, 68, 17, 16),
                }, new Vector2[] {
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f)
                });

                PixelNotes.CreateAnimation(NotesNames.UP_HIT, new Rectangle[] {
                    new Rectangle(34, 16, 17, 16),
                    new Rectangle(34, 68, 17, 16),
                    new Rectangle(34, 51, 17, 16),
                    new Rectangle(34, 68, 17, 16),
                }, new Vector2[] {
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f)
                });

                PixelNotes.CreateAnimation(NotesNames.RIGHT_HIT, new Rectangle[] {
                    new Rectangle(52, 16, 17, 16),
                    new Rectangle(52, 68, 17, 16),
                    new Rectangle(52, 51, 17, 16),
                    new Rectangle(52, 68, 17, 16),
                }, new Vector2[] {
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f),
                    new Vector2(-6.5f,  -6.5f)
                });

                PixelNotes.CreateAnimation(NotesNames.LEFT_BAR, new Rectangle[]
                {
                    new Rectangle(0, 92, 7, 8)
                });
                PixelNotes.CreateAnimation(NotesNames.DOWN_BAR, new Rectangle[]
                {
                    new Rectangle(7, 92, 7, 8)
                });
                PixelNotes.CreateAnimation(NotesNames.UP_BAR, new Rectangle[]
                {
                    new Rectangle(14, 92, 7, 8)
                });
                PixelNotes.CreateAnimation(NotesNames.RIGHT_BAR, new Rectangle[]
                {
                    new Rectangle(21, 92, 7, 8)
                });

                PixelNotes.CreateAnimation(NotesNames.LEFT_BAR_END, new Rectangle[]
                {
                    new Rectangle(0, 100, 7, 4)
                });
                PixelNotes.CreateAnimation(NotesNames.DOWN_BAR_END, new Rectangle[]
                {
                    new Rectangle(7, 100, 7, 4)
                });
                PixelNotes.CreateAnimation(NotesNames.UP_BAR_END, new Rectangle[]
                {
                    new Rectangle(14, 100, 7, 4)
                });

                PixelNotes.CreateAnimation(NotesNames.RIGHT_BAR_END, new Rectangle[]
                {
                    new Rectangle(21, 100, 7, 4)
                });

                return PixelNotes;
            }
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

            Game.Boyfriend.SetZoom(Zoom);
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

            Game.IsPixel = true;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}

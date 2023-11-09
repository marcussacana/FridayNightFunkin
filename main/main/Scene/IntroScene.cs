using Orbis.Audio;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.Audio;
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
    public class IntroScene : GLObject2D, ILoadable
    {
        Rectangle2D BG;

        StartMenuScene StartMenu;
        Random Rand = new Random();

        string[] IntroText = null;

        private Texture2D NewGroundsLogo;
        private AtlasText2D Text;
        private AtlasText2D TextLineTwo;
        private SpriteAtlas2D Atlas;
        private WavePlayer Theme;
        private OrbisAudioOut ThemeDriver;

        private SFXHelper SFX => SFXHelper.Default;
        public bool Loaded { get; set; }

        public int TotalProgress => 3 + SFX.TotalProgress + StartMenu.TotalProgress;

        public void Load(Action<int> OnProgressChanged)
        {
#if ORBIS
            SFX.Load((i) => { OnProgressChanged(i); });

            Theme = new WavePlayer();
            Theme.SetAudioDriver(ThemeDriver = new OrbisAudioOut());
            Theme.Loop = true;
            Theme.Open(Util.CopyFileToMemory("freakyMenu_48khz.wav"));
#endif

            OnProgressChanged?.Invoke(SFX.TotalProgress + 1);

            StartMenu = new StartMenuScene(Theme, ThemeDriver);
            StartMenu.Load((x) => OnProgressChanged?.Invoke(x + SFX.TotalProgress + 1));

            Atlas = new SpriteAtlas2D(Util.GetXML("alphabet.xml"), Util.CopyFileToMemory, true);

            Text = new AtlasText2D(Atlas, Util.FontBoldMap);
            TextLineTwo = new AtlasText2D(Atlas.Clone(false) as SpriteAtlas2D, Util.FontBoldMap);

            Text.SetText(" NINJAMUFFIN\nPHANTOMARCADE\n KAWAISPRITE\n   EVILSKER\nMARCUSSACANA");

            OnProgressChanged?.Invoke(SFX.TotalProgress + StartMenu.TotalProgress + 2);

            using (var Stream = Util.CopyFileToMemory("newgrounds_logo.dds"))
                NewGroundsLogo = new Texture2D(Stream, true);

            using (var Stream = Util.CopyFileToMemory("introText.txt"))
            {
                var Lines = Encoding.UTF8.GetString(Stream.ToArray()).Replace("\r\n", "\n").Split('\n');
                IntroText = Lines[Rand.Next(0, Lines.Length)].ToUpper().Replace("--", "\n").Split('\n');
            }

            Width = 1920;
            Height = 1080;

            BG = new Rectangle2D(1920, 1080, true);
            BG.Color = RGBColor.Black;

            AddChild(BG);
            AddChild(Text);
            UpdateCenter();

            Loaded = true;
            OnProgressChanged?.Invoke(SFX.TotalProgress + StartMenu.TotalProgress + 3);
        }

        private void UpdateCenter(Vector2? Size = null)
        {
            Text.Position = (this.Size / 2) - ((Size ?? Text.Size) / 2);
        }

        int IntroIndex = 0;
        int TextSeconds = 0;
        private void StepText()
        {
            switch (TextSeconds++)
            {
                case 0:
                case 1:
                    break;
                case 2:
                    Text.SetText("NOT IN ASSOCIATION\n      WITH");
                    Text.Position = new Vector2(550, 300);
                    break;
                case 3:
                    AddChild(NewGroundsLogo);
                    NewGroundsLogo.Position = new Vector2(800, 500);
                    break;
                case 4:
                    Text.SetText(IntroText.Last());
                    var SizeA = Text.Size;
                    Text.SetText(IntroText.First());
                    var SizeB = Text.Size;

                    var MaxSize = new Vector2(Math.Max(SizeA.X, SizeB.X), Math.Max(SizeA.Y, SizeB.Y));

                    UpdateCenter(MaxSize);

                    Text.SetText(IntroText.First());
                    NewGroundsLogo.Visible = false;
                    break;
                case 5:
                    TextLineTwo.SetText(IntroText.Last());
                    TextLineTwo.Position = Text.Position + Text.GetMiddle(TextLineTwo) + new Vector2(0, Text.Height);
                    AddChild(TextLineTwo);
                    break;
                case 6:
                    Text.SetText("FRIDAY\n\nFUNKING");
                    UpdateCenter();
                    Text.SetText("FRIDAY");
                    RemoveChild(TextLineTwo);
                    break;
                case 7:
                    Text.SetText("FRIDAY\n\n");
                    TextLineTwo.SetText("NIGHT");
                    TextLineTwo.Position = Text.Position + Text.GetMiddle(TextLineTwo) + new Vector2(0, 50);
                    AddChild(TextLineTwo);
                    break;
                case 8:
                    Text.SetText("FRIDAY\n\nFUNKING");
                    break;
                default:
                    EnterMainMenu();
                    break;
            }
        }

        private void EnterMainMenu()
        {
            Dispose();

            Application.Default.RemoveObjects(true);
            Application.Default.AddObject(StartMenu);
        }

        const int TextStepTick = Constants.ORBIS_MILISECOND * 1000;
        long CurrentStepTick;
        long FirstTick = -1;
        public override void Draw(long Tick)
        {
            if (FirstTick == -1)
            {
                Theme?.Resume();
                FirstTick = 0;
                base.Draw(Tick);
                return;
            }

            if (FirstTick == 0)
                FirstTick = Tick;

            if (CurrentStepTick == 0)
                CurrentStepTick = Tick;

            if (Tick - CurrentStepTick > TextStepTick)
            {
                CurrentStepTick = 0;
                StepText();
            }

            base.Draw(Tick);
        }
    }
}

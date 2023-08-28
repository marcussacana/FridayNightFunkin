using OrbisGL.GL;
using Orbis.Game;
using OrbisGL.Controls;
using OrbisGL.GL2D;
#if ORBIS
using System.IO;
using System.Reflection.Emit;
using Orbis.Internals;
using OrbisGL;
using OrbisGL.Audio;

namespace Orbis
{
    internal class Entrypoint : Application
    {
        public Entrypoint() : base(1920, 1080, 60, GPUMemoryConfig.HighMemory)
        {
            EnableKeyboard();

            EnableDualshock(new DualshockSettings());
            
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            var BG = new Rectangle2D(1920, 1080, true);
            BG.Color = RGBColor.White;

            var Lbl = new Text2D(32, null);
            
            Lbl.Color = RGBColor.Black;
            Lbl.Position = BG.GetMiddle(Lbl);
            Lbl.SetText("Initializing...");
            
            BG.AddChild(Lbl);
            
            AddObject(BG);

          /* IAudioPlayer player = new WavePlayer();
            IAudioOut driver = new OrbisAudioOut();
            player.SetAudioDriver(driver);
            player.Open(File.Open(Path.Combine(IO.GetAppBaseDirectory(), "assets/audio/Test.wav"), FileMode.Open));
            player.Resume();*/

            var SP = new SongPlayer(Util.GetSongByName("Bopeebo"));

            SP.Load((i) =>
            {
                var Progress = (int)(((double)i / SP.TotalProgress) * 100);
                Lbl.SetText($"Loading... {Progress}%");
                Lbl.Position = BG.GetMiddle(Lbl);
                DrawOnce();
                
                if (SP.Loaded)
                    SP.Begin();
            });
            
            AddObject(SP); 
        }
    }
}
#endif
using OrbisGL.GL;
using Orbis.Game;
using OrbisGL.Controls;
using OrbisGL.GL2D;
#if ORBIS
using System.IO;
using System.Reflection.Emit;
using Orbis.Scene;
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
            StartMenu Menu = new StartMenu();
            Menu.Load((i) =>
            {
                if (Menu.Loaded)
                {
                    AddObject(Menu);
                }
            });
        }
    }
}
#endif
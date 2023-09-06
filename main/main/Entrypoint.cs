#if ORBIS
using Orbis.Scene;
using OrbisGL;
using OrbisGL.GL;

namespace Orbis
{
    internal class Entrypoint : Application
    {
        public Entrypoint() : base(1920, 1080, 60, GPUMemoryConfig.Default)
        {
            EnableKeyboard();

            EnableDualshock(new DualshockSettings());
            
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            StartMenuScene Menu = new StartMenuScene();
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
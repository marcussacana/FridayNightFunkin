#if ORBIS
using Orbis.Interfaces;
using Orbis.Internals;
using Orbis.Scene;
using OrbisGL;
using OrbisGL.GL;

namespace Orbis
{
    internal class Entrypoint : Application
    {
        public Entrypoint() : base(1920, 1080, 60, GPUMemoryConfig.HighFlexible)
        {
            EnableKeyboard();

            EnableDualshock(new DualshockSettings());

            Kernel.Log("Initializing StartMenu");

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            ILoadable main = new IntroScene();
            main.Load((i) =>
            {
                if (main.Loaded)
                {
                    AddObject(main);
                }
            });
        }
    }
}
#endif
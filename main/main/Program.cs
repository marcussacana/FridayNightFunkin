using OrbisGL;
using OrbisGL.GL;

namespace Orbis
{

    public static class Program
    {
        public static void Main(string[] args)
        {
#if ORBIS
            var Display = new Entrypoint();
            Display.ClearColor = RGBColor.Black;
            Display.Run();
#endif
        }
    }

}
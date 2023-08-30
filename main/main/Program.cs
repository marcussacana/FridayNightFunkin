using OrbisGL;

namespace Orbis
{

    public static class Program
    {
        public static void Main(string[] args)
        {
#if ORBIS
            var Display = new Entrypoint();
            Shader.PrecompileShaders();
            Display.Run();
#endif
        }
    }

}
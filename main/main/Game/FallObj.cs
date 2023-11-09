using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Numerics;

namespace Orbis.Game
{
    internal class FallObj : GLObject2D
    {
        bool TextureMode = false;
        int Distance;
        int TimeMS;
        GLObject2D Obj;
        public FallObj(Texture Texture, int Distance, int TimeMS)
        {
            TextureMode = true;
            this.Distance = Distance;
            this.TimeMS = TimeMS;

            Obj = new Texture2D
            {
                Texture = Texture
            };

            Obj.RefreshVertex();
            AddChild(Obj);

            Width = Obj.Width;
            Height = Obj.Height;
        }

        public FallObj(GLObject2D Obj, int Distance, int TimeMS)
        {
            this.Distance = Distance;
            this.TimeMS = TimeMS;

            this.Obj = Obj;
            AddChild(Obj);

            Width = Obj.Width;
            Height = Obj.Height;
        }

        long BeginTick;

        public override void Draw(long Tick)
        {
            if (BeginTick == 0)
                BeginTick = Tick;

            var ElapsedMS = (Tick - BeginTick) / Constants.ORBIS_MILISECOND;

            ElapsedMS = Math.Min(ElapsedMS, TimeMS);

            float LinearProgress = ((float)ElapsedMS) / TimeMS;

            float CubicProgress = Geometry.CubicBezier(new Vector2(0.2f, -0.6f), new Vector2(0.6f, 1), LinearProgress);

            float CurrentY = Distance * CubicProgress;

            Obj.Position = new Vector2(0, CurrentY);
            Obj.Opacity = (byte)(255 * (1 - LinearProgress));

            if (Obj.Opacity == 0)
            {
                Dispose();
                return;
            }

            base.Draw(Tick);
        }
        public override void Dispose()
        {
            if (TextureMode)
                ((Texture2D)Obj).Texture = null;

            base.Dispose();
        }
    }
}

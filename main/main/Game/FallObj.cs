using OrbisGL;
using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Numerics;

namespace Orbis.Game
{
    internal class FallObj : GLObject2D
    {
        int Distance;
        int TimeMS;
        Texture2D Obj;
        public FallObj(Texture Texture, int Distance, int TimeMS)
        {
            this.Distance = Distance;
            this.TimeMS = TimeMS;

            Obj = new Texture2D();
            Obj.Texture = Texture;
            Obj.RefreshVertex();

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
            Obj.Texture = null;
            base.Dispose();
        }
    }
}

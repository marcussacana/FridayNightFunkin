using OrbisGL.GL;
using OrbisGL.GL2D;
using System;
using System.Numerics;

namespace Orbis.Game
{
    public class HealthBar : GLObject2D
    {
        NoteMenu Player1, Player2;

        float Health = 0;

        Texture2D P2Bar;
        Texture2D P1Bar;

        Sprite2D P1Icon;
        Sprite2D P2Icon;

        public event EventHandler<NoteMenu> OnPlayerDies;
        
        public HealthBar(NoteMenu Player1, NoteMenu Player2, Texture BarTexture, Texture Player1Icon, Texture Player2Icon)
        {
            this.Player1 = Player1;
            this.Player2 = Player2;

            Player1.OnNoteHit += NoteHit;
            Player2.OnNoteHit += NoteHit;
            Player1.OnNoteMissed += NoteMiss;
            Player2.OnNoteMissed += NoteMiss;

            P1Bar = new Texture2D();
            P1Bar.Texture = BarTexture;

            P2Bar = new Texture2D();
            P2Bar.Texture = BarTexture;

            P1Bar.Color = new RGBColor("#66FF33");
            P2Bar.Color = new RGBColor("#FF0000");

            P1Bar.RefreshVertex();
            P2Bar.RefreshVertex();

            var P1Tex = new Texture2D() { Texture = Player1Icon };
            var P2Tex = new Texture2D() { Texture = Player2Icon };

            P1Tex.Mirror = true;
            P2Tex.Mirror = false;

            P1Tex.RefreshVertex();
            P2Tex.RefreshVertex();

            P1Icon = new Sprite2D(P1Tex);
            P2Icon = new Sprite2D(P2Tex);

            P1Icon.Width = P1Tex.Width / 2;
            P2Icon.Width = P2Tex.Width / 2;
            P1Icon.Height = P1Tex.Height;
            P2Icon.Height = P2Tex.Height;

            P1Icon.ComputeAllFrames(2);
            P2Icon.ComputeAllFrames(2);

            AddChild(P1Bar);
            AddChild(P2Bar);

            AddChild(P1Icon);
            AddChild(P2Icon);

            P1Bar.Position = new Vector2((Application.Default.Width / 2) - P1Bar.Width / 2, (Application.Default.Height - 50) - P1Bar.Height / 2);
            P2Bar.Position = P1Bar.Position;

            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            Health = Math.Min(Health, 1);
            Health = Math.Max(Health, -1);

            if (Health == 1)
                OnPlayerDies?.Invoke(this, Player1);

            if (Health == -1 && !Player2.CPU)
                OnPlayerDies?.Invoke(this, Player2);


            float SplitPos = ((Health + 1) / 2) * P1Bar.Width;

            bool P1LowHealth = Health > 0.7;
            bool P2LowHealth = Health < -0.7;

            P1Icon.SetCurrentFrame(P1LowHealth ? 0 : 1);
            P2Icon.SetCurrentFrame(P2LowHealth ? 1 : 0);

            P2Bar.SetVisibleRectangle(new Rectangle(0, 0, SplitPos, P2Bar.Height));

            P1Icon.Position = P1Bar.Position + new Vector2(SplitPos - (P1Icon.Width*0.2f), -P1Icon.Height / 2);
            P2Icon.Position = P2Bar.Position + new Vector2(SplitPos - (P2Icon.Width*0.8f), -P2Icon.Height / 2);
        }

        private void NoteMiss(object sender, EventArgs e)
        {
            bool IsPlayer1 = sender == Player1;

            if (IsPlayer1)
            {
                Health += 0.0475f;
            }
            else if (!Player2.CPU)
            {
                Health -= 0.0475f;
            }

            UpdateHealthBar();
        }
        private void NoteHit(object sender, EventArgs e)
        {
            bool IsPlayer1 = sender == Player1;

            if (IsPlayer1)
            {
                Health -= 0.023f;
            }
            else if (!Player2.CPU)
            {
                Health += 0.023f;
            }

            UpdateHealthBar();
        }

        public override void Dispose()
        {
            P1Bar?.Dispose();
            P2Bar?.Dispose();
            P1Icon?.Dispose();
            P2Icon?.Dispose();

            base.Dispose();
        }
    }
}

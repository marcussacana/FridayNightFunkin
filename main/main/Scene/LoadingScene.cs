﻿using System;
using System.Numerics;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL.GL;
using OrbisGL.GL2D;

namespace Orbis.Scene;

public class LoadingScene :  GLObject2D, ILoadable
{
    public static Texture2D BG;
    public static Texture2D Bar;
    public static int BarWidth;
    public bool Loaded { get; private set; }
    public int TotalProgress { get; } = 2;
    public void Load(Action<int> OnProgressChanged)
    {
        Width = Application.Default.Width;
        Height = Application.Default.Height;
        
        if (BG == null)
        {
            BG = new Texture2D();
            BG.Texture = new Texture(true);
            
            using (var Data = Util.CopyFileToMemory("funkay.dds"))
                BG.Texture.SetDDS(Data, false);

            BG.AutoSize = false;
            BG.Width = Width;
            BG.Height = Height;
            BG.RefreshVertex();
        }
        
        OnProgressChanged?.Invoke(1);
        
        if (Bar == null)
        {
            Bar = new Texture2D();
            Bar.Texture = new Texture(true);
            Bar.Color = RGBColor.Blue;
            
            using (var Data = Util.CopyFileToMemory("healthBar.dds"))
                Bar.Texture.SetDDS(Data, false);
            
            Bar.RefreshVertex();

            BarWidth = Bar.Width;
            
            Bar.Position = new Vector2((Width - Bar.Width) - 200, (Height - 80) - Bar.Height / 2);

            Bar.AutoSize = false;
        }
        
        Bar.Opacity = 0;
        
        AddChild(BG);
        AddChild(Bar);

        Loaded = true;
        
        OnProgressChanged?.Invoke(2);
    }
    
    public void Load(ILoadable Target, Action OnLoaded)
    {
        if (!Target.Loaded)
        {
            Target.Load(i =>
            {
                double Progress = Math.Min((double)i / Target.TotalProgress, 1d);
                
                Bar.Opacity = 255;
                Bar.SetVisibleRectangle(0, 0, (int)(BarWidth * Progress), Bar.Height);

                if (i + 1 == Target.TotalProgress)
                    Bar.ClearVisibleRectangle();

                Application.Default.DrawOnce();

                if (Target.Loaded)
                {
                    OnLoaded?.Invoke();
                }
            });
        }
    }

    public override void Dispose()
    {
        RemoveChildren(false);
        base.Dispose();
    }
}
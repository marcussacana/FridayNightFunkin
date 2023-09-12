using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using Orbis.Interfaces;

namespace Orbis.Audio;

public class SFXHelper : ILoadable
{
    private SFXHelper() { }


    public static SFXHelper Default = new SFXHelper();

    Dictionary<SFXType, MemoryStream> SFXEntries = new Dictionary<SFXType, MemoryStream>();

    public void Dispose()
    {
        foreach (var Sound in SFXEntries.Values)
        {
            Sound.Dispose();
        }
        
        SFXEntries.Clear();
    }

    public void Draw(long Tick)
    {
        
    }

    public MemoryStream GetSFX(SFXType Type)
    {
        if (!SFXEntries.ContainsKey(Type))
            return null;

        var Entry= SFXEntries[Type];
        Entry.Position = 0;
        return Entry;
    }

    public bool Loaded { get; private set; }
    public int TotalProgress { get; private set; } = 8;
    public void Load(Action<int> OnProgressChanged)
    {
        //1
        SFXEntries[SFXType.Dies] = Util.CopyFileToMemory("fnf_loss_sfx_48khz.wav");
        SFXEntries[SFXType.DiesPixel] = Util.CopyFileToMemory("fnf_loss_sfx-pixel_48khz.wav");
        SFXEntries[SFXType.DeadLoop] = Util.CopyFileToMemory("gameOver_48khz.wav");
        SFXEntries[SFXType.DeadRetry] = Util.CopyFileToMemory("gameOverEnd_48khz.wav");
        
        OnProgressChanged?.Invoke(1);
        
        SFXEntries[SFXType.Countdown1] = Util.CopyFileToMemory("intro1_48khz.wav");
        SFXEntries[SFXType.Countdown2] = Util.CopyFileToMemory("intro2_48khz.wav");
        SFXEntries[SFXType.Countdown3] = Util.CopyFileToMemory("intro3_48khz.wav");
        SFXEntries[SFXType.CountdownGo] = Util.CopyFileToMemory("introGo_48khz.wav");
        OnProgressChanged?.Invoke(2);
        
        SFXEntries[SFXType.Countdown1Pixel] = Util.CopyFileToMemory("intro1-pixel_48khz.wav");
        SFXEntries[SFXType.Countdown2Pixel] = Util.CopyFileToMemory("intro2-pixel_48khz.wav");
        SFXEntries[SFXType.Countdown3Pixel] = Util.CopyFileToMemory("intro3-pixel_48khz.wav");
        SFXEntries[SFXType.CountdownGoPixel] = Util.CopyFileToMemory("introGo-pixel_48khz.wav");
        OnProgressChanged?.Invoke(3);

        SFXEntries[SFXType.Girlfriend1] = Util.CopyFileToMemory("GF_1_48khz.wav");
        SFXEntries[SFXType.Girlfriend2] = Util.CopyFileToMemory("GF_2_48khz.wav");
        SFXEntries[SFXType.Girlfriend3] = Util.CopyFileToMemory("GF_3_48khz.wav");
        SFXEntries[SFXType.Girlfriend4] = Util.CopyFileToMemory("GF_4_48khz.wav");
        OnProgressChanged?.Invoke(4);

        SFXEntries[SFXType.CarPass0] = Util.CopyFileToMemory("carPass0_48khz.wav");
        SFXEntries[SFXType.CarPass1] = Util.CopyFileToMemory("carPass1_48khz.wav");
        OnProgressChanged?.Invoke(5);

        SFXEntries[SFXType.NoteMiss1] = Util.CopyFileToMemory("missnote1_48khz.wav");
        SFXEntries[SFXType.NoteMiss2] = Util.CopyFileToMemory("missnote2_48khz.wav");
        SFXEntries[SFXType.NoteMiss3] = Util.CopyFileToMemory("missnote3_48khz.wav");
        OnProgressChanged?.Invoke(6);

        SFXEntries[SFXType.MenuChoice] = Util.CopyFileToMemory("scrollMenu_48khz.wav");
        SFXEntries[SFXType.MenuConfirm] = Util.CopyFileToMemory("confirmMenu_48khz.wav");
        SFXEntries[SFXType.MenuBack] = Util.CopyFileToMemory("cancelMenu_48khz.wav");
        OnProgressChanged?.Invoke(7);

        SFXEntries[SFXType.ThunderA] = Util.CopyFileToMemory("thunder_1_48khz.wav");
        SFXEntries[SFXType.ThunderB] = Util.CopyFileToMemory("thunder_2_48khz.wav");
        SFXEntries[SFXType.Train] = Util.CopyFileToMemory("train_passes_48khz.wav");
        Loaded = true;
        OnProgressChanged?.Invoke(5);
    }
}
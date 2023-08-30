using System;
using System.Net.Mail;
using System.Net.Mime;
using System.Numerics;
using System.Xml;
using Orbis.Game;
using Orbis.Interfaces;
using OrbisGL;
using OrbisGL.Audio;
using OrbisGL.Controls.Events;
using OrbisGL.GL;
using OrbisGL.GL2D;

namespace Orbis.BG;

public class StartMenu : GLObject2D, ILoadable
{
    
    private WavePlayer Theme;
    private SpriteAtlas2D Logo;
    private SpriteAtlas2D Girlfriend;
    
    public bool Loaded { get; private set; }
    public int TotalProgress { get; } = 3;
    public void Load(Action<int> OnProgressChanged)
    {
        var XML = Util.GetXML("logoBumpin.xml");
        Logo = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);
        Logo.SetActiveAnimation("logo bumpin");
        Logo.Position = new Vector2(50, 50);
        
        OnProgressChanged?.Invoke(1);
        
        XML = Util.GetXML("gfDanceTitle.xml");
        Girlfriend = new SpriteAtlas2D(XML, Util.CopyFileToMemory, true);
        Girlfriend.SetActiveAnimation("gfDance");
        Girlfriend.Position = new Vector2(1920, 1080) - new Vector2(Girlfriend.Width, Girlfriend.Height) - new Vector2(50, 50);
        
        OnProgressChanged?.Invoke(2);

        Theme = new WavePlayer();
        Theme.SetAudioDriver(new OrbisAudioOut());
        Theme.Open(Util.CopyFileToMemory("freakyMenu_48khz.wav"));

        Rectangle2D Rectangle = new Rectangle2D(1920, 1080, true);
        Rectangle.Color = RGBColor.Black;

        Rectangle.RefreshVertex();

        AddChild(Rectangle);
        AddChild(Logo);
        AddChild(Girlfriend);

        Application.Default.Gamepad.OnButtonUp += GamepadOnOnButtonUp;
        
        Loaded = true;
        
        OnProgressChanged?.Invoke(3);
    }

    private bool Started = false;

    private void GamepadOnOnButtonUp(object sender, ButtonEventArgs args)
    {
        if (Started)
            return;
        
        if (args.Button.HasFlag(OrbisPadButton.Options))
        {
            Started = true;
            LoadingBG BG = new LoadingBG();
            BG.Load(i =>
            {
                if (!BG.Loaded)
                    return;
                
                Application.Default.RemoveObjects();
                Application.Default.AddObject(BG);
                Dispose();
                
                var Song = new SongPlayer(Util.GetSongByName("Bopeebo"));
                BG.Load(Song, () =>
                {
                    Application.Default.RemoveObjects();
                    BG.Dispose();
                    
                    
                    Song.Begin();
                    Application.Default.AddObject(Song);
                });
            });
        }
    }
    private long LastFrameTick = 0;
    private long FrameTick = Constants.ORBIS_MILISECOND * 20;
    public void Draw(long Tick)
    {
        if (LastFrameTick == 0)
            Theme.Resume();
        
        long ElapsedTick = Tick - LastFrameTick;
        if (ElapsedTick > FrameTick)
        {
            LastFrameTick = Tick;
            Logo?.NextFrame();
            Girlfriend?.NextFrame();
        }
        
        base.Draw(Tick);
    }
    public override void Dispose()
    {
        Logo?.Dispose();
        Girlfriend?.Dispose();
        Theme.Dispose();
        base.Dispose();
    }
}
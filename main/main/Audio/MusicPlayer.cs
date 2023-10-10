using OrbisGL.Audio;
using System;
using System.IO;

namespace Orbis.Audio
{
    internal class MusicPlayer : IDisposable
    {
        bool Ogg = false;
        
        IAudioPlayer Other;
        IAudioPlayer Instrumental;
        IAudioPlayer Voices;
        IAudioPlayer SFXA;
        IAudioPlayer SFXB;

        IAudioOut InstrumentalDriver;
        IAudioOut OtherDriver;
        IAudioOut VoiceDriver;
        IAudioOut SFXADriver;
        IAudioOut SFXBDriver;

        public MusicPlayer(MemoryStream Instrumental, MemoryStream Voices, EventHandler OnMusicEnd, bool Ogg) {
            InstrumentalDriver = new OrbisAudioOut();
            OtherDriver = new OrbisAudioOut();
            VoiceDriver = new OrbisAudioOut();
            SFXADriver = new OrbisAudioOut();
            SFXBDriver = new OrbisAudioOut();

            this.Ogg = Ogg;

            this.Instrumental = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.Other = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.Voices = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.SFXA = Ogg ? new VorbisPlayer() : new WavePlayer(); 
            this.SFXB = Ogg ? new VorbisPlayer() : new WavePlayer(); 

            this.Instrumental.Open(Instrumental);
            this.Instrumental.SetAudioDriver(InstrumentalDriver);
            
            this.Voices.Open(Voices);
            this.Voices.SetAudioDriver(VoiceDriver);

            SFXA.SetAudioDriver(SFXADriver);
            SFXB.SetAudioDriver(SFXBDriver);
            
            Other.SetAudioDriver(OtherDriver);

            this.Instrumental.OnTrackEnd += OnMusicEnd;
            
            InstrumentalDriver.Resume();
            VoiceDriver.Resume();
            SFXADriver.Resume();
            SFXBDriver.Resume();
        }
        
        public void Resume()
        {
            Voices.Resume();
            Instrumental.Resume();
            
            SFXA.Resume();
            SFXB.Resume();
            
            InstrumentalDriver.Resume();
            VoiceDriver.Resume();
            SFXADriver.Resume();
            SFXADriver.Resume();
        }

        public void CloseSFX()
        {
            SFXA.Dispose();
            SFXB.Dispose();
            SFXADriver.Dispose();
            SFXBDriver.Dispose();

            SFXA = Ogg ? new VorbisPlayer() : new WavePlayer();
            SFXB = Ogg ? new VorbisPlayer() : new WavePlayer();

            SFXADriver = new OrbisAudioOut();
            SFXBDriver = new OrbisAudioOut();
            
            SFXA.SetAudioDriver(SFXADriver);
            SFXB.SetAudioDriver(SFXBDriver);
        }

        public void Pause()
        {
            //When you pause the wave/ogg player
            //the audio keep playing until the
            //buffer is empty, but pausing the audio driver
            //it make it pauses when the current sampler
            //block is played, allowing we pause with a
            //higher precision, initially this feature
            //it was intended to suspend the audio thread,
            //but after look a bit more seems that we need
            //jailbreak the process to call the pthread functions
            //so it became a bit overkill just to pause the sound.
            InstrumentalDriver.Interrupt();
            OtherDriver.Interrupt();
            VoiceDriver.Interrupt();
            SFXADriver.Interrupt();
            SFXBDriver.Interrupt();
        }


        public void MuteAll()
        {
            VoiceDriver.SetVolume(0);
            InstrumentalDriver.SetVolume(0);
            SFXADriver.SetVolume(0);
            SFXBDriver.SetVolume(0);
            OtherDriver.SetVolume(0);
        }
        public void MuteVoice()
        {
            VoiceDriver.SetVolume(0);
        }

        public void UnmuteVoice()
        {
            VoiceDriver.SetVolume(80);
        }

        public void MuteOther()
        {
            OtherDriver.SetVolume(0);
        }

        public void UnmuteOther()
        {
            OtherDriver.SetVolume(80);
        }

        public void MuteActiveSFX()
        {
            SFXADriver.SetVolume(0);
        }
        
        public void MutePassiveSFX()
        {
            SFXBDriver.SetVolume(0);
        }

        public void UnmuteActiveSFX()
        {
            SFXADriver.SetVolume(80);
        }

        public void UnmutePassiveSFX()
        {
            SFXBDriver.SetVolume(80);
        }

        public void PlayActiveSFX(MemoryStream Sound, byte Volume = 80)
        {
            SFXADriver.SetVolume(Volume);
            SFXA.Open(Sound);
            SFXA.Restart();
        }
        
        public void PlayPassiveSFX(MemoryStream Sound, byte Volume = 80)
        {
            SFXBDriver.SetVolume(Volume);
            SFXB.Open(Sound);
            SFXB.Restart();
        }

        public void PlayOther(MemoryStream Sound, byte Volume = 80, bool Loop = false)
        {
            OtherDriver.SetVolume(Volume);
            Other.Open(Sound);
            Other.Loop = Loop;
            Other.Restart();
        }
        
        public void SetActiveSFXVol(float Volume)
        {
            if (Volume < 0 || Volume > 1)
                throw new ArgumentOutOfRangeException($"{nameof(Volume)} must be in the range 0.0 <= X <= 1.0");

            SFXADriver.SetVolume((byte)(80 * Volume));
        }

        public void SetPassiveSFXVol(float Volume)
        {
            if (Volume < 0 || Volume > 1)
                throw new ArgumentOutOfRangeException($"{nameof(Volume)} must be in the range 0.0 <= X <= 1.0");

            SFXBDriver.SetVolume((byte)(80 * Volume));
        }

        public void Dispose()
        {
            Voices?.Dispose();
            VoiceDriver?.Dispose();
            Instrumental?.Dispose();
            InstrumentalDriver?.Dispose();
            
            SFXA?.Dispose();
            SFXB?.Dispose();
            SFXADriver?.Dispose();
            SFXBDriver?.Dispose();
            Other?.Dispose();
            OtherDriver?.Dispose();
        }
    }
}

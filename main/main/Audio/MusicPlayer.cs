using OrbisGL.Audio;
using System;
using System.IO;

namespace Orbis.Audio
{
    internal class MusicPlayer : IDisposable
    {
        bool Disposed = false;
        bool Ogg = false;
        
        IAudioPlayer Instrumental;
        IAudioPlayer Voices;
        IAudioPlayer SFXA;
        IAudioPlayer SFXB;

        IAudioOut InstrumentalDriver;
        IAudioOut VoiceDriver;
        IAudioOut SFXADriver;
        IAudioOut SFXBDriver;

        public MusicPlayer(MemoryStream Instrumental, MemoryStream Voices, EventHandler OnMusicEnd, bool Ogg) {
            InstrumentalDriver = new OrbisAudioOut();
            VoiceDriver = new OrbisAudioOut();
            SFXADriver = new OrbisAudioOut();
            SFXBDriver = new OrbisAudioOut();

            this.Ogg = Ogg;

            this.Instrumental = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.Voices = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.SFXA = Ogg ? new VorbisPlayer() : new WavePlayer(); 
            this.SFXB = Ogg ? new VorbisPlayer() : new WavePlayer();

            if (Instrumental == null)
            {
                this.Instrumental?.Dispose();
                this.InstrumentalDriver?.Dispose();

                this.Instrumental = null;
                this.InstrumentalDriver = null;
            }
            else
            {
                this.Instrumental.Open(Instrumental);
                this.Instrumental.SetAudioDriver(InstrumentalDriver);
            }

            if (Voices == null)
            {
                this.Voices?.Dispose();
                this.VoiceDriver?.Dispose();

                this.Voices = null;
                this.VoiceDriver = null;
            }
            else
            {
                this.Voices.Open(Voices);
                this.Voices.SetAudioDriver(VoiceDriver);
            }

            SFXA?.SetAudioDriver(SFXADriver);
            SFXB?.SetAudioDriver(SFXBDriver);

            this.Instrumental.OnTrackEnd += OnMusicEnd;
            
            InstrumentalDriver?.Resume();
            VoiceDriver?.Resume();
            SFXADriver?.Resume();
            SFXBDriver?.Resume();
        }
        
        public void Resume()
        {
            if (Disposed)
                return;

            Voices?.Resume();
            Instrumental?.Resume();
            
            SFXA?.Resume();
            SFXB?.Resume();
            
            InstrumentalDriver?.Resume();
            VoiceDriver?.Resume();
            SFXADriver?.Resume();
            SFXBDriver?.Resume();
        }

        public void CloseSFX()
        {
            SFXA?.Dispose();
            SFXB?.Dispose();
            SFXADriver?.Dispose();
            SFXBDriver?.Dispose();

            SFXA = Ogg ? new VorbisPlayer() : new WavePlayer();
            SFXB = Ogg ? new VorbisPlayer() : new WavePlayer();

            SFXADriver = new OrbisAudioOut();
            SFXBDriver = new OrbisAudioOut();
            
            SFXA?.SetAudioDriver(SFXADriver);
            SFXB?.SetAudioDriver(SFXBDriver);
        }

        public void Pause()
        {
            if (Disposed)
                return;

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
            InstrumentalDriver?.Interrupt();
            VoiceDriver?.Interrupt();
            SFXADriver?.Interrupt();
            SFXBDriver?.Interrupt();
        }


        public void MuteAll()
        {
            if (Disposed)
                return;

            VoiceDriver?.SetVolume(0);
            InstrumentalDriver?.SetVolume(0);
            SFXADriver?.SetVolume(0);
            SFXBDriver?.SetVolume(0);
        }
        public void MuteVoice()
        {
            if (Disposed)
                return;

            VoiceDriver?.SetVolume(0);
        }

        public void UnmuteVoice()
        {
            if (Disposed)
                return;

            VoiceDriver?.SetVolume(80);
        }
        
        public void MuteActiveSFX()
        {
            if (Disposed)
                return;

            SFXADriver?.SetVolume(0);
        }
        
        public void MutePassiveSFX()
        {
            if (Disposed)
                return;

            SFXBDriver?.SetVolume(0);
        }

        public void UnmuteActiveSFX()
        {
            if (Disposed)
                return;

            SFXADriver?.SetVolume(80);
        }

        public void UnmutePassiveSFX()
        {
            if (Disposed)
                return;

            SFXBDriver?.SetVolume(80);
        }

        public void PlayActiveSFX(MemoryStream Sound, byte Volume = 80)
        {
            if (Disposed)
                return;

            SFXADriver?.SetVolume(Volume);
            SFXA?.Open(Sound);
            SFXA?.Restart();
        }
        
        public void PlayPassiveSFX(MemoryStream Sound, byte Volume = 80, bool Loop  = false)
        {
            if (Disposed)
                return;

            SFXBDriver?.SetVolume(Volume);
            SFXB.Loop = Loop;
            SFXB?.Open(Sound);
            SFXB?.Restart();
        }
        public void StopPassiveSFX()
        {
            SFXBDriver?.SetVolume(0);
            SFXB?.Close();
            SFXBDriver?.Stop();
        }
        
        public void SetActiveSFXVol(float Volume)
        {
            if (Disposed)
                return;

            if (Volume < 0 || Volume > 1)
                throw new ArgumentOutOfRangeException($"{nameof(Volume)} must be in the range 0.0 <= X <= 1.0");

            SFXADriver?.SetVolume((byte)(80 * Volume));
        }

        public void SetPassiveSFXVol(float Volume)
        {
            if (Disposed)
                return;

            if (Volume < 0 || Volume > 1)
                throw new ArgumentOutOfRangeException($"{nameof(Volume)} must be in the range 0.0 <= X <= 1.0");

            SFXBDriver?.SetVolume((byte)(80 * Volume));
        }

        public void Dispose()
        {
            if (Disposed)
                return;

            Disposed = true;
            Voices?.Dispose();
            VoiceDriver?.Dispose();
            Instrumental?.Dispose();
            InstrumentalDriver?.Dispose();
            
            SFXA?.Dispose();
            SFXB?.Dispose();
            SFXADriver?.Dispose();
            SFXBDriver?.Dispose();
        }
    }
}

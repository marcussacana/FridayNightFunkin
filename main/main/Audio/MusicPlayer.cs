using OrbisGL.Audio;
using System;
using System.IO;

namespace Orbis.Audio
{
    internal class MusicPlayer : IDisposable
    {
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

            this.Instrumental = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.Voices = Ogg ? new VorbisPlayer() : new WavePlayer();
            this.SFXA = Ogg ? new VorbisPlayer() : new WavePlayer(); 
            this.SFXB = Ogg ? new VorbisPlayer() : new WavePlayer(); 

            this.Instrumental.Open(Instrumental);
            this.Instrumental.SetAudioDriver(InstrumentalDriver);
            
            this.Voices.Open(Voices);
            this.Voices.SetAudioDriver(VoiceDriver);

            SFXA.SetAudioDriver(SFXADriver);
            SFXB.SetAudioDriver(SFXBDriver);

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
            InstrumentalDriver.Suspend();
            VoiceDriver.Suspend();
            SFXADriver.Suspend();
            SFXBDriver.Suspend();
        }


        public void MuteVoice()
        {
            VoiceDriver.SetVolume(0);
        }

        public void UnmuteVoice()
        {
            VoiceDriver.SetVolume(80);
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

        public void PlayActiveSFX(MemoryStream Sound)
        {
            SFXADriver.SetVolume(80);
            SFXA.Open(Sound);
            SFXA.Resume();
        }
        
        public void PlayPassiveSFX(MemoryStream Sound)
        {
            SFXBDriver.SetVolume(80);
            SFXB.Open(Sound);
            SFXB.Resume();
        }
        
        public void Dispose()
        {
            Voices?.Dispose();
            VoiceDriver?.Dispose();
            Instrumental?.Dispose();
            InstrumentalDriver?.Dispose();
        }
    }
}

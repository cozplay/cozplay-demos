using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class Sound
{
    public AudioManager audioManager;
    public string name;
    public AudioClip clip;
    public AudioSource source;
    public Action<Sound> callback;
    public bool loop;
    public bool interrupts;

    private HashSet<Sound> interruptedSounds =
        new HashSet<Sound> ();

    /// returns a float from 0.0 to 1.0 representing how much
    /// of the sound has been played so far
    public float progress {
        get {
            if (source == null || clip == null)
                return 0f;
            return (float)source.timeSamples / (float)clip.samples;
        }
    }

    /// returns true if the sound has finished playing
    /// will always be false for looping sounds
    public bool finished {
        get {
            return !loop && progress >= 1f;
        }
    }

    /// returns true if the sound is currently playing,
    /// false if it is paused or finished
    /// can be set to true or false to play/pause the sound
    /// will register the sound before playing
    public bool playing {
        get {
            return source != null && source.isPlaying;
        } 
        set {
            if (value) {
                audioManager.RegisterSound(this);
            }
            PlayOrPause(value, interrupts);
        }
    }

    /// Try to avoid calling this directly
    /// Use AudioManager.NewSound instead
    public Sound(string newName) {
        name = newName;
        clip = (AudioClip)Resources.Load(name, typeof(AudioClip));
        if (clip == null)
            throw new Exception("Couldn't find AudioClip with name '"+name+"'. Are you sure the file is in a folder named 'Resources'?");
    }

    public void Update() {
        if (source != null)
            source.loop = loop;
        if (finished)
            Finish();
    }

    /// Try to avoid calling this directly
    /// Use the Sound.playing property instead
    public void PlayOrPause(bool play, bool pauseOthers) {
        if (pauseOthers) {
            if (play) {
                interruptedSounds = new HashSet<Sound>(audioManager.sounds.Where(snd => snd.playing &&
                                                                                        snd != this));
            }
            interruptedSounds.ToList().ForEach(sound => sound.PlayOrPause(!play, false));
        }
        if (play && !source.isPlaying) {
            source.Play();
        } else {
            source.Pause();
        }
    }

    /// performs necessary actions when a sound finishes
    public void Finish() {
        PlayOrPause(false, true);
        if (callback != null) 
            callback(this);
        MonoBehaviour.Destroy(source);
        source = null;
    }

    /// Reset the sound to its beginning
    public void Reset() {
        source.time = 0f;
    }
}

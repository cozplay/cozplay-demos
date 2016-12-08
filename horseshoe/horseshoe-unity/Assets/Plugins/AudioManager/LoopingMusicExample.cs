using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LoopingMusicExample : MonoBehaviour
{
    Sound snd;
    public GUIText progressBar;

    void Start() {
        // Set the param "loop" to true for background music & other looping sounds
        snd = AudioManager.Main.NewSound("Dungeon", loop: true);
    }

    void Update() {
        GetComponent<GUIText>().text = "Looping Music " + (snd.playing ? "▐▐" : "►");

        // Use the Sound class's "progress" property to see what % of the sound has been played so far:
        int progressTo20 = (int)(snd.progress * 20f);
        progressBar.GetComponent<GUIText>().text = "|"+(new string('|', progressTo20))+(new string(' ', 20-progressTo20))+"|";
    }

    void OnMouseDown() {
        // Toggling play/pause is as simple as setting a bool!
        snd.playing = !snd.playing;
    }
}

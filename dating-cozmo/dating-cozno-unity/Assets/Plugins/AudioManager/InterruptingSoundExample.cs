
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InterruptingSoundExample : MonoBehaviour
{
    Sound snd;
    public GameObject progressBar;

    void Start() {
        // Set the "interrupts" param to true to interrupt other sounds when this one plays
        snd = AudioManager.Main.NewSound("Secret", interrupts: true);
    }

    void Update() {
        GetComponent<GUIText>().text = "Interrupting Sound " + (snd.playing ? "▐▐" : "►");
        
        int progressTo20 = (int)(snd.progress * 20f);
        progressBar.GetComponent<GUIText>().text = "|"+(new string('|', progressTo20))+(new string(' ', 20-progressTo20))+"|";
    }

    void OnMouseDown() {
        snd.playing = !snd.playing;
    }
}

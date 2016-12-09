using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SimpleSoundExample : MonoBehaviour
{
    void OnMouseDown() {
        // Playing a sound from the Resources folder can be done in just 1 line!
		AudioManager.Main.PlayNewSound("MediumItem");
    }
}

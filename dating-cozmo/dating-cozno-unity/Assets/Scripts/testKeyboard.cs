using UnityEngine;
using System.Collections;

public class testKeyboard : MonoBehaviour {


	private TouchScreenKeyboard keyboard;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (keyboard == null) {
			keyboard = TouchScreenKeyboard.Open(null);
		}
		keyboard.active = true;
	}
}

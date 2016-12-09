using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class showKeyBoard : MonoBehaviour {
	
	public GameObject input;
	public GameObject keyboard;

	// Use this for initialization
	void Start () {
		input = GameObject.Find ("InputField");
		keyboard = GameObject.Find("test");
		keyboard.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {	
		if (input.GetComponent<InputField>().isFocused == true)
			keyboard.SetActive (true);
	}
}

/**
* TextTyper class to type text dynamically letter by letter
*
* @class TextTyper
*/
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextTyper : MonoBehaviour {

	public float letterPause = 0.05f;

	string message;
	Text textComp;

	// Use this for initialization
	void Start () {

	}

	public void StartTextTyperAnim(){
		textComp = GetComponent<Text>();
		textComp.supportRichText = true;
		message = textComp.text;
		textComp.text = "";
		StartCoroutine(TypeText ());
	}

	IEnumerator TypeText () {
		foreach (char letter in message.ToCharArray()) {
			textComp.text += letter;
			yield return 0;
			yield return new WaitForSeconds (letterPause);
		}
	}
}
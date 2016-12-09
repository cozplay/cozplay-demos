using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace HeathenEngineering.OSK.v2
{
	/// <summary>
	/// On screen keyboard behaviour.
	/// </summary>
	[AddComponentMenu("Heathen/OSK/v2/On Screen Keyboard (v2.x)")]
	public class OnScreenKeyboard : MonoBehaviour 
	{
		/// <summary>
		/// The key template to be used when generating the keyboard in editor; you can clear this value when complete
		/// </summary>
		public OnScreenKeyboardKey KeyTemplate;
		/// <summary>
		/// The active key e.g. the one that is highlighted.
		/// </summary>
		public OnScreenKeyboardKey ActiveKey;
		/// <summary>
		/// Occurs when key pressed.
		/// </summary>
		public event KeyboardEventHandler KeyPressed;
		/// <summary>
		/// The is input lower case.
		/// </summary>
		public bool IsLowerCase = true;
		/// <summary>
		/// The keys.
		/// </summary>
		public List<OnScreenKeyboardKey> Keys;


		// Use this for initialization
		void Start () 
		{
			try
			{
				UpdateStructure();
			}
			catch(Exception ex)
			{
				//This will disable the game object if we suffer fatal exception on initalization
				Debug.LogException(ex, this);
				gameObject.SetActive(false);
			}
		}

		public void UpdateStructure()
		{
			Keys = new List<OnScreenKeyboardKey>(gameObject.GetComponentsInChildren<OnScreenKeyboardKey>());
			
			//Load the key list
			if(Keys.Count < 1)
				Debug.LogWarning("Heathen On Screen Keyboard was unable to locate an OnScreeKeboardKey component in any of its children.\nPlease add at least 1 key or indicate a key on the OnScreenKeyboard behaviour by setting the ActiveKey value.", this);
			
			if(Keys.Count > 0 && (ActiveKey == null || !Keys.Contains(ActiveKey)))
			{
				//The developer didn't tell us where to start or gave us an orphan so pick the first child we found under us
				ActiveKey = Keys[0];
			}
			
			foreach (OnScreenKeyboardKey key in Keys) 
			{
				key.Keyboard = this;
			}
		}

		/// <summary>
		/// Sets the case displayed.
		/// </summary>
		/// <param name="ToUpper">If set to <c>true</c> to upper.</param>
		public void SetCase(bool ToUpper)
		{
			IsLowerCase = !ToUpper;
			foreach(OnScreenKeyboardKey key in Keys)
				key.SetCase(ToUpper);
		}

		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateDown()
		{
			ActiveKey = ActiveKey.DownKey;
			return ActiveKey;
		}

		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateUp()
		{
			ActiveKey = ActiveKey.UpKey;
			return ActiveKey;
		}

		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateLeft()
		{
			ActiveKey = ActiveKey.LeftKey;
			return ActiveKey;
		}

		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateRight()
		{
			ActiveKey = ActiveKey.RightKey;
			return ActiveKey;
		}

		/// <summary>
		/// Activates the key.
		/// This can be called to simulate a press event on the current key
		/// </summary>
		/// <returns>The key's string value.</returns>
		public string ActivateKey()
		{		
			if(KeyPressed != null)
				KeyPressed(this, new OnScreenKeyboardArguments(ActiveKey));
			
			switch(ActiveKey.type)
			{
			case KeyClass.String:
				if (IsLowerCase) {
					GameObject.FindObjectOfType<GameController> ().SendMessage ("OnKeyboardKeyPressed", ActiveKey.LowerCaseValue);
					//Debug.Log (ActiveKey.LowerCaseValue);
					return ActiveKey.LowerCaseValue;
				} else {
					GameObject.FindObjectOfType<GameController> ().SendMessage ("OnKeyboardKeyPressed", ActiveKey.UpperCaseValue);
					//Debug.Log (ActiveKey.UpperCaseValue);
					return ActiveKey.UpperCaseValue;
				}
				break;
			case KeyClass.Shift:
				SetCase(IsLowerCase);
				return "";
				break;
			case KeyClass.Return:
				GameObject.FindObjectOfType<GameController> ().SendMessage ("OnKeyboardKeyPressed", "return");
				return "\n";
				break;
			case KeyClass.Backspace:
				GameObject.FindObjectOfType<GameController> ().SendMessage ("OnKeyboardKeyPressed", "delete");

				return "";
				break;
			default:
				return "";
			}			
		}

		// Update is called once per frame
		void Update () 
		{
		}
	}
}

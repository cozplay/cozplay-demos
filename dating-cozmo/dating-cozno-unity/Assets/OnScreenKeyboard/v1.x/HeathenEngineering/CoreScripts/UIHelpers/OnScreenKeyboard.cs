using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace HeathenEngineering.OSK.v1
{
	/// <summary>
	/// On screen keyboard behaviour.
	/// </summary>
	[ExecuteInEditMode]
	[AddComponentMenu("Heathen/OSK/v1/On Screen Keyboard (v1.0)")]
	public class OnScreenKeyboard : MonoBehaviour 
	{
		/// <summary>
		/// The key template to be used when generating the keyboard in editor; you can clear this value when complete
		/// </summary>
		public OnScreenKeyboardKey KeyTemplate;
		/// <summary>
		/// The active key.
		/// </summary>
		public OnScreenKeyboardKey ActiveKey;
		/// <summary>
		/// Occurs when key pressed.
		/// </summary>
		public event KeyboardEventHandler KeyPressed;
		/// <summary>
		/// The color of the keyboard.
		/// </summary>
		public Color KeyboardColor = Color.white;
		/// <summary>
		/// The keyboard shader parameter to be edited.
		/// </summary>
		public string KeyboardShaderParameter = "_TintColor";
		/// <summary>
		/// The color of the background.
		/// </summary>
		public Color BackgroundColor = Color.white;
		/// <summary>
		/// The background shader parameter to be edited.
		/// </summary>
		public string BackgroundShaderParameter = "_TintColor";
		/// <summary>
		/// The color of the key text.
		/// </summary>
		public Color KeyTextColor = Color.white;
		/// <summary>
		/// The board renderer.
		/// </summary>
		public Renderer BoardRenderer;
		/// <summary>
		/// The board trim renderer.
		/// </summary>
		public Renderer TrimRenderer;
		/// <summary>
		/// The background alpha.
		/// </summary>
		[Range(0,1)]
		public float BackgroundAlpha = 0.12f;
		/// <summary>
		/// The trim alpha.
		/// </summary>
		[Range(0,1)]
		public float TrimAlpha = 0.24f;
		/// <summary>
		/// The font alpha.
		/// </summary>
		[Range(0,1)]
		public float FontAlpha = 0.24f;
		/// <summary>
		/// The focused font alpha.
		/// </summary>
		[Range(0,1)]
		public float FontFocusAlpha = 0.75f;
		/// <summary>
		/// The size of the font.
		/// </summary>
		public int FontSize = 64;
		/// <summary>
		/// The is input lower case.
		/// </summary>
		public bool IsLowerCase = true;
		/// <summary>
		/// The next input test time.
		/// </summary>
		private float NextTestTime = 0;
		/// <summary>
		/// The keys.
		/// </summary>
		[HideInInspector]
		public List<OnScreenKeyboardKey> Keys;
		private Color AppliedBoardColor = new Color(0.5f,0.5f,0.5f,0.15f);
		private Color AppliedTrimColor = new Color(0.5f,0.5f,0.5f,0.25f);
		private Color AppliedKeyColor = new Color(0.5f,0.5f,0.5f,0.25f);
		private Color AppliedKeyTextColor = new Color(0.5f,0.5f,0.5f,0.5f);
		private Color AppliedKeyTextFocusColor = new Color(0.5f,0.5f,0.5f,0.75f);
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
			
			foreach(OnScreenKeyboardKey key in Keys)
				key.Keyboard = this;
		}
		/// <summary>
		/// Applies the coloring to the board and keys.
		/// </summary>
		public void ApplyColoring()
		{
			AppliedBoardColor = BackgroundColor;
			AppliedBoardColor.a = BackgroundAlpha;

			AppliedTrimColor = KeyboardColor;
			AppliedTrimColor.a = TrimAlpha;

			AppliedKeyColor = KeyboardColor;
			AppliedKeyColor.a = TrimAlpha;

			AppliedKeyTextColor = KeyTextColor;
			AppliedKeyTextColor.a = FontAlpha;

			AppliedKeyTextFocusColor = KeyTextColor;
			AppliedKeyTextFocusColor.a = FontFocusAlpha;

			if(BoardRenderer != null)
			{
				if(!Application.isEditor || Application.isPlaying)
				{
					BoardRenderer.material.color = AppliedBoardColor;
					if(!string.IsNullOrEmpty(BackgroundShaderParameter))
						BoardRenderer.material.SetColor(BackgroundShaderParameter, AppliedBoardColor);
				}
				else
				{ //We must be in editor edit the shared material less we leak an instance
					BoardRenderer.sharedMaterial.color = AppliedBoardColor;
					if(!string.IsNullOrEmpty(BackgroundShaderParameter))
						BoardRenderer.sharedMaterial.SetColor(BackgroundShaderParameter, AppliedBoardColor);
				}
			}
			if(TrimRenderer != null)
			{
				if(!Application.isEditor || Application.isPlaying)
				{
					TrimRenderer.material.color = AppliedTrimColor;
					if(!string.IsNullOrEmpty(KeyboardShaderParameter))
						TrimRenderer.material.SetColor(KeyboardShaderParameter, AppliedTrimColor);
				}
				else
				{ //We must be in editor edit the shared material less we leak an instance
					TrimRenderer.sharedMaterial.color = AppliedTrimColor;
					if(!string.IsNullOrEmpty(KeyboardShaderParameter))
						TrimRenderer.sharedMaterial.SetColor(KeyboardShaderParameter, AppliedTrimColor);
				}
			}

			foreach(OnScreenKeyboardKey key in Keys)
			{
				if(key != null)
				{
					if(key.ButtonRenderer != null)
					{
						if(!Application.isEditor || Application.isPlaying)
						{
							key.ButtonRenderer.material.color = AppliedKeyColor;
							if(!string.IsNullOrEmpty(KeyboardShaderParameter))
								key.ButtonRenderer.material.SetColor(KeyboardShaderParameter, AppliedKeyColor);
						}
						else
						{
							if(key.ButtonRenderer.sharedMaterial != null)
							{ //We must be in editor edit the shared material less we leak an instance
								key.ButtonRenderer.sharedMaterial.color = AppliedKeyColor;
								if(!string.IsNullOrEmpty(KeyboardShaderParameter))
									key.ButtonRenderer.sharedMaterial.SetColor(KeyboardShaderParameter, AppliedKeyColor);
							}
							else
								Debug.LogWarning("You may have an instanced material in scene on key", key);
						}
					}
					if(key.Text != null)
					{
						key.Text.color = AppliedKeyTextColor;
						key.Text.fontSize = FontSize;
					}
					if(key == ActiveKey)
					{
						key.Text.color = AppliedKeyTextFocusColor;
					}
				}
				else
				{
					//Structure changed
					UpdateStructure();
				}
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
			ActiveKey.DownKey.Text.color = AppliedKeyTextFocusColor;
			ActiveKey.Text.color = AppliedKeyTextColor;
			ActiveKey = ActiveKey.DownKey;
			return ActiveKey;
		}
		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateUp()
		{
			ActiveKey.UpKey.Text.color = AppliedKeyTextFocusColor;
			ActiveKey.Text.color = AppliedKeyTextColor;
			ActiveKey = ActiveKey.UpKey;
			return ActiveKey;
		}
		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateLeft()
		{
			ActiveKey.LeftKey.Text.color = AppliedKeyTextFocusColor;
			ActiveKey.Text.color = AppliedKeyTextColor;
			ActiveKey = ActiveKey.LeftKey;
			return ActiveKey;
		}
		/// <summary>
		/// Navigates down from the current key.
		/// </summary>
		/// <returns>The key below the current.</returns>
		public OnScreenKeyboardKey NavigateRight()
		{
			ActiveKey.RightKey.Text.color = AppliedKeyTextFocusColor;
			ActiveKey.Text.color = AppliedKeyTextColor;
			ActiveKey = ActiveKey.RightKey;
			return ActiveKey;
		}
		/// <summary>
		/// Activates the key.
		/// This can be called to simulate a press event on the current key
		/// </summary>
		/// <returns>The key.</returns>
		public string ActivateKey()
		{		
			if(KeyPressed != null)
				KeyPressed(this, new OnScreenKeyboardArguments(ActiveKey));

			switch(ActiveKey.type)
			{
			case KeyClass.String:
				if(IsLowerCase)
					return ActiveKey.LowerCaseValue;
				else
					return ActiveKey.UpperCaseValue;
				break;
			case KeyClass.Shift:
				SetCase(IsLowerCase);
				return "";
				break;
			case KeyClass.Return:
				return "\n";
				break;
			case KeyClass.Backspace:
				return "";
				break;
			default:
				return "";
			}

		}
		// Update is called once per frame
		void Update () 
		{
			if(!Application.isEditor || Application.isPlaying)
			{
				if(ActiveKey != null && (ActiveKey.Text.fontSize != FontSize || ActiveKey.Text.color != AppliedKeyTextFocusColor))
				{
					//If we have an active key and its font size or font color doesnt match what we expect then it must have changed since last update
					ApplyColoring();
				}
			}
			else 
			{
				//If we have an active key and its font size or font color doesnt match what we expect then it must have changed since last update
				ApplyColoring();
			}
		}
	}
}

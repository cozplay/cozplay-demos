using UnityEngine;
using System.Collections;

namespace HeathenEngineering.OSK.v1
{
	/// <summary>
	/// Test controller.
	/// A simple demonstration of handling input on an OnScreenKeyboard
	/// </summary>
	public class TestController : MonoBehaviour {

		public OnScreenKeyboard Keyboard;
		public TextMesh InputText;
		public string WorkingString = "";
		private string cursor = "|";

		// Use this for initialization
		void Start () 
		{
			
		}
		
		// Update is called once per frame
		void Update () 
		{
			//This example is meant to be verbose; it is recomended that you write your own handler

			if(Keyboard != null)
			{
				//For the up arrow
				if(Input.GetKeyUp(KeyCode.UpArrow))
				{
					//Call navigate up on the keyboard to set the active key to the one directly above this one
					//This will switch the active key of the keyboard to the one indicated by the key as beeing its UpKey
					Keyboard.NavigateUp();
				}
				//For the down arrow
				else if(Input.GetKeyUp(KeyCode.DownArrow))
				{
					//Call navigate down on the keyboard to set the active key to the one directly below this one
					//This will switch the active key of the keyboard to the one indicated by the key as beeing its DownKey
					Keyboard.NavigateDown();
				}
				//For the left arrow
				else if(Input.GetKeyUp(KeyCode.LeftArrow))
				{
					//Call navigate left on the keyboard to set the active key to the one directly left of this one
					//This will switch the active key of the keyboard to the one indicated by the key as beeing its LeftKey
					Keyboard.NavigateLeft();
				}
				//For the right arrow
				else if(Input.GetKeyUp(KeyCode.RightArrow))
				{
					//Call navigate right on the keyboard to set the active key to the one directly right of this one
					//This will switch the active key of the keyboard to the one indicated by the key as beeing its RightKey
					Keyboard.NavigateRight();
				}
				//For the return
				else if(Input.GetKeyUp(KeyCode.Return))
				{
					//If the active key is a backspace type we need to edit or destination string removing a character
					if(Keyboard.ActiveKey.type == KeyClass.Backspace)
					{
						//Only operate if we have characters to remove
						if(WorkingString.Length > 0)
							WorkingString = WorkingString.Remove(WorkingString.Length-1,1);
					}
					else //For all other keys we can simply apend the output from the ActiveKey call this will also return newLine for Return type keys
						WorkingString += Keyboard.ActivateKey();
				}
				//For either of the shift keys
				//Note this is here for convenance and wouldn't be typical of an onscreen keyboard
				else if(Input.GetKeyUp(KeyCode.RightShift) || Input.GetKeyUp(KeyCode.LeftShift))
				{
					Keyboard.SetCase(Keyboard.IsLowerCase);
				}
				//For the backspace key
				//Again a matter of convenance 
				else if (Input.GetKeyUp(KeyCode.Backspace))
				{
					if(WorkingString.Length > 0)
						WorkingString = WorkingString.Remove(WorkingString.Length-1,1);
				}
				//For the escape key
				//This is simply for our demo to allow the player to clear the destination string
				else if(Input.GetKeyUp(KeyCode.Escape))
				{
					WorkingString = "";
				}
				//For the mouse button
				else if(Input.GetKeyUp (KeyCode.Mouse0))
				{
					//Cast a ray and see if we hit one of our keys
					//We will build our ray off the main camera from the position of the cursor
					Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
					RaycastHit rayHit;
					if(Physics.Raycast(mouseRay, out rayHit))
					{
						//If we hit somthing insure its an OnScreenKeyboardKey and belongs to us
						GameObject subject = rayHit.collider.gameObject;
						OnScreenKeyboardKey key = subject.GetComponent<OnScreenKeyboardKey>();
						if(key != null && key.Keyboard == Keyboard)
						{
							//Set the hit key as the active key of the keyboard
							Keyboard.ActiveKey = key;
							//Just like before we want to test if this is a backspace key if so we need to trim off our destination string
							if(Keyboard.ActiveKey.type == KeyClass.Backspace)
							{
								if(WorkingString.Length > 0)
									WorkingString = WorkingString.Remove(WorkingString.Length-1,1);
							}
							else //if its any other type we will simply append the output from ActivateKey
								WorkingString += Keyboard.ActivateKey();
						}
					}
				}
				//This is here just for demo and allows us to capture scroll movement from the mouse
				float scrollValue = Input.GetAxis("Mouse ScrollWheel");
				if(scrollValue != 0)
				{
					//Get the keyboards current color
					Color temp = Keyboard.KeyboardColor;
					//set up holders for the hue saturation and value
					float h, s, v = 0;
					//convert our RGB to an HSV (HueSaturationValue) color
					ColorToHSV(temp, out h, out s, out v);
					//Incrament our hue value and boost the scroll movement a bit
					h += scrollValue*20;
					//convert the HSV back to an RGB and reapply our original alpha value
					temp = ColorFromHSV(h,s,v,temp.a);
					//Update the color of the keyboard
					Keyboard.KeyboardColor = temp;
					//Now we do the same with the text color of the keyboard
					temp = Keyboard.KeyTextColor;
					ColorToHSV(temp, out h, out s, out v);
					h += scrollValue*20;
					temp = ColorFromHSV(h,s,v,temp.a);
					Keyboard.KeyTextColor = temp;

					//Once we have updated our colors its a good idea to force an ApplyColoring; the keyboard will test for some common changes and call
					//this on its own but since we are here we might as well help it out
					Keyboard.ApplyColoring();
				}
			}
			//Finaly update the destination string by setting its value to the text mesh and append the cursor so the player can see we are an input field
			InputText.text = WorkingString + cursor;
		}
		/// <summary>
		/// Colors from HSV.
		/// This is some stock code I have found at http://pastebin.com/683Gk9xZ#
		/// </summary>
		/// <returns>The from HS.</returns>
		/// <param name="h">The height.</param>
		/// <param name="s">S.</param>
		/// <param name="v">V.</param>
		/// <param name="a">The alpha component.</param>
		public static Color ColorFromHSV(float h, float s, float v, float a = 1)
		{
			// no saturation, we can return the value across the board (grayscale)
		    if (s == 0)
	            return new Color(v, v, v, a);
		    // which chunk of the rainbow are we in?
	        float sector = h / 60;
	        // split across the decimal (ie 3.87 into 3 and 0.87)
	        int i = (int)sector;
		    float f = sector - i;
	        float p = v * (1 - s);
	        float q = v * (1 - s * f);
	        float t = v * (1 - s * (1 - f));
	        // build our rgb color
	        Color color = new Color(0, 0, 0, a);
	        switch(i)
			{
				case 0:
					color.r = v;
					color.g = t;
					color.b = p;
				break;
				case 1:
					color.r = q;
					color.g = v;
					color.b = p;
				break;
				case 2:
					color.r  = p;
					color.g  = v;
					color.b  = t;
				break;
				case 3:
					color.r  = p;
					color.g  = q;
					color.b  = v;
				break;
				case 4:
					color.r  = t;
					color.g  = p;
					color.b  = v;
				break;
				default:
					color.r  = v;
					color.g  = p;
					color.b  = q;
				break;
			}
			return color;
		}
		/// <summary>
		/// Colors to HSV.
		/// More stock goodness from http://pastebin.com/683Gk9xZ#
		/// </summary>
		/// <param name="color">Color.</param>
		/// <param name="h">The height.</param>
		/// <param name="s">S.</param>
		/// <param name="v">V.</param>
		public static void ColorToHSV(Color color, out float h, out float s, out float v)
		{
			float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
			float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
			float delta = max - min;
			// value is our max color
			v = max;
			// saturation is percent of max
			if (!Mathf.Approximately(max, 0))
				s = delta / max;
			else
			{
				// all colors are zero, no saturation and hue is undefined
				s = 0;
				h = -1;
				return;
			}
			// grayscale image if min and max are the same
			if (Mathf.Approximately(min, max))
			{
				v = max;
				s = 0;
				h = -1;
				return;
			}
			// hue depends which color is max (this creates a rainbow effect)
			if (color.r == max)
				h = (color.g - color.b) / delta;            // between yellow & magenta
			else if (color.g == max)
				h = 2 + (color.b - color.r) / delta;                // between cyan & yellow
			else
				h = 4 + (color.r - color.g) / delta;                // between magenta & cyan
			// turn hue into 0-360 degrees
			h *= 60;
			if (h < 0 )
			h += 360;
		
		}
	}
}

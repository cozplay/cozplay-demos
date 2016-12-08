using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;

public class GameController : MonoBehaviour
{
	
	public GameObject[] slots;
	public AudioClip snap_sound, unsnap_sound, wrong_sound;
	public GameObject instructionsVideoObject, skipButton;

	private const string START_GAME = "StartGame";
	private bool enableInstructions = true;

	private PubnubHelper pubnubHelper;
	private string[] options;
	private static Color COZMO_COLOR = Color.magenta;
	private static Color PLAYER_COLOR = Color.cyan;
	private static Color DEFAULT_COLOR = Color.grey;
	private int emptySlot;
	private List<int> selectedBtnList, flashList, cozmoSlots;
	private int gameState;
	private int isCozmoMoveStarted = -1;
	private bool isPlayerTurn = true;
	private AudioSource audioSource;
	private Sound instructionsSound;

	void Start ()
	{
		audioSource = GetComponent<AudioSource> ();
		pubnubHelper = GameObject.FindObjectOfType<PubnubHelper> ();
		pubnubHelper.onMessageReceived = OnMessageReceived;

		if (enableInstructions) {
			ShowInstrucitons ();
		} else {
			InitGame();
		}

	}

	void ShowInstrucitons(){
		instructionsSound = AudioManager.Main.PlayNewSound ("HorseShoeInstructions");
		LeanTween.scale(skipButton,skipButton.transform.localScale*0.995f,0.35f).setLoopPingPong(-1);
		LeanTween.delayedCall (78f, () => {
			StopInstructions();
		});

	}

	void StopInstructions(){
		if(instructionsVideoObject.activeSelf){
			instructionsVideoObject.SetActive(false);
			instructionsSound.source.volume = 0;
			InitGame();
		}
	}

	public void OnSkipIntstrictions(){
		StopInstructions();

	}

	public void InitGame(){

		emptySlot = 5;
		gameState = 0; //0-init, 1-start 2-end
		selectedBtnList = new List<int> ();
		flashList = new List<int>{ 3, 4 };
		cozmoSlots = new List<int>{ 1, 2 };
		//highlight player slots to start the game
		ChangePlayerSlot (3, true, PLAYER_COLOR);
		ChangePlayerSlot (4, true, PLAYER_COLOR);
		ChangePlayerSlot (1, false, COZMO_COLOR);
		ChangePlayerSlot (2, false, COZMO_COLOR);
		ChangePlayerSlot (5, false, DEFAULT_COLOR);

		StartFlash (3, DEFAULT_COLOR);
		StartFlash (4, DEFAULT_COLOR);
	}

	public void OnCozmoMoveStart (int fromIndex, int toIndex)
	{
		isCozmoMoveStarted = fromIndex;
		StartFlash (fromIndex, DEFAULT_COLOR);
		LeanTween.delayedCall (0.5f, () => {
			StopFlash (fromIndex, DEFAULT_COLOR);
			StartFlash (toIndex, COZMO_COLOR);
		});
	}

	public void OnCozmoMoveEnd (int fromIndex, int toIndex)
	{
		StopFlash (toIndex, COZMO_COLOR);

		if (cozmoSlots.Contains (fromIndex)) {
			cozmoSlots.Remove (fromIndex);
		}
		if (!cozmoSlots.Contains (toIndex)) {
			cozmoSlots.Add (toIndex);
		}

		isCozmoMoveStarted = -1;
		isPlayerTurn = true;
	}


	public void OnSlotClick (int btnIndex)
	{
		if (!isPlayerTurn)
			return;
				
		//print ("OnSlotClick: " + btnIndex);
		if (cozmoSlots.Contains (btnIndex)) {
			print ("Wrong Slot: Cozmo's slot is picked!");
			PlaySound (wrong_sound);
			return;
		}

		if (gameState == 0) {
			if (flashList.Contains (btnIndex)) {
				StopFlash (btnIndex, PLAYER_COLOR);
				flashList.Remove (btnIndex);
				pubnubHelper.Publish ("PlayerReady:" + btnIndex);
				PlaySound (snap_sound);
			}
			if (flashList.Count == 0) {
				gameState = 1;
				isPlayerTurn = false;
				pubnubHelper.Publish (START_GAME);

			}
		} else if (gameState == 1) {
			//if flash list is empty - select slot and flash
			if (flashList.Count == 0) {
				if (selectedBtnList.Contains (btnIndex) && isValidSlotSelection (btnIndex)) {
					StartFlash (btnIndex, DEFAULT_COLOR);
					flashList.Add (btnIndex);
					PlaySound (unsnap_sound);
				} else {
					print ("select green slot to move first");
					PlaySound (wrong_sound);
					Color oriColor = slots [btnIndex].GetComponent<Image> ().color;
					StartFlash (btnIndex, Color.red, 0.2f);
					LeanTween.delayedCall (0.5f, () => {
						StopFlash (btnIndex, oriColor);
					});


				}

			} else if (flashList.Count == 1) {
				
				//if one slot is already selected to mvoe
				if (flashList.Contains (btnIndex)) {
					//deselect flash on currently flashing slot
					StopFlash (btnIndex, PLAYER_COLOR);
					flashList.Remove (btnIndex);
				} else if (!selectedBtnList.Contains (btnIndex)) {
					//select new slot and stop flashing old slot
					int prevFlashingIndex = flashList [0];
					flashList.Remove (prevFlashingIndex);
					StopFlash (prevFlashingIndex, DEFAULT_COLOR);
					ChangePlayerSlot (btnIndex, true, PLAYER_COLOR);
					ChangePlayerSlot (prevFlashingIndex, false, DEFAULT_COLOR);

					//player move complete - send cozmo message now
					pubnubHelper.Publish ("PlayerMove:" + prevFlashingIndex + "," + btnIndex);
					isPlayerTurn = false;
					PlaySound (snap_sound);
				}
			}

		}

	}

	void ChangePlayerSlot (int index, bool selected, Color color)
	{
		if (selected == true) {
			if (!selectedBtnList.Contains (index)) {
				selectedBtnList.Add (index);
			}
		} else {
			if (selectedBtnList.Contains (index)) {
				selectedBtnList.Remove (index);
			}

		}
		ChangeSlotColor (index, color);
	}

	void PrintList (List<int> list)
	{
		string str = "list: ";
		foreach (int s in list) {
			str += " " + s;
		}
		print (str);
	}

	void ChangeSlotColor (int index, Color color)
	{
		if (index < 1 || index > 5)
			return;
		slots [index].GetComponent<Image> ().color = color;
	}

	void StartFlash (int index, Color flashColor, float duration = 0.3f)
	{
		LeanTween.color (slots [index].GetComponent<RectTransform> (), flashColor, duration).setLoopPingPong (-1);
	}

	void StopFlash (int index, Color endColor)
	{
		LeanTween.cancel (slots [index]);
		ChangeSlotColor (index, endColor);
	}

	//Callback from pubnub when new message received from Cozmo
	void OnMessageReceived (string message)
	{
		message = message.Replace ("\"", "");
		string action = message.Split (':') [0];
		print (action);
		if (action.Equals ("CozmoMoveStart")) {
			string[] moves = message.Split (':') [1].Split (',');
			int from = int.Parse (moves [0]);
			int to = int.Parse (moves [1]);
			OnCozmoMoveStart (from, to);
		} else if (action.Equals ("CozmoMoveEnd")) {
			string[] moves = message.Split (':') [1].Split (',');
			int from = int.Parse (moves [0]);
			int to = int.Parse (moves [1]);
			OnCozmoMoveEnd (from, to);
		}
	}

	bool isValidSlotSelection (int index)
	{
		PrintList (cozmoSlots);
		if (index == 1 && (cozmoSlots.Contains (3) || selectedBtnList.Contains (3)) && (cozmoSlots.Contains (5) || selectedBtnList.Contains (5))) {
			return false;
		} else if (index == 2 && (cozmoSlots.Contains (5) || selectedBtnList.Contains (5)) && (cozmoSlots.Contains (4) || selectedBtnList.Contains (4))) {
			return false;
		} else if (index == 3 && (cozmoSlots.Contains (1) || selectedBtnList.Contains (1)) && (cozmoSlots.Contains (5) || selectedBtnList.Contains (5)) && (cozmoSlots.Contains (4) || selectedBtnList.Contains (4))) {
			return false;
		} else if (index == 4 && (cozmoSlots.Contains (2) || selectedBtnList.Contains (2)) && (cozmoSlots.Contains (5) || selectedBtnList.Contains (5)) && (cozmoSlots.Contains (3) || selectedBtnList.Contains (3))) {
			return false;
		}

		return true;
	}

	void PlaySound (AudioClip clip)
	{
		audioSource.clip = clip;
		audioSource.Play ();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}

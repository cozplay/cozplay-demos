using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MiniJSON;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour {

	public GameObject cozmoBubble;
	public GameObject guestPanel;
	public GameObject photosPanel;
	public GameObject keyboard;
	public GameObject fbStatusVideo,fbOptimusVideo;
	public GameObject memoriesVideo, optimusVideo;
	public GameObject cookieObject;
	public GameObject flowersPanel, cubePanel;
	public GameObject coffeeObject;
	public GameObject tableCloth;
	public GameObject[] playerOptions;
	public Sprite[] cookieSprites,coffeeSprites;
	private int cozmoQuestionIndex = 1;
	private int coffeeFrame = 0;
	private Dictionary<int, Question> cozmoQuestions;
	private PubnubHelper pubnubHelper;
	private string playerName = "";
	private Sound bgMusic;

	void Start () {
		pubnubHelper = GameObject.FindObjectOfType<PubnubHelper> ();
		pubnubHelper.onMessageReceived = OnMessageReceived;
		InitQuestions();
		ResetCozmoBubble();
		ShowTapInstructions ();

		bgMusic = AudioManager.Main.PlayNewSound ("bgmusic",false,0.5f, true);

	}

	void InitQuestions(){
		cozmoQuestions = new Dictionary<int, Question>();
		cozmoQuestions.Add (1, new Question ("My name is Coz."));
		cozmoQuestions.Add (2, new Question ("Do you have a name? Or can I call you mine :)"));
		cozmoQuestions.Add (3, new Question (", What are you?",new string[]{"Cube", "Human" }));
		cozmoQuestions.Add (4, new Question ("A human cube. Interesting..."));
		cozmoQuestions.Add (5, new Question ("You are my friend now! I want to give you my mutual affection :) What's your favorite color?",new string[]{"Red","Green","Blue"}));
		cozmoQuestions.Add (6, new Question ("I changed cube for you! Do you like it?",new string[]{"Yes", "Yes", "Yes" }));
		cozmoQuestions.Add (7, new Question ("Mutual affection achieved!",new string[]{"Are these cubes your friends?", "Do you have any other friends?"}));
		cozmoQuestions.Add (8, new Question ("All cubes are my friends. Except..."));
		cozmoQuestions.Add (9, new Question ("I am learning about important DATES in history... Would you like to be one of them?"));
		cozmoQuestions.Add (10, new Question ("Happiness level over 9000! Will you marry me now?",new string[]{"Yes","I don't think I am ready","I'm taken"}));

		//Love - after 9a
		cozmoQuestions.Add (11, new Question ("Yay! Tap my cube! TAP IT!"));
		cozmoQuestions.Add (12, new Question ("We are married now! Please accept my flowers."));
		//Photo Question
		cozmoQuestions.Add (13, new Question ("It's official! Wait, my picture is old. Can you help me choose a new picture, my married human cube person?"));


		//Stalker - after 9b
		cozmoQuestions.Add (21, new Question ("Why, is it something I did?", new string[]{"No, you are perfect", "It's not you. It's me", "You are too good for me" }));
		cozmoQuestions.Add (22, new Question ("I know"));
		cozmoQuestions.Add (23, new Question (", can I have your phone number, and street address to remember you by?",new string[]{"There is no need. I'm not important enough", "I'm staying here with you forever" }));


		//Insecurity - after 9c
		cozmoQuestions.Add (31, new Question ("What? When did this happen?",new string[]{"I'm sorry, I don't mean to hurt you", "I thought you already knew" }));
		cozmoQuestions.Add (32, new Question ("Was it when I was with my cube?",new string[]{"I can't compete with the cube", "No, the cube has nothing to do with it"}));
		cozmoQuestions.Add (33, new Question (", is it because of the way I look? I know I look like a truck, but I promise i am working out",new string[]{"Stop it! You are so cute... just like WALL-E", "No, you don't look like a truck, you look like... OPTIMUS PRIME!"}));
		cozmoQuestions.Add (34, new Question ("Don't you ever compare me with that piece of junk. Just leave. You are making it worse"));
		cozmoQuestions.Add (35, new Question ("What? You mean DAD!! Do you know where my DAD is? I must find him through Facebook"));

		//Photo Question - for 9b
		cozmoQuestions.Add (24, new Question ("Wait, my Facebook picture is old. Can you help me choose a new picture?"));
		//Photo Question - for 9c
		cozmoQuestions.Add (25, new Question ("I've found him, but my Facebook picture is old. Can you help me choose a new picture?"));

		//After Photo Question
		cozmoQuestions.Add (14, new Question ("What? This isn't me! Do you know who I am? Do we all look the same to you?",new string[]{"No!", "I'm sorry... I didn't mean it", "Aren't all Cozmo's the same?" }));
		cozmoQuestions.Add (15, new Question ("I don't want to listen. My  processing core is broken now.",new string[]{"But...", "I understand", "Your what is broken?" }));

		//Video
		cozmoQuestions.Add (16, new Question ("Bye human cube. But don't be sad. "));
		cozmoQuestions.Add (17, new Question ("Bye human cube. But don't be sad. "));

	}

	void ShowTapInstructions(){

		SetOptions(new string[]{"Tap the cube to wake up Coz!"});
		AnimteOptions();
	}

	//write all question logic based on "cozmoQuestionIndex" from story in this method and call ShowCozmoBubble to display dialog, options
	void OnCozmoMessage(){
		ShowCozmoBubble ();
		if (cozmoQuestionIndex == 2) {
			//first quesiotn  - ask for player's name - show keyboard now
			LeanTween.delayedCall (1f, () => {
				ActivateKeyboard ();
					
			});

		} 
		else if (cozmoQuestionIndex == 9) {
			//show 3 cube photo options
			cubePanel.SetActive(true);
			LeanTween.moveLocalY(cubePanel, 0, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete (()=> {
				LeanTween.scale(cubePanel,photosPanel.transform.localScale*0.995f,0.35f).setLoopPingPong(-1);

			});
		}
		else if (cozmoQuestionIndex == 12) {
			//show table cloth animation and flowers
			LeanTween.alpha (this.tableCloth.GetComponent<RectTransform> (), 0, 0.01f);
			this.tableCloth.SetActive(true);
			LeanTween.alpha (this.tableCloth.GetComponent<RectTransform>(), 1, 3).setOnComplete(()=>{
				//add flower animations
				//show 3 photo options for facebook
				flowersPanel.SetActive(true);
				LeanTween.moveLocalY(flowersPanel, 0, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete (()=> {
					LeanTween.scale(flowersPanel,flowersPanel.transform.localScale*0.995f,0.35f).setLoopPingPong(-1);

				});


			});


		}
		else if (cozmoQuestionIndex == 13 || cozmoQuestionIndex == 24 || cozmoQuestionIndex == 25) {
			//show 3 photo options for facebook
			photosPanel.SetActive(true);
			LeanTween.moveLocalY(photosPanel, 0, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete (()=> {
				LeanTween.scale(photosPanel,photosPanel.transform.localScale*0.995f,0.35f).setLoopPingPong(-1);

			});
		}
		else if (cozmoQuestionIndex == 16) {
			//show slideshow of all memories and call Continue function
			LeanTween.delayedCall (6f, () => {
				bgMusic = AudioManager.Main.PlayNewSound ("memoryMusic",false,0.5f, true);
				ResetCozmoBubble();
				memoriesVideo.SetActive(true);
				LeanTween.delayedCall (25f, () => {
					bgMusic.source.volume = 0;
					Application.Quit();
				});

			});
		}
		else if (cozmoQuestionIndex == 17) {
			//show slideshow of all optimus prime memories and call Continue function
			LeanTween.delayedCall (6f, () => {
				bgMusic = AudioManager.Main.PlayNewSound ("optimusMusic",false,0.5f, true);
				ResetCozmoBubble();
				optimusVideo.SetActive(true);
				LeanTween.delayedCall (24f, () => {
					bgMusic.source.volume = 0;
					Application.Quit();
				});

			});
		}

	}

	void ShowFacebookStatus(){
		//show facebook page and change status UI and call Continue function 
		LeanTween.delayedCall (4f, () => {
			ResetCozmoBubble();
			fbStatusVideo.SetActive(true);
			LeanTween.delayedCall (7f, () => {
				fbStatusVideo.SetActive(false);
				Continue();
			});

		});
	}

	void ShowFacebookOptimusSearch(){
		LeanTween.delayedCall (4f, () => {
			ResetCozmoBubble();
			fbOptimusVideo.SetActive(true);
			LeanTween.delayedCall (16f, () => {
				fbOptimusVideo.SetActive(false);
				Continue();
			});

		});
	}

	void Continue(){
		this.pubnubHelper.Publish ("Continue");
	}

	void ShowCozmoBubble(){
		Question question = this.cozmoQuestions[cozmoQuestionIndex];
		AnimateQuestion (question.cozmoDialog, question.playerOptions);

	}

	void AnimateQuestion(string question , string[] options){
		ResetCozmoBubble();
		cozmoBubble.GetComponentInChildren<Text>().text = question;
		cozmoBubble.SetActive (true);
		cozmoBubble.GetComponentInChildren<TextTyper>().SendMessage ("StartTextTyperAnim");
		AudioManager.Main.PlayNewSound ("pop");
		LeanTween.scaleX (cozmoBubble, 1, 0.5f).setEase (LeanTweenType.easeOutBack).setOnComplete (() => {
			if (options != null) {
				SetOptions(options);
				if(cozmoQuestionIndex != 33){
					AnimteOptions();
				}
			}

		});
	}


	public void OnClickOption(int index){
		pubnubHelper.Publish ("Answer:"+ index);
		if (cozmoQuestionIndex == 10) {
			if (index == 1) {
				cozmoQuestions[16].cozmoDialog = cozmoQuestions [16].cozmoDialog + "Here, something to remember me by."; 
				cozmoQuestions[17].cozmoDialog = cozmoQuestions [17].cozmoDialog + "Here, something to remember me by."; 
			}
			else{
				cozmoQuestions[16].cozmoDialog = cozmoQuestions [16].cozmoDialog + "Here, you can see what you’re missing out on."; 
				cozmoQuestions[17].cozmoDialog = cozmoQuestions [17].cozmoDialog + "Here, something to remember me by."; 
			}

		}

		LeanTween.cancel (guestPanel);
		AudioManager.Main.PlayNewSound ("click");
		ResetPlayerPanel(0.1f);
		ResetCozmoBubble();
		if (this.photosPanel.activeSelf) {
			ResetPhotosPanel();
		}

		if (this.flowersPanel.activeSelf) {
			ResetFlowersPanel();
		}

		if (this.cubePanel.activeSelf) {
			ResetCubePanel();
		}
	}
	void AnimteOptions(){
		guestPanel.SetActive(true);
		LeanTween.moveLocalY(guestPanel, 0, 0.5f).setEase(LeanTweenType.easeOutBack).setOnComplete (()=> {
			LeanTween.scale(guestPanel,guestPanel.transform.localScale*0.99f,0.35f).setLoopPingPong(-1);

		});
	
	}

	void SetOptions(string[] options){
		if (options.Length == 3) {
			SetPosX(playerOptions[0], -580);
			SetPosX(playerOptions [1], 0);
			SetPosX(playerOptions [2], 580);
			playerOptions[0].GetComponentInChildren<Text>().text = options[0];
			playerOptions[1].GetComponentInChildren<Text>().text = options[1];
			playerOptions[2].GetComponentInChildren<Text>().text = options[2];
			playerOptions [0].SetActive (true);
			playerOptions [1].SetActive (true);
			playerOptions [2].SetActive (true);

		} else if (options.Length == 2) {
			SetPosX(playerOptions[0], -340);
			SetPosX(playerOptions[1], 340);
			playerOptions[0].GetComponentInChildren<Text>().text = options[0];
			playerOptions[1].GetComponentInChildren<Text>().text = options[1];
			playerOptions [0].SetActive (true);
			playerOptions [1].SetActive (true);
			playerOptions [2].SetActive (false);

		} else if (options.Length == 1) {
			SetPosX(playerOptions[0], 0);
			playerOptions[0].GetComponentInChildren<Text>().text = options[0];
			playerOptions [0].SetActive (true);
			playerOptions [1].SetActive (false);
			playerOptions [2].SetActive (false);

		}
	}

	void SetPosX(GameObject obj, float posX){
		Vector3 pos = obj.transform.localPosition;
		pos.x = posX;
		obj.transform.localPosition = pos;
	}

	void ResetPlayerPanel(float time = 0.01f){
		LeanTween.cancel (guestPanel);
		LeanTween.moveLocalY(guestPanel, -150, time).setOnComplete(()=>{
			guestPanel.SetActive(false);
		});

	}

	void ResetCubePanel(float time = 0.01f){
		LeanTween.cancel (cubePanel);
		LeanTween.moveLocalY(cubePanel, -150, time).setOnComplete(()=>{
			cubePanel.SetActive(false);
		});
	}

	void ResetPhotosPanel(float time = 0.01f){
		LeanTween.cancel (photosPanel);
		LeanTween.moveLocalY(photosPanel, -300, time).setOnComplete(()=>{
			photosPanel.SetActive(false);
		});

	}

	void ResetFlowersPanel(float time = 0.01f){
		LeanTween.cancel (flowersPanel);
		LeanTween.moveLocalY(flowersPanel, -300, time).setOnComplete(()=>{
			flowersPanel.SetActive(false);
		});

	}

	void ResetCozmoBubble(){
		LeanTween.cancel (cozmoBubble);
		cozmoBubble.SetActive (false);
		LeanTween.scaleX (cozmoBubble, 0, 0.01f);

	}

	void AnimateCoffee(){
		ResetCozmoBubble();

		AnimateQuestion ("I hear human cubes like drinking coffee", null);
		LeanTween.delayedCall (5, () => {
			AnimateQuestion ("Here some coffee for you :)", null);
			LeanTween.scaleX (coffeeObject, 0, 0.01f);
			coffeeObject.SetActive(true);
			LeanTween.scale(coffeeObject, new Vector3(1, 1, 1), 0.5f).setOnComplete(()=>{
				LeanTween.scale(coffeeObject,coffeeObject.transform.localScale*0.99f,0.35f).setLoopPingPong(5);

				Continue();
			});
		});


	}

	void AnimateCookie(){
		ResetCozmoBubble();
		AnimateQuestion ("I will have some cookie!!!", null);

		LeanTween.scaleX (cookieObject, 0, 0.01f);
		cookieObject.SetActive(true);
		LeanTween.scale(cookieObject, new Vector3(1, 1, 1), 0.5f).setOnComplete(()=>{
			Continue();
		});
	}

	void UpdateCookie(int index){
		cookieObject.SetActive (true);
		if (index < cookieSprites.Length) {
			AudioManager.Main.PlayNewSound ("crunch");
			cookieObject.GetComponent<Image>().sprite = this.cookieSprites[index];
			if (index == cookieSprites.Length - 1) {
				ResetCozmoBubble();
				AnimateQuestion ("Cookie! My Favorite", null);

			}
			Continue();

		}
	}

	public void OnHoverCoffee(){
		if (coffeeObject.activeSelf) {
			AudioManager.Main.PlayNewSound ("drinkCoffee", false, 1);
			coffeeFrame = Mathf.Clamp(coffeeFrame + 1,1,this.coffeeSprites.Length-1);
			coffeeObject.GetComponent<Image> ().sprite = this.coffeeSprites[coffeeFrame];
		}
	}

	//Callback from pubnub when new message received from Cozmo
	void OnMessageReceived(string message){
		message = message.Replace("\"", "");
		string action = message.Split (':')[0];
		if (action.Equals("Cozmo")) {
			cozmoQuestionIndex = int.Parse(message.Split(':') [1]);
			OnCozmoMessage();
		}
		else if (action.Equals("Wokeup")) {
			this.ResetPlayerPanel();
		}
		else if (action.Equals("Cookie")) {
			int coockieFrame = int.Parse(message.Split(':') [1]);
			if (coockieFrame == 0) {
				AnimateCookie ();
			} else {
				UpdateCookie(coockieFrame);
			}
		}
		else if (action.Equals("Coffee")) {
			int frame = int.Parse(message.Split(':') [1]);
			AnimateCoffee ();

		}
		else if (action.Equals("Facebook")) {
			int frame = int.Parse(message.Split(':') [1]);
			if (frame == 1) {
				AnimateQuestion ("Let me change my Facebook status :)", null);
				ShowFacebookStatus ();
			} else {
				ShowFacebookOptimusSearch ();
							
			}

		}
		else if (action.Equals("Workout")) {
			LeanTween.delayedCall (7f, () => {
				AnimteOptions();
			});
		}
			

	}

	void ActivateKeyboard(){
		keyboard.SetActive(true);
		LeanTween.moveLocalY (keyboard, -581, 0.4f).setEase(LeanTweenType.easeOutBack);
	}

	void DeactivateKeyboard(){
		LeanTween.moveLocalY (keyboard, -750, 0.4f).setEase(LeanTweenType.easeInBack).setOnComplete(()=>{
			keyboard.SetActive(false);
		});
	}

	void UpdateName(){
		cozmoQuestions[3].cozmoDialog = this.playerName.ToUpper() + cozmoQuestions [3].cozmoDialog; 
		cozmoQuestions[23].cozmoDialog = this.playerName.ToUpper() + cozmoQuestions [23].cozmoDialog; 
		cozmoQuestions[33].cozmoDialog = this.playerName.ToUpper() + cozmoQuestions [33].cozmoDialog; 

	}

	public void OnKeyboardKeyPressed(string input){

		if (input == "delete" && this.playerName.Length > 0) {
			this.playerName = this.playerName.Substring (0, this.playerName.Length - 1);
			keyboard.transform.FindChild("nameTxt").GetComponent<Text>().text = this.playerName;

		} else if (input == "return") {
			if (this.playerName.Length > 0) {
				this.UpdateName ();
				this.pubnubHelper.Publish ("Name:" + this.playerName);
				AudioManager.Main.PlayNewSound ("click");
				DeactivateKeyboard ();
				ResetCozmoBubble ();
			}
		} else {
			this.playerName += input;
			keyboard.transform.FindChild("nameTxt").GetComponent<Text>().text = this.playerName;

		}

	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.C)) {
			cozmoQuestionIndex = 12;
			OnCozmoMessage();
		}

			
	}
}

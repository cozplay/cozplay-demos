using System;

[Serializable]
public class Question
{
	public string cozmoDialog;
	public string[] playerOptions;

	public Question(string cozmoDialog, string[] playerOptions = null){
		this.cozmoDialog = cozmoDialog;
		this.playerOptions = playerOptions;
	}
}


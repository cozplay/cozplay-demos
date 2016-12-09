using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Text;
using HeathenEngineering.OSK.v1;

[CustomEditor(typeof(OnScreenKeyboard))]
public class OnScreenKeyboardEditor : Editor
{
	private float RowsToBuild = 5;
	private List<OnScreenKeyboardRowBuilder> RowStructure = new List<OnScreenKeyboardRowBuilder>();
	private GameObject lastRootBuilt = null;
	private bool showBuilder = false;
	
	public override void OnInspectorGUI() 
	{
		if(RowStructure.Count == 0)
		{
			//Default a QWERTY in
			GenerateQWERTY();
		}
		
		//Cast our target pointer
		OnScreenKeyboard subject = target as OnScreenKeyboard;
		
		DrawDefaultInspector();
		
		//If we have a key template populated show the button
		if(subject.KeyTemplate != null)
		{
			showBuilder = EditorGUILayout.Foldout(showBuilder, "Keyboard Builder");
			if(showBuilder)
			{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Row Count: ");
				RowsToBuild = EditorGUILayout.FloatField(RowsToBuild);
				
				if(RowsToBuild < 1)
					RowsToBuild = 1;
				
				EditorGUILayout.EndHorizontal();
				//Manage the row structure
				if(RowsToBuild > RowStructure.Count)
				{
					for(int i = 0; i < RowsToBuild; i++)
					{
						if(i >= RowStructure.Count)
							RowStructure.Add(new OnScreenKeyboardRowBuilder());
					}
				}
				if(RowsToBuild < RowStructure.Count)
				{
					List<OnScreenKeyboardRowBuilder> nBuilders = new List<OnScreenKeyboardRowBuilder>();
					for(int i = 0; i < RowsToBuild; i++)
					{
						nBuilders.Add(RowStructure[i]);
					}
					
					RowStructure = nBuilders;
				}
				//Render the inputs for each row structure
				for(int i = 0; i < RowStructure.Count; i++)
				{
					if(RowStructure[i] == null)
						RowStructure[i] = new OnScreenKeyboardRowBuilder();
					//EditorGUILayout.LabelField("Row: " + (i+1).ToString(), EditorStyles.boldLabel);
					RowStructure[i].FoldOut = EditorGUILayout.Foldout(RowStructure[i].FoldOut, "Row: " + (i+1).ToString());
					if(RowStructure[i].FoldOut)
					{
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Key Count: ");
						RowStructure[i].keyCount = EditorGUILayout.IntField(RowStructure[i].keyCount);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Key Spacing: ");
						RowStructure[i].KeyShiftHorizontal = EditorGUILayout.FloatField(RowStructure[i].KeyShiftHorizontal);
						EditorGUILayout.EndHorizontal();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("Row Offset: ");
						RowStructure[i].RowShiftHorizontal = EditorGUILayout.FloatField(RowStructure[i].RowShiftHorizontal);
						EditorGUILayout.EndHorizontal();
						for(int ii = 0; ii < RowStructure[i].keyCount; ii++)
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Type: ", GUILayout.MinWidth( 25 ));
							if(ii >= RowStructure[i].types.Count)
								RowStructure[i].types.Add(KeyClass.String);
							RowStructure[i].types[ii] = (KeyClass)System.Enum.Parse(typeof(KeyClass), EditorGUILayout.EnumPopup(RowStructure[i].types[ii]).ToString());
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Upper: ", GUILayout.MinWidth( 25 ));
							if(ii >= RowStructure[i].upperValues.Count)
								RowStructure[i].upperValues.Add("A");
							RowStructure[i].upperValues[ii] = EditorGUILayout.TextField(RowStructure[i].upperValues[ii]);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Lower: ", GUILayout.MinWidth( 25 ));
							if(ii >= RowStructure[i].lowerValues.Count)
								RowStructure[i].lowerValues.Add("a");
							RowStructure[i].lowerValues[ii] = EditorGUILayout.TextField(RowStructure[i].lowerValues[ii]);
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.EndHorizontal();
						}
					}
				}
				
				if(GUILayout.Button("Template QWERTY"))
				{
					GenerateQWERTY();
				}

				if(GUILayout.Button("Template AZERTY"))
				{
					GenerateAZERTY();
				}
				
				if(GUILayout.Button("Generate Objects"))
				{
					GenerateObjects(subject);
				}
				EditorGUILayout.EndVertical();
			}
		}
	}
	
	private void GenerateObjects(OnScreenKeyboard subject)
	{
		//Before we start to loop and create lets create a root object so we can shift the keyboard keys as a set easily
		if(lastRootBuilt != null)
			GameObject.DestroyImmediate(lastRootBuilt);
		
		lastRootBuilt = new GameObject("Keys");
		//Now we apply this to our keyboard and center
		lastRootBuilt.transform.parent = subject.gameObject.transform;
		lastRootBuilt.transform.localPosition = Vector3.zero;
		
		List<OnScreenKeyboardKey[]> RowsBuilt = new List<OnScreenKeyboardKey[]>();
		int rowCounter = 0;
		foreach(OnScreenKeyboardRowBuilder builder in RowStructure)
		{
			OnScreenKeyboardKey[] nRow = GenerateKeys(rowCounter, lastRootBuilt, builder, subject.KeyTemplate);
			RowsBuilt.Add(nRow);
			rowCounter++;
		}
		
		//Once we have them all built we need to link the up and down navigations
		int rowIndex = 0;
		foreach(OnScreenKeyboardKey[] row in RowsBuilt)
		{
			int PreviousRow = rowIndex -1;
			int NextRow = rowIndex+1;
			
			if(NextRow >= RowsBuilt.Count)
				NextRow = 0;
			if(PreviousRow < 0)
				PreviousRow = RowsBuilt.Count-1;
			
			for(int i = 0; i < row.Length; i++)
			{
				//row upper link to previous row
				if(i < RowsBuilt[PreviousRow].Length)
					row[i].UpKey = RowsBuilt[PreviousRow][i];
				else
					row[i].UpKey = RowsBuilt[PreviousRow][RowsBuilt[PreviousRow].Length-1];
				
				//row lower link to next row
				if(i < RowsBuilt[NextRow].Length)
					row[i].DownKey = RowsBuilt[NextRow][i];
				else
					row[i].DownKey = RowsBuilt[NextRow][RowsBuilt[NextRow].Length-1];
			}

			rowIndex++;
		}
		//Finnaly we want to update the OnScreenKeyboard structure to render the results
		subject.UpdateStructure();
		
	}

	private void GenerateAZERTY()
	{
		RowStructure.Clear();
		RowsToBuild = 5;
		
		//Default a QWERTY in
		OnScreenKeyboardRowBuilder nBuilder1 = new OnScreenKeyboardRowBuilder();
		nBuilder1.keyCount = 14;
		nBuilder1.upperValues = new List<string>(new string[]{"~","!","@","#","$","%","^","&","*","(",")","_","+","◄"});
		nBuilder1.lowerValues = new List<string>(new string[]{"`","1","2","3","4","5","6","7","8","9","0","-","=","◄"});
		nBuilder1.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Backspace});
		
		OnScreenKeyboardRowBuilder nBuilder2 = new OnScreenKeyboardRowBuilder();
		nBuilder2.keyCount = 13;
		nBuilder2.RowShiftHorizontal = 0.55f;
		nBuilder2.upperValues = new List<string>(new string[]{"A","Z","E","R","T","Y","U","I","O","P","{","}","|"});
		nBuilder2.lowerValues = new List<string>(new string[]{"a","z","e","r","t","y","u","i","o","p","[","]","\\"});
		nBuilder2.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String});
		
		OnScreenKeyboardRowBuilder nBuilder3 = new OnScreenKeyboardRowBuilder();
		nBuilder3.keyCount = 12;
		nBuilder3.RowShiftHorizontal = 0.55f;
		nBuilder3.upperValues = new List<string>(new string[]{"Q","S","D","F","G","H","J","K","L","M","\"","←"});
		nBuilder3.lowerValues = new List<string>(new string[]{"q","s","d","f","g","h","j","k","l","m","'","←"});
		nBuilder3.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Return});
		
		OnScreenKeyboardRowBuilder nBuilder4 = new OnScreenKeyboardRowBuilder();
		nBuilder4.keyCount = 11;
		nBuilder4.RowShiftHorizontal = 1.66f;
		nBuilder4.upperValues = new List<string>(new string[]{"W","X","C","V","B","N",":","<",">","?","↓"});
		nBuilder4.lowerValues = new List<string>(new string[]{"w","x","c","v","b","n",";",",",".","/","↑"});
		nBuilder4.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Shift});
		
		OnScreenKeyboardRowBuilder nBuilder5 = new OnScreenKeyboardRowBuilder();
		nBuilder5.keyCount = 1;
		nBuilder5.RowShiftHorizontal = 6.6f;
		nBuilder5.upperValues = new List<string>(new string[]{" "});
		nBuilder5.lowerValues = new List<string>(new string[]{" "});
		nBuilder5.types = new List<KeyClass>(new KeyClass[] {KeyClass.String});
		
		RowStructure.Add(nBuilder1);
		RowStructure.Add(nBuilder2);
		RowStructure.Add(nBuilder3);
		RowStructure.Add(nBuilder4);
		RowStructure.Add(nBuilder5);
	}
	
	private void GenerateQWERTY()
	{
		RowStructure.Clear();
		RowsToBuild = 5;
		
		//Default a QWERTY in
		OnScreenKeyboardRowBuilder nBuilder1 = new OnScreenKeyboardRowBuilder();
		nBuilder1.keyCount = 14;
		nBuilder1.upperValues = new List<string>(new string[]{"~","!","@","#","$","%","^","&","*","(",")","_","+","◄"});
		nBuilder1.lowerValues = new List<string>(new string[]{"`","1","2","3","4","5","6","7","8","9","0","-","=","◄"});
		nBuilder1.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Backspace});
		
		OnScreenKeyboardRowBuilder nBuilder2 = new OnScreenKeyboardRowBuilder();
		nBuilder2.keyCount = 13;
		nBuilder2.RowShiftHorizontal = 0.55f;
		nBuilder2.upperValues = new List<string>(new string[]{"Q","W","E","R","T","Y","U","I","O","P","{","}","|"});
		nBuilder2.lowerValues = new List<string>(new string[]{"q","w","e","r","t","y","u","i","o","p","[","]","\\"});
		nBuilder2.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String});
		
		OnScreenKeyboardRowBuilder nBuilder3 = new OnScreenKeyboardRowBuilder();
		nBuilder3.keyCount = 12;
		nBuilder3.RowShiftHorizontal = 0.55f;
		nBuilder3.upperValues = new List<string>(new string[]{"A","S","D","F","G","H","J","K","L",":","\"","←"});
		nBuilder3.lowerValues = new List<string>(new string[]{"a","s","d","f","g","h","j","k","l",";","'","←"});
		nBuilder3.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Return});
		
		OnScreenKeyboardRowBuilder nBuilder4 = new OnScreenKeyboardRowBuilder();
		nBuilder4.keyCount = 11;
		nBuilder4.RowShiftHorizontal = 1.66f;
		nBuilder4.upperValues = new List<string>(new string[]{"Z","X","C","V","B","N","M","<",">","?","↓"});
		nBuilder4.lowerValues = new List<string>(new string[]{"z","x","c","v","b","n","m",",",".","/","↑"});
		nBuilder4.types = new List<KeyClass>(new KeyClass[] {KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.String,KeyClass.Shift});
		
		OnScreenKeyboardRowBuilder nBuilder5 = new OnScreenKeyboardRowBuilder();
		nBuilder5.keyCount = 1;
		nBuilder5.RowShiftHorizontal = 6.6f;
		nBuilder5.upperValues = new List<string>(new string[]{" "});
		nBuilder5.lowerValues = new List<string>(new string[]{" "});
		nBuilder5.types = new List<KeyClass>(new KeyClass[] {KeyClass.String});
		
		RowStructure.Add(nBuilder1);
		RowStructure.Add(nBuilder2);
		RowStructure.Add(nBuilder3);
		RowStructure.Add(nBuilder4);
		RowStructure.Add(nBuilder5);
	}
	
	private OnScreenKeyboardKey[] GenerateKeys(int row, GameObject keyRoot, OnScreenKeyboardRowBuilder builder, OnScreenKeyboardKey template)
	{
		OnScreenKeyboardKey[] results = new OnScreenKeyboardKey[builder.keyCount];
		//Loop through and generate the keys
		for(int i = 0; i < builder.keyCount; i++)
		{
			//Generate a new key object with an OnScreenKeyboardKey component
			GameObject key = GameObject.Instantiate(template.gameObject) as GameObject;
			key.name = "Row:" + row.ToString() + " Column:" + i.ToString();
			//Incase we have our template in our scene as disabled
			key.SetActive(true);
			//Get a pointer to the component
			OnScreenKeyboardKey screenKey = key.GetComponent<OnScreenKeyboardKey>();
			key.transform.parent = keyRoot.transform;
			//Now we apply the shift values note we are inverting the row so we shift down not up
			key.transform.localPosition = new Vector3((builder.KeyShiftHorizontal * i) + builder.RowShiftHorizontal, builder.KeyShiftHorizontal * -row, 0);
			//Set its values
			screenKey.UpperCaseValue = builder.upperValues[i];
			screenKey.LowerCaseValue = builder.lowerValues[i];
			screenKey.type = builder.types[i];
			
			//And we are going to start in lower so set the key value accordingly
			if(builder.upperValues[i].Trim() == "")
				screenKey.Text.text = "_";
			else
				screenKey.Text.text = builder.lowerValues[i];
			
			//While we are here lets setup our left navigation and our buddies right nav
			if(i > 0)
			{
				screenKey.LeftKey = results[i-1];
				results[i-1].RightKey = screenKey;
			}
			
			results[i] = screenKey;
		}
		//Our right nave for the last key isnt set yet so set it here
		results[results.Length-1].RightKey = results[0];
		results[0].LeftKey = results[results.Length-1];
		return results;
	}
}

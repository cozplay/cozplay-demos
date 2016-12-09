/*
 (C) 2015
 your R&D lab
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class VideoHelper:MonoBehaviour
{

	MovieTexture mt;
	RectTransform rt;
	Vector2 origPos;

	void Awake()
	{
		rt = GetComponent<RectTransform>();
		origPos = rt.anchoredPosition;


		RawImage rim = GetComponent<RawImage>();
		mt = (MovieTexture)rim.mainTexture;
	}

	void OnEnable(){
		mt.Play();
	}

	void OnDisable(){
		mt.Stop();
	}

	void Update()
	{

//		if (Input.GetMouseButtonDown(0))
//		{
//			if (mt.isPlaying)
//			{
//				mt.Stop();
//			}
//			else
//			{
//				mt.Stop();
//				mt.Play();
//			}
//		}



	}
}
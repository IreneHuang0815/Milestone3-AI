using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class textManager : MonoBehaviour {
	// Use this for initialization
	public static string state;

	Text text;
	void Start () {
		text = GetComponent<Text> ();
		state = "A";
	}
	
	// Update is called once per frame
	void Update () {
		text.text = "Currently in State: " + state;
	}
}

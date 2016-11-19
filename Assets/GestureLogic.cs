using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;
using System.Collections.Generic;

public class GestureLogic : MonoBehaviour {

    private List<Hand> gestures = new List<Hand>(); //List of gesture so far
    private Time holdPosition;                      //Time that certain position is held
    private static Hand holdHand;                        //Last state of hand before position hold

	void Start () {
        holdHand = 
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void CopyHand(Hand curr)
    {

    }
}

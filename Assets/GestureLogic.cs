using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;
using System.Collections.Generic;

public class GestureLogic : MonoBehaviour {

    private List<Hand> gestures = new List<Hand>();//List of gesture so far
    public List<Hand> correct = new List<Hand>(); //Correct passshake
    private float holdPositionTime;                      //Time that certain position is held
    private static Hand holdHand;   //Last state of hand before position hold
    public Hand endHand;

	void Start () {
        CopyHand(HandRepresentation.MostRecentHand);
        holdPositionTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void CopyHand(Hand curr)
    {
        holdHand = curr;
    }


    //compares inputted gesture sequence to current set PassShake. Returns 1 for success, 0 for failure.
    int checkPass()
    {
        if(gestures.Count - 1 == correct.Count) //including end gesture in sequence. checks to see if lengths are equal.
        {

        }
        return 0;
    }
}

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
        CopyHand(HandRepresentation.MostRecentHand);
	
	}
	
	// Update is called once per frame
	void Update () {
        if(DetectChange(HandModel._ha)
	
	}

    void CopyHand(Hand curr)
    {
        holdHand = curr;
    }

    bool DetectChange(Hand curr)
    {
        if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 5)
            return false;
        else if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 5)
            return false;
        else if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 5)
            return false;
        for(int i = 0; i < curr.Fingers.Count; i++)
        {
            if(curr.Fingers[i].StabilizedTipPosition.x - holdHand.Fingers[i].StabilizedTipPosition.x >= 5)
                return false;
            else if (curr.Fingers[i].StabilizedTipPosition.y - holdHand.Fingers[i].StabilizedTipPosition.y >= 5)
                return false;
            else if (curr.Fingers[i].StabilizedTipPosition.z - holdHand.Fingers[i].StabilizedTipPosition.z >= 5)
                return false;
        }
        return true;
    }
        
}

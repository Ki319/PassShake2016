using UnityEngine;
using System.Collections;
using Leap;
using Leap.Unity;
using System.Collections.Generic;

public class GestureLogic : MonoBehaviour
{

    private List<Hand> gestures = new List<Hand>();//List of gesture so far
    public List<Hand> correct = new List<Hand>(); //Correct passshake
    private float holdPositionTime;                      //Time that certain position is held
    private static Hand holdHand;   //Last state of hand before position hold
    public Hand endHand;
    private CapsuleHand hand;

    void Start()
    {
        CopyHand(hand.GetLeapHand());
        holdPositionTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (DetectChange(hand.GetLeapHand()))
        {
            if (Time.time - holdPositionTime >= 2500)
            {
                gestures.Add(holdHand);
            }
            holdPositionTime = Time.time;
            holdHand = hand.GetLeapHand();
        }
        if (gestures.Count - 1 == correct.Count)
            success = CheckPass();

    }

    void CopyHand(Hand curr)
    {
        holdHand = curr;
    }

    bool DetectChange(Hand curr)
    {
        if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 20)
            return false;
        else if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 20)
            return false;
        else if (curr.PalmPosition.x - holdHand.PalmPosition.x >= 20)
            return false;
        for (int i = 0; i < curr.Fingers.Count; i++)
        {
            if (curr.Fingers[i].StabilizedTipPosition.x - holdHand.Fingers[i].StabilizedTipPosition.x >= 20)
                return false;
            else if (curr.Fingers[i].StabilizedTipPosition.y - holdHand.Fingers[i].StabilizedTipPosition.y >= 20)
                return false;
            else if (curr.Fingers[i].StabilizedTipPosition.z - holdHand.Fingers[i].StabilizedTipPosition.z >= 20)
                return false;
        }
        return true;
    }

    //compares inputted gesture sequence to current set PassShake. Returns true for success, false for failure.
    bool CheckPass()
    {
        if (gestures.Count - 1 == correct.Count) //including end gesture in sequence. checks to see if lengths are equal.
        {
            for (int i = 0; i < correct.Count; i++)
            {
                for (int j = 0; j < gestures[i].Fingers.Count; j++)
                {
                    if ((gestures[i].Fingers[j].StabilizedTipPosition.x - gestures[i].PalmPosition.x) - (correct[i].Fingers[j].StabilizedTipPosition.x - correct[i].PalmPosition.x) >= 20)
                        return false;

                }
            }
        }
        return false;
    }
}

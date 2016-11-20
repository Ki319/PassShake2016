using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Leap.Unity;
using Leap;
using System;

public class GestureLogic : BaseInputModule {

    [Header(" Interaction Setup")]
    [Tooltip("The current Leap Data Provider for the scene.")]
    public LeapProvider LeapDataProvider;

    private List<float[][][]> gestures = new List<float[][][]> ();//List of gesture so far
    public List<float[][][]> correct = new List<float[][][]> ();  //Correct passshake
    private float holdPositionTime;                //Time that certain position is held
    private float[][][] startHand; //Last state of hand before position hold

    private Frame currentFrame;
    public float[][][] endHand;
    public int tolerance;                          //leeway in mm
    private bool success;
    private string path = "./password.txt";
    public bool passwordExists;
    private int mode;

    protected override void Start()
    {
        base.Start();

        if (LeapDataProvider == null)
        {
            LeapDataProvider = FindObjectOfType<LeapProvider>();
            if (LeapDataProvider == null || !LeapDataProvider.isActiveAndEnabled)
            {
                Debug.LogError("Cannot use LeapImageRetriever if there is no LeapProvider!");
                enabled = false;
                return;
            }
        }
        holdPositionTime = Time.time;
        tolerance = 20;
        success = false;
        mode = 0;
	}

    //Update the Head Yaw for Calculating "Shoulder Positions"s
    void Update()
    {
        currentFrame = LeapDataProvider.CurrentFrame;
    }

    public override void Process()
    {
        throw new NotImplementedException();
    }

    // Update is called once per frame
    void OnFixedFrame(Frame frame)
    {
        if (mode == 0)
        {
            DetectChange(hand.GetLeapHand());
            if (gestures.Count - 1 == correct.Count)
                success = CheckPass();
        }
        else if(mode == 1)
        {
            if(DetectChange(hand.GetLeapHand()))
            {
                if(Time.time - holdPositionTime >= 2500)
                {
                    writePassword(hand.GetLeapHand());
                    passwordExists = true;
                }
            }
        }
    }

    public bool getSuccess()
    {
        return success;
    }

    bool DetectChange(float[][][] hands)
    {
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (Mathf.Abs(hands[i][j][k] - holdHand.PalmPosition.x) >= tolerance)
                        return false;
                }
            }
        }
        if (Time.time - holdPositionTime >= 2500)
        {
            gestures.Add(hands);
        }
        holdPositionTime = Time.time;

        return true;
    }

    List <string> ToStringArray(Hand curr)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (Mathf.Abs(hands[i][j][k] - holdHand.PalmPosition.x) >= tolerance)
                        return false;
                }
            }
        }
        result.Add(curr.PalmPosition.x.ToString());
        result.Add(curr.PalmPosition.y.ToString());
        result.Add(curr.PalmPosition.z.ToString());
        for(int i = 0; i < curr.Fingers.Count; i++)
        {
            result.Add(curr.Fingers[i].StabilizedTipPosition.x.ToString());
            result.Add(curr.Fingers[i].StabilizedTipPosition.y.ToString());
            result.Add(curr.Fingers[i].StabilizedTipPosition.z.ToString());
        }
        return result;
    }

    //compares inputted gesture sequence to current set PassShake. Returns true for success, false for failure.
    bool CheckPass()
    {
            for(int i = 0; i < correct.Count; i++)
            {
                for(int j = 0; j < 2; j++) //for both hands
                {
                    for(int k = 1; k < 6; k++) //for the fingers
                    {
                        for(int l = 0; l < 3; l++) //for x, y, z
                        {
                           if (Mathf.Abs((gestures[i][j][k][l] - gestures[i][j][0][l]) - (correct[i][j][k][l] - correct[i][j][0][l])) >= tolerance)
                                return false;
                        }
                    }
                }
            }
            return true;
    }
}

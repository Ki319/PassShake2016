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
    private float startPositionTime;                //Time that certain position is held
    private float[][][] startHands; //Last state of hand before position hold

    private Frame currentFrame;
    public float[][][] endHands;
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
        startPositionTime = Time.time;
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
        List<Hand> handList = frame.Hands;

        //initialize hands position array
        float[][][] hands = new float[2][][];
        for(int i = 0; i < 2; i++) //0 is left, 1 is right hand
        {
            hands[i] = new float[6][];
            for(int j = 0; j < 6; j++) //0 is palm, 1-5 are fingers
            {
                hands[i][j] = new float[3]; //0, 1, 2 are x, y, z
            }
        }
        for(int i = 0; i < 2; i++)
        {
            for(int j = 0; j < 6; j++)
            {
                for(int k = 0; k < 3; k++)
                {
                    hands[i][j][k] = 0.0f;
                }
            }
        }

        if(handList.Count != 0)
        {
            //Left hand is 0th index, Right hand is 1st index
            if(handList.Count == 1)
            {
               if(handList[0].IsLeft) //Left Hand Only
               {
                    //Palm
                    hands[0][0][0] = handList[0].PalmPosition.x;
                    hands[0][0][1] = handList[0].PalmPosition.y;
                    hands[0][0][2] = handList[0].PalmPosition.z;
                    //fingers
                    for(int a = 1; a < 6; a++)
                    {
                        hands[0][a][0] = handList[0].Fingers[a - 1].TipPosition.x;
                        hands[0][a][1] = handList[0].Fingers[a - 1].TipPosition.y;
                        hands[0][a][2] = handList[0].Fingers[a - 1].TipPosition.z;
                    }
     
               } else { //Right Hand Only

                    //Palm
                    hands[1][0][0] = handList[0].PalmPosition.x;
                    hands[1][0][1] = handList[0].PalmPosition.y;
                    hands[1][0][2] = handList[0].PalmPosition.z;
                    //fingers
                    for (int a = 1; a < 6; a++)
                    {
                        hands[1][a][0] = handList[0].Fingers[a - 1].TipPosition.x;
                        hands[1][a][1] = handList[0].Fingers[a - 1].TipPosition.y;
                        hands[1][a][2] = handList[0].Fingers[a - 1].TipPosition.z;
                    }
                }
            } else if(handList.Count == 2) //Left and Right
            {
                int leftIndex;
                int rightIndex;
                if(handList[0].IsLeft)
                {
                    leftIndex = 0;
                    rightIndex = 1;
                } else
                {
                    leftIndex = 1;
                    rightIndex = 0;
                }

                //Palm
                hands[0][0][0] = handList[leftIndex].PalmPosition.x;
                hands[0][0][1] = handList[leftIndex].PalmPosition.y;
                hands[0][0][2] = handList[leftIndex].PalmPosition.z;
                //fingers
                for (int a = 1; a < 6; a++)
                {
                    hands[0][a][0] = handList[leftIndex].Fingers[a - 1].TipPosition.x;
                    hands[0][a][1] = handList[leftIndex].Fingers[a - 1].TipPosition.y;
                    hands[0][a][2] = handList[leftIndex].Fingers[a - 1].TipPosition.z;
                }

                //Palm
                hands[1][0][0] = handList[rightIndex].PalmPosition.x;
                hands[1][0][1] = handList[rightIndex].PalmPosition.y;
                hands[1][0][2] = handList[rightIndex].PalmPosition.z;
                //fingers
                for (int a = 1; a < 6; a++)
                {
                    hands[1][a][0] = handList[rightIndex].Fingers[a - 1].TipPosition.x;
                    hands[1][a][1] = handList[rightIndex].Fingers[a - 1].TipPosition.y;
                    hands[1][a][2] = handList[rightIndex].Fingers[a - 1].TipPosition.z;
                }
            }
            DetectChange(hands);
        }
        if (gestures.Count - 1 == correct.Count)
            success = CheckPass();
    }

    public bool getSuccess()
    {
        return success;
    }

    bool DetectChange(float[][][] hands)
    {
        for (int i = 0; i < 2; i++) //L/R hand
        {
            for (int j = 0; j < 6; j++) //digits + palm
            {
                for (int k = 0; k < 3; k++) //x, y, z
                {
                    if (Mathf.Abs(hands[i][j][k] - startHands[i][j][k]) <= tolerance) //checks to see if currenthand has moved more than tolerance mm
                        return false;
                }
            }
        }

        if (Time.time - startPositionTime >= 2000)
        {
            gestures.Add(hands);
        }
        startPositionTime = Time.time;

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

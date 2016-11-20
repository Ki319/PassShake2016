using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Leap.Unity;
using Leap;
using System;
using PassShake;

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
    private float[][][] finisher;
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
        correct = new PasswordData(path).getHandGesture();
        startPositionTime = Time.time;
        tolerance = 20;
        success = false;
        mode = 0;
        finisher = new float[2][][];
        for(int i = 0; i < 2; i++)
        {
            finisher[i] = new float[6][];
            for(int j = 0; j < 6; j++)
            {
                finisher[i][j] = new float[3];
                for(int k = 0; k < 3; k++)
                {
                    if (i == 0)
                        finisher[i][j][k] = 0.0f;
                }
            }
        }
        finisher[1][0][0] = 0.005362828f;
        finisher[1][0][1] = 0.7433285f;
        finisher[1][0][2] = -9.486539f;
        finisher[1][1][0] = -0.08700792f;
        finisher[1][1][1] = 0.7299139f;
        finisher[1][1][2] = -9.482934f;
        finisher[1][2][0] = -0.01756124f;
        finisher[1][2][1] = 0.7061447f;
        finisher[1][2][2] = -9.481342f;
        finisher[1][3][0] = -0.007958782f;
        finisher[1][3][1] = 0.7018657f;
        finisher[1][3][2] = -9.483487f;
        finisher[1][4][0] = 0.006049518f;
        finisher[1][4][1] = 0.7037531f;
        finisher[1][4][2] = -9.484939f;
        finisher[1][5][0] = 0.01607722f;
        finisher[1][5][1] = 0.7070413f;
        finisher[1][5][2] = -9.476624f;

        /*resetter = new float[2][][];
        for (int i = 0; i < 2; i++)
        {
            resetter[i] = new float[6][];
            for (int j = 0; j < 6; j++)
            {
                resetter[i][j] = new float[3];
                for (int k = 0; k < 3; k++)
                {
                    if (i == 0)
                        resetter[i][j][k] = 0.0f;
                }
            }
        }
        resetter[1][0][0] = 0.005362828f;
        resetter[1][0][1] = 0.7433285f;
        resetter[1][0][2] = -9.486539f;
        resetter[1][1][0] = -0.08700792f;
        resetter[1][1][1] = 0.7299139f;
        resetter[1][1][2] = -9.482934f;
        resetter[1][2][0] = -0.01756124f;
        resetter[1][2][1] = 0.7061447f;
        resetter[1][2][2] = -9.481342f;
        resetter[1][3][0] = -0.007958782f;
        resetter[1][3][1] = 0.7018657f;
        resetter[1][3][2] = -9.483487f;
        resetter[1][4][0] = 0.006049518f;
        resetter[1][4][1] = 0.7037531f;
        resetter[1][4][2] = -9.484939f;
        resetter[1][5][0] = 0.01607722f;
        resetter[1][5][1] = 0.7070413f;
        resetter[1][5][2] = -9.476624f;*/
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
        for (int i = 0; i < 2; i++) //0 is left, 1 is right hand
        {
            hands[i] = new float[6][];
            for (int j = 0; j < 6; j++) //0 is palm, 1-5 are fingers
            {
                hands[i][j] = new float[3]; //0, 1, 2 are x, y, z
            }
        }
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    hands[i][j][k] = 0.0f;
                }
            }
        }

        if (handList.Count != 0)
        {
            //Left hand is 0th index, Right hand is 1st index
            if (handList.Count == 1)
            {
                if (handList[0].IsLeft) //Left Hand Only
                {
                    //Palm
                    hands[0][0][0] = handList[0].PalmPosition.x;
                    hands[0][0][1] = handList[0].PalmPosition.y;
                    hands[0][0][2] = handList[0].PalmPosition.z;
                    //fingers
                    for (int a = 1; a < 6; a++)
                    {
                        hands[0][a][0] = handList[0].Fingers[a - 1].TipPosition.x;
                        hands[0][a][1] = handList[0].Fingers[a - 1].TipPosition.y;
                        hands[0][a][2] = handList[0].Fingers[a - 1].TipPosition.z;
                    }

                }
                else
                { //Right Hand Only

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
            }
            else if (handList.Count == 2) //Left and Right
            {
                int leftIndex;
                int rightIndex;
                if (handList[0].IsLeft)
                {
                    leftIndex = 0;
                    rightIndex = 1;
                }
                else
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
            if (mode == 0)
            {
                if (CheckFinisher(hands))
                {
                    CheckPass();
                    return;
                }
                if(DetectChange(hands))
                {
                    if (Time.time - startPositionTime >= 2000)
                    {
                        gestures.Add(hands);
                        //SpriteRenderer.Enabled = true;
                    }
                }
                startPositionTime = Time.time;
                startHands = hands;
            }
            else if (mode == 1)
            {
                if(CheckFinisher(hands))
                {
                    new PasswordData(path).write(path, correct);
                    mode = 0;
                    return;
                }
                if (DetectChange(hands))
                {
                    if (Time.time - startPositionTime >= 2000)
                    {
                        correct.Add(hands);
                        //SpriteRenderer.Enabled = true;
                    }
                }
                startPositionTime = Time.time;
                startHands = hands;
            }
        }
        
    }

    public bool getSuccess()
    {
        return success;
    }

    public void setModeTest()
    {
        mode = 0;
    }

    public void setModePass()
    {
        mode = 1;
        correct.Clear();
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

        return true;
    }

    bool SetGesturePassword(float[][][] hands)
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
        startHands = hands;

        return true;
    }

    List <string> ToStringArray(float[][][] hands)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    result.Add(hands[i][j][k].ToString());
                }
            }
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

    bool CheckFinisher(float[][][] check)
    {
        if (check[1][1][0] >= check[1][0][0])
            return false;
        for (int k = 1; k < 6; k++) //for the fingers
        {
            for (int l = 0; l < 3; l++) //for x, y, z
            {
                if (Mathf.Abs((check[1][k][l] - check[1][0][l]) - (finisher[1][k][l] - finisher[1][0][l])) >= tolerance)
                    return false;
            }
        }
        return true;
    }

    /*bool CheckResetter(float[][][] check)
    {
        if (check[1][1][0] <= check[1][0][0])
            return false;
        for (int k = 1; k < 6; k++) //for the fingers
        {
            for (int l = 0; l < 3; l++) //for x, y, z
            {
                if (Mathf.Abs((check[1][k][l] - check[1][0][l]) - (resetter[1][k][l] - resetter[1][0][l])) >= tolerance)
                    return false;
            }
        }
        return true;
    }*/
}

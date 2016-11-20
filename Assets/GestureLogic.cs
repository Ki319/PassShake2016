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
        correct = new PasswordData(path).getHandGesture();
        startPositionTime = Time.time;
        tolerance = 20;
        success = false;
        mode = 0;
        float[][][] finisher = new float[2][][];
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
         DetectChange(hand.GetLeapHand());
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
                    if (Mathf.Abs(hands[i][j][k] - startHand[i][j][k]) <= tolerance) //checks to see if currenthand has moved more than tolerance mm
                        return false;
                }
            }
        }

        if (Time.time - startPositionTime >= 2000)
        {
            gestures.Add(hands);
        }
        startPositionTime = Time.time;
        startHand = hands;

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
                    if (Mathf.Abs(hands[i][j][k] - startHand[i][j][k]) <= tolerance) //checks to see if currenthand has moved more than tolerance mm
                        return false;
                }
            }
        }

        if (Time.time - startPositionTime >= 2000)
        {
            gestures.Add(hands);
        }
        startPositionTime = Time.time;
        startHand = hands;

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
}

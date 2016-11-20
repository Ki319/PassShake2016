﻿using UnityEngine;
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

    private List<List<float[][][]> gestures = new List<List<float[]>[]> ();//List of gesture so far
    public List<float[][][]> correct = new List<List<float[]>[]> ();  //Correct passshake
    private float holdPositionTime;                //Time that certain position is held
    private float[][][] startHand; //Last state of hand before position hold

    private Frame currentFrame;
    public List<float[]>[] endHand;
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
        
        startHand[0] = new List<float[]>();
        startHand[1] = new List<float[]>();

        endHand[0] = new List<float[]>();
        endHand[1] = new List<float[]>();
        holdPositionTime = Time.time;
        tolerance = 20;
        loadPassword();
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
            if (DetectChange(hand.GetLeapHand()))
            {
                if (Time.time - holdPositionTime >= 2500)
                {
                    gestures.Add(startHand);
                }
                holdPositionTime = Time.time;
                startHand = hand.GetLeapHand();
            }
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

    private void loadPassword()
    {
        if (File.Exists(path))
        {
            passwordExists = true;
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                int i = 0;
                bool done = false;
                while(done == false)
                {
                    try
                    {
                        Hand h = new Hand();
                        //palm
                        h.PalmPosition.x = reader.ReadSingle();
                        h.PalmPosition.y = reader.ReadSingle();
                        h.PalmPosition.z = reader.ReadSingle();
                        for(int j = 0; j < 5; j++) //fingers
                        {
                            h.Fingers[j].StabilizedTipPosition.x = reader.ReadSingle();
                            h.Fingers[j].StabilizedTipPosition.y = reader.ReadSingle();
                            h.Fingers[j].StabilizedTipPosition.z = reader.ReadSingle();
                        }
                        correct.Add(h);

                    } catch (EndOfStreamException e)
                    {
                        done = true;
                    }
                }
            }
        }
        passwordExists = false;
    }

    private void writePassword(Hand curr)
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Append)))
        {
            List<string> coordinates = ToStringArray(curr);
            for (int i = 0; i < coordinates.Count; i++)
            {
                writer.Write(coordinates[i]);
                writer.Write(" ");
            }
        }
    }

    public bool getSuccess()
    {
        return success;
    }

    bool DetectChange(List<float[]>[] hands)
    {
        for (int i = 0; i < 2; i++)
        {
            List<float[]> currHand = hands[i];
            for (int j = 0; j < 3; j++)
            {
                if (Mathf.Abs(currHand[0][j] - holdHand.PalmPosition.x) >= tolerance)
                for (int k = 1; k < 6; k++)
                {
                    if (Mathf.Abs(currHand[k][j] - holdHand.Fingers[i].StabilizedTipPosition.x) >= tolerance)
                        return false;
                }
            }
        }
        return true;
    }

    List <string> ToStringArray(Hand curr)
    {
        List<string> result = new List<string>();
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
                for(int j = 0; j < gestures[i].Fingers.Count; j++)
                {
                    if (Mathf.Abs((gestures[i].Fingers[j].StabilizedTipPosition.x - gestures[i].PalmPosition.x) - (correct[i].Fingers[j].StabilizedTipPosition.x - correct[i].PalmPosition.x)) >= tolerance)
                        return false;
                    if (Mathf.Abs((gestures[i].Fingers[j].StabilizedTipPosition.y - gestures[i].PalmPosition.y) - (correct[i].Fingers[j].StabilizedTipPosition.y - correct[i].PalmPosition.y)) >= tolerance)
                        return false;
                    if (Mathf.Abs((gestures[i].Fingers[j].StabilizedTipPosition.z - gestures[i].PalmPosition.z) - (correct[i].Fingers[j].StabilizedTipPosition.z - correct[i].PalmPosition.z)) >= tolerance)
                        return false;
                }
            }
            return true;
    }
}

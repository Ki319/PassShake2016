using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Leap.Unity;
using Leap;
using System;
using System.IO;

namespace PassShake
{
    public class GestureDetection : BaseInputModule
    {
        private const string path = "./password.txt";
        private const string term = "./terminators.txt";

        [Header(" Interaction Setup")]
        [Tooltip("The current Leap Data Provider for the scene.")]
        public LeapProvider LeapDataProvider;

        private PasswordData data;
        private PasswordData terminators;

        private List<float[][][]> current = new List<float[][][]>();
        private List<float[][][]> termList = new List<float[][][]>();

        private float startPositionTime = 0;

        [SerializeField]
        private float tolerance = .6f;

        [SerializeField]
        private float timer = 1f;

        [SerializeField]
        private bool setMode = true;
        private bool findTerm = false;  //Serialize?

        [SerializeField]
        private CheckmarkSprite checkmark;
        private ResetSprite reset;

        [SerializeField]
        private LeapHandController controller;

        private Frame currentFrame;

        private float[][][] startPosition;
        private float[][][] sequenceTerminator;
        private float[][][] sequenceResetter;

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
            findTerm = false;
        }

        protected override void Awake()
        {
            base.Awake();

            data = new PasswordData(path);

            startPositionTime = Time.time;

            startPosition = genGesture();

            sequenceTerminator = genGesture();
            sequenceResetter = genGesture();

            sequenceTerminator[1][0][0] = 0.005362828f;
            sequenceTerminator[1][0][1] = 0.7433285f;
            sequenceTerminator[1][0][2] = -9.486539f;
            sequenceTerminator[1][1][0] = -0.08700792f;
            sequenceTerminator[1][1][1] = 0.7299139f;
            sequenceTerminator[1][1][2] = -9.482934f;
            sequenceTerminator[1][2][0] = -0.01756124f;
            sequenceTerminator[1][2][1] = 0.7061447f;
            sequenceTerminator[1][2][2] = -9.481342f;
            sequenceTerminator[1][3][0] = -0.007958782f;
            sequenceTerminator[1][3][1] = 0.7018657f;
            sequenceTerminator[1][3][2] = -9.483487f;
            sequenceTerminator[1][4][0] = 0.006049518f;
            sequenceTerminator[1][4][1] = 0.7037531f;
            sequenceTerminator[1][4][2] = -9.484939f;
            sequenceTerminator[1][5][0] = 0.01607722f;
            sequenceTerminator[1][5][1] = 0.7070413f;
            sequenceTerminator[1][5][2] = -9.476624f;

            normalize(sequenceTerminator);

            for (int i = 1; i < 6; i++)
            {
                sequenceResetter[0][i][0] = -sequenceTerminator[1][i][0];
                sequenceResetter[0][i][1] = sequenceTerminator[1][i][1];
                sequenceResetter[0][i][2] = sequenceTerminator[1][i][2];
            }
        }

        void Update()
        {
            currentFrame = LeapDataProvider.CurrentFrame;
        }

        public override void Process()
        {
            List<Hand> handList = currentFrame.Hands;

            float[][][] newHandPosition = genGesture();
            foreach (Hand h in handList)
            {
                int i = h.IsLeft ? 0 : 1;
                addCoord(newHandPosition[i], 0, h.PalmPosition);
                for (int j = 0; j < 5; j++)
                {
                    addCoord(newHandPosition[i], j + 1, h.Fingers[j].TipPosition);
                }
            }

            normalize(newHandPosition);

            if (handList.Count > 1)
            {
                foreach (Hand h in currentFrame.Hands)
                {
                    HandRepresentation rep = controller.getGraphics(h);
                    if (rep != null)
                    {
                        ((CapsuleHand)((HandProxy)rep).handModels[0]).ShiftPosition();
                    }
                }
            }
            else
            {
                foreach (Hand h in currentFrame.Hands)
                {
                    HandRepresentation rep = controller.getGraphics(h);
                    if (rep != null)
                    {
                        ((CapsuleHand)((HandProxy)rep).handModels[0]).NormalPosition();
                    }
                }
            }

            /*if (findTerm)    //In mode to set terminator gesture
            {
                if (DetectChange(newHandPosition))
                {
                    if (Time.time - startPositionTime >= timer && nonZero(newHandPosition))
                    {
                        termList.Add(newHandPosition);
                        if (termList.Count > 10)
                        {
                            terminators = new PasswordData(term);
                            terminators.write(term, termList);
                            sequenceTerminator = averageTerminators();
                            findTerm = false;
                        }
                    }
                    startPosition = newHandPosition;
                    startPositionTime = Time.time;
                    checkmark.hide();
                    foreach (Hand h in currentFrame.Hands)
                    {
                        HandRepresentation rep = controller.getGraphics(h);
                        if (rep != null)
                        {
                            ((CapsuleHand)((HandProxy)rep).handModels[0]).Normal();
                        }
                    }
                }
                else
                {
                    if (Time.time - startPositionTime >= timer)
                    {
                        checkmark.show();
                        foreach (Hand h in currentFrame.Hands)
                        {
                            HandRepresentation rep = controller.getGraphics(h);
                            if (rep != null)
                            {
                                ((CapsuleHand)((HandProxy)rep).handModels[0]).Green();
                            }
                        }
                    }
                }
                return;
            }*/

            if (setMode)
            {
                int i = CheckSequences(newHandPosition);
                DetectAndModify(newHandPosition, i);
                if (i == 2)
                {
                    data.write(path, current);
                    new Scene_Manager().LoadMainMenu();
                }
                else if (i == 3)
                {
                    current.Clear();
                    reset.hide();
                }
            }
            else
            {
                int i = CheckSequences(newHandPosition);
                DetectAndModify(newHandPosition, i);
                if (i == 2)
                {
                    if (compareData(data.getHandGesture()))
                    {
                        new Scene_Manager().LoadSuccess();
                    }
                    else
                    {
                        new Scene_Manager().LoadFail();
                    }
                }
                else if (i == 3)
                {
                    current.Clear();
                }
            }
        }

        private float[][][] genGesture()
        {
            float[][][] gesture = new float[2][][];
            for (int i = 0; i < 2; i++)
            {
                gesture[i] = new float[6][];
                for (int j = 0; j < 6; j++)
                {
                    gesture[i][j] = new float[3];
                    for (int k = 0; k < 3; k++)
                    {
                        gesture[i][j][k] = 0f;
                    }
                }
            }
            return gesture;
        }

        private void addCoord(float[][] f, int pos, Vector vec)
        {
            f[pos][0] = vec.x;
            f[pos][1] = vec.y;
            f[pos][2] = vec.z;
        }

        private void normalize(float[][][] f)
        {
            for (int i = 5; i >= 1; i--)
            {
                for (int j = 0; j < 3; j++)
                {
                    f[0][i][j] -= f[0][0][j];
                    f[1][i][j] -= f[1][0][j];
                }
            }
            for (int i = 0; i < 3; i++)
            {
                f[0][0][i] = 0;
                f[1][0][i] = 0;
            }
        }

        private bool nonZero(float[][][] handPosition)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        if (Mathf.Abs(handPosition[i][j][k]) >= .01f)
                            return true;
                    }
                }
            }

            return false;
        }

        private bool CheckTerminator(float[][][] handPosition)
        {
            return CheckPositions(sequenceTerminator, handPosition, 2);
        }

        private bool CheckResetter(float[][][] handPosition)
        {
            return CheckPositions(sequenceResetter, handPosition, 2);
        }

        private bool DetectChange(float[][][] handPosition)
        {
            return CheckPositions(startPosition, handPosition);
        }

        private bool CheckPositions(float[][][] firstPosition, float[][][] handPosition, float scale = 1)
        {
            float current = 0;
            float total = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        current = Mathf.Abs(handPosition[i][j][k] - firstPosition[i][j][k]);
                        total += current;
                        if (total >= tolerance * scale)
                            return true;
                        if (current >= (tolerance * scale) / 5f)
                            return true;
                    }
                }
            }

            return false;
        }

        private int CheckSequences(float[][][] newHandPosition)
        {
            if (!CheckTerminator(startPosition) && nonZero(startPosition))
            {
                if (Time.time - startPositionTime >= timer * 2)
                {
                    startPositionTime = Time.time;
                    return 2;
                }
                foreach (Hand h in currentFrame.Hands)
                {
                    HandRepresentation rep = controller.getGraphics(h);
                    if (rep != null)
                    {
                        ((CapsuleHand)((HandProxy)rep).handModels[0]).Normal();
                    }
                }
                return 1;
            }

            if (!CheckResetter(startPosition) && nonZero(startPosition))
            {
                if (DetectChange(newHandPosition))
                {
                    checkmark.hide();
                    foreach (Hand h in currentFrame.Hands)
                    {
                        HandRepresentation rep = controller.getGraphics(h);
                        if (rep != null)
                        {
                            ((CapsuleHand)((HandProxy)rep).handModels[0]).Normal();
                        }
                    }
                    if (Time.time - startPositionTime >= timer * 2)
                    {
                        //Debug.Log("CLEARED!" + Time.time);
                        reset.show();
                        startPositionTime = Time.time;
                        return 3;
                    }
                }
                else if(Time.time - startPositionTime >= timer * 2)
                {
                    checkmark.show();
                    foreach (Hand h in currentFrame.Hands)
                    {
                        HandRepresentation rep = controller.getGraphics(h);
                        if (rep != null)
                        {
                            ((CapsuleHand)((HandProxy)rep).handModels[0]).Green();
                        }
                    }
                }

                return 1;
            }
            return 0;
        }

        private void DetectAndModify(float[][][] newHandPosition, int i)
        {
            if (DetectChange(newHandPosition))
            {
                if (i == 0 && Time.time - startPositionTime >= timer && nonZero(startPosition))
                {
                    current.Add(newHandPosition);
                }
                startPosition = newHandPosition;
                startPositionTime = Time.time;
                if (i == 0)
                {
                    checkmark.hide();
                    foreach (Hand h in currentFrame.Hands)
                    {
                        HandRepresentation rep = controller.getGraphics(h);
                        if (rep != null)
                        {
                            ((CapsuleHand)((HandProxy)rep).handModels[0]).Normal();
                        }
                    }
                }
            }
            else
            {
                if (i == 0 && Time.time - startPositionTime >= timer && nonZero(startPosition))
                {
                    checkmark.show();
                    foreach (Hand h in currentFrame.Hands)
                    {
                        HandRepresentation rep = controller.getGraphics(h);
                        if (rep != null)
                        {
                            ((CapsuleHand)((HandProxy)rep).handModels[0]).Green();
                        }
                    }
                }
            }
        }

        private bool compareData(List<float[][][]> firstData)
        {
            if (firstData == null || firstData.Count != current.Count)
                return false;
            for (int l = 0; l < firstData.Count; l++)
            {
                float[][][] indexOne = firstData[l];
                float[][][] indexTwo = current[l];
                if (CheckPositions(indexOne, indexTwo))
                {
                    return false;
                }
            }
            return true;
        }
    }
}

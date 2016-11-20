using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using System.Collections.Generic;
using Leap.Unity;
using Leap;
using System;

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
        private bool findTerm = false;

        [SerializeField]
        private CheckmarkSprite checkmark;

        [SerializeField]
        private CapsuleHand leftHand;
        [SerializeField]
        private CapsuleHand rightHand;

        private Frame currentFrame;

        private float[][][] startPosition;
        private float[][][] endPosition;
        private float[][][] sequenceTerminator;

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

            data = new PasswordData(path);

            startPositionTime = Time.time;

            startPosition = genGesture();
            endPosition = genGesture();
            sequenceTerminator = genGesture();
            if(terminators.getHandGesture().Count != 0)
            {
                sequenceTerminator = averageTerminators();
            }

            /*sequenceTerminator[1][0][0] = 0.005362828f;
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

            normalize(sequenceTerminator);*/
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

            if(findTerm)
            {
                termList.Add(newHandPosition);
                if (termList.Count > 10)
                {
                    terminators = new PasswordData(term);
                    terminators.write(term, termList);
                    sequenceTerminator = averageTerminators();
                }
            }

            if (setMode)
            {
                //Debug.Log("ENTERED " + (Time.time - startPositionTime) + " " + CheckTerminator(newHandPosition) + " " + DetectChange(newHandPosition));
                if (!CheckTerminator(newHandPosition) && Time.time - startPositionTime >= timer)
                {
                    data.write(path, current);
                    new Scene_Manager().LoadMainMenu();
                    return;
                }

                if (DetectChange(newHandPosition))
                {
                    if (Time.time - startPositionTime >= timer)
                    {
                        current.Add(newHandPosition);
                    }
                    startPosition = newHandPosition;
                    startPositionTime = Time.time;
                    checkmark.hide();
                }
                else
                {
                    if (Time.time - startPositionTime >= timer)
                    {
                        checkmark.show();
                    }
                }
            }
        }

        public float[][][] averageTerminators()
        {
            terminators.loadPassword(term);
            termList = terminators.getHandGesture();
            float[][][] result = genGesture();
            float avgx = 0.0f, avgy = 0.0f, avgz = 0.0f;
            for (int t = 0; t < termList.Count; t++)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            avgx += termList[t][i][j][k];
                            avgy += termList[t][i][j][k];
                            avgz += termList[t][i][j][k];
                        }
                        avgx /= termList.Count;
                        avgy /= termList.Count;
                        avgz /= termList.Count;
                        result[i][j][0] = avgx;
                        result[i][j][1] = avgy;
                        result[i][j][2] = avgz;
                    }
                }
            }
            normalize(result);
            return result;
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
                    float f1 = f[0][i][j];
                    float f2 = f[0][0][j];
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

        private bool CheckTerminator(float[][][] handPosition)
        {
            return CheckPositions(sequenceTerminator, handPosition);
        }

        private bool DetectChange(float[][][] handPosition)
        {
            return CheckPositions(startPosition, handPosition);
        }

        private bool CheckPositions(float[][][] firstPosition, float[][][] handPosition)
        {
            float total = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 1; j < 6; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        total += Mathf.Abs(handPosition[i][j][k] - firstPosition[i][j][k]);
                        if (total >= tolerance)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}

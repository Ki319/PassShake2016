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

        [Header(" Interaction Setup")]
        [Tooltip("The current Leap Data Provider for the scene.")]
        public LeapProvider LeapDataProvider;

        private PasswordData data;

        private List<float[][][]> current = new List<float[][][]>();

        private float startPositionTime = 0;

        [SerializeField]
        private int tolerance = 20;

        private Frame currentFrame;

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

        }

        void Update()
        {
            currentFrame = LeapDataProvider.CurrentFrame;
        }

        public override void Process()
        {

        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;
using Leap.Unity;

namespace PassShake
{
    public class GestureHand : CapsuleHand
    {
        public override void UpdateHand()
        {
            base.UpdateHand();

            jointMat.color = new Color(1f, 1f, 1f, 1f);

            /*for (int i = 0; i < _cylinderTransforms.Count; i++)
            {
                Transform cylinder = _cylinderTransforms[i];
                cylinder
            }*/
        }
    }
}

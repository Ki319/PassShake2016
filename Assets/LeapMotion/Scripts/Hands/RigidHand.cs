/******************************************************************************\
* Copyright (C) Leap Motion, Inc. 2011-2016.                                   *
* Leap Motion proprietary. Licensed under Apache 2.0                           *
* Available at http://www.apache.org/licenses/LICENSE-2.0.html                 *
\******************************************************************************/

using UnityEngine;
using System.Collections;
using Leap;

namespace Leap.Unity
{
    /** A physics model for our rigid hand made out of various Unity Collider. */
    public class RigidHand : SkeletalHand
    {
        public override ModelType HandModelType
        {
            get
            {
                return ModelType.Physics;
            }
        }
        public float filtering = 0.5f;

        public override bool SupportsEditorPersistence()
        {
            return true;
        }

        public override void InitHand()
        {
            base.InitHand();
        }

        public override void UpdateHand()
        {
            Rigidbody palmBody;
            for (int f = 0; f < fingers.Length; ++f)
            {
                if (fingers[f] != null)
                {
                    fingers[f].UpdateFinger();
                }
            }

            if (palm != null)
            {
                palmBody = palm.GetComponent<Rigidbody>();
                if (palmBody)
                {
                    palmBody.MovePosition(GetPalmCenter());
                    palmBody.MoveRotation(GetPalmRotation());
                }
                else {
                    palm.position = GetPalmCenter();
                    palm.rotation = GetPalmRotation();
                }
            }

            GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
            int i = 0;
            for (i = 0; i < gameObjects.Length && !gameObjects[i].ToString().StartsWith("LeapHandController"); i++) ;

            float differenceX = GetPalmCenter().x - gameObjects[i].transform.position.x;
            float differenceY = GetPalmCenter().y - gameObjects[i].transform.position.y;
            float differenceZ = GetPalmCenter().z - gameObjects[i].transform.position.z;

            Vector3 vec = new Vector3(0, 0, 0);

            vec.x = differenceX * 2;
            vec.y = differenceY * 2;
            vec.z = differenceZ * 2;

            palmBody = palm.GetComponent<Rigidbody>();
            if(palmBody)
            {
                palmBody.MovePosition(palm.position + vec);
            }
            else
            {
                palm.Translate(vec);
            }
            for (int f = 0; f < fingers.Length; ++f)
            {
                if (fingers[f] != null)
                {
                    FingerModel finger = fingers[f];
                    for (int b = 0; b < finger.bones.Length; ++b)
                    {
                        Transform bone = finger.bones[b];
                        if(bone)
                        {
                            Rigidbody boneBody = bone.GetComponent<Rigidbody>();
                            if (boneBody)
                            {
                                boneBody.MovePosition(vec + bone.position);
                            }
                            else
                            {
                                bone.Translate(vec);
                            }
                        }
                    }
                }
            }
        }
    }
}

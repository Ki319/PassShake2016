using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap;
using System;

namespace Leap.Unity
{
    /** A basic Leap hand model constructed dynamically vs. using pre-existing geometry*/
    public class CapsuleHand : IHandModel
    {

        private const int THUMB_BASE_INDEX = (int)Finger.FingerType.TYPE_THUMB * 4;
        private const int PINKY_BASE_INDEX = (int)Finger.FingerType.TYPE_PINKY * 4;

        protected const float SPHERE_RADIUS = 0.005f;
        private const float CYLINDER_RADIUS = 0.003f;
        protected const float PALM_RADIUS = 0.015f;

        private static Color[] _leftColorList = { new Color(0.0f, 0.0f, 1.0f) };
        private static Color[] _rightColorList = { new Color(1.0f, 0.0f, 0.0f) };

        private float shiftX = 0;

        [SerializeField]
        private Chirality handedness;

        [SerializeField]
        private Material _material;

        [SerializeField]
        protected Mesh _sphereMesh;

        [SerializeField]
        private int _cylinderResolution = 12;

        private bool _hasGeneratedMeshes = false;
        protected Material jointMat;

        [SerializeField, HideInInspector]
        protected List<Transform> _serializedTransforms;

        private Transform[] _jointSpheres;
        private Transform mockThumbJointSphere;
        private Transform palmPositionSphere;

        private Transform wristPositionSphere;

        private List<Renderer> _armRenderers;
        protected List<Transform> _cylinderTransforms;
        protected List<Transform> _sphereATransforms;
        protected List<Transform> _sphereBTransforms;

        private Transform armFrontLeft, armFrontRight, armBackLeft, armBackRight;
        private Hand hand_;

        public override ModelType HandModelType
        {
            get
            {
                return ModelType.Graphics;
            }
        }

        public override Chirality Handedness
        {
            get
            {
                return handedness;
            }
            set { }
        }

        public override bool SupportsEditorPersistence()
        {
            return true;
        }

        public override Hand GetLeapHand()
        {
            return hand_;
        }

        public override void SetLeapHand(Hand hand)
        {
            hand_ = hand;
        }

        void OnValidate()
        {
            //Resolution must be at least 3!
            _cylinderResolution = Mathf.Max(3, _cylinderResolution);

            //Update visibility on validate so that the user can toggle in real-time
            if (_armRenderers != null)
            {
                updateArmVisibility();
            }
        }

        public override void InitHand()
        {
            if (_material != null)
            {
                jointMat = new Material(_material);
                jointMat.hideFlags = HideFlags.DontSaveInEditor;
            }

            if (_serializedTransforms != null)
            {
                for (int i = 0; i < _serializedTransforms.Count; i++)
                {
                    var obj = _serializedTransforms[i];
                    if (obj != null)
                    {
                        DestroyImmediate(obj.gameObject);
                    }
                }
                _serializedTransforms.Clear();
            }
            else {
                _serializedTransforms = new List<Transform>();
            }

            _jointSpheres = new Transform[4 * 5];
            _armRenderers = new List<Renderer>();
            _cylinderTransforms = new List<Transform>();
            _sphereATransforms = new List<Transform>();
            _sphereBTransforms = new List<Transform>();

            createSpheres();
            createCylinders();

            updateArmVisibility();

            _hasGeneratedMeshes = false;
        }

        public override void BeginHand()
        {
            base.BeginHand();

            if (hand_.IsLeft)
            {
                jointMat.color = _leftColorList[0];
            }
            else {
                jointMat.color = _rightColorList[0];
            }
            shiftX = 0;
        }

        public override void UpdateHand()
        {
            //Update the spheres first
            updateSpheres();
            GameObject[] gameObjects = gameObject.scene.GetRootGameObjects();
            int i = 0;
            for (i = 0; i < gameObjects.Length && !gameObjects[i].ToString().StartsWith("LeapHandController"); i++) ;
            
            HandPool handpool = gameObjects[i].GetComponent<HandPool>();

            float differenceX = palmPositionSphere.position.x - gameObjects[i].transform.position.x + shiftX;
            float differenceY = palmPositionSphere.position.y - gameObjects[i].transform.position.y;
            float differenceZ = palmPositionSphere.position.z - gameObjects[i].transform.position.z;

            Vector3 vec = new Vector3(0, 0, 0);

            if (handpool.lockX)
                vec.x = -differenceX;
            else
                vec.x = differenceX * 2;

            if (handpool.lockY)
                vec.y = -differenceY;
            else
                vec.y = differenceY * 2;

            if (handpool.lockZ)
                vec.z = -differenceZ;
            else
                vec.z = differenceZ * 2;

            for (i = 0; i < _cylinderTransforms.Count; i++)
            {
                Transform cylinder = _cylinderTransforms[i];

                cylinder.Translate(vec);
            }

            for (i = 0; i < _serializedTransforms.Count; i++)
            {
                Transform sphere = _serializedTransforms[i];

                sphere.Translate(vec);
            }


            updateCylinders();
        }

        //Transform updating methods

        private void updateSpheres()
        {
            //Update all spheres
            palmPositionSphere.position = hand_.PalmPosition.ToVector3();
            //transform.position.Set(transform.position.x, transform.position.y, 100);
            // Debug.LogError(differenceZ);
            // palmPositionSphere.position.Set(palmPositionSphere.position.x, palmPositionSphere.position.y, -9.5f);

            List<Finger> fingers = hand_.Fingers;
            for (int i = 0; i < fingers.Count; i++)
            {
                Finger finger = fingers[i];
                for (int j = 0; j < 4; j++)
                {
                    int key = getFingerJointIndex((int)finger.Type, j);
                    Transform sphere = _jointSpheres[key];
                    sphere.position = finger.Bone((Bone.BoneType)j).NextJoint.ToVector3();
                    // sphere.position.Set(sphere.position.x, sphere.position.y, sphere.position.z + differenceZ);
                }
            }

            Vector3 wristPos = hand_.PalmPosition.ToVector3();
            wristPositionSphere.position = wristPos;
            // wristPositionSphere.position.Set(wristPositionSphere.position.x, wristPositionSphere.position.y, wristPositionSphere.position.z);

            Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];

            Vector3 thumbBaseToPalm = thumbBase.position - hand_.PalmPosition.ToVector3();
            //thumbBaseToPalm.z += differenceZ;
            mockThumbJointSphere.position = hand_.PalmPosition.ToVector3() + Vector3.Reflect(thumbBaseToPalm, hand_.Basis.xBasis.ToVector3());
            // mockThumbJointSphere.position.Set(mockThumbJointSphere.position.x, mockThumbJointSphere.position.y, mockThumbJointSphere.position.z + differenceZ);
        }

        public void Green()
        {
            jointMat.color = new Color(0, 1, 0);
        }

        public void Normal()
        {
            if (hand_.IsLeft)
            {
                jointMat.color = _leftColorList[0];
            }
            else {
                jointMat.color = _rightColorList[0];
            }
        }

        public void ShiftPosition()
        {
            if(hand_.IsLeft)
            {
                shiftX = .25f;
            }
            else
            {
                shiftX = -.25f;
            }
        }

        public void NormalPosition()
        {
            shiftX = 0;
        }

        private void updateArm()
        {
            var arm = hand_.Arm;
            Vector3 right = arm.Basis.xBasis.ToVector3() * arm.Width * 0.7f * 0.5f;
            Vector3 wrist = arm.WristPosition.ToVector3();
            Vector3 elbow = arm.ElbowPosition.ToVector3();

            float armLength = Vector3.Distance(wrist, elbow);
            wrist -= arm.Direction.ToVector3() * armLength * 0.05f;

            armFrontRight.position = wrist + right;
            armFrontLeft.position = wrist - right;
            armBackRight.position = elbow + right;
            armBackLeft.position = elbow - right;
        }

        private void updateCylinders()
        {
            for (int i = 0; i < _cylinderTransforms.Count; i++)
            {
                Transform cylinder = _cylinderTransforms[i];
                Transform sphereA = _sphereATransforms[i];
                Transform sphereB = _sphereBTransforms[i];

                Vector3 delta = sphereA.position - sphereB.position;

                if (!_hasGeneratedMeshes)
                {
                    MeshFilter filter = cylinder.GetComponent<MeshFilter>();
                    filter.sharedMesh = generateCylinderMesh(delta.magnitude / transform.lossyScale.x);
                }

                cylinder.position = sphereA.position;

                if (delta.sqrMagnitude <= Mathf.Epsilon)
                {
                    //Two spheres are at the same location, no rotation will be found
                    continue;
                }

                cylinder.LookAt(sphereB);
            }

            _hasGeneratedMeshes = true;
        }

        private void updateArmVisibility()
        {
            for (int i = 0; i < _armRenderers.Count; i++)
            {
                _armRenderers[i].enabled = false;
            }
        }

        //Geometry creation methods

        private void createSpheres()
        {
            //Create spheres for finger joints
            List<Finger> fingers = hand_.Fingers;
            for (int i = 0; i < fingers.Count; i++)
            {
                Finger finger = fingers[i];
                for (int j = 0; j < 4; j++)
                {
                    int key = getFingerJointIndex((int)finger.Type, j);
                    _jointSpheres[key] = createSphere("Joint", SPHERE_RADIUS);
                }
            }

            mockThumbJointSphere = createSphere("MockJoint", SPHERE_RADIUS);
            palmPositionSphere = createSphere("PalmPosition", PALM_RADIUS);
            wristPositionSphere = createSphere("WristPosition", SPHERE_RADIUS);

            armFrontLeft = createSphere("ArmFrontLeft", SPHERE_RADIUS, true);
            armFrontRight = createSphere("ArmFrontRight", SPHERE_RADIUS, true);
            armBackLeft = createSphere("ArmBackLeft", SPHERE_RADIUS, true);
            armBackRight = createSphere("ArmBackRight", SPHERE_RADIUS, true);
        }

        private void createCylinders()
        {
            //Create cylinders between finger joints
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    int keyA = getFingerJointIndex(i, j);
                    int keyB = getFingerJointIndex(i, j + 1);

                    Transform sphereA = _jointSpheres[keyA];
                    Transform sphereB = _jointSpheres[keyB];

                    createCylinder("Finger Joint", sphereA, sphereB);
                }
            }

            //Create cylinders between finger knuckles
            for (int i = 0; i < 4; i++)
            {
                int keyA = getFingerJointIndex(i, 0);
                int keyB = getFingerJointIndex(i + 1, 0);

                Transform sphereA = _jointSpheres[keyA];
                Transform sphereB = _jointSpheres[keyB];

                createCylinder("Hand Joints", sphereA, sphereB);
            }

            //Create the rest of the hand
            Transform thumbBase = _jointSpheres[THUMB_BASE_INDEX];
            Transform pinkyBase = _jointSpheres[PINKY_BASE_INDEX];
            createCylinder("Hand Bottom", thumbBase, mockThumbJointSphere);
            createCylinder("Hand Side", pinkyBase, mockThumbJointSphere);

            createCylinder("ArmFront", armFrontLeft, armFrontRight, true);
            createCylinder("ArmBack", armBackLeft, armBackRight, true);
            createCylinder("ArmLeft", armFrontLeft, armBackLeft, true);
            createCylinder("ArmRight", armFrontRight, armBackRight, true);
        }

        private int getFingerJointIndex(int fingerIndex, int jointIndex)
        {
            return fingerIndex * 4 + jointIndex;
        }

        protected Transform createSphere(string name, float radius, bool isPartOfArm = false)
        {
            GameObject sphere = new GameObject(name);
            _serializedTransforms.Add(sphere.transform);

            sphere.AddComponent<MeshFilter>().mesh = _sphereMesh;
            sphere.AddComponent<MeshRenderer>().sharedMaterial = jointMat;
            sphere.transform.parent = transform;
            sphere.transform.localScale = Vector3.one * radius * 2;

            sphere.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            sphere.layer = gameObject.layer;

            if (isPartOfArm)
            {
                _armRenderers.Add(sphere.GetComponent<Renderer>());
            }

            return sphere.transform;
        }

        private void createCylinder(string name, Transform jointA, Transform jointB, bool isPartOfArm = false)
        {
            GameObject cylinder = new GameObject(name);
            _serializedTransforms.Add(cylinder.transform);

            cylinder.AddComponent<MeshFilter>();
            cylinder.AddComponent<MeshRenderer>().sharedMaterial = _material;
            cylinder.transform.parent = transform;

            _cylinderTransforms.Add(cylinder.transform);
            _sphereATransforms.Add(jointA);
            _sphereBTransforms.Add(jointB);

            cylinder.gameObject.layer = gameObject.layer;
            cylinder.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy | HideFlags.HideInInspector;

            if (isPartOfArm)
            {
                _armRenderers.Add(cylinder.GetComponent<Renderer>());
            }
        }

        private Mesh generateCylinderMesh(float length)
        {
            Mesh mesh = new Mesh();
            mesh.name = "GeneratedCylinder";
            mesh.hideFlags = HideFlags.DontSave;

            List<Vector3> verts = new List<Vector3>();
            List<Color> colors = new List<Color>();
            List<int> tris = new List<int>();

            Vector3 p0 = Vector3.zero;
            Vector3 p1 = Vector3.forward * length;
            for (int i = 0; i < _cylinderResolution; i++)
            {
                float angle = (Mathf.PI * 2.0f * i) / _cylinderResolution;
                float dx = CYLINDER_RADIUS * Mathf.Cos(angle);
                float dy = CYLINDER_RADIUS * Mathf.Sin(angle);

                Vector3 spoke = new Vector3(dx, dy, 0);

                verts.Add((p0 + spoke) * transform.lossyScale.x);
                verts.Add((p1 + spoke) * transform.lossyScale.x);

                colors.Add(Color.white);
                colors.Add(Color.white);

                int triStart = verts.Count;
                int triCap = _cylinderResolution * 2;

                tris.Add((triStart + 0) % triCap);
                tris.Add((triStart + 2) % triCap);
                tris.Add((triStart + 1) % triCap);

                tris.Add((triStart + 2) % triCap);
                tris.Add((triStart + 3) % triCap);
                tris.Add((triStart + 1) % triCap);
            }

            mesh.SetVertices(verts);
            mesh.SetIndices(tris.ToArray(), MeshTopology.Triangles, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();
            mesh.UploadMeshData(true);

            return mesh;
        }
    }
}

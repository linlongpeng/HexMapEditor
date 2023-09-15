using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace
{
    public class HexMapData : ScriptableObject
    {
        public int maxWidth;
        public int maxHeight;
        public Vector3 basePos;
        public Quaternion baseRot;
        public List<HexMapChild> hexChildList = new List<HexMapChild>();
    }

    [Serializable]
    public class HexMapChild
    {
        public int MapType = 1;
        public int X;
        public int Z;
        
        public Quaternion rot;
        public string name;

        [NonSerialized]
        public Transform Transform;

        public HexMapChild(int x, int z, Quaternion rot)
        {
            this.X = x;
            this.Z = z;
            this.rot = rot;
        }
    }
}

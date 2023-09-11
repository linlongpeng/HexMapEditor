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
        public enum MapTypeFlag
        {
            type1,
            type2,
            type3,
        }
        public int X;
        public int Z;
        public MapTypeFlag MapType = MapTypeFlag.type1;
        
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

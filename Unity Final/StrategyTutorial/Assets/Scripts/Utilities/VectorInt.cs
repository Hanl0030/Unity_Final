using UnityEngine;
using System;

namespace OnLooker
{
    [Serializable]
    public struct Vector2Int
    {

        public Vector2Int(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }
        [SerializeField]
        private int m_X;
        [SerializeField]
        private int m_Y;

        public int x
        {
            get { return m_X; }
            set { m_X = value; }
        }
        public int y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        public static Vector2Int zero
        {
            get { return new Vector2Int(0, 0); }
        }
    }

    [Serializable]
    public struct Vector3Int
    {
        public Vector3Int(int x, int y,int z)
        {
            m_X = x;
            m_Y = y;
            m_Z = z;
        }
        [SerializeField]
        private int m_X;
        [SerializeField]
        private int m_Y;
        [SerializeField]
        private int m_Z;

        public int x
        {
            get { return m_X; }
            set { m_X = value; }
        }
        public int y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }
        public int z
        {
            get { return m_Z; }
            set { m_Z = value; }
        }
        public static Vector3Int zero
        {
            get { return new Vector3Int(0,0, 0); }
        }
    }

    public enum EShiftDirection
    {
        EAST,
        NORTH_EAST,
        NORTH,
        NORTH_WEST,
        WEST,
        SOUTH_WEST,
        SOUTH,
        SOUTH_EAST
    }

    public class VectorUtils
    {
        public static void Vector2IntShift(ref int x, ref int y, EShiftDirection aDirection, int aAmount)
        {
            switch (aDirection)
            {
                case EShiftDirection.EAST:
                    x += aAmount;
                    break;
                case EShiftDirection.NORTH_EAST:
                    x += aAmount;
                    y += aAmount;
                    break;
                case EShiftDirection.NORTH:
                    y += aAmount;
                    break;
                case EShiftDirection.NORTH_WEST:
                    y += aAmount;
                    x -= aAmount;
                    break;
                case EShiftDirection.WEST:
                    x -= aAmount;
                    break;
                case EShiftDirection.SOUTH_WEST:
                    x -= aAmount;
                    y -= aAmount;
                    break;
                case EShiftDirection.SOUTH:
                    y -= aAmount;
                    break;
                case EShiftDirection.SOUTH_EAST:
                    x += aAmount;
                    y -= aAmount;
                    break;
            }


        }
    }
    

}
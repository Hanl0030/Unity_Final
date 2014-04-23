using UnityEngine;
using System.Collections;
using System;
namespace OnLooker
{
    [Flags()]
    public enum PathState
    {
        INVALID = 0,
        WALKABLE = 1,
        PLACEABLE = 2,
        GROUND = 4,
        AIR = 8
    }
    public enum ListState
    {
        STATE_RAW,
        STATE_OPEN,
        STATE_CLOSED
    }
    [Serializable]
    public class PathNodeDebugData
    {
        public PathNodeDebugData()
        {
        }
        public PathNodeDebugData(Terrain aTerrain, float aYOffset, int aMapIndex, Vector2Int aMapCoords, Vector2 aNodeDistance)
        {
            m_Terrain = aTerrain;
            m_YOffset = aYOffset;
            m_MapIndex = aMapIndex;
            m_MapCoords = aMapCoords;
            m_NodeDistance = aNodeDistance;
        }

        private Terrain m_Terrain;
        private float m_YOffset;

        //Location in the map
        private int m_MapIndex;
        private Vector2Int m_MapCoords;
        private Vector2 m_NodeDistance;
        

        public Terrain terrain
        {
            get { return m_Terrain; }
            set { m_Terrain = value; }
        }
        public float yOffset
        {
            get { return m_YOffset; }
            set { m_YOffset = value; }
        }
        public int mapIndex
        {
            get { return m_MapIndex; }
            set { m_MapIndex = value; }
        }
        public Vector2Int mapCoords
        {
            get { return m_MapCoords; }
            set { m_MapCoords = value; }
        }
        public Vector2 nodeDistance
        {
            get { return m_NodeDistance; }
            set { m_NodeDistance = value; }
        }
    }

    [Serializable]
    public class PathNode
    {
        
        private PathNode m_ParentNode;

        private int m_Heuristic;
        private int m_MovementCost;
        private int m_TotalCost;
        private int m_Weight;
        private Vector2Int m_NodePosition; //Position for the node in the pathmap
        private Vector3 m_Position; //Position for the node in the world space
        private PathNode m_Target;
        private PathState m_PathState;
        private ListState m_ListState;

        [SerializeField]
        private PathNodeDebugData m_DebugData = null;

        public PathNode parent
        {
            get { return m_ParentNode; }
        }
        public int hScore
        {
            get { return m_Heuristic; }
        }
        public int gScore
        {
            get { return m_MovementCost; }
        }
        public int fScore
        {
            get { return m_TotalCost; }
        }
        public int weight
        {
            get { return m_Weight; }
            set { m_Weight = value; }
        }
        public bool placeable
        {
            get { return (m_PathState & PathState.PLACEABLE) == PathState.PLACEABLE ? true : false; }
        }
        public bool walkable
        {
            get { return (m_PathState & PathState.WALKABLE) == PathState.WALKABLE ? true : false; }
        }
        public bool ground
        {
            get { return ((m_PathState & PathState.WALKABLE) == PathState.WALKABLE && (m_PathState & PathState.GROUND) == PathState.GROUND) ? true : false; }
        }
        public bool air
        {
            get { return ((m_PathState & PathState.WALKABLE) == PathState.WALKABLE && (m_PathState & PathState.AIR) == PathState.GROUND) ? true : false; }
        }
        public bool invalid
        {
            get { return m_PathState == PathState.INVALID ? true : false; }
        }
        public ListState state
        {
            get{return m_ListState;}
            set { m_ListState = value; }
        }
        public Vector2Int nodePosition
        {
            get { return m_NodePosition; }
            set { m_NodePosition = value; }
        }
        public Vector3 position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        public PathNode target
        {
            get { return m_Target; }
        }
        public Vector3 targetPosition
        {
            get { if (m_Target != null) { return m_Target.position; } else { return Vector3.zero; } }
        }



        public PathNode()
        {
            m_Heuristic = 0;
            m_MovementCost = 0;
            m_TotalCost = 0;
            m_Weight = 10;
            m_Position = Vector3.zero;
            setPathState(true, true, true, true);
            m_ListState = ListState.STATE_RAW;
        }
        //Setup the Path state based on booleans
        public void setPathState(bool aGround, bool aAir, bool aPlaceable, bool aWalkable)
        {
            PathState pathstate = PathState.INVALID;
            if (aGround == true)
            {
                pathstate |= PathState.GROUND;
            }
            if (aAir == true)
            {
                pathstate |= PathState.AIR;
            }
            if (aPlaceable == true)
            {
                pathstate |= PathState.PLACEABLE;
            }
            if (aWalkable == true)
            {
                pathstate |= PathState.WALKABLE;
            }
            m_PathState = pathstate;
        }
        //Set the path state to invalid (clearing it)
        public void isInvalid()
        {
            m_PathState = PathState.INVALID;
        }
        //Remove a state from the pathstate
        public void removePathState(PathState aState)
        {
            if ((m_PathState & aState) == aState)
            {
                m_PathState -= aState;
            }
        }
        //Add a state to the path state
        public void addPathState(PathState aState)
        {
            if ((m_PathState & aState) != aState)
            {
                m_PathState |= aState;
            }
        }

        //Calculates the movement cost
        public void setParent(PathNode aParent, bool aIsDiagonal)
        {
            if (aParent != null)
            {
                if (aIsDiagonal == true)
                {
                    m_MovementCost = aParent.gScore + m_Weight + 4; //Additional score for diagonal
                }
                else
                {
                    m_MovementCost = aParent.gScore + m_Weight;
                }
                m_ParentNode = aParent;
            }
            else
            {
                m_ParentNode = null;
                m_MovementCost = 0;
            }
        }

        //Calculates the heuristic
        public void setTarget(PathNode aTarget)
        {
            m_Target = aTarget;
            if (m_Target != null)
            {
                setTarget(m_Target.position.x, m_Target.position.y, m_Target.position.z);
            }
            else
            {
                setTarget(0.0f, 0.0f, 0.0f);
            }
        }
        void setTarget(float x, float y, float z)
        {
            m_Heuristic = (int)(1.0f * (Mathf.Abs(x - m_Position.x) + Mathf.Abs(y - m_Position.y) + Mathf.Abs(z - m_Position.z)) + 0.5f);
        }

        public void calculateFScore()
        {
            m_TotalCost = gScore * hScore;
        }
        public void reset()
        {
            setTarget(null);
            setParent(null, false);
            calculateFScore();
            m_ListState = ListState.STATE_RAW;
        }


        //Debug
        public void debugDraw()
        {
            //Draw Debug Data
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(m_Position, Vector3.one);

            
            if (m_DebugData != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(m_Position, new Vector3(m_DebugData.nodeDistance.x, 1.0f, m_DebugData.nodeDistance.y));
            }
        }

        public PathNodeDebugData debugData
        {
            get { return m_DebugData; }
            set { m_DebugData = value; }
        }
        
    }

    //public class PathNode : MonoBehaviour
    //{
    //    public int m_Heuristic; //H
    //    public int m_MovementCost; //G
    //    public int m_TotalCost; //F
    //    public int m_Weight;
    //
    //    public PathNode m_Parent;
    //
    //    // Use this for initialization
    //    void Start()
    //    {
    //
    //    }
    //
    //    // Update is called once per frame
    //    void Update()
    //    {
    //
    //    }
    //
    //
    //    public void calculateFValue()
    //    {
    //        m_TotalCost = m_MovementCost * m_Heuristic;
    //    }
    //
    //    void OnDrawGizmos()
    //    {
    //        Gizmos.color = Color.yellow;
    //        Gizmos.DrawWireCube(transform.position, transform.localScale);
    //    }
    //}
}
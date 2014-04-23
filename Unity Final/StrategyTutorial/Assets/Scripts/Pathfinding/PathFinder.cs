using UnityEngine;
using System.Collections.Generic;
using System;

namespace OnLooker
{
    [Serializable]
    public class PathFinder
    {
        private List<PathNode> m_OpenList = null;
        private List<PathNode> m_ClosedList = null;


        //Position of the PathFinder Map
        [SerializeField]
        private Vector3 m_Position = Vector3.zero;
        
        //Terrain associated with PathMap
        [SerializeField]
        private Terrain m_Terrain = null;
        //How many nodes in the x and y
        private Vector2Int m_MapSize = Vector2Int.zero;
        private Vector2 m_NodeDistance = new Vector2(5.0f, 5.0f);
        private float m_YOffset;

        private PathNode[] m_PathMap = null;

        #region pathfindingFields
        PathNode m_TargetNode;
        PathNode m_StartNode;
        bool m_Searching = false;
        //Variables to determine pathing for a unit
        bool m_GroundType;
        bool m_AirType;
        bool m_Building;

        #endregion

        public PathFinder()
        {
            m_OpenList = new List<PathNode>();
            m_ClosedList = new List<PathNode>();
        
        }

        public int nodeCount
        {
            get { if (m_PathMap == null) { return m_MapSize.x * m_MapSize.y; } else { return m_PathMap.Length; } }
        }
        public Vector2Int mapSize
        {
            get { return m_MapSize; }
            set { m_MapSize = value; }
        }

        public Vector2 nodeDistance
        {
            get { return m_NodeDistance; }
            set { m_NodeDistance = value; }
        }
        public float yOffset
        {
            get { return m_YOffset; }
            set { m_YOffset = value; }
        }

        public void generatePathMap(int width, int height)
        {
            if (m_Terrain == null)
            {
                //NO TERRAIN - FATAL ERROR
                Debug.Log("Terrain Null - GeneratePathMap");
                return;
            }
            m_MapSize.x = width;
            m_MapSize.y = height;
            generatePathMap();
        }
        public void generatePathMap()
        {
            if (m_Terrain == null)
            {
                //NO TERRAIN - FATAL ERROR
                Debug.Log("Terrain Null - GeneratePathMap");
                return;
            }
            if (m_MapSize.x == 0 || m_MapSize.y == 0)
            {
                //Bad Path Map
                Debug.Log("Bad Size - GeneratePathMap");
                return;
            }
            int count = nodeCount;
            Debug.Log("Generating Map [" + count + "]");
            m_PathMap = new PathNode[count];
            for (int i = 0; i < count; i++)
            {
                m_PathMap[i] = new PathNode();
            }
            resetMap();
        }

        public void resetMap()
        {
            if (m_Terrain == null)
            {
                //NO TERRAIN - FATAL ERROR
                Debug.Log("Terrain Null - GeneratePathMap");
                return;
            }
            //Create a Grid map based off the nodeCount
            //Reset all the nodes in the path map to their default state
            //Takes weight info from a file and assigns the weight of the path node.
            if (m_PathMap != null)
            {
                int count = nodeCount;
                for (int i = 0; i < count; i++)
                {
                    if (m_PathMap[i] != null)
                    {
                        Vector2Int nodeCoord = convertIndexToCoords(i);
                        if (nodeCoord.x == -1 || nodeCoord.y == -1)
                        {
                            Debug.Log("Invalid Coord");
                            continue;
                        }
                        Vector3 worldPosition = new Vector3(m_Position.x + (m_NodeDistance.x * nodeCoord.x), 0.0f, m_Position.z + (m_NodeDistance.y * nodeCoord.y));
                        m_PathMap[i].reset();
                        m_PathMap[i].nodePosition = new Vector2Int(nodeCoord.x, nodeCoord.y);
                        m_PathMap[i].position = new Vector3(worldPosition.x , m_Terrain.SampleHeight(worldPosition) + m_YOffset, worldPosition.z);
                        m_PathMap[i].setPathState(true, true, true, true); //set these from data from level
                        m_PathMap[i].weight = 10; //set these from data from level
                        m_PathMap[i].debugData = new PathNodeDebugData(m_Terrain, m_Terrain.SampleHeight(worldPosition),i,nodeCoord,m_NodeDistance);
                    }
                }
            }
        }

        public bool hasGeneratedPathMap()
        {
            if (m_PathMap != null)
            {
                return true;
            }
            return false;
        }
        #region pathFinding
        //find a path in world space
        public List<PathNode> findPath(Vector3 aStart, Vector3 aEnd)
        {
            if (m_PathMap == null)
            {
                //No Path Map
				return null;
            }
            Vector2Int start = pathNodeCoordForWorld(aStart);
            Vector2Int end = pathNodeCoordForWorld(aEnd);

            m_StartNode = getNodeForCoord(start.x, start.y);
            m_TargetNode = getNodeForCoord(end.x, end.y);
            if (m_TargetNode == null || m_StartNode == null)
            {
                //Invalid Coordinate
				return null;
            }

            clearOpenList();
            clearClosedList();

            //Calculate G and H score
            m_StartNode.setParent(m_StartNode, false);
            m_StartNode.setTarget(m_TargetNode);
            //Insert this node into the open list
            insertNodeIntoOpenList(m_StartNode);

            int searchCount = 0;
			m_Searching = true;
            while (m_Searching == true)
            {
                if (moveNodeToClosedList(m_StartNode) == true)
                {
                    if(m_Searching == false)
                    break;
                }

                lookAdjacentNode(m_StartNode);
                m_StartNode = getNodeFromOpenList();
                searchCount++;
                if (searchCount > nodeCount * 2)
                {
                    //safe break
                    break;
                }

            }


            List<PathNode> path = new List<PathNode>();

            PathNode startNode = getNodeForCoord(start.x, end.y);
            PathNode endNode = getNodeForCoord(end.x, end.y);

            while (endNode != startNode)
            {
                path.Add(endNode);
                endNode = endNode.parent;
            }
            path.Add(startNode);
			return path;
        }

        void clearOpenList()
        {
            if (m_PathMap == null)
            {
                //No Pathmap
                return;
            }
            if (m_OpenList != null)
            {
                m_OpenList.Clear();
            }
            int count = nodeCount;
            for (int i = 0; i < count; i++)
            {
                if (m_PathMap[i] != null)
                {
                    m_PathMap[i].reset();
                }
            }
        }
        void clearClosedList()
        {
            if (m_ClosedList != null)
            {
                m_ClosedList.Clear();
            }
        }

        bool nodeOnOpenList(PathNode aNode)
        {
            if (aNode == null || m_OpenList == null)
            {
                return true;
            }
            else
            {
                //Search the list
                for (int i = 0; i < m_OpenList.Count; i++)
                {
                    //If a node exists already - return true
                    if (m_OpenList[i] == aNode)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        void insertNodeIntoOpenList(PathNode aNode)
        {
            if (m_OpenList.Count < (nodeCount - 1))
            {
                if (aNode != null)
                {
                    if (aNode.state != ListState.STATE_CLOSED)
                    {
                        aNode.state = ListState.STATE_OPEN;
                        if (nodeOnOpenList(aNode) == false)
                        {
                            m_OpenList.Add(aNode);
                        }
                    }
                }
            }
        }

        //return true if good , false if cant move
        bool moveNodeToClosedList(PathNode aNode)
        {
            if (aNode != null)
            {
                if (aNode == m_TargetNode)
                {
                    aNode.state = ListState.STATE_CLOSED;
                    m_OpenList.Remove(aNode);
                    m_Searching = false;
                    return true;
                }
                if (aNode.walkable == true)
                {
                    bool valid = true;
                    if (aNode.ground == false && m_GroundType == true)
                    {
                        valid = false;
                    }
                    if (aNode.air == false && m_AirType == true)
                    {
                        valid = false;
                    }
                    if (aNode.placeable == false && m_Building == true)
                    {
                        valid = false;
                    }
                    else
                    {
                        if (valid == false)
                        {
                            valid = true;
                        }
                    }

                    if (valid == true)
                    {
                        aNode.state = ListState.STATE_CLOSED;
                        m_OpenList.Remove(aNode);
                    }
                    return true;

                }
                else
                {
                    //unable to move
                    return false;
                }
            }
            return false;
        }

        PathNode getNodeFromOpenList()
        {
            PathNode lowestFScoreNode = null;
            if (m_OpenList != null)
            {
                if (m_OpenList.Count == 0)
                {
                    return null;
                }
                lowestFScoreNode = m_OpenList[0];
                for (int i = 0; i < m_OpenList.Count; i++)
                {
                    if (lowestFScoreNode != null && m_OpenList[i] != null)
                    {
                        if (lowestFScoreNode.fScore > m_OpenList[i].fScore)
                        {
                            lowestFScoreNode = m_OpenList[i];
                        }
                    }
                }
            }
            return lowestFScoreNode;
        }

        void lookAdjacentNode(PathNode aNode)
        {
            if (aNode != null)
            {
                int x = aNode.nodePosition.x;
                int y = aNode.nodePosition.y;
                PathNode adjacentNode = null;

                for (int i = 0; i < 8; i++)
                {
                    VectorUtils.Vector2IntShift(ref x, ref y, ((EShiftDirection)i), 1);
                    adjacentNode = getNodeForCoord(x, y);
                    VectorUtils.Vector2IntShift(ref x, ref y, ((EShiftDirection)i), -1);
                    if (adjacentNode != null)
                    {
                        if (adjacentNode.state == ListState.STATE_CLOSED || (adjacentNode.walkable == false) || (adjacentNode.ground == false && m_GroundType == true) || (adjacentNode.air == false && m_AirType == true))
                        {
                            //skip
                            continue;
                        }
                        //Normal
                        if (i % 2 == 0)
                        {
                            adjacentNode.reset();
                            adjacentNode.setTarget(m_TargetNode);
                            adjacentNode.setParent(aNode, false);
                            adjacentNode.calculateFScore();
                        }
                        else //Diagonal
                        {
                            adjacentNode.reset();
                            adjacentNode.setTarget(m_TargetNode);
                            adjacentNode.setParent(aNode, true);
                            adjacentNode.calculateFScore();
                        }
                        insertNodeIntoOpenList(adjacentNode);
                    }
                    //null node skip
                }


            }
        }
        

        #endregion
        #region helpers

        Vector2Int convertIndexToCoords(int aIndex)
        {

            if (m_Terrain == null || m_PathMap == null)
            {
                return new Vector2Int(-1,-1);
            }
            if (aIndex < 0 || aIndex > nodeCount)
            {
                return new Vector2Int(-1,-1);
            }
            return new Vector2Int(aIndex % m_MapSize.x, aIndex / m_MapSize.x);
        }
        int convertCoordsToIndex(int x, int y)
        {
            if(m_Terrain == null || m_PathMap == null)
            {
                return -1;
            }
            if (x < 0 || x >= m_MapSize.x || y < 0 || y >= m_MapSize.y)
            {
                return -1;
            }
            return x + (y * m_MapSize.x);
        }

        public PathNode getNodeForIndex(int aIndex)
        {
            if (aIndex < 0 || aIndex > nodeCount || m_PathMap == null)
            {
                return null;
            }
            return m_PathMap[aIndex];
        }
        public PathNode getNodeForCoord(int x, int y)
        {
            return getNodeForIndex(convertCoordsToIndex(x, y));
        }

        public Vector2Int pathNodeCoordForWorld(Vector3 aMousePosition)
        {
            return new Vector2Int((int)(aMousePosition.x / m_NodeDistance.x + 0.5f), (int)(aMousePosition.z / m_NodeDistance.y + 0.5f));
        }
        public Vector3 worldPositionForNodeCoord(Vector2Int aNodePosition)
        {
            return worldPositionForNodeCoord(aNodePosition.x, aNodePosition.y);
        }
        public Vector3 worldPositionForNodeCoord(int x, int y)
        {
            int index = convertCoordsToIndex(x, y);
            if (index == -1)
            {
                return Vector3.zero;
            }
            return m_PathMap[index].position + Vector3.up * m_YOffset;
        }
        


        #endregion

        public void debugDraw()
        {
            if (m_PathMap != null)
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    if (m_PathMap[i] != null)
                    {
                        m_PathMap[i].debugDraw();
                    }
                }
            }
        }
    }

}

//public class PathFinder : MonoBehaviour {
//
//	// Use this for initialization
//	void Start () {
//	
//	}
//	
//	// Update is called once per frame
//	void Update () {
//	
//	}
//}

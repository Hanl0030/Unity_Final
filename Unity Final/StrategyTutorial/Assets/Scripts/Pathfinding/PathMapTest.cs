using UnityEngine;
using System.Collections;
using OnLooker;
public class PathMapTest : MonoBehaviour 
{
    //Button Variables to Generate the Pathing Map
    [SerializeField]
    private Rect m_ButtonBounds;
    [SerializeField]
    private string m_ButtonLabel;
    
    //Mouse Input
    [SerializeField]
    private Vector3 m_MousePosition;

    //Node Link Map Size 
    [SerializeField]
    private Vector2 m_MapSize;
    private Vector2 m_PreviousMapSize;

    //Node Linker Variables - Links with Editor
    [SerializeField]
    private Vector2 m_NodeDistance; //Editor Visisble
    private Vector2 m_PreviousNodeDistance; //Checking against

    //Node Linker Y Offset
    [SerializeField]
    private float m_NodeYOffset;
    private float m_PreviousYOffset;

    [SerializeField]
    private PathFinder m_PathFinder = new PathFinder();

    

    

	// Use this for initialization
	void Start () 
    {
        m_PreviousMapSize = m_MapSize;
        m_PreviousNodeDistance = m_NodeDistance;
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (m_PathFinder != null)
        {
            //If there was a chance in node distance notify the pathfinder.
            if(m_NodeDistance != m_PreviousNodeDistance)
            {
                Debug.Log("Distance Changed");
                m_PreviousNodeDistance = m_NodeDistance;
                if (m_PathFinder.hasGeneratedPathMap() == true)
                {
                    m_PathFinder.nodeDistance = m_PreviousNodeDistance;
                    m_PathFinder.resetMap();
                }
            }
            if (m_MapSize != m_PreviousMapSize)
            {
                Debug.Log("Size changed");
                m_PreviousMapSize = m_MapSize;
                if (m_PathFinder.hasGeneratedPathMap() == false)
                {
                    m_PathFinder.mapSize = new Vector2Int((int)m_PreviousMapSize.x, (int)m_PreviousMapSize.y);
                }
            }
            if (m_PreviousYOffset != m_NodeYOffset)
            {
                m_PreviousYOffset = m_NodeYOffset;
                if (m_PathFinder.hasGeneratedPathMap() == true)
                {
                    m_PathFinder.yOffset = m_PreviousYOffset;
                    m_PathFinder.resetMap();
                }
            }
        }
	}

    void FixedUpdate()
    {
        //m_MousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out hit,1000.0f))
        {
            m_MousePosition = hit.point;
        }
        
    }

    void OnGUI()
    {
        if (GUI.Button(m_ButtonBounds, m_ButtonLabel))
        {
            if (m_PathFinder != null)
            {
                if (m_PathFinder.hasGeneratedPathMap() == false)
                {
                    float startTime = Time.time;
                    Debug.Log("Started: " + startTime);
                    m_PathFinder.generatePathMap((int)m_PreviousMapSize.x, (int)m_PreviousMapSize.y);
                    float endTime = Time.time;

                    Debug.Log("Ended: " + endTime);
                    Debug.Log("Generated Pathmap: " + (endTime - startTime));
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (m_PathFinder != null)
        {
            m_PathFinder.debugDraw();
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(m_MousePosition, 2.0f);
    }

    public PathFinder pathFinder
    {
        get { return m_PathFinder; }
        set { m_PathFinder = value; }
    }
    public Vector3 mousePosition
    {
        get { return m_MousePosition; }
    }
}

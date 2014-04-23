using UnityEngine;
using System.Collections.Generic;
using OnLooker;
public class PathNodeClickTest : MonoBehaviour 
{
    [SerializeField]
    private PathMapTest m_PathMap;

    private MeshRenderer m_Renderer;

	public List<PathNode> m_Path;

	// Use this for initialization
	void Start () 
    {
        m_Renderer = GetComponent<MeshRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (m_Renderer != null)
        {
            m_Renderer.material.color = new Color(1.0f, 0.0f, 1.0f);
        }
        if (Input.GetMouseButtonDown(0) && m_PathMap != null)
        {
            Vector3 objectPosition = Vector3.zero;
            PathFinder pathFinder = m_PathMap.pathFinder;
            if (pathFinder != null)
            {
                objectPosition = pathFinder.worldPositionForNodeCoord(pathFinder.pathNodeCoordForWorld(m_PathMap.mousePosition));
                Debug.Log(objectPosition);
            }
            
            transform.position = objectPosition;
        }

		if(Input.GetKeyDown(KeyCode.Alpha1) && m_PathMap != null)
		{
			PathFinder pathFinder = m_PathMap.pathFinder;
			if(pathFinder != null)
			{
				m_Path = pathFinder.findPath(new Vector3(5.274f,0.0f,5.1945f),new Vector3(14.4f,0.0f,14.9f));
			}
		}
	}
}

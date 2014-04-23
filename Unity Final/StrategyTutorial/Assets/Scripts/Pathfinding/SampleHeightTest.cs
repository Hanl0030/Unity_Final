using UnityEngine;
using System.Collections;


//This class simple controls a game object and samples height based on the terrain given.
//The y position of the unit is then clamped to the height of the terrain + an offset
public class SampleHeightTest : MonoBehaviour 
{
    [SerializeField]
    private Terrain m_Terrain = null;
    [SerializeField]
    private Vector3 m_OffsetPosition;
    [SerializeField]
    private float m_MovementSpeed = 5.0f;
    [SerializeField]
    private float m_YOffset = 1.0f;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Translate(Input.GetAxis("Horizontal") * m_MovementSpeed * Time.deltaTime, 0.0f, Input.GetAxis("Vertical") * m_MovementSpeed * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, m_OffsetPosition.y + m_YOffset, transform.position.z);
	}
    void FixedUpdate()
    {
        if (m_Terrain != null)
        {
            m_OffsetPosition = transform.position;
            m_OffsetPosition.y = m_Terrain.SampleHeight(transform.position);
        }
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, m_OffsetPosition);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(m_OffsetPosition, transform.localScale);
    }
}

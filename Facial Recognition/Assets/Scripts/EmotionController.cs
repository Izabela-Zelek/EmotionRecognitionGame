using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// Received emotion output from Flask server which is ran on Jupyter Notebook
/// Sends emotion to FaceController to display the emotion
/// Implements timer for anger before starting de-escalation technique
/// </summary>
public class EmotionController : MonoBehaviour
{
    private GameObject m_player = null; // Stores reference to the player
    private bool m_isAngry = false; // A bool which defines whether anger was detected
    private float m_angerTimer = 0.0f; // Counter to take note of how long player is angry
    private float m_angerMax = 2.0f; // Max allowed time for anger before de-escalation technique, Set low for testing purposes 
    public GameObject AngerBall; // Stores reference to ball used in de-escalation technique
    public Transform AngerPos; // Position the player is teleported to when during de-escalation technique
    private Vector3 m_originalPos; // Stores reference to player's position before teleported
    private Quaternion m_originalRot; // Stores reference to player's rotation before teleported
    private bool m_started = false; // A bool which defines whether de-escalation was called
    /// <summary>
    /// Starts routine at start which receives emotion data
    /// </summary>
    void Start()
    {
        StartCoroutine(GetEmotion());
    }

    /// <summary>
    /// Function which grabs emotion data being sent on the server
    /// </summary>
    /// <returns></returns>
    IEnumerator GetEmotion()
    {
        while (true)
        {
            UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:5000/data"); // Connects to local server
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success) // If successfully received output from server
            {
                string jsonResponse = request.downloadHandler.text; // Saves output as string

                if(jsonResponse.Contains("Angry")) //If detected emotion is anger
                {
                    m_isAngry = true; 
                }
                else if(m_isAngry) //If detected emotion is NOT anger and the bool was set to true previously
                {
                    m_isAngry = false;
                    m_angerTimer = 0.0f; // Resets timer
                }

                if (m_player != null) //If can find player reference
                { 
                    m_player.GetComponent<FaceController>().SetEmotion(jsonResponse); // Sends emotion data to FaceController to be displayed
                }
            }
        }
    }

    /// <summary>
    /// Starts and Stops de-escalation techniques
    /// </summary>
    private void Update()
    {
        if (m_isAngry && m_player != null) // If player reference exists and anger was detected
        {
            CheckAnger(); // Starts de-escalation
        }
        // If no longer angry, has player reference, de-escalation technique was called and finished
        if(!m_isAngry && m_player!= null && AngerBall.GetComponent<BallController>().Isfinished() && m_started)
        {
            m_started = false; 
            m_player.gameObject.transform.parent.parent.GetChild(1).SetPositionAndRotation(m_originalPos, m_originalRot); //Reset player position and rotation to original before teleportation
            m_player.gameObject.transform.parent.parent.GetComponent<PlayerController>().enabled = true; // Turns on player controls to allow movement
        }
    }

    /// <summary>
    /// Public function which allows other scripts to call and assign themselves as the player gameobject
    /// </summary>
    /// <param name="t_player"></param>
    public void LinkPlayer(GameObject t_player)
    {
        if (m_player == null) //If no previous reference
        { 
            m_player = t_player; // Set reference
        }
    }

    /// <summary>
    /// Function which counts down how long player is angry and starts de-escalation
    /// </summary>
    private void CheckAnger()
    {
        m_angerTimer += Time.deltaTime; // Adds time to counter
        if(m_angerTimer >= m_angerMax && !m_started) // If counter is over max allowed time and de-escalation wasn't already called
        {
            m_started = true; // Sets de-escalation called to true
            m_originalPos = m_player.gameObject.transform.parent.parent.GetChild(1).transform.position; // Saves original player position before teleportation
            m_originalRot = m_player.gameObject.transform.parent.parent.GetChild(1).transform.rotation; // Saves original player rotation before teleportation
            m_player.gameObject.transform.parent.parent.GetComponent<PlayerController>().enabled = false; // Turns off player movement
            m_player.gameObject.transform.parent.parent.GetChild(1).SetPositionAndRotation(AngerPos.position, AngerPos.rotation); // Teleports player to de-escalation game position
            AngerBall.GetComponent<BallController>().StartTechnique(gameObject); // Calls de-escalation game to start
        }
    }

    /// <summary>
    /// Globally available function which returns whether player is still angry
    /// </summary>
    /// <returns></returns>
    public bool CheckIfAngry()
    {
        return m_isAngry;
    }
}

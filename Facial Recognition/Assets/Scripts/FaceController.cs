using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Handles the changes to player avatar based on player's emotions
/// </summary>
public class FaceController : MonoBehaviour
{
    public Material Happy; // Stores the picture which is applied to avatar when happy
    public Material Sad; // Stores the picture which is applied to avatar when sad
    public Material Scared; // Stores the picture which is applied to avatar when scared
    public Material Angry; // Stores the picture which is applied to avatar when angry
    public Material Neutral; // Stores the picture which is applied to avatar when neutral
    private Renderer m_rend; // Stores reference to renderer which allows change to avatar

    /// <summary>
    /// Finds and applies renderer, Links player to the emotion controller script
    /// </summary>
    private void Start()
    {
        m_rend = gameObject.GetComponent<Renderer>(); // Stores renderer
        GameObject.FindWithTag("Emotion").GetComponent<EmotionController>().LinkPlayer(gameObject); // Links player
    }

    /// <summary>
    /// Globally available function which allows other scripts to set the avatar emotion
    /// </summary>
    /// <param name="emotion"></param>
    public void SetEmotion(string emotion)
    {
        if (emotion.Contains("Happy"))
        {
            m_rend.material = Happy;
        }
        else if (emotion.Contains("Sad"))
        {
            m_rend.material = Sad;
        }
        else if (emotion.Contains("Fear"))
        {
            m_rend.material = Scared;
        }
        else if (emotion.Contains("Angry"))
        {
            m_rend.material = Angry;
        }
        else if (emotion.Contains("Neutral"))
        {
            m_rend.material = Neutral;
        }
    }
}

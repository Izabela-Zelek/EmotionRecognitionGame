using System.Collections;
using UnityEngine;
using TMPro;
/// <summary>
/// Implements the de-escalation game
/// </summary>
public class BallController : MonoBehaviour
{
    public Transform startPos; //Starting position for ball
    public TextMeshPro signText; // Stores reference to sign
    private float m_breatheIn = 4.0f; //How long to breathe in
    private float m_hold = 7.0f; //How long to hold breath in
    private float m_breatheOut = 8.0f; //How long to breathe Out
    private bool m_finished = false; // A bool which defines whether game is finished

    /// <summary>
    /// Globally available function which allows other scripts to start the game. Takes in reference to the emotion controller
    /// </summary>
    /// <param name="controller"></param>
    public void StartTechnique(GameObject controller)
    {
        StartCoroutine(StartDeEscalation(controller));
    }
    /// <summary>
    /// Function which moves ball in a way which simulates the 4-7-8 breathing technique
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    private IEnumerator StartDeEscalation(GameObject controller)
    {
        m_finished = false; 
        transform.position = startPos.position; // Sets position of ball at the start
       
        float passedTime = 0.0f; // Sets time that has passed to 0
        Vector3 riseTargetPos = new Vector3(startPos.position.x + 100.0f, startPos.position.y + 100.0f, startPos.position.z); //Sets the target position to diagonal from start position
        signText.text = "Breathe In"; // Sets sign text 
        while(passedTime < m_breatheIn) // Loops until 4 seconds have passed
        {
            transform.position = Vector3.Lerp(startPos.position, riseTargetPos, passedTime / m_breatheIn); //Slowly moves ball in a diagonal direction
            passedTime += Time.deltaTime; //Increases the passed time
            yield return null;
        }

        transform.position = riseTargetPos; // Makes sure ball is set at correct ending position

        passedTime = 0.0f; // Resets time that has passed
        Vector3 rightTargetPos = new Vector3(riseTargetPos.x + 300.0f, riseTargetPos.y, riseTargetPos.z); //Sets the target position to horizontal from new start position
        signText.text = "Hold"; // Sets sign text
        while (passedTime < m_hold) // Loops until 7 seconds have passed
        {
            transform.position = Vector3.Lerp(riseTargetPos, rightTargetPos, passedTime / m_hold); //Slowly moves ball in a horizontal direction
            passedTime += Time.deltaTime; //Increases the passed time
            yield return null;
        }

        transform.position = rightTargetPos; // Makes sure ball is set at correct ending position

        passedTime = 0.0f;  // Resets time that has passed
        Vector3 fallTargetPos = new Vector3(rightTargetPos.x + 100.0f, startPos.position.y, rightTargetPos.z); //Sets the target position to horizontal from new start position
        signText.text = "Breathe Out"; // Sets sign text

        while (passedTime < m_breatheOut) // Loops until 8 seconds have passed
        {
            transform.position = Vector3.Lerp(rightTargetPos, fallTargetPos, passedTime / m_breatheOut); //Slowly moves ball in a diagonal direction down
            passedTime += Time.deltaTime; //Increases the passed time
            yield return null;
        }

        transform.position = fallTargetPos; // Makes sure ball is set at correct ending position

        if (controller.GetComponent<EmotionController>().CheckIfAngry()) // If player is still angry
        {
            StartCoroutine(StartDeEscalation(controller)); // Restarts game
        }
        else
        {
            m_finished = true; // If not angry, exits out
        }
    }

    /// <summary>
    /// Globally available function which returns whether game has finished
    /// </summary>
    /// <returns></returns>
    public bool Isfinished()
    {
        return m_finished;
    }
}

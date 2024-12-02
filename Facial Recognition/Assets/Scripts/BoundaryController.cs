using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Restarts scene is players fail
/// </summary>
public class BoundaryController : MonoBehaviour
{
    /// <summary>
    /// Detects whether player has collided with the world bounds
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player") //If it was the player that collided with the bounds
        {
            SceneManager.LoadScene("SampleScene"); // Reloads the scene
        }
    }
}

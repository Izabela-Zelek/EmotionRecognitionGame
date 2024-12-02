using UnityEngine;
/// <summary>
/// Adds ability for player to right click to press buttons
/// </summary>
public class InputController : MonoBehaviour
{ 
    /// <summary>
    /// Constantly checks for right mouse button clicks to press buttons
    /// </summary>
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) //Checks for right button press
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // Casts a ray from player to determine whether player is looking at object
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit)) // If the ray hits something
            {
                if(hit.collider.gameObject.name == "Button") // If it hit a button
                {
                    hit.collider.gameObject.GetComponent<ButtonController>().OnClick(); // Start button functionality
                }
            }
        }
    }
}

using UnityEngine;
/// <summary>
/// Controller for buttons in every puzzle
/// </summary>
public class ButtonController : MonoBehaviour
{
    public bool CorrectAnswer; //A bool which defines whether the current button is the right answer
    private GameObject[] m_floors; //A list of all the floor objects which are disabled when the answer is wrong
    public GameObject Entrance1; //The wall on Player 1's side which disappears when the answer is correct, allows player to move onto next level
    public GameObject Entrance2; //The wall on Player 2's side which disappears when the answer is correct, allows player to move onto next level
    public GameObject[] LinkedButtons; //Stores reference to the other buttons in the level
    private bool m_buttonsOn = true; //A bool which defines whether player can press the buttons

    /// <summary>
    /// Populates the list by finding all instances of Floor gameobject
    /// </summary>
    private void Start()
    {
        m_floors = GameObject.FindGameObjectsWithTag("Floor");
    }

    /// <summary>
    /// Implements functionality to the buttons
    /// </summary>
    public void OnClick()
    {
        if (m_buttonsOn) //If buttons are turned on
        {
            //Lowers the button to simulate it being pressed
            gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y - 0.07f, gameObject.transform.localPosition.z);

            if (CorrectAnswer) //If it is the correct answer
            {
                Entrance1.SetActive(false); //Disappears wall blocking entrance
                Entrance2.SetActive(false); //Disappeara wall blocking entrance
                foreach(var button in LinkedButtons) //For each button in the lost
                {
                    button.GetComponent<ButtonController>().DisableButtons(); //Changes bool on each button to disable the functionality
                }
            }
            else //if it is the wrong answer
            {
                foreach (var floor in m_floors) //For each stored floor
                {
                    floor.SetActive(false); // Disappears the floor
                }
            }
        }
    }

    /// <summary>
    /// A globally available function which can be accessed by other scripts
    /// </summary>
    public void DisableButtons()
    {
        // Turns off button functionality
        m_buttonsOn = false;
    }
}

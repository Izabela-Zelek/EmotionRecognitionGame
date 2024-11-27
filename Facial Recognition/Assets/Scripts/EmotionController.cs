using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class EmotionController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetEmotion());
    }

    IEnumerator GetEmotion()
    {
        while (true)
        {//Grabbing data from jupyter server
            UnityWebRequest request = UnityWebRequest.Get("http://127.0.0.1:5000/data");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log(jsonResponse);
            }
            else
            {
                Debug.Log("Error: " + request.error);
            }
        }
    }
}

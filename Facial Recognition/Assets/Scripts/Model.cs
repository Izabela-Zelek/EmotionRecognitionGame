using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;
public class Model : MonoBehaviour
{
    public NNModel modelAsset;
    private Unity.Barracuda.Model m_RuntimeModel;
    private IWorker worker;
    private WebCamTexture cameraTexture;
    private int targetWidth = 48;
    private int targetHeight = 48;
    private int channels = 1;

    // Start is called before the first frame update
    void Start()
    {
        //Loading model from asset
        m_RuntimeModel = ModelLoader.Load(modelAsset);

        //Creating a worker to run the model
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, m_RuntimeModel);

        cameraTexture = new WebCamTexture();
        cameraTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (cameraTexture.didUpdateThisFrame)
        {
            //Processing camera frame and making predictions
            Texture2D texture = new Texture2D(targetWidth, targetHeight);
            Graphics.ConvertTexture(cameraTexture, texture);
            texture.Apply();

            Tensor inputTensor = new Tensor(1,targetHeight,targetWidth, channels);

            Debug.Log($"Tensor shape before reshape: {inputTensor.shape}");
            worker.Execute(inputTensor);

            //Grabbing and processing result
            Tensor output = worker.PeekOutput();
            ProcessOutput(output);
        }
        
    }

    void ProcessOutput(Tensor output)
    {
        float[] probabilities = output.ToReadOnlyArray();
        int predictedIndex = ArgMax(probabilities);

        string emotion = IndexToEmotion(predictedIndex);
        Debug.Log(emotion.ToString());
    }

    int ArgMax(float[] probabilities)
    {
        int maxIndex = 0;
        float maxValue = probabilities[0];

        for(int i = 1; i < probabilities.Length; i++)
        {
            if(probabilities[i] > maxValue)
            {
                maxValue = probabilities[i];
                maxValue = i;
            }
        }
        return maxIndex;
    }

    string IndexToEmotion(int index)
    {
        string[] emotions = { "Angry", "Disgust", "Fear", "Happy", "Sad", "Surprise", "Neutral" };
        return emotions[index];
    }
}

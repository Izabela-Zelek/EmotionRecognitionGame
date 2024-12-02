using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using System.IO;
/// <summary>
/// Another failed implementation
/// Real implementation found in EmotionController
/// </summary>
public class OldModel : MonoBehaviour
{
    public NNModel modelAsset;
    private Unity.Barracuda.Model m_RuntimeModel;
    private IWorker worker;
    private WebCamTexture cameraTexture;
    private int targetWidth = 48;
    private int targetHeight = 48;
    private int channels = 3; //1 with efficientnet, 3 with dense net
    //public Image uiImage;

    string[] emotionLabels = { "Angry", "Disgust", "Fear", "Happy", "Neutral", "Sad", "Surprise" };

    // Start is called before the first frame update
    void Start()
    {
        //Loading model from asset
        m_RuntimeModel = ModelLoader.Load(modelAsset);

        //Creating a worker to run the model
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, m_RuntimeModel);
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)
        {
            Debug.LogError("No webcam devices found!");
            return;
        }
        cameraTexture = new WebCamTexture();
        Debug.Log(cameraTexture.deviceName);
        cameraTexture.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cameraTexture.isPlaying)
        {
            Debug.LogError("WebCamTexture is not playing!");
        }
        else if (cameraTexture.didUpdateThisFrame)
        {
            //Debug.Log($"WebCamTexture width: {cameraTexture.width}, height: {cameraTexture.height}");

            //Processing camera frame and making predictions
            Texture2D texture = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.ARGB32, false);
            texture.SetPixels(cameraTexture.GetPixels());
            texture.Apply();
            Debug.Log(texture.width);
            Debug.Log(texture.height);
            // Debug: Check pixels of camera texture for comparison
            //Color[] pixels = cameraTexture.GetPixels();
            //Debug.Log($"First pixel color: {pixels[0]}");

            // Debug: Check pixels of texture for comparison
            // Color[] pixels2 = texture.GetPixels();
            //Debug.Log($"First pixel color: {pixels2[0]}");

            Debug.Log("WebCamTexture format: " + cameraTexture.graphicsFormat);

            //uiImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            SaveWebCamImage(texture, "webcam_image.png");
            //Tensor inputTensor = new Tensor(1,targetHeight,targetWidth, channels);

            Tensor inputTensor = ConvertTextureToTensor(texture);

            //Debug.Log("Input tensor shape: " + inputTensor.shape.ToString());
            worker.Execute(inputTensor);

            //Grabbing and processing result
            Tensor output = worker.PeekOutput();
            // Debug.Log("Output tensor shape: " + output.shape.ToString());
            ProcessOutput(output);

            inputTensor.Dispose();
            output.Dispose();
        }

    }

    Tensor ConvertTextureToTensor(Texture2D texture)
    {
        // Calculating new size
        int newWidth = texture.width / 15;
        int newHeight = Mathf.RoundToInt(texture.height / 15);

        int cropWidth = Mathf.Min(texture.width, 720);
        int cropHeight = Mathf.Min(texture.height, 720);

        // Calculate the starting point to crop from the center of the texture
        int startX = (texture.width - cropWidth) / 2;
        int startY = (texture.height - cropHeight) / 2;

        // Get the pixels from the center-cropped region
        Color[] cropPixels = texture.GetPixels(startX, startY, cropWidth, cropHeight);

        // Create a new Texture2D with the desired target size
        Texture2D resizedTexture = new Texture2D(720, 720);

        // Resize the image: Apply the center-cropped pixels to the resized texture
        resizedTexture.SetPixels(cropPixels);
        resizedTexture.Apply();

        SaveWebCamImage(resizedTexture, "resized_webcam_image.png");

        //Creating a new texture with the target dimensions
        Texture2D paddedTexture = new Texture2D(targetWidth, targetHeight);
        Color transparent = new Color(0, 0, 0, 0); // Transparent padding color

        // Filling target image with transparent values
        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                paddedTexture.SetPixel(x, y, transparent);
            }
        }

        // Copying image to center of padded
        int offsetX = (targetWidth - newWidth) / 2;
        int offsetY = (targetHeight - newHeight) / 2;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                paddedTexture.SetPixel(x + offsetX, y + offsetY, resizedTexture.GetPixel(x, y));
            }
        }

        paddedTexture.Apply();
        SaveWebCamImage(paddedTexture, "padded_webcam_image.png");


        Color[] pixels = paddedTexture.GetPixels();
        Tensor tensor = new Tensor(1, targetHeight, targetWidth, channels);

        int idx = 0;
        for (int i = 0; i < targetHeight; i++)
        {
            for (int j = 0; j < targetWidth; j++)
            {
                Color pixel = pixels[idx++];
                tensor[0, i, j, 0] = pixel.r; // Red channel
                tensor[0, i, j, 1] = pixel.g; // Green channel
                tensor[0, i, j, 2] = pixel.b; // Blue channel
            }
        }

        //Normalizing pixel values to [0, 1] by dividing by 255
        for (int i = 0; i < tensor.length; i++)
        {
            tensor[i] = tensor[i] / 255.0f;  // Normalize values between 0 and 1
        }

        return tensor;
    }
    void ProcessOutput(Tensor output)
    {
        float[] probabilities = output.ToReadOnlyArray();

        //Debug.Log("Raw model output probabilities: ");
        foreach (var probability in probabilities)
        {
            Debug.Log(probability);
        }

        int predictedIndex = ArgMax(probabilities);

        string emotion = emotionLabels[predictedIndex];
        Debug.Log(emotion.ToString());
    }

    int ArgMax(float[] probabilities)
    {
        int maxIndex = 0;
        float maxValue = probabilities[0];

        for (int i = 1; i < probabilities.Length; i++)
        {
            if (probabilities[i] > maxValue)
            {
                maxValue = probabilities[i];
                maxIndex = i;
            }
        }
        return maxIndex;
    }

    void SaveWebCamImage(Texture2D texture, string name)
    {
        if (texture == null)
        {
            Debug.LogError("Texture is null!");
            return;
        }
        Color[] pixels = texture.GetPixels();
        if (pixels.Length == 0)
        {
            Debug.LogError("No pixels found in the texture!");
        }
        else
        {
            Debug.Log("Pixels found, ready to save image.");
        }

        byte[] imageBytes = texture.EncodeToPNG();  // Encode texture to PNG format
        string filePath = Path.Combine(Application.persistentDataPath, name);

        try
        {
            File.WriteAllBytes(filePath, imageBytes);  // Write the byte array to a file
            Debug.Log("Webcam image saved to: " + filePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to save webcam image: " + e.Message);
        }
    }

}


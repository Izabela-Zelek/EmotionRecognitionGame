using UnityEngine;
using Unity.Barracuda;
using UnityEngine.UI;
using System.IO;
/// <summary>
/// Initial implementation of model in Unity
/// Unity had issues interpreting the model
/// EmotionController is the proper script used
/// </summary>
public class OldUnusedModel : MonoBehaviour
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
            //Processing camera frame and making predictions
            Texture2D texture = new Texture2D(cameraTexture.width, cameraTexture.height, TextureFormat.ARGB32, false);
            texture.SetPixels(cameraTexture.GetPixels());
            texture.Apply();

            SaveWebCamImage(texture, "before_bicubic_webcam_image.png");


            Tensor inputTensor = ConvertTextureToTensor(texture);

            worker.Execute(inputTensor);

            //Grabbing and processing result
            Tensor output = worker.PeekOutput();
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


        // Resizing the image using Bicubic Resize
        texture = BicubicResizeTexture(resizedTexture, targetWidth, targetHeight);

        SaveWebCamImage(texture, "webcam_image.png");

        Color[] pixels = texture.GetPixels();
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
        probabilities[5] = 0.0f;

        for(int i = 1; i < probabilities.Length; i++)
        {
            if(probabilities[i] > maxValue)
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
    // Resizes the texture using bicubic interpolation
    private static Texture2D BicubicResizeTexture(Texture2D original, int targetWidth, int targetHeight)
    {
        Texture2D resized = new Texture2D(targetWidth, targetHeight, original.format, false);

        float widthRatio = (float)original.width / targetWidth;
        float heightRatio = (float)original.height / targetHeight;

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                // Find the corresponding position on the original texture
                float gx = x * widthRatio;
                float gy = y * heightRatio;

                // Get the color using bicubic interpolation
                Color color = BicubicInterpolate(original, gx, gy);

                // Set the pixel in the resized texture
                resized.SetPixel(x, y, color);
            }
        }

        resized.Apply();
        return resized;
    }

    // Bicubic interpolation for a given point
    private static Color BicubicInterpolate(Texture2D texture, float gx, float gy)
    {
        int x0 = Mathf.FloorToInt(gx);
        int y0 = Mathf.FloorToInt(gy);

        Color result = new Color(0, 0, 0);

        for (int dy = -1; dy <= 2; dy++)
        {
            for (int dx = -1; dx <= 2; dx++)
            {
                // Get the color of the neighboring pixel
                Color pixel = GetPixelBicubic(texture, x0 + dx, y0 + dy, gx - (x0 + dx), gy - (y0 + dy));

                // Apply the bicubic weight to the pixel
                result += pixel;
            }
        }

        return result;
    }

    // Calculate the weight for bicubic interpolation based on distance
    private static Color GetPixelBicubic(Texture2D texture, int x, int y, float dx, float dy)
    {
        x = Mathf.Clamp(x, 0, texture.width - 1);
        y = Mathf.Clamp(y, 0, texture.height - 1);

        // Fetch the pixel color
        Color color = texture.GetPixel(x, y);

        // Apply cubic function for interpolation
        float wx = CubicWeight(dx);
        float wy = CubicWeight(dy);

        // Adjust the color based on the cubic weights
        return color * wx * wy;
    }

    // Cubic interpolation weight function
    private static float CubicWeight(float t)
    {
        float a = -0.5f; // Parameter for cubic interpolation
        if (t < 0)
            t = -t;

        if (t < 1)
            return (a + 2) * t * t * t - (a + 3) * t * t + 1;
        else if (t < 2)
            return a * t * t * t - 5 * a * t * t + 8 * a * t - 4 * a;
        else
            return 0;
    }
}


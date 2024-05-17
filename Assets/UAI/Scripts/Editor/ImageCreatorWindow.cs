using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace UAI
{
    public class ImageCreatorWindow : EditorWindow
    {
        // String variable that holds the initialization prompt for DALL-E 2
        private string SystemInitPrompt = "You are an AI trained to create images from text descriptions.";

        private List<Texture2D> images = new List<Texture2D>();

        private Texture2D selectedImage;

        private bool useDallE_3 = true;
        private bool useHDquality = true;

        private Vector2 scrollPos;
        private string imageDescription = "A futuristic city with flying cars.";
        private int imageCount = 1; 
        private int selectedSizeIndex = 0;
        private string[] sizeOptions = new string[] {"256x256", "512x512", "1024x1024"};
        private string[] sizeOptionsDallE3 = new string[] {"1024x1024", "1792x1024", "1024x1792"};
 
        private GUIStyle style;

        private int seamThicknesInPixel = 10;
        private int seamThicknesInPixelPreviously = 9;

        private Texture2D selectedImageSeamless;
        private Texture2D selectedImageSeamlessPreviously;
        private Texture2D selectedImageSeamlessWithBorder;
        private bool showSettings = true;

        /* Creates a new editor window for generating images */
        [MenuItem("Tools/AI Assistant/Image Creator", false, 3)]
        static void Init()
        {
            ImageCreatorWindow window = GetWindow<ImageCreatorWindow>("Image Creator");
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 665, 900);
            window.Show();
        } 

        int screen = 0;
        /* Draws the GUI for the editor window */
        void OnGUI()
        {
            if (style == null)
            {
                style = new GUIStyle(EditorStyles.textArea);
                style.wordWrap = true; 
            }

            GUILayout.Space(10);

            //tab menu for the different screens
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                    GUI.enabled = screen != 0;
                    if (GUILayout.Button("Image Creation", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        screen = 0;
                        selectedImage = null;
                        images = new List<Texture2D>();
                    }
                    // if (GUILayout.Button("Image Edit", GUILayout.Width(150), GUILayout.Height(30)))
                    // {
                    //     screen = 1;
                    // }

                    GUI.enabled = screen != 2;

                    if (GUILayout.Button("Image Texture Seamless", GUILayout.Width(150), GUILayout.Height(30)))
                    {
                        screen = 2;
                        selectedImage = null;
                        images = new List<Texture2D>();
                        showSettings = true;
                        seamThicknesInPixelPreviously = seamThicknesInPixel - 1;
                    }
                    GUI.enabled = true;

                GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            switch (screen)
            {
                case 0:
                    imageCreationScreen();
                    break;
                case 1:
                    // imageEditScreen();
                    break;
                case 2:
                    imageTextureSeamlessScreen();
                    break;
            }

        }

        private void imageTextureSeamlessScreen(){
            GUILayout.Space(10);
            GUILayout.Label( new GUIContent("Seamless Texture Editor (i)","If you have a Texture, that is not seamless, you can make it seamless with this tool. \nJust select the Texture, describe the image or use the promp you used to create it, \nadjust the seam thickness and settings and press the generate button." ), EditorStyles.boldLabel); 

            GUILayout.Space(10);

            // foldout for the settings
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings");
            if(showSettings){ 
                GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                        selectedImageSeamless = (Texture2D)EditorGUILayout.ObjectField("Texture", selectedImageSeamless, typeof(Texture2D), false);
                        GUILayout.Label("Describe the image you want to generate");
                        // Takes input from the user about the image
                        imageDescription = EditorGUILayout.TextArea(imageDescription, style, GUILayout.Height(50));
                                
                        // if(!useDallE_3){
                            imageCount = (int)EditorGUILayout.Slider("Number of Images", imageCount, 1, 10);
                        // }else{
                        //     //label with information that Dalles 3 only supports 1 image
                        //     GUILayout.Label("DALL-E 3 only supports 1 image per request");
                        //     GUI.enabled = false;
                        //     imageCount = (int)EditorGUILayout.Slider("Number of Images", imageCount, 1, 1);
                        //     GUI.enabled = true;
                        // }
                        // selectedSizeIndex should be selected automatically by the size of the selectedImageSeamless
                        //
                        if(selectedImageSeamless != null){
                            if(selectedImageSeamless.width == 256){
                                selectedSizeIndex = 0;
                            }else if(selectedImageSeamless.width == 512){
                                selectedSizeIndex = 1;
                            }else if(selectedImageSeamless.width == 1024){
                                selectedSizeIndex = 2;
                            }
                        }
                        GUI.enabled = false;
                        // if(!useDallE_3){
                            selectedSizeIndex = EditorGUILayout.Popup("Image Size", selectedSizeIndex, sizeOptions);
                        // }else{
                        //     selectedSizeIndex = EditorGUILayout.Popup("Image Size", selectedSizeIndex, sizeOptionsDallE3);
                        // } 
                        GUI.enabled = true; 
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                        // if(selectedImage == null){

                            seamThicknesInPixel = EditorGUILayout.IntSlider("Seam Thickness", seamThicknesInPixel, 1, 100); 
                            if(selectedImageSeamless != null){
                                if(seamThicknesInPixelPreviously != seamThicknesInPixel || selectedImageSeamless != selectedImageSeamlessPreviously){
                                    selectedImageSeamlessWithBorder = ApplyBorder(selectedImageSeamless, seamThicknesInPixel);
                                    seamThicknesInPixelPreviously = seamThicknesInPixel;
                                    selectedImageSeamlessPreviously = selectedImageSeamless;
                                }

                                GUILayout.Label(new GUIContent(selectedImageSeamlessWithBorder), GUILayout.Width(200), GUILayout.Height(200));
                            }  
                        // }
                    GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            } 
            //disable button if no description is entered or DALLEClient is WaitingForResponse 
            GUI.enabled = !(imageDescription.Length == 0 || DALLEClient.Instance.status == DALLEStatus.WaitingForResponse); 
            
            if (GUILayout.Button("Generate seamless Image", GUILayout.Height(40)))
            {
                string size = "";
                // if(!useDallE_3){
                    size = sizeOptions[selectedSizeIndex];
                // }else{
                //     size = sizeOptionsDallE3[selectedSizeIndex];
                // }
                SendRequestToDALLESeamless(imageDescription, imageCount, size);
            }  

            GUILayout.Space(10);

            
            // Shows waiting message while waiting for response from the OpenAI DALL-E 2 model
            if (DALLEClient.Instance.status == DALLEStatus.WaitingForResponse)
            {
                GUILayout.Label("Waiting for response...");
            } 

            GUILayout.Space(10);

            // Displays the generated images
            if (images.Count > 0)
            {
                printGeneratedImages();
            }
        }

        private void imageCreationScreen(){
            
            GUILayout.Space(10);
            GUILayout.Label("Image Creator", EditorStyles.boldLabel);
            GUILayout.Label("Describe the image you want to generate");

            // Takes input from the user about the image
            imageDescription = EditorGUILayout.TextArea(imageDescription, style, GUILayout.Height(50));
  
            if(!useDallE_3){
                imageCount = (int)EditorGUILayout.Slider("Number of Images", imageCount, 1, 10);
            }else{
                //label with information that Dalles 3 only supports 1 image
                GUILayout.Label("DALL-E 3 only supports 1 image per request");
                GUI.enabled = false;
                imageCount = (int)EditorGUILayout.Slider("Number of Images", imageCount, 1, 1);
                GUI.enabled = true;
            }

            string[] sizes = sizeOptions;
            if(useDallE_3){
                sizes = sizeOptionsDallE3;
            }
            selectedSizeIndex = EditorGUILayout.Popup("Image Size", selectedSizeIndex, sizes);
 

            GUILayout.Space(10);
            useDallE_3 = EditorGUILayout.Toggle("Use DALL-E 3", useDallE_3);
            if(useDallE_3){
                useHDquality = EditorGUILayout.Toggle("Use HD Quality", useHDquality);
            }
            GUILayout.Space(10);

            //disable button if no description is entered or DALLEClient is WaitingForResponse 
            GUI.enabled = !(imageDescription.Length == 0 || DALLEClient.Instance.status == DALLEStatus.WaitingForResponse); 
            if (GUILayout.Button("Generate Image", GUILayout.Height(40)))
            {
                selectedImage = null;
                string size = "";
                if(!useDallE_3){
                    size = sizeOptions[selectedSizeIndex];
                }else{
                    size = sizeOptionsDallE3[selectedSizeIndex];
                }
                SendRequestToDALLE(imageDescription, imageCount, size); 
            }

            // Shows waiting message while waiting for response from the OpenAI DALL-E 2 model
            if (DALLEClient.Instance.status == DALLEStatus.WaitingForResponse)
            {
                GUILayout.Label("Waiting for response...");
            } 

            GUILayout.Space(10);

            // Displays the generated images
            if (images.Count > 0)
            {
                printGeneratedImages();
            }
        }

        private void printGeneratedImages(){ 
            GUILayout.BeginHorizontal();
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(140), GUILayout.Height(550)); 
                    GUILayout.BeginVertical( GUILayout.Width(130));
                        GUILayout.Label("Generated Images", EditorStyles.boldLabel);
                        foreach (var image in images)
                        { 
                            GUI.enabled = selectedImage != image;
                            if(GUILayout.Button(new GUIContent(image), GUILayout.Width(130), GUILayout.Height(130)))
                            {
                                selectedImage = image;
                            } 
                        }
                        GUI.enabled = true;
                    GUILayout.EndVertical();
                EditorGUILayout.EndScrollView();
                GUILayout.BeginVertical( GUILayout.Width(512), GUILayout.Height(512));
                    if (selectedImage != null)
                    {
                        GUILayout.Label("Preview Image", EditorStyles.boldLabel);
                        GUILayout.Label(new GUIContent(selectedImage), GUILayout.Width(512), GUILayout.Height(512));
                        if (GUILayout.Button("Save Image")){ 
                            SaveImage(selectedImage, "GeneratedImage.png"); 
                        }
                    }
                GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void SendRequestToDALLESeamless(string prompt, int count, string size)
        {
            images.Clear();
            DALLEClient.Instance.SystemInitPrompt = SystemInitPrompt;

            DALLEClient.Instance.OnResponseReceived = null;
            DALLEClient.Instance.OnResponseReceived += OnAPIResponseReceived;
 
            byte[] pngData = ShiftTexture(selectedImageSeamless).EncodeToPNG(); 

            Texture2D mask = CreateMask(selectedImageSeamless.width, selectedImageSeamless.height, seamThicknesInPixel);
            byte[] pngDataMask = ShiftTexture(mask).EncodeToPNG();


            string base64PngImage = Convert.ToBase64String(pngData);
            string base64PngMask = Convert.ToBase64String(pngDataMask);
 
            string model = "dall-e-2";
            if(useDallE_3){ 
                Debug.Log("Info: For image edit only DALL-E 2 is supported at this time");
            }

            // DALLEClient.Instance.SendRequestImageEdit(pngData, pngDataMask, prompt, count, size);
            DALLEClient.Instance.SendRequestImageEdit2(ShiftTexture(selectedImageSeamless), ShiftTexture(mask), prompt, count, size, model);
        }
 
        private void SendRequestToDALLE(string prompt, int count, string size)
        {
            images.Clear();
            DALLEClient.Instance.SystemInitPrompt = SystemInitPrompt;

            DALLEClient.Instance.OnResponseReceived = null;
            DALLEClient.Instance.OnResponseReceived += OnAPIResponseReceived;

            string model = "dall-e-3";
            if(!useDallE_3){
                model = "dall-e-2";
            }


            DALLEClient.Instance.SendRequest(prompt, count, size, model);
        }

        /* Called when the response from DALL-E 2 is received */
        private void OnAPIResponseReceived(List<Texture2D> responseImages)
        {
            images = responseImages;

            if(screen == 2){
                for(int i = 0; i < images.Count; i++)
                {
                    images[i] = ShiftTexture(images[i]); 
                }
                showSettings = false;
            }
            if(images.Count > 0)
                selectedImage = images[0];

            Repaint();
        }

        /* Saves the generated image to disk */
        private void SaveImage(Texture2D image, string filename)
        {
            //ask for the path to save the image
            string path = EditorUtility.SaveFilePanel("Save Image", "", filename, "png");

            //if the user didn't cancel
            if (path.Length != 0)
            {
                //get the bytes from the texture and save them to disk
                System.IO.File.WriteAllBytes(path, image.EncodeToPNG());
                // Debug.Log("Saved to " + path);
                string folderPath = path.Substring(0, path.LastIndexOf("/"));
                //refresh the asset database so the image is visible in the editor
                AssetDatabase.Refresh();

                //call next frame
                EditorApplication.delayCall += () =>
                {
                    //remove everything before "Assets/" in the path
                    string assetPathRelative = path.Substring(path.IndexOf("Assets/"));
                    // Debug.Log("Relative path is " + assetPathRelative);
                    //load the asset
                    Texture2D asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPathRelative);

                    //if it's not null, set the texture type to sprite
                    if (asset != null)
                    {
                        //focus project tab and navigate the path in project view not in explorer
                        EditorUtility.FocusProjectWindow();
                        Selection.activeObject = asset;
                    }
                };


            }
        }

        public Texture2D CreateMask(int width, int height, int seamThicknesInPixel)
        {
            Texture2D mask = new Texture2D(width, height, TextureFormat.ARGB32, false);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color color = Color.black;
                    if (i < seamThicknesInPixel || j < seamThicknesInPixel || i > width - seamThicknesInPixel || j > height - seamThicknesInPixel)
                    {
                        color.a = 0;
                    }
                    else
                    {
                        color.a = 1;
                    }
                    mask.SetPixel(i, j, color);
                }
            }

            mask.Apply();

            return mask;
        }


        public Texture2D ShiftTexture(Texture2D original)
        {
            int width = original.width;
            int height = original.height;
            
            Texture2D shiftedTexture = new Texture2D(width, height);

            
            RenderTexture tmp = RenderTexture.GetTemporary( original.width,original.height,0,RenderTextureFormat.Default,RenderTextureReadWrite.Linear);
            Graphics.Blit(original, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D myTexture2D = new Texture2D(original.width, original.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int shifted_i = (i + width / 2) % width;
                    int shifted_j = (j + height / 2) % height;
                    shiftedTexture.SetPixel(i, j, myTexture2D.GetPixel(shifted_i, shifted_j));
                }
            }

            shiftedTexture.Apply();

            return shiftedTexture;
        }

        public Texture2D ApplyBorder(Texture2D original, int seamThicknessInPixels)
        {
            Texture2D borderedTexture = new Texture2D(original.width, original.height, TextureFormat.ARGB32, false);

            RenderTexture tmp = RenderTexture.GetTemporary( original.width,original.height,0,RenderTextureFormat.Default,RenderTextureReadWrite.Linear);
            Graphics.Blit(original, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D myTexture2D = new Texture2D(original.width, original.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);

            for (int y = 0; y < original.height; y++)
            {
                for (int x = 0; x < original.width; x++)
                {
                    // If the pixel is within the border, set it to white
                    if (x < seamThicknessInPixels || y < seamThicknessInPixels || x >= original.width - seamThicknessInPixels || y >= original.height - seamThicknessInPixels)
                    {
                        borderedTexture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        borderedTexture.SetPixel(x, y, myTexture2D.GetPixel(x, y));
                    }
                }
            } 
            borderedTexture.Apply();

            return borderedTexture;
        }


    }
}

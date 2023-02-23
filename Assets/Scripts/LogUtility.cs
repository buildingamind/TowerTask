using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Flags]
public enum AttributeEnum : int
{
    xPosition = 0x01,
    yPosition = 0x02,
    zPosition = 0x04,
    xAngle = 0x08,
    yAngle = 0x10,
    zAngle = 0x20,
}

public class LogUtility : MonoBehaviour
{
    [Header("[?] Which Objects are to be Logged?")]
    [Tooltip("GameObjects whose attributes will be logged.")]
    public List<GameObject> loggedObjects;

    [Header("[?] Which Attributes of these Objects are to be logged?")]
    [Tooltip("Which attributes of the object to log.")]
    public AttributeEnum objectAttributes;

    [Header("[?] How frequently should the Cameras be Sampled? (in Steps)")]
    [Tooltip("Record from the camera every number of frames. 0 = Never. 1 = All frames. 2 = every other frame. 5 = every fifth frame, etc.")]
    public int cameraFrequency = 0;

    [Header("[?] What is the maximum number of images to take from each camera?")]
    [Tooltip("This is meant to prevent data explosion on long runs, yet allow for early camera debugging.")]
    public int maximumCaptures = 100;

    [Header("[?] Which Cameras will be Sampled?")]
    [Tooltip("Cameras which will be rendered. These cameras must have Render Textures.")]
    public List<Camera> loggedCameras;

    // public int objectLogFrequency = 1;

    StreamWriter logWriter;
    
    int frame;
   
    public string runID;
    [ReadOnly]
    public int currentEpisode;
    [ReadOnly]
    public int currentStep;
    [ReadOnly]
    public int captureCount;

    [ReadOnly]
    public string logPath;
    [ReadOnly]
    public string objectPath;
    [ReadOnly]
    public string framesPath;

    private void Awake()
    {
        StaticManager.logUtility = this;
    }

    void Start()
    {        
        /*if (!Application.isEditor)  // We won't log from the editor
        {*/
            if (runID.Equals(string.Empty))
            {
                runID = ArgumentParser.Options.runID;
            }
            
            /*cameraFrequency = ArgumentParser.Options.cameraFrequency;*/

            // Create Logging Directory
            Directory.CreateDirectory(string.Format("{0}/Logs/{1}", Application.dataPath, runID));
            logPath = string.Format("{0}/Logs/{1}", Application.dataPath, runID);

            if (loggedObjects.Count > 0)
            {
                objectPath = string.Format("{0}/Objects", logPath);
                Directory.CreateDirectory(objectPath);
                logWriter = new StreamWriter(string.Format("{0}/{1}.csv", objectPath, runID), false);
            }

            if (loggedCameras.Count > 0 && cameraFrequency > 0)
            {

                framesPath = string.Format("{0}/Frames", logPath);
                foreach (Camera cam in loggedCameras)
                {
                    Directory.CreateDirectory(string.Format("{0}/{1}", framesPath, cam.name));
                }
            }

            WriteHeaders();
        /*}*/
    }

    void WriteHeaders()
    {
        // Write the headers to the log file

        string header = "episode,step,";
        foreach (GameObject obj in loggedObjects)
        {
            foreach (string attr in objectAttributes.ToString().Split(new[] { ", " }, StringSplitOptions.None))
            {
                header += string.Format("{0}.{1},", obj.name, attr);
            }
        }
        Write(header.ToLower());
    }

    public void AddRecord(int episode, int step)
    {
        currentStep = step;

        // Collect the attribute data. 
        // Include any other attributes you want to collect in this method.
        // Also include the metric in the objectAttribute enum so you can select it.

        string rowLog = string.Format("{0},{1},", episode, step);
        foreach (GameObject obj in loggedObjects)
        {
            if (objectAttributes.HasFlag(AttributeEnum.xPosition))
            {
                rowLog += obj.transform.position.x + ",";
            }
            if (objectAttributes.HasFlag(AttributeEnum.yPosition))
            {
                rowLog += obj.transform.position.y + ",";
            }
            if (objectAttributes.HasFlag(AttributeEnum.zPosition))
            {
                rowLog += obj.transform.position.z + ",";
            }
            if (objectAttributes.HasFlag(AttributeEnum.xAngle))
            {
                rowLog += obj.transform.rotation.x + ",";
            }
            if (objectAttributes.HasFlag(AttributeEnum.yAngle))
            {
                rowLog += obj.transform.rotation.y + ",";
            }
            if (objectAttributes.HasFlag(AttributeEnum.zAngle))
            {
                rowLog += obj.transform.rotation.z + ",";
            }
        }
        if (!rowLog.Equals(""))
        {
            Write(rowLog.ToLower());
        }

        if (cameraFrequency > 0 && frame % cameraFrequency == 0 && (captureCount < maximumCaptures || maximumCaptures < 0))
        {
            foreach (Camera cam in loggedCameras)
            {
                CaptureCamera(cam, frame);
            }
            captureCount++;
        }
        frame++;
    }

    void CaptureCamera(Camera cam, int i)
    {
        if (cam.targetTexture != null)
        {
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = cam.targetTexture;

            cam.Render();
            Texture2D Image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
            Image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
            Image.Apply();
            RenderTexture.active = currentRT;

            var Bytes = Image.EncodeToPNG();
            Destroy(Image);

            File.WriteAllBytes(string.Format("{0}/{1}/{2}.png", framesPath, cam.name, i.ToString("0000000")), Bytes);
        }
    }

    void Write(string line)
    {
        if (logWriter == null)
        {
            return;
        }
        logWriter.WriteLine(line);
        logWriter.Flush();
    }
}

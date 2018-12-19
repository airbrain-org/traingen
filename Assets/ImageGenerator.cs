﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageGenerator : MonoBehaviour
{
    public Vector2 m_angleRangeDegrees = new Vector2(-90f, 90f);
    public Vector2 m_elevationRangeDegrees = new Vector2(-5f, 5f);

    public Vector2 m_distanceRangeMeters = new Vector2(-200.0f, 200.0f);
    public Vector2 m_offsetRangeMeters = new Vector2(-200.0f, 200.0f);
    public Vector2 m_altitudeRangeMeters = new Vector2(-20.0f, 20.0f);

    public enum Format { RAW, JPG, PNG };
    public Format m_format = Format.JPG;
    public int m_xResolution = 1024;
    public int m_yResolution = 768;

    public float m_orbitDampening = 10f;
    public float m_scrollDampening = 6f;

    public string m_observedObjectTag = "Smoke";

    public bool m_enableAutoImageCapture = false;
    public bool m_enableMovementDampening = true;

    public int m_maxImagesPerObservation = 5;
    public float m_visibilityProportion = 0.5f;
    public bool m_isObservedVisible;

    // Offset from the observed object(s) of the pivot point.  This is used to position the
    // pivot object relative to the observed object(s).
    //
    // Default value: new Vector3((float)282.7, (float)401.4, (float)-411.7)
    public Vector3 m_observedOffset = new Vector3(282.7f, 401.4f, -411.7f);

    // Retain the initial camera (not pivot) orientation, in case the camera is moved manually.
    //
    // Default value: new Vector3(7f, 285f, -23f)
    public Vector3 m_initialLocalRotation = new Vector3(7f, 285f, -23f);

    // Offset from the pivot point, acting as the parent of the camera object.  This
    // is used to position the camera object.
    // Default value: new Vector3((float)192.0, (float)51.0, (float)-446.0);
    public Vector3 m_pivotOffset = new Vector3(192.0f, 51.0f, -446.0f);

    // The initial orientation of the pivot object.  This is used to control the rotation
    // of the camera relative to the pivot object.
    // Default value: new Vector3((float)-24.0, (float)451.0, (float)25.0);
    public Vector3 m_pivotRotation = new Vector3(-24.0f, 451.0f, 25.0f);

    private string m_folderBase;
    private string m_folderObserved;
    private string m_folderNothing;
    private int m_fileCounter = 0;

    private Camera m_camera;
    private RenderTexture m_renderTexture;

    private Vector3 m_targetRotation;
    private Quaternion m_targetRotationQt;
    private Vector3 m_targetTransform;

    private List<GameObject> m_observed = new List<GameObject>();
    private int m_indexObserved;
    private int m_imageCount;
    private int m_visibleCount;
    private Vector3 m_pivotPosition;

    private System.Random m_random = new System.Random();

    private enum CameraState
    {
        Error,
        Uninitialized,
        Stopped,
        StartMotion,
        InMotion,
        Rendering,
        PositionReached,
        ImageCaptured,
    }
    private CameraState m_cameraState;
    private bool m_autoImageCaptureState;

    // Use this for initialization
    void Start()
    {
        m_camera = GetComponentInParent<Camera>();
        m_cameraState = CameraState.Uninitialized;
        m_renderTexture = new RenderTexture(m_xResolution, m_yResolution, 24);

        var observed = GameObject.FindGameObjectsWithTag(m_observedObjectTag);
        m_observed.AddRange(observed);
        if (m_observed.Count == 0)
        {
            Debug.Log(string.Format("No '{0}' tagged objects located.", m_observedObjectTag));
            return;
        }

        // Hide all of the observed objects.
        foreach (GameObject go in observed)
        {
            go.SetActive(false);
        }
    }

    public void LateUpdate()
    {
        switch (m_cameraState)
        {
            case CameraState.Uninitialized:
                m_cameraState = InitializeObservation();
                break;

            case CameraState.Stopped:
                if (Input.GetKeyDown(KeyCode.RightShift) || m_autoImageCaptureState)
                {
                    m_cameraState = SelectObservation();
                }
                break;

            case CameraState.StartMotion:
                m_cameraState = StartMoveCamera();
                if (m_enableAutoImageCapture)
                {
                    m_autoImageCaptureState = true;
                }
                else
                {
                    m_autoImageCaptureState = false;
                }
                break;

            case CameraState.InMotion:
                m_cameraState = UpdateMoveCamera();
                break;

            case CameraState.Rendering:
                m_cameraState = WaitForRandomize();
                break;

            case CameraState.PositionReached:
                m_cameraState = SaveCameraImage();
                break;

            case CameraState.ImageCaptured:
                m_cameraState = CameraState.Stopped;
                Debug.Log(string.Format("Image count: {0}, visible count: {1}", m_imageCount, m_visibleCount));
                break;

            case CameraState.Error:
                Debug.LogError("Error: ImageGenerator is in an error state.");
                break;
        }
    }

    private CameraState InitializeObservation()
    {
        m_imageCount = 0;
        m_indexObserved = 0;
        m_visibleCount = 0;

        // Make the first observed visible.
        m_observed[m_indexObserved].SetActive(true);
        m_isObservedVisible = true;

        return CameraState.Stopped;
    }

    private CameraState SelectObservation()
    {
        // Track the number of images generated for this observed object.  When the maximum number has been reached
        // then move on to the next object.
        if (m_imageCount >= m_maxImagesPerObservation)
        {
            m_observed[m_indexObserved].SetActive(false);
            m_imageCount = 0;
            m_visibleCount = 0;

            // Time to return to the beginning of the observed list?
            if (++m_indexObserved > m_observed.Count - 1)
            {
                m_indexObserved = 0;
            }
            // Make the new observed visible.
            m_observed[m_indexObserved].SetActive(true);
            m_isObservedVisible = true;
        }
        // Have enough visible observations been collected?
        else if (((float)m_visibleCount / (float)m_maxImagesPerObservation) >= m_visibilityProportion)
        {
            m_observed[m_indexObserved].SetActive(false);
            m_isObservedVisible = false;
        }

        // If the observed can be seen, then randomize its appearance. The observed may take time to complete
        // the change in it's appearance, and we will wait in the state machine.
        if (m_isObservedVisible)
        {
            // If the observed object is a particle system, then use the default Randomization member function.
            ParticleObservation po = m_observed[m_indexObserved].GetComponent<ParticleObservation>();
            if (po != null)
            {
                po.Randomize();
            }
        }

        return CameraState.StartMotion;
    }

    private CameraState StartMoveCamera()
    {
        // Initialize the position of the pivot, relative to the location of the observed object.
        m_pivotPosition = m_observedOffset + m_observed[m_indexObserved].transform.position;

        // Move the location of the pivot to the position calculated above, offset from the location of the observed object.
        transform.parent.transform.position = m_pivotPosition;

        // Reset the local rotation, in case the camera was moved manually.
        transform.localRotation = Quaternion.Euler(m_initialLocalRotation.x, m_initialLocalRotation.y, m_initialLocalRotation.z); 

        // Generate the target (pivot) rotation and the target (camera) local position for the camera.  In contrast
        // to the above values which are camera rotation and pivot position.  So we generate new images by rotation with the pivot 
        // in the parent object, and changing local position in the camera object.  The other values (calculated above) should not
        // change, unless the camera is moved manually.
        m_targetRotation.x = m_pivotRotation.x + RandomUtils.GenerateRandom(m_elevationRangeDegrees);
        m_targetRotation.y = m_pivotRotation.y + RandomUtils.GenerateRandom(m_angleRangeDegrees);
        m_targetRotation.z = m_pivotRotation.z;

        m_targetRotationQt = Quaternion.Euler(m_targetRotation.x, m_targetRotation.y, m_targetRotation.z);

        m_targetTransform.x = m_pivotOffset.x + RandomUtils.GenerateRandom(m_offsetRangeMeters);
        m_targetTransform.y = m_pivotOffset.y + RandomUtils.GenerateRandom(m_altitudeRangeMeters); 
        m_targetTransform.z = m_pivotOffset.z + RandomUtils.GenerateRandom(m_distanceRangeMeters);

        Debug.Log(string.Format("rot: elevation:{0},angle:{1}, pos: offset:{2},altitude:{3},distance:{4}",
            m_targetRotation.x, m_targetRotation.y,
            m_targetTransform.x, m_targetTransform.y, m_targetTransform.z));

        return CameraState.InMotion;
    }

    private bool IsVisible()
    {
        var planes = GeometryUtility.CalculateFrustumPlanes(m_camera);

        var position = m_observed[m_indexObserved].transform.position;
        bool observedVisible = true;
        foreach (var plane in planes)
        {
            if (plane.GetDistanceToPoint(position) < 0)
            {
                observedVisible = false;
                break;
            }
        }
        if (observedVisible)
            return true;

        return false;
    }
    
    private CameraState UpdateMoveCamera()
    {
        if (m_enableMovementDampening)
        {
            transform.parent.rotation = Quaternion.Lerp(transform.parent.rotation, m_targetRotationQt, Time.deltaTime * m_orbitDampening);
            transform.localPosition = new Vector3(
                Mathf.Lerp(transform.localPosition.x, m_targetTransform.x, Time.deltaTime * m_scrollDampening),
                Mathf.Lerp(transform.localPosition.y, m_targetTransform.y, Time.deltaTime * m_scrollDampening),
                Mathf.Lerp(transform.localPosition.z, m_targetTransform.z, Time.deltaTime * m_scrollDampening));
        }
        else
        {
            transform.parent.rotation = m_targetRotationQt;
            transform.localPosition = m_targetTransform;
        }

        if (Mathf.Abs(transform.localPosition.x - m_targetTransform.x) < 1.0 &&
            Mathf.Abs(transform.localPosition.y - m_targetTransform.y) < 1.0 &&
            Mathf.Abs(transform.localPosition.z - m_targetTransform.z) < 1.0 &&
            Mathf.Abs(transform.parent.rotation.x - m_targetRotationQt.x) < 1.0 &&
            Mathf.Abs(transform.parent.rotation.y - m_targetRotationQt.y) < 1.0 &&
            Mathf.Abs(transform.parent.rotation.z - m_targetRotationQt.z) < 1.0)
        {
            // Is the observed visible, but invisible because of a change in camera position?
            if (m_isObservedVisible)
            {
                if (!IsVisible())
                {
                    Debug.Log("Looking for a field of view WITH observations.");
                    // Move to a different position.
                    return CameraState.StartMotion;
                }

                m_visibleCount++;
                Debug.Log(string.Format("Ready for rendering, observation #:{0}", m_visibleCount));
                // Allow time for the observed to finalize it's appearance.
                return CameraState.Rendering;
            }
            else
            {
                // The observed is invisible, so skip rendering.
                return CameraState.PositionReached;
            }
        }

        // The camera is not yet in position, so remain in this state.
        return CameraState.InMotion;
    }

    private CameraState WaitForRandomize()
    {
        ParticleObservation po = m_observed[m_indexObserved].GetComponent<ParticleObservation>();
        if (po != null)
        {
            if (po.IsRandomizeFinished())
            {
                return CameraState.PositionReached;
            }
            else
            {
                return CameraState.Rendering;
            }
        }

        return CameraState.PositionReached;
    }

    private CameraState SaveCameraImage()
    {
        Debug.Log("Entering SaveImage()");

        m_camera.targetTexture = m_renderTexture;
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = m_renderTexture;

        // Render the camera image from the cameras "targetTexture" and restore the active texture to it's original value.
        m_camera.Render();
        Texture2D image = new Texture2D(m_xResolution, m_yResolution, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, m_xResolution, m_yResolution), 0, 0);
        image.Apply();
        RenderTexture.active = currentRT;
        m_camera.targetTexture = null;

        // Encode texture into PNG data.
        byte[] bytes = image.EncodeToPNG();

        // Save to a file.
        string filename = CreateUniqueFilename(Convert.ToInt32(image.width), Convert.ToInt32(image.height));

        Debug.Log(string.Format("Saving image file to: {0}", filename));
        System.IO.File.WriteAllBytes(filename, bytes);

        m_imageCount++;
        return CameraState.ImageCaptured;
    }

    private string CreateUniqueFilename(int width, int height)
    {
        string folder;

        // If folder not specified by now use a good default
        if (m_folderBase == null || m_folderBase.Length == 0)
        {
            m_folderBase = Application.dataPath;
            if (Application.isEditor)
            {
                // Put screenshots in folder above asset path so unity doesn't index the files
                var stringPath = m_folderBase + "/..";
                m_folderBase = Path.GetFullPath(stringPath);
            }

            // Make sure directories exist
            m_folderBase += "/screenshots";
            System.IO.Directory.CreateDirectory(m_folderBase);
            m_folderObserved = m_folderBase + "/" + m_observedObjectTag;
            System.IO.Directory.CreateDirectory(m_folderObserved);
            m_folderNothing = m_folderBase + "/" + "nothing";
            System.IO.Directory.CreateDirectory(m_folderNothing);

            // Count number of files of specified format in folder
            string mask = string.Format("screen_{0}x{1}*.{2}", width, height, m_format.ToString().ToLower());
            int fileCounterObserved = Directory.GetFiles(m_folderObserved, mask, SearchOption.TopDirectoryOnly).Length;
            int fileCounterNothing = Directory.GetFiles(m_folderNothing, mask, SearchOption.TopDirectoryOnly).Length;
            m_fileCounter = fileCounterNothing + fileCounterObserved;
        }

        // Segregate the images by category
        if (m_isObservedVisible)
        {
            folder = m_folderObserved;
        }
        else
        {
            folder = m_folderNothing;
        }

        // Use width, height, and counter for unique file name
        var filename = string.Format("{0}/screen_{1}x{2}_{3}.{4}", folder, width, height, m_fileCounter, m_format.ToString().ToLower());

        // Up counter for next call
        ++m_fileCounter;

        // Return unique filename
        return filename;
    }
}
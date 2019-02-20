using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides utilities to simplify calculation of 
/// </summary>
public class GameObjectUtils
{
    ParticleSystem.Particle[] m_Particles;

    /// <summary>
    /// Calculate the percentage of the particle system that is visible in the camera's field of 
    /// view.  This is useful for determining if enough smoke is visible in the randomly placed
    /// camera.
    /// </summary>
    /// <param name="camera">Unity Camera object</param>
    /// <param name="go">Unity GameObject</param>
    /// <returns>
    /// true: if enough of the particle system is visible, as determined
    /// by the ParticleObservation property
    /// false: if not enough of the particle system is visible</returns>
    public bool IsInsidePercent(Camera camera, GameObject go)
    {
        ParticleSystem m_System = go.GetComponent<ParticleSystem>();
        if (m_System == null)
        {
            Debug.Log("Error: Observation does not have a particle system.");
            return false;
        }

        // Create the particle buffer, if has not already been created.
        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];

        ParticleObservation po = go.GetComponent<ParticleObservation>();
        if (po == null)
        {
            Debug.Log("Observation does not contain a ParticleObservation component.");
            return false;
        }

        Renderer m_Renderer = go.GetComponent<Renderer>();
        if (!m_Renderer.isVisible)
        {
            Debug.Log("Observation is being culled from the camera view.");
            return false;
        }

        int numParticlesAlive = m_System.GetParticles(m_Particles);
        int numActiveParticles = 0;
        int numInactiveParticles = 0;
        int isBehindPlaneCount = 0;
        int isOffscreenCount = 0;

        for (int i = 0; i < numParticlesAlive; i++)
        {
            bool isBehindPlane = false;
            Vector3 cameraNormal = camera.transform.TransformDirection(Vector3.forward);
            Vector3 vectorFromCamera = m_Particles[i].position - camera.transform.position;
            float cameraNormalDot = Vector3.Dot(cameraNormal, vectorFromCamera);

            if (cameraNormalDot <= 0)
            {
                isBehindPlane = true;
                isBehindPlaneCount++;
            }

            bool isOffscreen = false;
            Vector3 particleScreen = camera.WorldToViewportPoint(m_Particles[i].position);
            if (particleScreen.x < 0 || particleScreen.x > 1 ||
                particleScreen.y < 0 || particleScreen.y > 1)
            {
                isOffscreen = true;
                isOffscreenCount++;
            }

            if (!isBehindPlane && !isOffscreen)
            {
                numActiveParticles++;
            }
            else
            {
                numInactiveParticles++;
            }
        }

        // Is the number of active particles high enough?
        Debug.Log(string.Format("Behind screen: {0}, Off screen: {1}, Active: {2}, Max: {3}, Percent: {4}", 
            isBehindPlaneCount,
            isOffscreenCount,
            numActiveParticles,
            (float)(numActiveParticles + numInactiveParticles), 
            (float)numActiveParticles / (float)(numActiveParticles + numInactiveParticles)));

        return ((float)numActiveParticles / (float)(numActiveParticles + numInactiveParticles)) >= po.m_percentVisible;
    }

    /// <summary>
    /// Is the entire game object within the viewable area of the camera?
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="go"></param>
    /// <returns></returns>
    public static bool IsInside(Camera camera, GameObject go)
    {
        // Retrieve the rectangles defining the viewable area of the camera.
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // Retrieve a collider for the game object.  
        var collider = go.GetComponent<Collider>();

        // Return true if the object is the camer's field of view.
        return (GeometryUtility.TestPlanesAABB(planes, collider.bounds));
    }
}

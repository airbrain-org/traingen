using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides utilities to simplify calculation of 
/// </summary>
public class GameObjectUtils
{
    public enum TestPlanesResults
    {
        /// <summary>
        /// The AABB is completely in the frustrum.
        /// </summary>
        Inside = 0,
        /// <summary>
        /// The AABB is partially in the frustrum.
        /// </summary>
        Intersect,
        /// <summary>
        /// The AABB is completely outside the frustrum.
        /// </summary>
        Outside
    }

    public static bool IsInside(Camera camera, GameObject go, float percent)
    {
        ParticleSystem m_System = null;
        ParticleSystem.Particle[] m_Particles = null;

        if (m_System == null)
            m_System = go.GetComponent<ParticleSystem>();

        if (m_Particles == null || m_Particles.Length < m_System.main.maxParticles)
            m_Particles = new ParticleSystem.Particle[m_System.main.maxParticles];

        var m_Renderer = go.GetComponent<Renderer>();
        if (!m_Renderer.isVisible)
        {
            Debug.Log("Observation is being culled from the camera view.");
            return false;
        }

        // GetParticles is allocation free because we reuse the m_Particles buffer between updates
        int numParticlesAlive = m_System.GetParticles(m_Particles);

        // Is the number of active particles too low?
        //        float percentActive = (float)numParticlesAlive / (float)m_System.main.maxParticles;
        //        if (percentActive < percent)
        //        {
        //            Debug.Log(string.Format("Percent of active particles is too low: {0}", percentActive));
        //            return false;
        //        }

        var planes = GeometryUtility.CalculateFrustumPlanes(camera);

        // Calculate the total number of viewable active particles.
        int numActiveParticles = 0;
        for (int i = 0; i < numParticlesAlive; i++)
        {
            //            Vector3 screenPoint = camera.WorldToViewportPoint(m_Particles[i].position);
            //            Vector3 screenPoint = camera.WorldToScreenPoint(m_Particles[i].position);
            //            if (screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1)
            //            {
            //                numActiveParticles++;
            //            }
            //            if (screenPoint.z > 0 && 
            //                screenPoint.x >= 0 && screenPoint.x < 1024 &&
            //                screenPoint.y >= 0 && screenPoint.y < 768)
            bool isBehindPlane = false;
            foreach (var plane in planes)
            {
                Vector3 particleViewport = camera.WorldToViewportPoint(m_Particles[i].position);
                //                if (plane.GetDistanceToPoint(m_Particles[i].position) < 0)
                if (plane.GetDistanceToPoint(particleViewport) < 0)
                {
                    isBehindPlane = true;
                    break;
                }
            }

            bool isOffscreen = false;
            Vector3 particleScreen = camera.WorldToScreenPoint(m_Particles[i].position);
            if (particleScreen.x >= 1024 || particleScreen.x < 0 ||
                particleScreen.y >= 768 || particleScreen.y < 0 ||
                particleScreen.z < 1000 || particleScreen.z > 3000 )
            {
                isOffscreen = true;
            }

            if (!isBehindPlane && !isOffscreen)
            //            if (!isBehindPlane && !isOffscreen)
            {
                numActiveParticles++;
            }
        } 

        // Is the number of active particles high enough?
        Debug.Log(string.Format("Active: {0}, Max: {1}, Percent: {2}", numActiveParticles,
            m_System.main.maxParticles, (float)numActiveParticles / (float)m_System.main.maxParticles));
        return ((float)numActiveParticles / (float)m_System.main.maxParticles) >= percent;
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
        // TODO-JYW: Create a method to preload the colliders, using the game object
        // as the index.
        //        var renderer = go.GetComponent<Renderer>();
        var collider = go.GetComponent<Collider>();
        var renderer = go.GetComponent<Renderer>();

        // Obtain the bounds of the collider for this object, defining an AABB.
        var viewportBounds = collider.bounds;
//        viewportBounds = renderer.bounds;
        var worldBounds = collider.bounds;
        //        var viewportBounds = renderer.bounds;
        //       var worldBounds = renderer.bounds;
        //        viewportBounds.min = camera.WorldToScreenPoint(viewportBounds.min);
        //        viewportBounds.max = camera.WorldToScreenPoint(viewportBounds.max);
        viewportBounds.min = camera.WorldToViewportPoint(viewportBounds.min);
        viewportBounds.max = camera.WorldToViewportPoint(viewportBounds.max);
        viewportBounds.center = camera.WorldToViewportPoint(viewportBounds.center);

//        viewportBounds.min = camera.WorldToScreenPoint(viewportBounds.min);
//        viewportBounds.max = camera.WorldToScreenPoint(viewportBounds.max);
//        viewportBounds.center = camera.WorldToScreenPoint(viewportBounds.center);

        //bounds.min = camera.WorldToScreenPoint(bounds.min);
        //bounds.max = camera.WorldToScreenPoint(bounds.max);

        // TODO-JYW: TESTING-TESTING:
        return (GeometryUtility.TestPlanesAABB(planes, viewportBounds));

        // Determine if the game object is entirely viewable from the camera.
        return (TestPlanesAABBInternalFast(planes, ref viewportBounds, ref worldBounds) == TestPlanesResults.Inside);
    }

    /// <summary>
    /// This is crappy performant, but easiest version of TestPlanesAABBFast to use.
    /// </summary>
    /// <param name="planes"></param>
    /// <param name="bounds"></param>
    /// <returns></returns>
//    public static TestPlanesResults TestPlanesAABBInternalFast(Plane[] planes, ref Bounds viewportBounds, ref Bounds worldBounds)
//    {
//        var min = viewportBounds.min;
//        var max = viewportBounds.max;
//
//        return TestPlanesAABBInternalFast(planes, ref min, ref max);
//    }

    /// <summary>
    /// This is a faster AABB cull than brute force that also gives additional info on intersections.
    /// Calling Bounds.Min/Max is actually quite expensive so as an optimization you can precalculate these.
    /// http://www.lighthouse3d.com/tutorials/view-frustum-culling/geometric-approach-testing-boxes-ii/
    /// </summary>
    /// <param name="planes"></param>
    /// <param name="boundsMin"></param>
    /// <param name="boundsMax"></param>
    /// <returns></returns>
    public static TestPlanesResults TestPlanesAABBInternalFast(Plane[] planes, ref Bounds viewport, ref Bounds worldport, bool testIntersection = false)
    {
        Vector3 vmin, vmax;
        Vector3 boundsMax = viewport.max;
        Vector3 boundsMin = viewport.min;
        var testResult = TestPlanesResults.Inside;

        for (int planeIndex = 0; planeIndex < planes.Length; planeIndex++)
        {
            var normal = planes[planeIndex].normal;
            var planeDistance = planes[planeIndex].distance;

            Debug.Log(string.Format("min distance: {0}, max distance: {1}", planes[planeIndex].GetDistanceToPoint(viewport.min), 
                planes[planeIndex].GetDistanceToPoint(viewport.max)));

            // X axis
            if (normal.x < 0)
            {
                vmin.x = boundsMin.x;
                vmax.x = boundsMax.x;
            }
            else
            {
                vmin.x = boundsMax.x;
                vmax.x = boundsMin.x;
            }

            // Y axis
            if (normal.y < 0)
            {
                vmin.y = boundsMin.y;
                vmax.y = boundsMax.y;
            }
            else
            {
                vmin.y = boundsMax.y;
                vmax.y = boundsMin.y;
            }

            // Z axis
            if (normal.z < 0)
            {
                vmin.z = boundsMin.z;
                vmax.z = boundsMax.z;
            }
            else
            {
                vmin.z = boundsMax.z;
                vmax.z = boundsMin.z;
            }

            var dot1 = normal.x * vmin.x + normal.y * vmin.y + normal.z * vmin.z;
            if (dot1 + planeDistance < 0)
            // TODO-JYW: LEFT-OFF: Convert the bounds to viewport coords.  Also, calculate the bounds
            // with a Renderer object, not a boxcollider.
            // (Camera.WorldToViewportPoint
            // TODO-JYW: TESTING-TESTING
//            if (planes[planeIndex].GetDistanceToPoint(viewport.max) < 0 || planes[planeIndex].GetDistanceToPoint(viewport.min) < 0)
            // if (planes[planeIndex].GetDistanceToPoint(boundsMin) < 0 || planes[planeIndex].GetDistanceToPoint(boundsMax) > 0)
                return TestPlanesResults.Outside;

            if (testIntersection)
            {
                var dot2 = normal.x * vmax.x + normal.y * vmax.y + normal.z * vmax.z;
                if (dot2 + planeDistance <= 0)
                    testResult = TestPlanesResults.Intersect;
            }
        }

        return testResult;
    }
}

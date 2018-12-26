using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class added to particle systems to allow the ImageGenerator class to initiate
/// the randomization of the particle system settings.
/// </summary>
public class ParticleObservation : MonoBehaviour
{
    /// <summary>
    /// The percentage of random variation in the particle system Start Liftime.
    /// </summary>
    public float m_startLifetimePercentVariation = .20f;

    /// <summary>
    /// The percentage of random variation in the particle system Start Speed.
    /// </summary>
    public float m_startSpeedPercentVariation = .20f;

    /// <summary>
    /// The percentage of random variation in the particle system Start Size.
    /// </summary>
    public float m_startSizePercentVariation = .20f;

    /// <summary>
    /// The percentage of random variation in the wait time used to generate
    /// smoke after the particle system is restarted.  
    /// </summary>
    public float m_particleFlowTimePercentVariation = .10f;

    /// <summary>
    /// The number of seconds to wait for the particle system to generate smoke
    /// after it has been reset. This time period is accelerated by modifying the 
    /// value of the Unity time scale in Time.timeScale.
    /// </summary>
    public float m_waitTimeSecsForRandomize = 30;

    private float m_currentWaitTimeSecsForRandomize;
    private ParticleSystem m_ps;
    private ParticleSystem.MainModule m_main;
    private ParticleSystem.SizeBySpeedModule m_sizeBySpeed;

	// Use this for initialization
	void Start ()
    {
        m_ps = GetComponent<ParticleSystem>();

        if (m_ps == null)
        {
            Debug.Log("No particle system is associated with this object.");
        }

        m_main = m_ps.main;
        m_sizeBySpeed = m_ps.sizeBySpeed;
    }
	
    /// <summary>
    /// Called by the ImageGenerator to determine if the radomization process for this particular Observation
    /// is completed. A new image will not be generated until the randomization process is completed.
    /// </summary>
    /// <returns>
    /// If true, the randomization process is finished and image generation may commence.
    /// If false, the radomization process is not yet complete.
    /// </returns>
    public bool IsRandomizeFinished()
    {
        m_currentWaitTimeSecsForRandomize -= Time.deltaTime;

        // Has the randomize wait time been satisfied? If so, then return the time scale back
        // to normal. (I wish this were possible in real life. :-))
        if (m_currentWaitTimeSecsForRandomize <= 0)
        {
            Time.timeScale = 1f;
            m_currentWaitTimeSecsForRandomize = 0;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Initiate the randomization of the appearance of the particle system for this object.  This is 
    /// accomplished by changing the value of certain settings using random values within a predefined 
    /// range.
    /// </summary>
    public void Randomize()
    {
        Vector2 minMax;
        ParticleSystem.MinMaxCurve curve = new ParticleSystem.MinMaxCurve();
        curve.mode = ParticleSystemCurveMode.TwoConstants;

        minMax = RandomUtils.GenerateRandomRange(new Vector2(m_main.startLifetime.constantMin, m_main.startLifetime.constantMax),
            m_startLifetimePercentVariation);
        curve.constantMin = minMax.x;
        curve.constantMax = minMax.y;
        m_main.startLifetime = curve;

        minMax = RandomUtils.GenerateRandomRange(new Vector2(m_main.startSpeed.constantMin, m_main.startSpeed.constantMax),
            m_startSpeedPercentVariation);
        curve.constantMin = minMax.x;
        curve.constantMax = minMax.y;
        m_main.startSpeed = curve;

        minMax = RandomUtils.GenerateRandomRange(new Vector2(m_main.startSize.constantMin, m_main.startSize.constantMax),
            m_startSizePercentVariation);
        curve.constantMin = minMax.x;
        curve.constantMax = minMax.y;
        m_main.startSize = curve;

        // Initialize the period of time to wait for the particle flow to be completed, and the 
        // time scale to accelerate the flow and reduce the wait time.
        m_ps.Clear();
        m_currentWaitTimeSecsForRandomize = RandomUtils.GenerateRandom(m_waitTimeSecsForRandomize, m_particleFlowTimePercentVariation);
        Time.timeScale = 90f;
    }
}

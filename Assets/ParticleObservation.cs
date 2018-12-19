using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleObservation : MonoBehaviour
{
    public float m_startLifetimePercentVariation = .20f;
    public float m_startSpeedPercentVariation = .20f;
    public float m_startSizePercentVariation = .20f;
    public float m_particleFlowTimePercentVariation = .10f;
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

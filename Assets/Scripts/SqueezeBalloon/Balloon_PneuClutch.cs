//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: BALLOONS!!
//
//=============================================================================

﻿using UnityEngine;
using System.Collections;
using HaptGlove;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

//-------------------------------------------------------------------------
public class Balloon_PneuClutch : MonoBehaviour
{
    public enum BalloonColor
    {
        Red,
        OrangeRed,
        Orange,
        YellowOrange,
        Yellow,
        GreenYellow,
        Green,
        BlueGreen,
        Blue,
        VioletBlue,
        Violet,
        RedViolet,
        LightGray,
        DarkGray,
        Random
    };

    public GameObject popPrefab;

    public float maxVelocity = 5f;

    public float lifetime = 15f;
    public bool burstOnLifetimeEnd = false;

    public GameObject lifetimeEndParticlePrefab;
    public SoundPlayOneshot_Android lifetimeEndSound;

    private float destructTime = 0f;
    private float releaseTime = 99999f;

    public SoundPlayOneshot_Android collisionSound;
    private float lastSoundTime = 0f;
    private float soundDelay = 0.2f;

    private bool bParticlesSpawned = false;

    private static float s_flLastDeathSound = 0f;

    public HaptGloveHandler gloveHandler;
    public Grasping graspingScript;
    private FingerMapping fingerMappingLeftScript;
    private Color red = new Color(237, 29, 37, 255) / 255;
    private Color thisColor;

    //-------------------------------------------------
    void Start()
    {
        destructTime = Time.time + lifetime + Random.value;

        thisColor = gameObject.GetComponent<MeshRenderer>().material.color;

        //Debug.Log("RED:" + red.r + "\t" + red.g + "\t" + red.b);
        //Debug.Log("This:" + thisColor.r + "\t" + thisColor.g + "\t" + thisColor.b);
    }


    //-------------------------------------------------
    //void Update()
    //{
    //    if ((destructTime != 0) && (Time.time > destructTime))
    //    {
    //        if (burstOnLifetimeEnd)
    //        {
    //            SpawnParticles(lifetimeEndParticlePrefab, lifetimeEndSound);
    //        }

    //        Destroy(gameObject);
    //    }
    //}


    //-------------------------------------------------
    private void SpawnParticles(GameObject particlePrefab, SoundPlayOneshot_Android sound)
    {
        // Don't do this twice
        if (bParticlesSpawned)
        {
            return;
        }

        bParticlesSpawned = true;

        if (particlePrefab != null)
        {
            GameObject particleObject =
                Instantiate(particlePrefab, transform.position, transform.rotation) as GameObject;
            particleObject.GetComponent<ParticleSystem>().Play();
            Destroy(particleObject, 2f);
        }

        if (sound != null)
        {
            float lastSoundDiff = Time.time - s_flLastDeathSound;
            if (lastSoundDiff < 0.1f)
            {
                sound.volMax *= 0.25f;
                sound.volMin *= 0.25f;
            }

            sound.Play();
            s_flLastDeathSound = Time.time;
        }
    }


    //-------------------------------------------------
    private bool startSqueeze = false;
    private float[] fingerPos;
    private float squeezeStart = 0;
    private float squeezeEnergy = 0.08f;
    private float explosionThreshold = 0f;
    private float r, g, b;
    private Color bufColor;
    public float[] iniDistance = new float[4];
    public float[] curDistance = new float[4];
    public Vector3[] fingerPosition = new Vector3[5];
    public float avgSqueezeRatio;
    MixedRealityPose pose;
    private Handedness handedness;
    public float squeezeThreshold = 0.2f;
    private bool[] squeezeState = new bool[2];
    void Update()
    {
        if (gameObject.GetComponent<HapMaterial>().isGrasped)
        {
            gloveHandler = gameObject.GetComponent<HapMaterial>().graspedHand.GetComponent<HaptGloveHandler>();
            graspingScript = gloveHandler.gameObject.GetComponent<Grasping>();
            if (gloveHandler.whichHand == HaptGloveHandler.HandType.Left)
                handedness = Handedness.Left;
            else
                handedness = Handedness.Right;

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.ThumbTip, handedness, out pose))
                fingerPosition[0] = pose.Position;

            float totalSqueezeRatio = 0;
            int counts = 0;
            for (int i = 0; i < 4; i++)
            {
                if (graspingScript.touchedFingers[i+1])
                {
                    //calculate distance
                    if (HandJointUtils.TryGetJointPose((TrackedHandJoint)(11 + 5 * i), handedness, out pose))
                        fingerPosition[i+1] = pose.Position;
                    if (iniDistance[i] == 0)
                        iniDistance[i] = Vector3.Distance(fingerPosition[0], fingerPosition[i+1]);
                    curDistance[i] = Vector3.Distance(fingerPosition[0], fingerPosition[i+1]);

                    counts++;
                    if (iniDistance[i] != 0)
                    {
                        totalSqueezeRatio += (1 - curDistance[i] / iniDistance[i]);
                    }
                    
                }
                
            }

            if (counts != 0)
            {
                avgSqueezeRatio = totalSqueezeRatio / counts;

                if (avgSqueezeRatio >= squeezeThreshold)
                {
                    SpawnParticles(popPrefab, null);

                    GetComponent<Rigidbody>().isKinematic = true;
                    this.transform.localScale = Vector3.zero;
                    Destroy(gameObject, 0.1f); 
                }
                else if (avgSqueezeRatio >= (2f / 3f) * squeezeThreshold)
                {
                    if (!squeezeState[1])
                    {
                        //Increase pressure
                        IncreaseDecreasePressure(counts, true);
                        Debug.Log("Increase pressure 2");
                        squeezeState[1] = true;
                    }
                }
                else if (avgSqueezeRatio >= (1f / 3f) * squeezeThreshold)
                {
                    if (!squeezeState[0])
                    {
                        //Increase pressure
                        IncreaseDecreasePressure(counts, true);
                        Debug.Log("Increase pressure 1");
                        squeezeState[0] = true;
                    }
                }


                if (avgSqueezeRatio <= (1f / 3f * 0.9f) * squeezeThreshold)
                {
                    if (squeezeState[0])
                    {
                        //Decrease pressure
                        IncreaseDecreasePressure(counts, false);
                        Debug.Log("Decrease pressure 1");
                        squeezeState[0] = false;
                    }
                }
                else if (avgSqueezeRatio <= (2f / 3f * 0.9f) * squeezeThreshold)
                {
                    if (squeezeState[1])
                    {
                        //Decrease pressure
                        IncreaseDecreasePressure(counts, false);
                        Debug.Log("Decrease pressure 2");
                        squeezeState[1] = false;
                    }
                }

            }
            


            





            //fingerMappingLeftScript = gameObject.GetComponent<HapMaterial>().graspedHand.GetComponent<FingerMapping>();
            //fingerPos = fingerMappingLeftScript.normalizedData;
            //if (startSqueeze == false)
            //{
            //    squeezeStart = fingerPos[0];
            //    explosionThreshold = squeezeStart + squeezeEnergy;
            //    startSqueeze = true;
            //}

            ////Color change
            //r = Tool.RemapNumberClamped(fingerPos[0], squeezeStart, explosionThreshold, thisColor.r, red.r);
            //g = Tool.RemapNumberClamped(fingerPos[0], squeezeStart, explosionThreshold, thisColor.g, red.g);
            //b = Tool.RemapNumberClamped(fingerPos[0], squeezeStart, explosionThreshold, thisColor.b, red.b);
            //bufColor = new Color(r, g, b);
            //gameObject.GetComponent<MeshRenderer>().material.color = bufColor;
            ////Debug.Log("BufColor:" + r + "\t" + bufColor.g + "\t" + bufColor.b);

            //if (fingerPos[0] > explosionThreshold)
            //{
            //    //ApplyDamage();
            //    SpawnParticles(popPrefab, null);
            //    this.transform.localScale = Vector3.zero;
            //    Destroy(gameObject, 1);
            //    startSqueeze = false;

            //    //byte[][] clutchStates =
            //    //{
            //    //    new byte[] {0, 2}, new byte[] {1, 2}, new byte[] {2, 2}, new byte[] {3, 2}, new byte[] {4, 2}
            //    //};
            //    //byte targetPres = 40;
            //    //Haptics.ApplyHaptics(clutchStates, targetPres);
            //}
        }
        else
        {
            startSqueeze = false;
            ResetGraspPara();
        }


        //// Slow-clamp velocity
        //if ( balloonRigidbody.velocity.sqrMagnitude > maxVelocity )
        //{
        //	balloonRigidbody.velocity *= 0.97f;
        //}

        //         // Slow-clamp rotation velocity
        //         balloonRigidbody.maxAngularVelocity = 7f;
    }

    private void IncreaseDecreasePressure(int counts, bool isIncrease)
    {
        byte state = 0x00;
        if (!isIncrease)
            state = 0x02;
        
        byte[][] clutchStates = new byte[counts + 1][];
        byte[][] valveTimings = new byte[counts + 1][];
        for (int i = 0; i < counts + 1; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (graspingScript.touchedFingers[j])
                {
                    clutchStates[i] = new byte[] { (byte)i, state };
                    valveTimings[i] = new byte[] { 0x03, 0x08 };
                }
            }
        }

        byte[] btdata = gloveHandler.haptics.ApplyHapticsWithTiming(clutchStates, valveTimings, false);
        gloveHandler.BTSend(btdata);
        Debug.Log("haptics.which hand: " + gloveHandler.haptics.whichHand);
    }

    private void ResetGraspPara()
    {
        iniDistance = new float[4];
        curDistance = new float[4];
        fingerPosition = new Vector3[5];
        avgSqueezeRatio = 0;
}


    //-------------------------------------------------
    private void ApplyDamage()
    {
        SpawnParticles(popPrefab, null);
        Destroy(gameObject);
    }


    //-------------------------------------------------
    //void OnCollisionEnter( Collision collision )
    //{
    //	if ( bParticlesSpawned )
    //	{
    //		return;
    //	}

    //	Hand collisionParentHand = null;

    //	BalloonHapticBump balloonColliderScript = collision.gameObject.GetComponent<BalloonHapticBump>();

    //	if ( balloonColliderScript != null && balloonColliderScript.physParent != null )
    //	{
    //		collisionParentHand = balloonColliderScript.physParent.GetComponentInParent<Hand>();
    //	}

    //	if ( Time.time > ( lastSoundTime + soundDelay ) )
    //	{
    //		if ( collisionParentHand != null ) // If the collision was with a controller
    //		{
    //			if ( Time.time > ( releaseTime + soundDelay ) ) // Only play sound if it's not immediately after release
    //			{
    //				collisionSound.Play();
    //				lastSoundTime = Time.time;
    //			}
    //		}
    //		else // Collision was not with a controller, play sound
    //		{
    //			collisionSound.Play();
    //			lastSoundTime = Time.time;

    //		}
    //	}

    //	if ( destructTime > 0 ) // Balloon is released away from the controller, don't do the haptic stuff that follows
    //	{
    //		return;
    //	}

    //	if ( balloonRigidbody.velocity.magnitude > ( maxVelocity * 10 ) )
    //	{
    //		balloonRigidbody.velocity = balloonRigidbody.velocity.normalized * maxVelocity;
    //	}

    //	if ( hand != null )
    //	{
    //		ushort collisionStrength = (ushort)Mathf.Clamp( Util.RemapNumber( collision.relativeVelocity.magnitude, 0f, 3f, 500f, 800f ), 500f, 800f );

    //		hand.TriggerHapticPulse( collisionStrength );
    //	}
    //}


    //-------------------------------------------------
    public void SetColor(BalloonColor color)
    {
        GetComponentInChildren<MeshRenderer>().material.color = BalloonColorToRGB(color);
    }


    //-------------------------------------------------
    private Color BalloonColorToRGB(BalloonColor balloonColorVar)
    {
        Color defaultColor = new Color(255, 0, 0);

        switch (balloonColorVar)
        {
            //case BalloonColor.Red:
            //	return new Color( 237, 29, 37, 255 ) / 255;
            case BalloonColor.OrangeRed:
                return new Color(241, 91, 35, 255) / 255;
            case BalloonColor.Orange:
                return new Color(245, 140, 31, 255) / 255;
            case BalloonColor.YellowOrange:
                return new Color(253, 185, 19, 255) / 255;
            case BalloonColor.Yellow:
                return new Color(254, 243, 0, 255) / 255;
            case BalloonColor.GreenYellow:
                return new Color(172, 209, 54, 255) / 255;
            case BalloonColor.Green:
                return new Color(0, 167, 79, 255) / 255;
            case BalloonColor.BlueGreen:
                return new Color(108, 202, 189, 255) / 255;
            case BalloonColor.Blue:
                return new Color(0, 119, 178, 255) / 255;
            case BalloonColor.VioletBlue:
                return new Color(82, 80, 162, 255) / 255;
            case BalloonColor.Violet:
                return new Color(102, 46, 143, 255) / 255;
            case BalloonColor.RedViolet:
                return new Color(182, 36, 102, 255) / 255;
            case BalloonColor.LightGray:
                return new Color(192, 192, 192, 255) / 255;
            case BalloonColor.DarkGray:
                return new Color(128, 128, 128, 255) / 255;
            case BalloonColor.Random:
                int randomColor = Random.Range(0, 12);
                return BalloonColorToRGB((BalloonColor) randomColor);
        }

        return defaultColor;
    }
}



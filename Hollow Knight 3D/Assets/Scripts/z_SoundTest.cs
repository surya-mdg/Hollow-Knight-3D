using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class z_SoundTest : MonoBehaviour
{
    [SerializeField] private AudioSource dash;
    [SerializeField] private AudioSource dashSkid; //Also used in Jump Attack
    [SerializeField] private AudioSource throwNeedleEgalis;
    [SerializeField] private AudioSource throwNeedleSwish;
    [SerializeField] private AudioSource jumpSound; //Used in Jump Attack, Spin Needle, Fake Jump & Rest
    [SerializeField] private AudioSource jumpAttackShaw;
    [SerializeField] private AudioSource spinNeedleAdiros;
    [SerializeField] private AudioSource spinNeedleRope;
    [SerializeField] private AudioSource walk;
    [SerializeField] private AudioSource restHit;
    [SerializeField] private AudioSource restNeedleHit;
    [SerializeField] private AudioSource restFinalHit;
    [SerializeField] private AudioSource restFinalBlast;
    [SerializeField] private AudioSource exitSound;
    [SerializeField] private AudioSource startSound;

    public float loopTime = 1f;

    private float buffer = 0;


    void Update()
    {
        buffer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            dash.Play();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            dashSkid.Play();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            throwNeedleEgalis.Play();

        if (Input.GetKeyDown(KeyCode.Alpha4))
            throwNeedleSwish.Play();

        if (Input.GetKeyDown(KeyCode.Alpha5))
            jumpSound.Play();

        if (Input.GetKeyDown(KeyCode.Alpha6))
            jumpAttackShaw.Play();

        if (Input.GetKeyDown(KeyCode.Alpha7))
            spinNeedleAdiros.Play();

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            spinNeedleRope.Play();
            buffer = loopTime;
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            walk.Play();
            buffer = loopTime;
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
            restHit.Play();

        if (Input.GetKeyDown(KeyCode.Keypad0))
            restNeedleHit.Play();

        if (Input.GetKeyDown(KeyCode.Keypad1))
            restFinalHit.Play();

        if (Input.GetKeyDown(KeyCode.Keypad4))
            restFinalBlast.Play();

        if (Input.GetKeyDown(KeyCode.Keypad7))
            exitSound.Play();

        if (Input.GetKeyDown(KeyCode.Keypad2))
            startSound.Play();

        if (buffer < 0)
        {
            walk.Stop();
            spinNeedleRope.Stop();
        }
    }
}

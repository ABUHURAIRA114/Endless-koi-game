using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimBase : MonoBehaviour
{
    [SerializeField] private Transform initPoint, endPoint, saqibT, runPoint;
    [SerializeField] private List<Anim> animations;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioSource iGuess, Kutta, puckHit;
    [SerializeField] private float speed;
    [SerializeField] private bool destroyOnEnd = false;
    private int i = 0;
    private Anim currentAnim;
    private Anim CurrentAnim
    {
        get => currentAnim;
        set
        {
            if (currentAnim != value)
            {
                currentAnim = value;
                i = 0;
            }
        }
    }
    public Action OnEnd;
    private bool playAnim, someOtherControlBool;
    private bool hitPoint => Vector3.Distance(saqibT.transform.position, new Vector3(endPoint.position.x, initPoint.position.y, endPoint.position.z)) < 0.1f;
    private bool runPointReached => Vector3.Distance(saqibT.transform.position, new Vector3(runPoint.position.x, initPoint.position.y- .337f, runPoint.position.z)) < 0.1f;
    public bool PlayAnim
    {
        get => playAnim;
        set
        {
            playAnim = value;
            i = 0;
            someOtherControlBool = false;
            secondStart = false;

        }
    }
    public bool isPlaying, secondStart;

    private void LateUpdate()
    {
        PlayAnimation();
    }

    float t = 0;
    private void PlayAnimation()
    {
        if (Time.timeScale == 0) return;
        if (!PlayAnim)
        {
            if (runPointReached)
            {
                CurrentAnim = animations[(isPlaying ? 3 : 4 )];
                spriteRenderer.sprite = CurrentAnim.sprites[i];

                if (t < 1 / speed)
                {
                    t += Time.deltaTime;
                    return;
                }
                t = 0;

                if (i == CurrentAnim.sprites.Count - 1) i = 0;
                else i++;
            }
            else saqibT.position = Vector3.Lerp(saqibT.position, new Vector3(runPoint.position.x, initPoint.position.y - 0.337f, runPoint.position.z), Time.deltaTime * speed);

            return;
        }

        if (!hitPoint && !someOtherControlBool)
        {
            CurrentAnim = animations[0];
            saqibT.position = Vector3.Lerp(saqibT.position, new Vector3(endPoint.position.x, initPoint.position.y, endPoint.position.z), Time.deltaTime * speed);
        }

        spriteRenderer.sprite = CurrentAnim.sprites[i];
        
        if (!hitPoint && !someOtherControlBool) return;

        if (iGuess.time >= iGuess.clip.length*.85f)
        {
            CurrentAnim = animations[2];
            Kutta.Play();
            secondStart = true;
        }

        if (secondStart)
            CurrentAnim = animations[2];
        else
            CurrentAnim = animations[1];

        if (iGuess.isPlaying == false && secondStart == false)
            iGuess.Play();

        if (t < 1 / speed)
        {
            t += Time.deltaTime;
            return;
        }
     
        if (CurrentAnim == animations[2] && i >= CurrentAnim.sprites.Count - 2)
        {
            someOtherControlBool = true;
            saqibT.position = Vector3.Lerp(saqibT.position, new Vector3(runPoint.position.x, initPoint.position.y - .337f, runPoint.position.z), Time.deltaTime * speed);
        }
     
        t = 0;
        if (CurrentAnim == animations[2] && i == CurrentAnim.sprites.Count - 1)
        {
            if (destroyOnEnd)
                Destroy(gameObject);
            PlayAnim = false;
        }

        if (i == CurrentAnim.sprites.Count - 1)
            i = 0;
        else
           i++;

        if (CurrentAnim == animations[2] && i == CurrentAnim.sprites.Count - 2)
        {
            puckHit.Play();
            OnEnd?.Invoke();
        }
    }

    public void SetAnim(int index)
    {
        if (CurrentAnim != animations[index])
        {
            CurrentAnim = animations[index];
            i = 0;
        }
    }
}

[Serializable]

public class Anim
{
    public List<Sprite> sprites;
}
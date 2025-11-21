using UnityEngine;

public class Tweening
{
    private static bool isTweening = false;
    public static void Tween(RectTransform obj, Vector2 from, Vector2 to, float speed)
    {
        Vector2 targetPos = obj.anchoredPosition;
        if (!isTweening)
        {
            targetPos = from;
            isTweening = true;
        }
        else {
            ODTweening(targetPos.x, to.x, speed);
            ODTweening(targetPos.y, to.y, speed);
        }

        obj.anchoredPosition = targetPos;
    }

    public static void Tween(RectTransform obj, Vector2 to, float speed)
    {
        Vector2 dirVect;
        dirVect = to - obj.anchoredPosition;

        obj.anchoredPosition += dirVect * speed * Time.deltaTime;
    }

    public static float ODTweening(float from, float to, float speed)
    {
        if (Mathf.Abs(from - to) > speed * Time.deltaTime)
        {
            from += ((to - from) / Mathf.Abs(from - to)) * speed * Time.deltaTime;
        }
        else
        {
            from = to;
        }

        return from;
    }
}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomColor : MonoBehaviour
{
    private void OnEnable()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.color = GetRandomColor();
        }
    }

    private Color GetRandomColor()
    {
        float r = Random.Range(0.2f, 1f);
        float g = Random.Range(0.2f, 1f);
        float b = Random.Range(0.2f, 1f);
        return new Color(r, g, b);
    }

}

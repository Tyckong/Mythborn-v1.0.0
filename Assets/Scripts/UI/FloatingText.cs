using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float fadeDuration = 1f;

    private TMP_Text textMesh;
    private float alpha;
    private Color startColor;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
        if (textMesh != null)
        {
            startColor = textMesh.color;
            alpha = startColor.a;
        }
    }

    public void Setup(string text, Color color)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
            textMesh.color = color;
            startColor = color; //Update Base Color depending on damage or healing.
            alpha = color.a;
        }
    }

    void Update()
    {
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);    //Move upwards animation...
        alpha -= (1f / fadeDuration) * Time.deltaTime;                          //Fade away animation...

        if (textMesh != null)
        {
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }
        if (alpha <= 0)                                                         //If invisible => KILL.
        {
            Destroy(gameObject);
        }
    }
}
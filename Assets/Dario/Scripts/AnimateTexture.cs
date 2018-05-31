using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateTexture : MonoBehaviour
{
    public Sprite[] sprites;
    public float framesPerSecond = 10f;

    void Start()
    {
        StartCoroutine(updateTiling());
    }

    private IEnumerator updateTiling()
    {
        while (true)
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                gameObject.GetComponent<Image>().sprite = sprites[i];
                yield return new WaitForSeconds(1f / framesPerSecond);
            }
        }

    }
}

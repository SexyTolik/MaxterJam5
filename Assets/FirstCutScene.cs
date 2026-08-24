using System.Collections;
using UnityEngine;

public class FirstCutScene : MonoBehaviour
{
    public Camera MainCamera;
    public float targetSize = 7.69f;

    void Start()
    {
        StartCoroutine(proletCameri());
    }

    IEnumerator proletCameri()
    {
        while(MainCamera.orthographicSize < targetSize)
        {
            MainCamera.orthographicSize+=0.05f;
            yield return new WaitForSeconds(0.01f);
        }
        Destroy(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    public string nameScene;
    public float duration;
    void Start()
    {
        InvokeRepeating("LoadNextScene",duration,0);
    }

    public void LoadNextScene()
    {
        Debug.Log("LoadScene");
        CutsceneManager.Instance.PlayCuCutsceneSequencetscene(nameScene, duration);
    }
}

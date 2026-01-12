using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest1Starter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        QuestManager.Instance.StartQuest("Q1");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

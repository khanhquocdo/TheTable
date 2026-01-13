using UnityEngine;

public class TriggerWhenTankDestroy : MonoBehaviour
{
 
    void OnDestroy()
    {
        Scene1Story.Instance.RunOutro();
    }
}
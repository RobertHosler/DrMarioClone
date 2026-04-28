using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class LevelDisplay : MonoBehaviour
{
    void Start()
    {
        VirusSpawner spawner = GetComponentInParent<VirusSpawner>();
        GetComponent<TMP_Text>().text = $"Level {spawner.level}";
    }
}

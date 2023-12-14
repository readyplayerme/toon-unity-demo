using System.Collections;
using ReadyPlayerMe.Core;
using UnityEngine;

public class Talk: MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitAndPlaySound());
    }
    private IEnumerator WaitAndPlaySound()
    {
        yield return new WaitForSeconds(1);
        GetComponent<AudioSource>().loop=true;
        GetComponent<VoiceHandler>().PlayCurrentAudioClip();
    }
}

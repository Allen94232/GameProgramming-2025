using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
    private bool isPlaying = true;
    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps != null && isPlaying)
            ps.Play();
    }

    public void TriggerIsPlaying()
    {
        isPlaying = !isPlaying;

        if (ps == null) return;

        if (isPlaying)
            ps.Play();
        else
            ps.Stop();
    }
}

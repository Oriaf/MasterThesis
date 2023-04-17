using UnityEngine;

public class CatheterSound : MonoBehaviour
{
    public double[] chord = { 659.25, 830.61, 987.77 }; //E5, G#5, B5 (E Major Chord)

    private SpatialTone instrument;

    // Start is called before the first frame update
    void Start()
    {
        instrument = new SpatialTone(chord, 0.5f, 48000);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        Vector3 pos = Vector3.zero;

        if (instrument == null) Debug.LogWarning("Catheter Instrument not initialized yet");
        else instrument.sampleInstrument(data, channels, pos);
    }
}

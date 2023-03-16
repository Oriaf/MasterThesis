using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSynthesis : MonoBehaviour
{

	public double[] frequency = {440.0, 554.37, 659.25}; //A4, C#6, E5 (A Major Chord)
	
	public double[] timbreMul = {1, 2, 4, 8};
	public float[] timbreAmp = {1.0f, 0.5f, 0.25f, 0.125f};
	
	public float volume = 0.1f; // Should never be above 0.1f, or there is a risk of damage!
	
	private float gain;
	private double[] phase;
	private double samplingFrequency = 48000.0;
	

    // Start is called before the first frame update
    void Start()
    {
        phase = new double[frequency.Length];
		for(int i = 0; i < frequency.Length; i++) phase[i] = 0.0;
    }

    // Update is called once per frame
    void Update()
    {
		if(volume > 0.1f) volume = 0.1f;
		
		if(Input.GetKeyDown(KeyCode.Space))  gain = volume;
		else if(Input.GetKeyUp(KeyCode.Space)) gain = 0.0f;
		
    }
	
	/*
	 *	Samples the instrument at a given point in the waveform.
	 *	This sample corresponds to the sum of the tonal frequencies making up its waveform.
	 *	The amplitude of the sample is not scaled by volume, but normalized
	 */
	float sampleTone(double phase){
		const double TAO = Mathf.PI * 2;
		
		float toneSample = 0;
		float cumulativeTimbreVol = 0;
		
		for(int i = 0; i < timbreMul.Length; i++){
			double p = phase * timbreMul[i];
			p = p > TAO ? p - TAO : p;
			toneSample += timbreAmp[i] * Mathf.Sin((float) p);
			
			cumulativeTimbreVol += timbreAmp[i];
		}
		toneSample = toneSample / cumulativeTimbreVol;
		
		return toneSample;
	}
	
	/*
	 *	Write the waveform of the instrument to the databuffer for the given tone being played.
	 *  The toneIndex of the instrument corresponds to the length of the frequency array.
	 *	The resulting wave form will be scaled by volume
	 */
	void sampleSynth(float[] data, int channels, int toneIndex){
		double increment = frequency[toneIndex] * 2.0 * Mathf.PI / samplingFrequency;
		
		for(int i = 0; i < data.Length; i += channels){
			phase[toneIndex] += increment;
			if(phase[toneIndex] > Mathf.PI * 2) phase[toneIndex] -= Mathf.PI * 2;
			
			// Sample the tone of the instrument and write it to each channel
			float tone = gain * sampleTone(phase[toneIndex]);
			for(int j = 0; j < channels; j++){
				data[i + j] += tone;
			}
			
		}
	}
	
	void OnAudioFilterRead(float[] data, int channels){
		for(int j = 0; j < data.Length; j++){
				data[j] = 0;
		}
	
		for(int i = 0; i < frequency.Length; i++){
			sampleSynth(data, channels, i);
		}
		
		//Normalize the volume
		for(int i = 0; i < data.Length; i++){
			data[i] = data[i] / (float) frequency.Length;
		}
		
		
	}
}

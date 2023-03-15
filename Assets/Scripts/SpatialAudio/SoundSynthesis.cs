using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSynthesis : MonoBehaviour
{

	public double[] frequency = {440.0, 554.37, 659.25}; //A4, C#6, E5 (A Major Chord)
	
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
	
	void OnAudioFilterRead(float[] data, int channels){
		for(int j = 0; j < data.Length; j++){
				data[j] = 0;
		}
	
		for(int i = 0; i < frequency.Length; i++){
			double increment = frequency[i] * 2.0 * Mathf.PI / samplingFrequency;
		
			for(int j = 0; j < data.Length; j += channels){
				phase[i] += increment;
				
				data[j] += (float)(gain * Mathf.Sin((float) phase[i]));
				
				if(channels == 2) data[j + 1] = data[j];
				
				if(phase[i] > Mathf.PI * 2) phase[i] -= Mathf.PI * 2;
				
			}
		}
		
		
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardChirpOscillator : Oscillator
{
	private int N; // Number of octaves to cover
	double phi;
	private double duration; // Duration in seconds until the frequency is one octave higher
	private double x;

	public ShepardChirpOscillator(double f_start, float g, double sr, int oct, double p, double dur) : base(f_start, g, sr){
		phi = p;
		N = oct;
		duration = dur;
	}
	
	public double getDuration() { return duration; }
	public void setDuration(double d) { duration = d; }
	
	public double getX() { return x; }
	public void setX(double X) { x = X; }

	private double PHI(double t){
		//Debug.Log(t + ", " + (x * t + phi) % 1.0);
	
		return (x * t + phi) % 1.0;
	}
	
	/*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
	override public void sampleTone(float[] data, int channels){
		double increment = 1.0 / sampleRate;
	
		// frequency[0] * System.Math.Pow(2, N * PHI(i, pos.x, time)
		
		double constantPart = 2.0 * System.Math.PI * frequency;
	
		for(int i = 0; i < data.Length; i += channels){
			pos += increment;
			//if(pos > duration) pos -= duration;
			
			// Sample the tone of the instrument and write it to each channel
			double arg = constantPart * System.Math.Pow(2, N * PHI(pos));
			
			float tone = Mathf.Sin((float) arg);
			
			for(int j = 0; j < channels; j++){
				data[i + j] = gain * tone;
			}
		}
	}
}

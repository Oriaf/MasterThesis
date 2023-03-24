using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardTone
{
	// Instrument settings
	private float gain;
	private double sampleRate;
	private int N; // # of Partials
	
	// Partial constants
	private double[] frequency;
	private ShepardChirpOscillator[] partial;
	private double scaleFactorX;
	private double scaleFactorY = 15;

	public ShepardTone(double f0, double g, double sr, int n){
		gain = (float) g;
		sampleRate = sr;
		N = n;
		
		frequency = new double[N];
		partial = new ShepardChirpOscillator[N];
		//partial = new ExpChirpOscillator[N];
		for(int i = 0; i < N; i++){
			frequency[i] = System.Math.Pow(2, i) * f0;
			partial[i] = new ShepardChirpOscillator(frequency[0], (float) 1.0f, sampleRate, N, (double) i / (double) (N - 1));				
		}
		
		scaleFactorX = 4.0 / (double) N; // Make sure that x values are scaled so that the period T = 1 / (xN) of a repetition is > 250 ms (4 Hz)
	}
	
	public void sampleInstrument(float[] data, int channels, Vector3 pos){
		float[] buffer = new float[data.Length];
		
		double increment = 1.0 / (sampleRate);
		
		partial[0].setX(pos.x * scaleFactorX);
		partial[0].setY(pos.y);
		partial[0].sampleTone(data, channels);
		
		for(int i = 1; i < N; i++){
			partial[i].setX(pos.x * scaleFactorX);
			partial[i].setY(pos.y * scaleFactorY);
			partial[i].sampleTone(buffer, channels);

			for(int j = 0; j < data.Length; j++){
				data[j] += buffer[j];
				

			}
		}
		
		// Normalize the sound level
		for(int i = 0; i < data.Length; i++){
			data[i] = data[i] / (N);
		}
	}

}

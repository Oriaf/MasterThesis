using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardTone : Instrument
{
	// Instrument settings
	private int N; // # of Partials
	
	// Partial constants
	//private double[] frequency;
	private ShepardChirpOscillator[] partial;
	private double scaleFactorX;
	private double scaleFactorY = 15;

	public double sigma0 = 0.3;
	public double mu0 = 0.06;

	public ShepardTone(double f0, float g, double sr, int n) : base(f0, g, sr){
		N = n;
		
		partial = new ShepardChirpOscillator[N];
		for(int i = 0; i < N; i++){
			partial[i] = new ShepardChirpOscillator(f0, (float) 1.0f, sampleRate, N, (double) i / (double) (N - 1));				
		}
		
		scaleFactorX = (4.0 / (double) N) / 4.0; // Make sure that x values are scaled so that the period T = 1 / (xN) of a repetition is > 250 ms (4 Hz). Currently > 1s
	}
	
	override public void sampleInstrument(float[] data, int channels, Vector3 pos){
        for (int i = 0; i < N; i++)
        {
			partial[i].SIGMA_0 = sigma0;
			partial[i].MU_0 = mu0;
        }

        float[] buffer = new float[data.Length];
		
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
			data[i] = data[i] / ((float) N);
		}
	}

}

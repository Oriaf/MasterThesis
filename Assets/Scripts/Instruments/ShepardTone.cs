using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardTone
{
	private const double MU_0 = 1.0; // Don't put this above 2!!!!!!
	private const double SIGMA_0 = 1.0; // Don't put this below 1!!!!!
	private const double V1 = 1.0;
	private const double V2 = 1.0;
	private const double OMEGA_MOD = 50;
	
	private const double SHEPARD_DURATION = 1.0; // Duration of a repetition of the shepard tone in seconds
	private const double EPSILON = 0.00000000001;

	// Instrument settings
	private float gain;
	private double sampleRate;
	private int N; // # of Partials
	
	// Partial constants
	private double[] frequency;
	//private SineOscillator[] partial;
	private ShepardChirpOscillator[] partial;
	private double[] phi;
	
	// Calculated values
	private double maxAmp;
	
	// Internal state
	private double time;
	
	//public double getMaxAmp() { return maxAmp; }

	public ShepardTone(double f0, double g, double sr, int n){
		gain = (float) g;
		sampleRate = sr;
		N = n;
		
		maxAmp = -1;
		
		time = 0.0;
		
		frequency = new double[N];
		partial = new ShepardChirpOscillator[N];
		//partial = new ExpChirpOscillator[N];
		phi = new double[N];
		for(int i = 0; i < N; i++){
			frequency[i] = System.Math.Pow(2, i) * f0;
			//ampConst[i] = (System.Math.Cos(System.Math.Log((frequency[i] - f0) / (System.Math.Pow(2, N) * f0))) - 1) / -2;
			phi[i] = (double) i / (double) (N - 1);	
			//partial[i] = new SineOscillator(frequency[i], (float) 1.0f, sampleRate);
			partial[i] = new ShepardChirpOscillator(frequency[0], (float) 1.0f, sampleRate, N, phi[i], SHEPARD_DURATION);				
		}
	}
	
	private bool doubleEqual(double a, double b, double error){
		return System.Math.Abs(a - b) < error;
	}
	private bool floatEqual(float a, float b, float error){
		return Mathf.Abs(a - b) < error;
	}
	
	private double PHI(int i, double x, double t){
		//if((x * t + phi[i]) >= 1.0) Debug.Log("i: " + i + ", PHI_i: " + (x * t + phi[i]) % 1.0 + ", t: " + t);
	
		return (x * t + phi[i]) % 1.0;
	}
	
	private double mu(double z){
		return (z < 0) ? MU_0 : MU_0 - z;
	}
	
	private double sigma(double y){
		return (y < 0) ? SIGMA_0 : SIGMA_0 - V2 * y;
	}
	
	private double A(int i, Vector3 pos, double t){
		double PHI_i = PHI(i, pos.x, t);
		
		double power = System.Math.Pow(PHI_i - mu(pos.z), 2) / (2 * System.Math.Pow(sigma(pos.y), 2));
		double sqrt = System.Math.Sqrt(2 * System.Math.PI * sigma(pos.y));
		
		return System.Math.Pow(System.Math.E, power) / sqrt;
	}
	
	public void sampleInstrument(float[] data, int channels, Vector3 pos){
		float[] buffer = new float[data.Length];
		
		double increment = 1.0 / (sampleRate);
		
		double shepardDuration = (!floatEqual(pos.x, 0.0f, (float) EPSILON)) ? SHEPARD_DURATION / pos.x : System.Double.PositiveInfinity;
		
		partial[0].setX(pos.x);
		partial[0].sampleTone(buffer, channels);
		/*partial[0].setDuration(shepardDuration);
		partial[0].sampleTone(data, channels);*/
		
		double t = time;
		for(int i = 0; i < data.Length; i++){
			data[i] += (float) buffer[i];
			data[i] = (float) A(0, pos, t) * buffer[i];
			//data[i] = 0.0f;
			
			t += increment;
		}
		
		for(int i = 1; i < N; i++){
			//partial[i].setDuration(shepardDuration);
			partial[i].setX(pos.x);
			partial[i].sampleTone(buffer, channels);

			t = time;
			for(int j = 0; j < data.Length; j++){
				//partial[i].setFrequency(frequency[0] * System.Math.Pow(2, N * PHI(i, pos.x, time)));
				data[j] += (float) A(i, pos, t) * buffer[j];
				//data[j] += (float) buffer[j];
				
				t += increment;
			}
		}
		
		// Normalize the sound level
		for(int i = 0; i < data.Length; i++){
			data[i] = gain * data[i] / N;
		}
		
		time += increment * data.Length;
	}

}

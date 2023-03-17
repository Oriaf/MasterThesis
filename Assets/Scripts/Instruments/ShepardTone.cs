using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardTone
{
	private float gain;
	private double sampleRate;
	private int N; // # of Partials
	
	private double[] frequency;
	private SineOscillator[] partial;
	private double[] ampConst;
	
	private double maxAmp;
	
	public double getMaxAmp() { return maxAmp; }

	public ShepardTone(double f0, double g, double sr, int n){
		gain = (float) g;
		sampleRate = sr;
		N = n;
		
		maxAmp = -1;
		
		frequency = new double[N];
		partial = new SineOscillator[N];
		ampConst = new double[N];
		for(int i = 0; i < N; i++){
			frequency[i] = System.Math.Pow(2, i) * f0;
			//ampConst[i] = (System.Math.Cos(System.Math.Log((frequency[i] - f0) / (System.Math.Pow(2, N) * f0))) - 1) / -2;
			
			double phi_i = (double) i / (double) (N - 1);
			double PHI_i = (phi_i) % 1.0; //0 to 1 // Should vary with x and t
			
			const double mu_0 = 0.5;
			double mu = mu_0; // Brightness //Should vary with z
			
			const double sigma_0 = 1.0;
			double sigma = sigma_0; //fullness // Should vary with y
			
			const double omega_mod = 50;
			//double beta = ; //Roughness
			
			double power = System.Math.Pow(PHI_i - mu, 2) / (2 * System.Math.Pow(sigma, 2));
			double sqrt = System.Math.Sqrt(2 * System.Math.PI * sigma);
			
			ampConst[i] = System.Math.Pow(System.Math.E, power) / sqrt;
			if(ampConst[i] > maxAmp) maxAmp = ampConst[i];
			
			partial[i] = new SineOscillator(frequency[i], (float) ampConst[i], sampleRate);
		}
	}
	
	public void sampleInstrument(float[] data, int channels){
		float[] buffer = new float[data.Length];
		
		partial[0].sampleTone(data, channels);
		for(int i = 1; i < N; i++){
			partial[i].sampleTone(buffer, channels);
			
			for(int j = 0; j < data.Length; j++){
				data[j] += buffer[j];
			}
		}
		
	}

}

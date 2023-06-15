using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShepardChirpOscillator : Oscillator
{
    private int N; // Number of octaves to cover
    double phi;
    private double x;
    private double y;

    private double phase = 0.0;

    /*
		The shepard tone is affected by an envelope A(t) indirectly dependent on the frequency of the shepard tone.
		Higher pitches are perceived as louder, hence we want the amplitude function to fall off towards the higher end of the spectra.
		Thus MU_0 should be close to 0 to ensure a gradual roll off of the tone with the pitch.
		Similarly SIGMA_0 should be balanced so that as much of the spectra is included, whilst still making sure the higher pitches go to 0.
	
		MU_0 and SIGMA_0 need to be selected so that if the frequency f = f_0 * 2^(N * PHI(x, t)) is plotted in a log-lin plot
		against the A(t) function, then the bell curve is centered
	*/
    public double MU_0 = 0.3;
    public double SIGMA_0 = 0.06;
    private const double OMEGA_MOD = 2.0 * System.Math.PI * 50;
    private const double a = 1.0;
    private const double b = 1.0;
    private const double c = 0.1;

    public ShepardChirpOscillator(double f_start, float g, double sr, int oct, double p) : base(f_start, g, sr)
    {
        phi = p;
        N = oct;

        //SIGMA_0 = 1.4 / (double) N;
    }

    public double getX() { return x; }
    public void setX(double X) { x = X; }

    public double getY() { return y; }
    public void setY(double Y) { y = Y; }

    private double PHI(double t)
    {
        double a = (x * t + phi);
        double b = 1.0;
        return System.Math.IEEERemainder(a, b); // Mathematical Modulo
    }

    private double A(double t)
    {
        double power = System.Math.Pow(PHI(t) - MU_0, 2) / (-2 * System.Math.Pow(SIGMA_0, 2));
        double sqrt = System.Math.Sqrt(2 * System.Math.PI * SIGMA_0);

        return System.Math.Pow(System.Math.E, power) / sqrt;
    }

    private double G(double t)
    {
        return (y < 0) ? 1.0 + 0.5 * System.Math.Sin(2.0 * System.Math.PI * -y * t) : 1.0;
    }

    private double beta()
    {
        return (y > 0) ? a * System.Math.Pow(y, b) + c : 0;
    }

    /*
	 * Fills a data buffer with samples from the Oscillator.
	 * The data is filled in based on the current settings of the Oscillator
	 */
    override public void sampleTone(float[] data, int channels)
    {

        double constantPart = 2.0 * System.Math.PI * frequency;

        double increment = 1.0 / sampleRate;
        double phaseIncrement = constantPart * System.Math.Pow(2, N * PHI(pos)) / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            pos += increment;
            phase += phaseIncrement;
            if (phase > Mathf.PI * 2) phase -= Mathf.PI * 2;

            // Sample the tone of the instrument and write it to each channel
            double arg = phase; //FM Synthesis

            double disonance = beta() * System.Math.Cos(OMEGA_MOD * pos);

            float tone = Mathf.Cos((float)(arg + disonance));

            for (int j = 0; j < channels; j++)
            {
                data[i + j] = (float)G(pos) * (float) A(pos) * tone;
            }
        }
    }
}

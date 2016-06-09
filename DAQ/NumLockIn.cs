using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using SharedCode;


namespace DAQ
{
    public static class NumLockIn
    {
        public static double GetAmplitude(double[] amps, double modulationFreq, int maxHarmonic, ExperimentParameters parameters)
        {
            double IntegrationTime = (double)parameters.NumberOfSamplesPerIntegrationTime / parameters.SampleRate;
            Complex[] cAmps = new Complex[amps.Length];
            for(int i = 0; i < amps.Length; i++)
            {
                cAmps[i] = new Complex(amps[i], 0.0);
            }
            Fourier.Forward(cAmps, FourierOptions.NoScaling);

            double[] allHarmonics = new double[maxHarmonic];
            double result = 0.0;
            for (int i = 0; i < maxHarmonic; i++)
            {
                allHarmonics[i] = GetAbs(cAmps[(int)(IntegrationTime * modulationFreq)]);
                result += allHarmonics[i];
            }
            
            return result;
        }

        private static double GetAbs(Complex c)
        {
            return Math.Sqrt(c.Real * c.Real + c.Imaginary * c.Imaginary);
        }
    }
}

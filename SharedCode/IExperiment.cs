using System;

namespace SharedCode
{
	public interface IExperiment
	{
		void Initialise(ExperimentParameters p);
		DataPoint SetupAndAcquire(double scanParameterValue);
		void Dispose();
	}
}


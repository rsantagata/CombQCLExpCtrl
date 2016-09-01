using System;

namespace SharedCode
{
	public interface IExperiment
	{
		void Initialise(ExperimentParameters p);
		DataPoint Acquire(double scanParameterValue);
		void Dispose();
	}
}


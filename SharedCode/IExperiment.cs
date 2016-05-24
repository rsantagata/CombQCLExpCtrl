using System;

namespace SharedCode
{
	public interface IExperiment
	{
		void Initialise(ExperimentParameters p);
		DataSet Run();
		void Dispose();
	}
}


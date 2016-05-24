using System;

namespace SharedCode
{
	public interface IExperimentHardware
	{
		void Initialise(ExperimentParameters p);
		DataSet Run();
		void Dispose();
	}
}


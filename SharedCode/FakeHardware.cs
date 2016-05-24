using System;

namespace SharedCode
{
	public class FakeHardware : IExperimentHardware
	{
		DataSet data;
		ExperimentParameters parameters;

		object updateDataLock = new object();

		public FakeHardware ()
		{
		}

		public void Initialise(ExperimentParameters p)
		{
			parameters = p;
			data = new DataSet ();	
		}
		public DataSet Run()
		{
			Random rnd = new Random();
			lock (updateDataLock)
			{
				for (int i = 0; i < parameters.NumberOfPoints; i++) {
                    data.Add(new DataPoint(parameters.AINames, new double[] 
                    { i, rnd.Next(-1, 1) + 10 * Math.Cos(0.1 * i), rnd.Next(-1, 1) + 10 * Math.Cos(0.1 * i + 1.5) ,  rnd.Next(-1, 1) + 10 * Math.Cos(0.1 * i + 2.5) }));
				}
			}
			return data;
		}
		public void Dispose()
		{
			
		}

	}
}


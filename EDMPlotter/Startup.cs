﻿using Microsoft.Owin;
using Owin;
[assembly: OwinStartup(typeof(EDMPlotter.Startup))]

namespace EDMPlotter
{
	public class Startup
	{
		public void Configuration(IAppBuilder app)
		{
			// Any connection or hub wire up and configuration should go here
			app.MapSignalR();
		}
	}
}
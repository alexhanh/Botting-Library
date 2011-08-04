using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;

namespace GR.Gambling.Bot
{
	public abstract class QueuedJob : IInterruptableJob
	{
		public void Execute(JobExecutionContext context)
		{	
			Run(context);

			JobQueue jobQueue = (JobQueue)context.JobDetail.JobDataMap["JobQueue"];
			
			jobQueue.ScheduleNext();
		}

		public void Interrupt()
		{
			Stop();
		}

		public abstract void Run(JobExecutionContext context);
		public abstract void Stop();
	}
}

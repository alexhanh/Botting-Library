using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;

namespace GR.Gambling.Bot
{
	public class QueuedJobDetail
	{
		private DateTime startTime;
		private Type jobType;
		private JobDataMap jobDataMap;

		public QueuedJobDetail(JobDataMap jobDataMap, Type jobType, DateTime startTime)
		{
			this.startTime = startTime;
			this.jobType = jobType;
			this.jobDataMap = jobDataMap;
		}

		public DateTime StartTime { get { return startTime; } }
		public JobDataMap JobDataMap { get { return jobDataMap; } }
		public Type JobType { get { return jobType; } }
	}
}

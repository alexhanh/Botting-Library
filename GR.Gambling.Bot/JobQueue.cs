using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Quartz;

namespace GR.Gambling.Bot
{
	public class JobQueue
	{
		private Queue<QueuedJobDetail> queue = new Queue<QueuedJobDetail>();
		private IScheduler scheduler;
		private string name;
		private long counter = 0;
		private object queueLock = new object();
		private bool scheduled = false;

		public JobQueue(IScheduler scheduler, string queueName)
		{
			this.name = queueName;
			this.scheduler = scheduler;
		}

		public void Enqueue(QueuedJobDetail queuedJobDetail)
		{
			lock (queueLock)
			{
				Console.WriteLine("[" + name + "] enqueue " + queuedJobDetail.JobType);
				queue.Enqueue(queuedJobDetail);

				if (!scheduled)
					ScheduleNext();
			}
		}

		/// <summary>
		/// Don't use this method. It is used by QueuedJob's Execute().
		/// </summary>
		public void ScheduleNext()
		{
			lock (queueLock)
			{
				if (queue.Count > 0)
				{
					QueuedJobDetail queuedJobDetail = queue.Dequeue();

					JobDetail jobDetail = new JobDetail("QueueJob" + counter, null, queuedJobDetail.JobType);
					jobDetail.Volatile = true;

					jobDetail.JobDataMap = queuedJobDetail.JobDataMap;
					jobDetail.JobDataMap["JobQueue"] = this;

					Trigger trigger = new SimpleTrigger("QueueJobTrigger" + counter, null, queuedJobDetail.StartTime);
					
					scheduler.ScheduleJob(jobDetail, trigger);

					counter++;

					scheduled = true;

					return;
				}

				scheduled = false;
			}
		}
	}
}

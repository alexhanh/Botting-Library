using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using GR.Common.Logging;

using Quartz;

using GR.Net.Mail;

namespace GR.Gambling.Bot
{
    public class Global
    {
        private static IExceptionHandler exceptionHandler = new DullExceptionHandler();
        private static ConsoleCapturer consoleCapturer = new ConsoleCapturer(50000);
		//private static IScheduler scheduler;
		//private static JobQueue jobQueue = new JobQueue(

        public static IExceptionHandler ExceptionHandler { get { return exceptionHandler; } set { exceptionHandler = value; } }

        public static ConsoleCapturer ConsoleCapturer { get { return consoleCapturer; } }

		//public static JobQueue JobQueue { get { return jobQueue; } }
    }
}
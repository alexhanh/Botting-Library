using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GR.Gambling.Bot
{
    public interface IExceptionHandler
    {
        void OnException(Exception e);
    }

    public class DullExceptionHandler : IExceptionHandler
    {
        public void OnException(Exception e)
        {
            // Simply re-throw the exception
            throw e;
        }
    }
}

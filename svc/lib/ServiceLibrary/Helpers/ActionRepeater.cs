using System;

namespace ServiceLibrary.Helpers
{
    public class ActionRepeater
    {
        public static int Retry(Action action, int numberOfRetries = 3)
        {
            if (numberOfRetries <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfRetries));
            }

            int counter = 1;
            while (counter <= numberOfRetries)
            {
                try
                {
                    action();
                    return counter;
                }
                catch
                {
                    if (counter == numberOfRetries)
                    {
                        throw;
                    }
                }
                counter++;
            }
            return counter;
        }
    }
}

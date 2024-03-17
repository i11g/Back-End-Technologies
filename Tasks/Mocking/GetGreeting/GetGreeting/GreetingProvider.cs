using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetGreeting
{
    public class GreetingProvider
    {    
        private ITimeProvider _provider;
        public GreetingProvider(ITimeProvider provider)
        {
            this._provider = provider;
            
        }
        public string GetGreeting()
        {
            var hour = _provider.GetCurrentTime().Hour;

            if (hour >= 5 && hour < 12)
            {
                return "Good morning!";
            }
            else if (hour >= 12 && hour < 18)
            {
                return "Good afternoon!";
            }
            else if (hour >= 18 && hour < 22)
            {
                return "Good evening!";
            }
            else
            {
                return "Good night!";
            }
        }
    }

}

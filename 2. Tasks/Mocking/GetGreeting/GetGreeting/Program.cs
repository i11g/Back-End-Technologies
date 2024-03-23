namespace GetGreeting
{
    class Program
    {
        static void Main(string[] args)
        {
            GreetingProvider greetingProvider = new GreetingProvider(new FakeTiemProvider(new DateTime(2002,2,2)));
            string greeting = greetingProvider.GetGreeting();
            Console.WriteLine(greeting);
        }
    }

}
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hi!"); 
    }

    private string MakeGreeting(string name)
    {
        if (name == null) // No braces and spacing issues
            throw new ArgumentNullException("name");
        return "Hello, " + name; 
    }
}

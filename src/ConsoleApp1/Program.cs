// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");


List<String> a = new List<string>() { "A", "B", "C", "E" };
List<String> b = new List<string>() { "A", "B", "C", "D" };


var result = a.Union(b).ToList();

foreach(var item in result)
{
    Console.WriteLine(item);
}
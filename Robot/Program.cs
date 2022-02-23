using Robot;

IRobot rob = new Robot.Robot();
while (true)
{
    var result = rob.Execute(Console.ReadLine());
    Console.WriteLine($"Result: {result.ToString()}");
}




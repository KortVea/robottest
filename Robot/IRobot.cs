namespace Robot;

public interface IRobot
{
    public ExecResult Execute(string command);
}

public enum ExecResult
{
    OK = 0, DENIED, ERROR
}

public enum Bearing
{
    WEST = 0, NORTH, EAST, SOUTH
}
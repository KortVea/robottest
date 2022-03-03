namespace Robot;

public class Robot : IRobot
{
    private int? _x;
    private int? _y;
    private Bearing? _direction;
    private int sideLength;
    private HashSet<(int, int)> toAvoid = new ();

    public Robot(int sideLength)
    {
        this.sideLength = sideLength;
    }
    private bool HasBearing => this.Direction.HasValue;

    private bool IsValidState =>
        this.X != null
        && this.Y != null
        && this.HasBearing;

    private bool IsValidCoordinate(int? i) =>
        i.HasValue && i.Value >= 0 && i.Value <= this.sideLength - 1;
    
    private int? X
    {
        get => this._x;
        set
        {
            if (IsValidCoordinate(value))
            {
                this._x = value;
            }
        }
    }
    
    private int? Y
    {
        get => this._y;
        set
        {
            if (IsValidCoordinate(value))
            {
                this._y = value;
            }
        }
    }

    private Bearing? Direction
    {
        get => this._direction;
        set
        {
            if(value == null)
                return;
            this._direction = value;
        }
    }


    public ExecResult Execute(string? command)
    {
        // info : action actionArgs
        var info = command?.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        
        if (info == null || info.Length == 0)
            return ExecResult.ERROR;
        
        switch (info.Length)
        {
            case 1 when info[0] != "PLACE":
                return (ExecResult) (this
                    .GetType()
                    .GetMethod(info[0])?
                    .Invoke(this, null) ?? ExecResult.ERROR);
            case 2:
            {
                var actionArgs = info[1].Split(',', StringSplitOptions.TrimEntries);
            
                if (actionArgs.Length != 2 && actionArgs.Length != 3)
                    return ExecResult.ERROR;

                if (!int.TryParse(actionArgs[0], out var x) || !int.TryParse(actionArgs[1], out var y))
                    return ExecResult.ERROR;
            
                Bearing? bearingValue = null;
                
                if (actionArgs.Length == 3 
                    && Enum.TryParse<Bearing>(actionArgs[2], out var bearing))
                {
                    bearingValue = bearing;
                }
                    
                return (ExecResult) (
                    this
                        .GetType()
                        .GetMethod(info[0])?
                        .Invoke(this, new object[] {x, y, bearingValue})
                    ?? ExecResult.ERROR);
            }
            default:
                return ExecResult.ERROR;
        }
    }

    public ExecResult PLACE(int x, int y, Bearing? bearing)
    {
        if (!HasBearing && bearing == null)
            return ExecResult.DENIED;

        if (this.toAvoid.Contains((x, y)))
        {
            return ExecResult.DENIED;
        }

        if (!IsValidCoordinate(x) || !IsValidCoordinate(y) )
        {
            return ExecResult.DENIED;
        }
        
        this.X = x;
        this.Y = y;
        this.Direction = bearing;
        return ExecResult.OK;

    }

    public ExecResult LEFT()
    {
        if (!this.HasBearing)
            return ExecResult.DENIED;
        
        switch (this.Direction)
        {
            case Bearing.WEST:
                this.Direction = Bearing.SOUTH;
                break;
            case Bearing.NORTH:
                this.Direction = Bearing.WEST;
                break;
            case Bearing.EAST:
                this.Direction = Bearing.NORTH;
                break;
            case Bearing.SOUTH:
                this.Direction = Bearing.EAST;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return ExecResult.OK;
    }

    public ExecResult AVOID(int x, int y, Bearing? _)
    {
        if (!this.IsValidCoordinate(x) || !this.IsValidCoordinate(y))
            return ExecResult.DENIED;
        
        this.toAvoid.Add((x, y));
        return ExecResult.OK;
    }
    
    public ExecResult RIGHT()
    {
        if (!this.HasBearing)
            return ExecResult.DENIED;
        
        switch (this.Direction)
        {
            case Bearing.WEST:
                this.Direction = Bearing.NORTH;
                break;
            case Bearing.NORTH:
                this.Direction = Bearing.EAST;
                break;
            case Bearing.EAST:
                this.Direction = Bearing.SOUTH;
                break;
            case Bearing.SOUTH:
                this.Direction = Bearing.WEST;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return ExecResult.OK;
    }

    public ExecResult MOVE()
    {
        if (!this.IsValidState)
            return ExecResult.DENIED;
        if (!this.CanMove(this.Direction.Value))
            return ExecResult.DENIED;
        
        switch (this.Direction)
        {
            case Bearing.WEST:
                this.X -= 1;
                break;
            case Bearing.NORTH:
                this.Y += 1;
                break;
            case Bearing.EAST:
                this.X += 1;
                break;
            case Bearing.SOUTH:
                this.Y -= 1;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return ExecResult.OK;
    }

    private bool CanMove(Bearing current)
    {
        if (!this.X.HasValue || !this.Y.HasValue)
        {
            return false;
        }
        switch (current)
        {
            case Bearing.WEST:
                return !this.toAvoid.Contains((this.X.Value - 1, this.Y.Value));
            case Bearing.NORTH:
                return !this.toAvoid.Contains((this.X.Value, this.Y.Value + 1));
            case Bearing.EAST:
                return !this.toAvoid.Contains((this.X.Value + 1, this.Y.Value));
            case Bearing.SOUTH:
                return !this.toAvoid.Contains((this.X.Value, this.Y.Value - 1));
            default:
                throw new ArgumentOutOfRangeException(nameof(current), current, null);
        }
    }
    public ExecResult REPORT()
    {
        var report = this.ToString();
        Console.WriteLine(report);
        return string.IsNullOrEmpty(report) ? ExecResult.DENIED : ExecResult.OK;
    }

    public override string? ToString() =>
        this.IsValidState
            ? $"{this._x},{this._y},{this._direction.ToString()}"
            : null;
}
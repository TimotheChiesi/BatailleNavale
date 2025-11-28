namespace Models;

public class Ship
{
    public char Symbol { get; set; } // A-F
    public int Size { get; set; } // 1-4
    
    public int StartRow { get; set; }
    public int StartCol { get; set; }
    public bool IsHorizontal { get; set; }
}
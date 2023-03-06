namespace Iterum.PacketsGenerator;

public struct BoundedRangeModel
{
    public readonly float min;
    public readonly float max;
    public readonly float precision;
    
    public BoundedRangeModel(float minValue, float maxValue, float precision)
    {
        this.min = minValue;
        this.max = maxValue;
        this.precision = precision;
    }
}
//All credit : http://stackoverflow.com/questions/16199227/optional-return-in-c-net

public struct optional<T>
{
	public bool exists { get; private set; }
	private T val;
	public T data
	{
		get
		{
			if (exists)
				return val;
			else
				throw new System.InvalidOperationException();
		}
		set
		{
			exists = true;
			val = value;
		}
	}

	public optional(T value)
	{
		val = value;
		exists = true;
	}
	
	public static explicit operator T(optional<T> other)
	{
		return other.data;
	}

	public static implicit operator optional<T>(T value)
	{
		return new optional<T>(value);
	}

    public static bool operator ==(optional<T> lhs, optional<T> rhs)
    {
        return (!lhs.exists && !rhs.exists) || (lhs.exists && rhs.exists && lhs.data.Equals(rhs.data));
    }

    public static bool operator !=(optional<T> lhs, optional<T> rhs)
    {
        return !(lhs == rhs);
    }

    public override string ToString()
    {
        if (exists)
        {
            return "exists: " + val;
        }
        else
        {
            return "nonexistent: " + val.GetType().ToString(); 
        }
    }
}
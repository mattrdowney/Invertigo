//Forgot to give credit! : http://stackoverflow.com/questions/16199227/optional-return-in-c-net

public struct Optional<T>
{
	public bool HasValue { get; private set; }
	private T val;
	public T Value
	{
		get
		{
			if (HasValue)
				return val;
			else
				throw new System.InvalidOperationException();
		}
		set
		{
			HasValue = true;
			val = value;
		}
	}
	
	public Optional(T value)
	{
		val = value;
		HasValue = true;
	}
	
	public static explicit operator T(Optional<T> optional)
	{
		return optional.Value;
	}
	public static implicit operator Optional<T>(T value)
	{
		return new Optional<T>(value);
	}
	
	public override bool Equals(object obj)
	{
		if (obj is Optional<T>)
			return this.Equals((Optional<T>)obj);
		else
			return false;
	}
	public bool Equals(Optional<T> other)
	{
		if (HasValue && other.HasValue)
			return object.Equals(val, other.val);
		else
			return HasValue == other.HasValue;
	}
}
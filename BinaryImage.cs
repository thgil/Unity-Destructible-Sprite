using System.Collections;

public struct BinaryImage
{
	public int x;				// Width of the reference texture
	public int y;				// Height of the reference texture
	private BitArray b;	// BinaryImage of the reference texture
	public int Length;

	public BinaryImage(int x, int y)
	{
		this.x = x;
		this.y = y;

		b = new BitArray(x*y);
		Length = b.Length;
	}

	public BinaryImage(int x, int y, bool b)
	{
		this.x = x;
		this.y = y;

		this.b = new BitArray(x*y, b);
		Length = this.b.Length;
	}

	public void Set(int i, bool v)
	{
		b.Set(i, v);
	}

	public bool Get(int i)
	{
		return b.Get(i);
	}
}

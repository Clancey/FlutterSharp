// Compatibility aliases for generated APIs that surface generic Flutter types
// without explicit type arguments.

namespace Flutter
{
	/// <summary>
	/// Non-generic compatibility alias for generated references to Animation.
	/// </summary>
	public class Animation : Animation<object>
	{
	}

	/// <summary>
	/// Non-generic compatibility alias for generated references to ValueListenable.
	/// </summary>
	public interface ValueListenable : ValueListenable<object>
	{
	}
}

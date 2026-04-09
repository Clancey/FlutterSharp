using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FlutterSharp.CodeGen.Config
{
	/// <summary>
	/// Loads the default UI surface policy from disk.
	/// </summary>
	public static class UiSurfacePolicyLoader
	{
		private const string DefaultPolicyRelativePath = "Config/UiSurfacePolicy.json";

		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};

		/// <summary>
		/// Resolves and loads the default policy file.
		/// </summary>
		public static (UiSurfacePolicy Policy, string Path) LoadDefault()
		{
			var path = ResolveDefaultPolicyPath()
				?? throw new FileNotFoundException($"Could not locate {DefaultPolicyRelativePath}.");

			var json = File.ReadAllText(path);
			var policy = JsonSerializer.Deserialize<UiSurfacePolicy>(json, JsonOptions)
				?? throw new InvalidOperationException($"Failed to deserialize UI surface policy from '{path}'.");

			return (policy, path);
		}

		private static string? ResolveDefaultPolicyPath()
		{
			var baseDirCandidate = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultPolicyRelativePath);
			if (File.Exists(baseDirCandidate))
			{
				return baseDirCandidate;
			}

			var currentDirCandidate = Path.Combine(Directory.GetCurrentDirectory(), DefaultPolicyRelativePath);
			if (File.Exists(currentDirCandidate))
			{
				return currentDirCandidate;
			}

			for (var current = new DirectoryInfo(Directory.GetCurrentDirectory()); current != null; current = current.Parent)
			{
				var candidate = Path.Combine(current.FullName, DefaultPolicyRelativePath);
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}

			return null;
		}
	}
}

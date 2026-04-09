using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FlutterSharp.CodeGen.Models;

namespace FlutterSharp.CodeGen.Analysis
{
	/// <summary>
	/// Persists UI surface manifests to disk.
	/// </summary>
	public static class UiSurfaceManifestStore
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			WriteIndented = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Converters =
			{
				new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
			}
		};

		/// <summary>
		/// Writes the manifest to disk.
		/// </summary>
		public static async Task SaveAsync(UiSurfaceManifest manifest, string path, CancellationToken cancellationToken)
		{
			var directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrWhiteSpace(directory))
			{
				Directory.CreateDirectory(directory);
			}

			await using var stream = File.Create(path);
			await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
		}

		/// <summary>
		/// Loads the manifest from disk.
		/// </summary>
		public static async Task<UiSurfaceManifest> LoadAsync(string path, CancellationToken cancellationToken)
		{
			await using var stream = File.OpenRead(path);
			return await JsonSerializer.DeserializeAsync<UiSurfaceManifest>(stream, JsonOptions, cancellationToken)
				?? throw new InvalidDataException($"Failed to deserialize UI surface manifest from '{path}'.");
		}
	}
}

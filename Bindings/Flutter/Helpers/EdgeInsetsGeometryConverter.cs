using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Flutter {
	public class EdgeInsetsGeometryConverter : JsonConverter<EdgeInsetsGeometry> {
		public override EdgeInsetsGeometry ReadJson (JsonReader reader, Type objectType, [AllowNull] EdgeInsetsGeometry existingValue, bool hasExistingValue, JsonSerializer serializer)
			=> EdgeInsetsGeometry.Parse (reader.ReadAsString ());

		public override void WriteJson (JsonWriter writer, [AllowNull] EdgeInsetsGeometry value, JsonSerializer serializer)
		{
			serializer.Serialize (writer, value.ToString());
		}
	}
}
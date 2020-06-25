using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Flutter {
	public class FlutterObjectConverter : JsonConverter<FlutterObject> {
		public override FlutterObject ReadJson (JsonReader reader, Type objectType, [AllowNull] FlutterObject existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var wiget = existingValue ?? (Widget)Activator.CreateInstance (objectType);
			wiget.properties = serializer.Deserialize<Dictionary<string, object>> (reader);
			return wiget;
		}

		public override void WriteJson (JsonWriter writer, [AllowNull] FlutterObject value, JsonSerializer serializer)
		{
			value.BeforeJSon ();
			serializer.Serialize (writer,value.properties);
		}
	}
}

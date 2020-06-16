﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NGettext.Loaders
{
	internal class ContentType
	{
		private static readonly Regex Regex = new Regex(@"^(?<type>\w+)\/(?<subType>\w+)(?:\s*;\s*(?<paramName>\w+)\s*=\s*(?<paramValue>(?:[0-9\w_-]+)|(?:"".+ "")))*", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public ContentType(string contentType)
		{
			if (contentType == null)
				throw new ArgumentNullException(nameof(contentType));
			if (string.IsNullOrEmpty(contentType))
				throw new ArgumentException("Parameter cannot be an empty string", nameof(contentType));

			Source = contentType;
			parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			ParseValue();
		}

		private IDictionary<string, string> parameters;

		public string Source { get; private set; }
		public string Type { get; private set; }
		public string SubType { get; private set; }
        public string MediaType => Type + "/" + MediaType;

        public string CharSet => GetParameter("charset");

        public string GetParameter(string name)
		{
            parameters.TryGetValue(name, out string value);
            return value;
		}

		private void ParseValue()
		{
			var match = Regex.Match(Source);
			if (!match.Success)
				throw new FormatException("Failed to parse content type: invalid format");

			Type = match.Groups["type"].Value;
			SubType = match.Groups["subType"].Value;

			var paramNames = match.Groups["paramName"].Captures;
			var paramValues = match.Groups["paramValue"].Captures;

			for (var i = 0; i < paramNames.Count; i++)
			{
                Capture paramName = paramNames[i];
                Capture paramValue = paramValues[i];

                string name = paramName.Value;
                string value = paramValue.Value;

				parameters[name] = value;
			}
		}
	}
}

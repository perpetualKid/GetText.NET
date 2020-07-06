using System;
using System.Collections.Generic;
using System.Reflection;

namespace GetText.WindowsForms
{
    internal class PropertiesValuesStore : Dictionary<string, object>
	{
	}

	public class ObjectPropertiesStore
	{
		private readonly Dictionary<int, PropertiesValuesStore> store = new Dictionary<int, PropertiesValuesStore>();

		public ObjectPropertiesStore()
		{
		}

		public void SetState(object obj, string propertyName)
		{
			SetState(obj, propertyName, null);
		}

		public void SetState(object obj, string propertyName, object value)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (value == null)
			{
				PropertyInfo pi = obj.GetType().GetProperty(propertyName);
				if (pi != null && pi.CanRead)
					value = pi.GetValue(obj, null);
				else
					throw new Exception($"Property '{propertyName}' not exists or write-only. Object: {obj}");
			}
			PropertiesValuesStore propStore;
			store.TryGetValue(obj.GetHashCode(), out propStore);
			if (propStore == null)
			{
				propStore = new PropertiesValuesStore();
				store.Add(obj.GetHashCode(), propStore);
			}

			if (propStore.ContainsKey(propertyName))
			{
				propStore[propertyName] = value;
			}
			else
			{
				propStore.Add(propertyName, value);
			}
		}

		public object GetState(object obj, string propertyName)
		{
			if (obj == null)
				throw new ArgumentNullException(nameof(obj));

			if (!store.TryGetValue(obj.GetHashCode(), out PropertiesValuesStore propStore))
				return null;

			propStore.TryGetValue(propertyName, out object result);
			return result;
		}

		public string GetStateString(object obj, string propertyName)
		{
			return GetState(obj, propertyName)?.ToString();
		}
	}
}

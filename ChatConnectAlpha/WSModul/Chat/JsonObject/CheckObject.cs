using System;
using System.Linq;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections.Generic;

namespace ChatConnect.WebModul.Chat.JsonObject
{
    static class JsCheckObject
    {
        public static T JsCheckDesirializer<T>(JsonTextReader jsreader)
        {
			return (T)JsObjectDesirializer(typeof(T), jsreader);
        }
		private static object JsObjectDesirializer(Type type, JsonTextReader jsreader)
        {
            List<PropertyInfo> collection = type.GetProperties().ToList();

            if (jsreader.TokenType != JsonToken.StartObject)
                return null;

            object Obj = Activator.CreateInstance(type);

            PropertyInfo jsproperty = null;
            while (jsreader.Read())
            {
                if (jsproperty != null)
                {
					Type t = jsproperty.PropertyType;

					if (t.IsArray)
					{
						IList<object> arr = JsArrayDesirializer(t, jsreader);
						if (arr == null)
							return null;
						jsproperty.SetValue(Obj, arr.ToArray());
					}
					if (t.IsClass &&
						t.Assembly.FullName == type.Assembly.FullName)
                    {
                        object obj = JsObjectDesirializer(t, jsreader);
                        if (obj == null)
                            return null;
                        jsproperty.SetValue(Obj, obj);
                    }
					if (t.IsGenericType &&
						t.GetGenericTypeDefinition() == typeof(IList<>))
					{
						IList<object> arr = JsArrayDesirializer(t, jsreader);
						if (arr == null)
							return null;
						jsproperty.SetValue(Obj, arr);
					}
				}

                if (collection.Count == 0)
                    break;
                switch (jsreader.TokenType)
                {
                    case JsonToken.Date:
                        if (jsproperty == null)
                            return null;
                        jsproperty.SetValue(Obj, Convert.ToDateTime(jsreader.Value));
                        if (collection.Contains(jsproperty))
                            collection.Remove(jsproperty);
                        else
                            return null;
                        break;
                    case JsonToken.Float:
                        if (jsproperty == null)
                            return null;
                        if (jsproperty.PropertyType == typeof(float))
                            jsproperty.SetValue(Obj, Convert.ToDecimal(jsreader.Value));
                        else if (jsproperty.PropertyType == typeof(double))
                            jsproperty.SetValue(Obj, Convert.ToDouble(jsreader.Value));
                        else
                            return null;
                        if (collection.Contains(jsproperty))
                            collection.Remove(jsproperty);
                        else
                            return null;
                        break;
                    case JsonToken.String:
                        if (jsproperty == null)
                            return null;
                        if (jsproperty.PropertyType == typeof(string))
                            jsproperty.SetValue(Obj, Convert.ToString(jsreader.Value));
                        else
                            return null;
                        if (collection.Contains(jsproperty))
                            collection.Remove(jsproperty);
                        else
                            return null;
                        break;
                    case JsonToken.Integer:
                        if (jsproperty == null)
                            return null;
						if (jsproperty.PropertyType.IsEnum)
							jsproperty.SetValue(Obj, Convert.ChangeType(jsreader.Value, 
								      Enum.GetUnderlyingType(jsproperty.PropertyType)));
                        else if (jsproperty.PropertyType == typeof(int))
                            jsproperty.SetValue(Obj, Convert.ToInt32( jsreader.Value ));
                        else if (jsproperty.PropertyType == typeof(long))
                            jsproperty.SetValue(Obj, Convert.ToInt64( jsreader.Value ));
                        else
                            return null;
                        if (collection.Contains(jsproperty))
                            collection.Remove(jsproperty);
                        else
                            return null;
                        break;
                    case JsonToken.Boolean:
                        if (jsproperty == null)
                            return null;
                        if (jsproperty.PropertyType != typeof(bool))
                            jsproperty.SetValue(Obj, Convert.ToBoolean(jsreader.Value));
                        else
                            return null;
                        if (collection.Contains(jsproperty))
                            collection.Remove(jsproperty);
                        else
                            return null;
                        break;
                    case JsonToken.PropertyName:
						jsproperty = Obj.GetType().GetProperty((string)jsreader.Value);
                        if (jsproperty == null)
                            return null;
                        break;
					
                    default:
                        return null;
                }
            }
            if (jsreader.TokenType == JsonToken.EndObject)
            {
                if (collection.Count == 0)
                    return Obj;
                else
                    return null;
            }
            else
                return null;
        }
		private static IList<object> JsArrayDesirializer(Type type, JsonTextReader jsreader)
		{
			IList<object> tokenlist = new List<object>();
			if (jsreader.TokenType != JsonToken.StartArray)
				return null;

					while (true)
					{
						if (jsreader.TokenType == JsonToken.EndArray)
							return tokenlist;

						object obj = JsObjectDesirializer(type.GenericTypeArguments[0], jsreader);
						if (obj == null)
							return null;
				
						tokenlist.Add(obj);
					}
		}
	}
}

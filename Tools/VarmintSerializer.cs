using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace MonoVarmint.Tools
{
    //--------------------------------------------------------------------------------------
    /// Attributes
    //--------------------------------------------------------------------------------------
    public class VarmintSerializerIgnoreAttribute : Attribute { }

    //--------------------------------------------------------------------------------------
    /// <summary>
    /// No-nonsense serializer that just works
    /// </summary>
    //--------------------------------------------------------------------------------------
    public class VarmintSerializer
    {
        Dictionary<object, int> _serializedObjects = new Dictionary<object, int>();
        Dictionary<int, object> _objectCache = new Dictionary<int, object>();

        bool _bestEffort;

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ctor - external construction not allowed
        /// </summary>
        //--------------------------------------------------------------------------------------
        private VarmintSerializer(bool bestEffort)
        {
            _bestEffort = bestEffort;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Short names
        /// </summary>
        //--------------------------------------------------------------------------------------
        class _ID
        {
            public const string Type = "Typ";
            public const string Object = "Obj";
            public const string Property = "Prop";
            public const string Name = "Name";
            public const string Value = "Val";
            public const string InstanceId = "ID";
            public const string ArrayElement = "_";
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SerializeObject
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void SerializeObject(XmlWriter writer, object serializeMe)
        {
            if (serializeMe == null)
            {
                // Don't write anything
            }
            else if (_serializedObjects.ContainsKey(serializeMe))
            {
                writer.WriteStartElement(_ID.Object);
                var cachedInstanceId = _serializedObjects[serializeMe];
                writer.WriteAttributeString(_ID.InstanceId, cachedInstanceId.ToString());
                writer.WriteEndElement();
            }
            else
            {
                var instanceId = _serializedObjects.Count;
                _serializedObjects.Add(serializeMe, instanceId);
                var type = serializeMe.GetType();
                    writer.WriteStartElement(_ID.Object);
                    writer.WriteAttributeString(_ID.Type, type.FullName);
                    writer.WriteAttributeString(_ID.InstanceId, instanceId.ToString());
                if (type.IsArray)
                {
                    Array array = serializeMe as Array;
                    var elementType = type.GetElementType();
                    foreach (var element in array)
                    {
                        writer.WriteStartElement(_ID.ArrayElement);
                        SerializeValue(writer, elementType, element);
                        writer.WriteEndElement();
                    }
                }
                else if(type.IsGenericType)
                {
                    throw new ApplicationException("Can't do generic types yet");
                }
                else
                {
                    foreach (var propertyInfo in type.GetProperties())
                    {
                        SerializeProperty(writer, propertyInfo, serializeMe);
                    }
                }
                    writer.WriteEndElement();
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SerializeValue
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void SerializeValue(XmlWriter writer, Type valueType, object value)
        {
            if (value == null) return;
            if (valueType.IsValueType || valueType.Name == "String" || valueType.IsEnum)
            {
                writer.WriteAttributeString(_ID.Value, value.ToString());
            }
            else
            {
                SerializeObject(writer, value);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SerializeProperty
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void SerializeProperty(XmlWriter writer, PropertyInfo propertyInfo, object serializeMe)
        {
            // Don't serialize ignored properties
            var ignoreAttribute = (VarmintSerializerIgnoreAttribute)propertyInfo.
                GetCustomAttribute(typeof(VarmintSerializerIgnoreAttribute));
            if (ignoreAttribute != null)
            {
                return;
            }

            var value = propertyInfo.GetValue(serializeMe);

            // No need to serialize null values
            if (value == null) return;

            writer.WriteStartElement(_ID.Property);
            writer.WriteAttributeString(_ID.Name, propertyInfo.Name);
            SerializeValue(writer, propertyInfo.PropertyType, value);
            writer.WriteEndElement();
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// SerializeInternal
        /// </summary>
        //--------------------------------------------------------------------------------------
        private string SerializeInternal(object serializeMe)
        {
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "  ";
            var output = new StringWriter();
            using (var writer = XmlWriter.Create(output, settings))
            {
                SerializeObject(writer, serializeMe);
            }

            return output.ToString();
        }

        private static Dictionary<string, Type> _cachedTypes = new Dictionary<string, Type>();
        private static List<Assembly> _knownAssemblies = new List<Assembly>();

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeclareAssembly - call this to help the deserialize know where to find your types
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static void DeclareAssembly(Assembly assembly)
        {
            if (!_knownAssemblies.Contains(assembly))
            {
                _knownAssemblies.Add(assembly);
            }
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// GetWidgetType - Find and cache the widget type
        /// </summary>
        //--------------------------------------------------------------------------------------
        private static Type GetCachedType(string typeName)
        {
            if (_cachedTypes.ContainsKey(typeName)) return _cachedTypes[typeName];
            var searchName = typeName;

            var isArray = false;
            if(typeName.EndsWith("[]"))
            {
                isArray = true;
                searchName = typeName.Substring(0, typeName.Length - 2);
            }

            Type foundType = null;
            foreach (var assembly in _knownAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (searchName == type.FullName)
                    {
                        foundType = type;
                        break;
                    }
                }
            }

            if(foundType == null)
            {
                throw new ApplicationException("Could not find type: " + typeName);
            }
            if (isArray) foundType = foundType.MakeArrayType();
            _cachedTypes[typeName] = foundType;
            return foundType;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadValue
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object ReadValue(XmlReader reader, Type valueType)
        {
            object output;
            if (valueType.IsValueType || valueType.Name == "String")
            {
                var text = reader.GetAttribute(_ID.Value);
                if (valueType.IsEnum)
                {
                    output = Enum.Parse(valueType, text);
                }
                else
                {
                    switch (valueType.Name)
                    {
                        case "String": output = text; break;
                        case "Int16": output = Int16.Parse(text); break;
                        case "UInt16": output = UInt16.Parse(text); break;
                        case "Int32": output = Int32.Parse(text); break;
                        case "UInt32": output = UInt32.Parse(text); break;
                        case "Int64": output = Int64.Parse(text); break;
                        case "UInt64": output = UInt64.Parse(text); break;
                        case "Double": output = Double.Parse(text); break;
                        case "Single": output = Single.Parse(text); break;
                        case "DateTime": output = DateTime.Parse(text); break;
                        case "Boolean": output = Boolean.Parse(text); break;
                        case "Char": output = text[0]; break;
                        case "Byte": output = Byte.Parse(text); break;
                        default: throw new ApplicationException("Don't know how to DeSerialize " + valueType.Name);
                    }
                }
            }
            else
            {
                if (reader.IsEmptyElement) output = null;
                else
                {
                    reader.Read();
                    output = ReadObject(reader);
                }
            }

            return output;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadProperty
        /// </summary>
        //--------------------------------------------------------------------------------------
        private void ReadProperty(XmlReader reader, Type parentType, object parent)
        {
            if(reader.NodeType != XmlNodeType.Element || reader.Name != _ID.Property)
            {
                throw new ApplicationException("Expected reader to be on a Property");
            }

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    var propertyName = reader.GetAttribute(_ID.Name);
                    var propertyInfo = parentType.GetProperty(propertyName);
                    if (propertyInfo == null)
                    {
                        if (!_bestEffort)
                        {
                            throw new ApplicationException("Could not find property " + propertyName + " on " + parentType.Name);
                        }
                    }
                    else
                    {
                        var value = ReadValue(reader, propertyInfo.PropertyType);
                        if (propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic)
                        {
                            propertyInfo.SetValue(parent, value);
                        }
                    }

                    if (reader.IsEmptyElement) return;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != _ID.Property) throw new ApplicationException("Expected end of Property but got end of '" + reader.Name + "'");
                    break;
                }
            } while (reader.Read());
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadArrayElement
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object ReadArrayElement(XmlReader reader, Type elementType)
        {
            object output = null;
            if (reader.NodeType != XmlNodeType.Element || reader.Name != _ID.ArrayElement)
            {
                throw new ApplicationException("Expected reader to be on an array element");
            }

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    output = ReadValue(reader, elementType);

                    if (reader.IsEmptyElement) break;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != _ID.ArrayElement) throw new ApplicationException("Expected end of ArrayElement but got end of '" + reader.Name + "'");
                    break;
                }
            } while (reader.Read());

            return output;
        }


        //--------------------------------------------------------------------------------------
        /// <summary>
        /// ReadObject
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object ReadObject(XmlReader reader)
        {
            object output = null;
            Type nodeType = null;

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch(reader.Name)
                    {
                        case _ID.Object:
                            if(output != null) throw new ApplicationException("Object was already constructed");
                            var instanceId = int.Parse(reader.GetAttribute(_ID.InstanceId));
                            var typeName = reader.GetAttribute(_ID.Type);

                            // No type means that this has already been read and should be cached
                            if (typeName == null) 
                            {
                                if (!_objectCache.ContainsKey(instanceId)) throw new ApplicationException("Could not find cached object with id: " + instanceId);
                                reader.Read(); 
                                return _objectCache[instanceId];
                            }
                            else
                            {
                                nodeType = GetCachedType(typeName);
                                if (nodeType.IsArray)
                                {
                                    output = ReadArray(reader, nodeType);
                                    reader.Read();
                                    return output;
                                }
                                else
                                {
                                    output = Activator.CreateInstance(nodeType);
                                }
                                _objectCache[instanceId] = output;
                            }
                            break;
                        case _ID.Property:
                            if (output == null) throw new ApplicationException("Got a field, but no object was constructed");
                            ReadProperty(reader, nodeType, output);
                            break;
                        default:
                            throw new ApplicationException("Did not expect serial note type: " + reader.Name);
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != _ID.Object)  throw new ApplicationException("Expected end of Object but got end of '" + reader.Name + "'");
                    break;
                }
            } while (reader.Read());

            return output;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeserializeInternal
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object ReadArray(XmlReader reader, Type arrayType)
        {
            if (reader.Name != _ID.Object) throw new ApplicationException("Expected object node.");
            var collectedObjects = new List<object>();
            var elementType = arrayType.GetElementType();

            if (reader.IsEmptyElement)
            {
                return Array.CreateInstance(elementType, 0);
            }

            reader.Read();

            do
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name != _ID.ArrayElement) throw new ApplicationException("Expected an array element, but got " + reader.Name);
                    collectedObjects.Add(ReadArrayElement(reader, elementType));
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name != _ID.Object) throw new ApplicationException("Expected end of Object but got end of '" + reader.Name + "'");
                    break;
                }
            } while (reader.Read());

            var output = Array.CreateInstance(elementType, collectedObjects.Count);
            for(int i = 0; i < collectedObjects.Count; i++)
            {
                output.SetValue(collectedObjects[i], i);
            }

            return output;
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeserializeInternal
        /// </summary>
        //--------------------------------------------------------------------------------------
        private object DeserializeInternal(string serializerOutput, params Assembly[] typeSources)
        {
            foreach(var assembly in typeSources)
            {
                if (!_knownAssemblies.Contains(assembly)) _knownAssemblies.Add(assembly);
            }

            var arrayAssembly = typeof(Array).GetTypeInfo().Assembly;
            if (!_knownAssemblies.Contains(arrayAssembly)) _knownAssemblies.Add(arrayAssembly);

            using (var reader = XmlReader.Create(new StringReader(serializerOutput)))
            {
                var lineInfo = (IXmlLineInfo)reader;
                try
                {
                    return ReadObject(reader);
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Serializer parse error on line " + lineInfo.LineNumber
                        + ": " + e.Message, e);
                }
            }

        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// Serialize - turn an object into XML
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static string Serialize(object serializeMe, bool bestEffort = true)
        {
            var serializer = new VarmintSerializer(bestEffort);
            return serializer.SerializeInternal(serializeMe);
        }

        //--------------------------------------------------------------------------------------
        /// <summary>
        /// DeSerialize - turn xml into an object
        /// </summary>
        //--------------------------------------------------------------------------------------
        public static object DeSerialize(string serializerOutput, bool bestEffort = true)
        {
            var serializer = new VarmintSerializer(bestEffort);
            return serializer.DeserializeInternal(serializerOutput, Assembly.GetCallingAssembly(), Assembly.GetExecutingAssembly());
        }
    }
}

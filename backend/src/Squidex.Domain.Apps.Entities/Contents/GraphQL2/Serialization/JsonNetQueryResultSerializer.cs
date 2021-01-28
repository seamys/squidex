// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using Newtonsoft.Json;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL2.Serialization
{
    public sealed class JsonNetQueryResultSerializer : IQueryResultSerializer
    {
        private readonly JsonSerializer serializer;

        public JsonNetQueryResultSerializer(JsonSerializerSettings serializerSettings)
        {
            this.serializer = JsonSerializer.CreateDefault(serializerSettings);
        }

        public Task<string> SerializeToStringAsync(IQueryResult result)
        {
            var stringBuilder = new StringBuilder();

            using (var writer = new JsonTextWriter(new StringWriter(stringBuilder))
            {
                Formatting = Formatting.Indented
            })
            {
                WriteResult(writer, result);
            }

            return Task.FromResult(stringBuilder.ToString());
        }

        public Task SerializeAsync(IQueryResult result, Stream stream, CancellationToken cancellationToken = default)
        {
            using (var writer = new JsonTextWriter(new StreamWriter(stream))
            {
                Formatting = Formatting.Indented
            })
            {
                WriteResult(writer, result);
            }

            return Task.CompletedTask;
        }

        private void WriteResult(JsonWriter writer, IQueryResult result)
        {
            writer.WriteStartObject();

            WritePatchInfo(writer, result);
            WriteErrors(writer, result.Errors);
            WriteData(writer, result.Data);
            WriteExtensions(writer, result.Extensions);
            WriteHasNext(writer, result);

            writer.WriteEndObject();
        }

        private static void WritePatchInfo(JsonWriter writer, IQueryResult result)
        {
            if (result.Label != null)
            {
                writer.WritePropertyName("label");
                writer.WriteValue(result.Label);
            }

            if (result.Path != null)
            {
                WritePath(writer, result.Path);
            }
        }

        private static void WriteHasNext(JsonWriter writer, IQueryResult result)
        {
            if (result.HasNext.HasValue)
            {
                writer.WritePropertyName("hasNext");
                writer.WriteValue(result.HasNext.Value);
            }
        }

        private void WriteData(JsonWriter writer, IReadOnlyDictionary<string, object?>? data)
        {
            if (data != null && data.Count > 0)
            {
                writer.WritePropertyName("data");

                if (data is IResultMap resultMap)
                {
                    WriteResultMap(writer, resultMap);
                }
                else
                {
                    WriteDictionary(writer, data);
                }
            }
        }

        private void WriteErrors(JsonWriter writer, IReadOnlyList<IError>? errors)
        {
            if (errors != null && errors.Count > 0)
            {
                writer.WritePropertyName("errors");

                writer.WriteStartArray();

                for (var i = 0; i < errors.Count; i++)
                {
                    WriteError(writer, errors[i]);
                }

                writer.WriteEndArray();
            }
        }

        private void WriteError(JsonWriter writer, IError error)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("message");
            writer.WriteValue(error.Message);

            WriteLocations(writer, error.Locations);
            WritePath(writer, error.Path);
            WriteExtensions(writer, error.Extensions);

            writer.WriteEndObject();
        }

        private static void WriteLocations(JsonWriter writer, IReadOnlyList<Location>? locations)
        {
            if (locations != null && locations.Count > 0)
            {
                writer.WritePropertyName("locations");

                writer.WriteStartArray();

                for (var i = 0; i < locations.Count; i++)
                {
                    WriteLocation(writer, locations[i]);
                }

                writer.WriteEndArray();
            }
        }

        private static void WriteLocation(JsonWriter writer, Location location)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("line");
            writer.WriteValue(location.Line);

            writer.WritePropertyName("column");
            writer.WriteValue(location.Column);

            writer.WriteEndObject();
        }

        private static void WritePath(JsonWriter writer, HotChocolate.Path? path)
        {
            if (path != null && path is not RootPathSegment)
            {
                writer.WritePropertyName("path");

                WritePathValue(writer, path);
            }
        }

        private static void WritePathValue(JsonWriter writer, HotChocolate.Path path)
        {
            if (path is RootPathSegment)
            {
                writer.WriteStartArray();
                writer.WriteEndArray();
                return;
            }

            writer.WriteStartArray();

            IReadOnlyList<object> list = path.ToList();

            for (var i = 0; i < list.Count; i++)
            {
                switch (list[i])
                {
                    case NameString n:
                        writer.WriteValue(n.Value);
                        break;

                    case string s:
                        writer.WriteValue(s);
                        break;

                    case int n:
                        writer.WriteValue(n);
                        break;

                    case short n:
                        writer.WriteValue(n);
                        break;

                    case long n:
                        writer.WriteValue(n);
                        break;

                    default:
                        writer.WriteValue(list[i].ToString());
                        break;
                }
            }

            writer.WriteEndArray();
        }

        private void WriteExtensions(JsonWriter writer, IReadOnlyDictionary<string, object?>? dict)
        {
            if (dict != null && dict.Count > 0)
            {
                writer.WritePropertyName("extensions");

                WriteDictionary(writer, dict);
            }
        }

        private void WriteDictionary(JsonWriter writer, IReadOnlyDictionary<string, object?> dictionary)
        {
            writer.WriteStartObject();

            foreach (KeyValuePair<string, object?> item in dictionary)
            {
                writer.WritePropertyName(item.Key);

                WriteFieldValue(writer, item.Value);
            }

            writer.WriteEndObject();
        }

        private void WriteResultMap(JsonWriter writer, IResultMap resultMap)
        {
            writer.WriteStartObject();

            for (var i = 0; i < resultMap.Count; i++)
            {
                ResultValue value = resultMap[i];

                if (value.HasValue)
                {
                    writer.WritePropertyName(value.Name);

                    WriteFieldValue(writer, value.Value);
                }
            }

            writer.WriteEndObject();
        }

        private void WriteList(JsonWriter writer, IList list)
        {
            writer.WriteStartArray();

            for (var i = 0; i < list.Count; i++)
            {
                WriteFieldValue(writer, list[i]);
            }

            writer.WriteEndArray();
        }

        private void WriteResultMapList(JsonWriter writer, IResultMapList list)
        {
            writer.WriteStartArray();

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] is { } map)
                {
                    WriteResultMap(writer, map);
                }
                else
                {
                    WriteFieldValue(writer, null);
                }
            }

            writer.WriteEndArray();
        }

        private void WriteFieldValue(JsonWriter writer, object? value)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            switch (value)
            {
                case IResultMap resultMap:
                    WriteResultMap(writer, resultMap);
                    break;

                case IResultMapList resultMapList:
                    WriteResultMapList(writer, resultMapList);
                    break;

                case IReadOnlyDictionary<string, object?> dict:
                    WriteDictionary(writer, dict);
                    break;

                case IList list:
                    WriteList(writer, list);
                    break;

                case IError error:
                    WriteError(writer, error);
                    break;

                case HotChocolate.Path p:
                    WritePathValue(writer, p);
                    break;

                case NameString n:
                    writer.WriteValue(n.Value);
                    break;

                default:
                    serializer.Serialize(writer, value);
                    break;
            }
        }
    }
}

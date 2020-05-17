using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Consul;
using Newtonsoft.Json;


    public interface ITxnEndpoint
    {
        public void Create();
    }

    public class TxnOp
    {
        public List<KVTxnOp> KV { get; set; }
        public List<NodeTxnOp> Node { get; set; }
        public List<ServiceTxnOp> Service { get; set; }
        public List<CheckTxnOp> Check { get; set; }
    }

    public class TxnResult
    {
        public KVPair KV { get; set; }
        public Node Node { get; set; }
        public Service Service { get; set; }
        public Check Check { get; set; }
    }

    /// <summary>
    /// KVTxnResponse  is used to return the results of a transaction.
    /// </summary>
    public class KVTxnResponse
    {
        [JsonIgnore]
        public bool Success { get; internal set; }
        [JsonProperty]
        public List<TxnError> Errors { get; internal set; }
        [JsonProperty]
        public List<TxnResult> Results { get; internal set; }

        public KVTxnResponse()
        {
            Results = new List<TxnResult>();
            Errors = new List<TxnError>();
        }

        internal KVTxnResponse(TxnResponse txnRes)
        {
            if (txnRes == null)
            {
                Results = new List<TxnResult>(0);
                Errors = new List<TxnError>(0);
                return;
            }

            if (txnRes.Results == null)
            {
                Results = new List<TxnResult>(0);
            }
            else
            {
                Results = new List<KVPair>(txnRes.Results.Count);
                foreach (var txnResult in txnRes.Results)
                {
                    Results.Add(txnResult.KV);
                }
            }

            if (txnRes.Errors == null)
            {
                Errors = new List<TxnError>(0);
            }
            else
            {
                Errors = txnRes.Errors;
            }
        }
    }

    public class NodeTxnOp
    {
        public NodeTxnVerb Verb { get; set; }
    }

    public class ServiceTxnOp
    {
        public ServiceTxnVerb Verb { get; set; }
    }

    public class CheckTxnOp
    {
        public CheckTxnVerb Verb { get; set; }
    }

    [JsonConverter(typeof(KVTxnVerbTypeConverter))]
    public class NodeTxnVerb : IEquatable<NodeTxnVerb>
    {
        static NodeTxnVerb()
        {
            Set = new NodeTxnVerb("set");
            Delete = new NodeTxnVerb("delete");
            DeleteCAS = new NodeTxnVerb("delete-cas");
            Get = new NodeTxnVerb("get");
            CAS = new NodeTxnVerb("cas");
        }

        private NodeTxnVerb(string operation)
        {
            Operation = operation;
        }

        public string Operation { get; }

        public static NodeTxnVerb Set { get; }
        public static NodeTxnVerb Delete { get; }
        public static NodeTxnVerb DeleteCAS { get; }
        public static NodeTxnVerb CAS { get; }
        public static NodeTxnVerb Get { get; }

        public bool Equals(NodeTxnVerb other)
        {
            return Operation == other.Operation;
        }

        public override bool Equals(object other)
        {
            // other could be a reference type, the is operator will return false if null
            return other is NodeTxnVerb && Equals(other as NodeTxnVerb);
        }

        public override int GetHashCode()
        {
            return Operation.GetHashCode();
        }
    }


    [JsonConverter(typeof(CheckTxnVerbTypeConverter))]
    public class CheckTxnVerb : IEquatable<CheckTxnVerb>
    {
        static CheckTxnVerb()
        {
            Set = new CheckTxnVerb("set");
            Delete = new CheckTxnVerb("delete");
            DeleteCAS = new CheckTxnVerb("delete-cas");
            Get = new CheckTxnVerb("get");
            CAS = new CheckTxnVerb("cas");
        }

        private CheckTxnVerb(string operation)
        {
            Operation = operation;
        }

        public string Operation { get; }

        public static CheckTxnVerb Set { get; }
        public static CheckTxnVerb Delete { get; }
        public static CheckTxnVerb DeleteCAS { get; }
        public static CheckTxnVerb CAS { get; }
        public static CheckTxnVerb Get { get; }

        public bool Equals(CheckTxnVerb other)
        {
            return Operation == other.Operation;
        }

        public override bool Equals(object other)
        {
            // other could be a reference type, the is operator will return false if null
            return other is CheckTxnVerb && Equals(other as CheckTxnVerb);
        }

        public override int GetHashCode()
        {
            return Operation.GetHashCode();
        }
    }


    [JsonConverter(typeof(ServiceTxnVerbTypeConverter))]
    public class ServiceTxnVerb : IEquatable<ServiceTxnVerb>
    {
        static ServiceTxnVerb()
        {
            Set = new ServiceTxnVerb("set");
            Delete = new ServiceTxnVerb("delete");
            DeleteCAS = new ServiceTxnVerb("delete-cas");
            Get = new ServiceTxnVerb("get");
            CAS = new ServiceTxnVerb("cas");
        }

        private ServiceTxnVerb(string operation)
        {
            Operation = operation;
        }

        public string Operation { get; }

        public static ServiceTxnVerb Set { get; }
        public static ServiceTxnVerb Delete { get; }
        public static ServiceTxnVerb DeleteCAS { get; }
        public static ServiceTxnVerb CAS { get; }
        public static ServiceTxnVerb Get { get; }

        public bool Equals(ServiceTxnVerb other)
        {
            return Operation == other.Operation;
        }

        public override bool Equals(object other)
        {
            // other could be a reference type, the is operator will return false if null
            return other is ServiceTxnVerb && Equals(other as ServiceTxnVerb);
        }

        public override int GetHashCode()
        {
            return Operation.GetHashCode();
        }
    }

    public class ServiceTxnVerbTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((ServiceTxnVerb)value).Operation);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var status = (string)serializer.Deserialize(reader, typeof(string));
            switch (status)
            {
                case "set":
                    return ServiceTxnVerb.Set;
                case "delete":
                    return ServiceTxnVerb.Delete;
                case "delete-cas":
                    return ServiceTxnVerb.DeleteCAS;
                case "cas":
                    return ServiceTxnVerb.CAS;
                case "get":
                    return ServiceTxnVerb.Get;
                default:
                    throw new ArgumentException($"Invalid ${nameof(ServiceTxnVerb)} value during deserialization");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ServiceTxnVerb);
        }
    }

    public class NodeTxnVerbTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((NodeTxnVerb)value).Operation);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var status = (string)serializer.Deserialize(reader, typeof(string));
            switch (status)
            {
                case "set":
                    return NodeTxnVerb.Set;
                case "delete":
                    return NodeTxnVerb.Delete;
                case "delete-cas":
                    return NodeTxnVerb.DeleteCAS;
                case "cas":
                    return NodeTxnVerb.CAS;
                case "get":
                    return NodeTxnVerb.Get;
                default:
                    throw new ArgumentException($"Invalid ${nameof(NodeTxnVerb)} value during deserialization");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NodeTxnVerb);
        }
    }

    public class CheckTxnVerbTypeConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((CheckTxnVerb)value).Operation);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            var status = (string)serializer.Deserialize(reader, typeof(string));
            switch (status)
            {
                case "set":
                    return CheckTxnVerb.Set;
                case "delete":
                    return CheckTxnVerb.Delete;
                case "delete-cas":
                    return CheckTxnVerb.DeleteCAS;
                case "cas":
                    return CheckTxnVerb.CAS;
                case "get":
                    return CheckTxnVerb.Get;
                default:
                    throw new ArgumentException($"Invalid ${nameof(CheckTxnVerb)} value during deserialization");
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CheckTxnVerb);
        }
    }

using System;

namespace NineYi.Msa.Infra.Messaging
{
    public class MessageState
    {
        public MessageMeta Meta { get; set; }
        public object Message { get; set; }
    }

    public class MessageMeta
    {
        public string RoutingKey { get; set; }
        public string CorrelationId { get; set; }
    }

    public class MessageStateNotFound : Exception
    {
        public string CorrelationId { get; private set; }

        public MessageStateNotFound(string correlationId) :
            base($"MessageState was not found by correlationId({correlationId}).")
        {
            CorrelationId = correlationId;
        }
    }

    public class MessageStateDeserializedFailed : Exception
    {
        public string CorrelationId { get; private set; }
        public string Content { get; private set; }

        public MessageStateDeserializedFailed(string correlationId, string content, Exception innerException = null) :
            base($"Deserialize MessageState failed. CorrelationId => {correlationId}, Content => {content}", innerException)
        {
            CorrelationId = correlationId;
            Content = content;
        }
    }
}

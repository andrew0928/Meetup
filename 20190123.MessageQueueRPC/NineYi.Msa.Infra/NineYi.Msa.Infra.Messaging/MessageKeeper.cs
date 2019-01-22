using Newtonsoft.Json;
using System;
using System.IO;

namespace NineYi.Msa.Infra.Messaging.MessageKeeper
{
    public interface IMessageKeeper
    {
        MessageState Read(string correlationId);
        void Save(MessageState messageState);
        void Delete(string correlationId);
    }

    public class FileMessageKeeper : IMessageKeeper
    {
        public string BasePath { get; private set; }

        public FileMessageKeeper(string basePath)
        {
            BasePath = basePath;
            if (!Directory.Exists(BasePath))
            {
                Directory.CreateDirectory(BasePath);
            }
        }

        private string GetTargetPath(string correlationId) => Path.Combine(BasePath, $"{correlationId}.json");

        public MessageState Read(string correlationId)
        {
            var targetPath = GetTargetPath(correlationId);
            if (!File.Exists(targetPath))
            {
                throw new MessageStateNotFound(correlationId);
            }

            using (var sw = new StreamReader(targetPath))
            {
                var content = sw.ReadToEnd();
                try
                {
                    return JsonConvert.DeserializeObject<MessageState>(content);
                }
                catch (Exception ex)
                {
                    throw new MessageStateDeserializedFailed(correlationId, content, ex);
                }
            }
        }

        public void Save(MessageState messageState)
        {
            using (var sw = new StreamWriter(Path.Combine(BasePath, $"{messageState.Meta.CorrelationId}.json")))
            {
                sw.Write(JsonConvert.SerializeObject(messageState));
            }
        }

        public void Delete(string correlationId)
        {
            var targetPath = GetTargetPath(correlationId);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
        }
    }
}

using System;

namespace TSClientGen
{
    /// <summary>
    /// Атрибут применяется к контроллерам или к action'ам контроллеров.
    /// Позволяет в рантайме указать host, на который будет производиться вызов api 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class TSExternalHostAttribute : Attribute
    {
        public TSExternalHostAttribute(string hostId)
        {
            HostId = hostId;
        }
        
        /// <summary>
        /// Идентификатор хоста, по которому в рантайме будет указан сам хост для выполнения вызова api-метода
        /// </summary>
        public string HostId { get; }        
    }
}
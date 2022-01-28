using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace BackendTemplate.BLL.Authentication
{
    [DataContract]
    public class APIResponseDTO<T>
    {
        [DataMember]
        public string Result { get; set; }

        [DataMember]
        public T Data { get; set; }

        public APIResponseDTO(string result, T data)
        {
            this.Result = result;
            this.Data = data;
        }
    }
}

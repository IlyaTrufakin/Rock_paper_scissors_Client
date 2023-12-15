using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock_paper_scissors_Client
{
    internal class SerializedObject
    {

        public string ObjectType { get; set; } // Маркер типа объекта
        public byte[] Data { get; set; } // Данные объекта

        public SerializedObject(string objectType, byte[] data)
        {
            ObjectType = objectType;
            Data = data;
        }

    }
}

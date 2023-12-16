using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Rock_paper_scissors_Client
{
     class PrepareObjectToSend
    {

        public string ObjectType { get; set; } // тип передаваемых данных - указывает что именно сериализовано
        public byte[] Data { get; set; } // Данные объекта

        public PrepareObjectToSend(string objectType, byte[] data)
        {
            ObjectType = objectType;
            Data = data;
        }


        public byte[] SerializedObject()
        {
            // Получаем байты для ObjectType
            byte[] typeBytes = Encoding.UTF8.GetBytes(ObjectType);

            // Создаем массив для объединенных данных и типа объекта
            byte[] result = new byte[typeBytes.Length + Data.Length];

            // Копируем данные ObjectType и Data в итоговый массив
            Buffer.BlockCopy(typeBytes, 0, result, 0, typeBytes.Length);
            Buffer.BlockCopy(Data, 0, result, typeBytes.Length, Data.Length);

            return result;
        }
    }
}

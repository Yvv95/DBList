using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBList
{
    public static class FieldsClass
    {

        public static List<Field> fields = new List<Field>();
        
        /// <summary>
        /// Добавить поле
        /// </summary>
        public static void AddValue(string _name, string _value)
        {
            bool exists = false;

            foreach (var item in fields)
            {
                if (item.name == _name)
                {
                    item.value = _value;
                    item.selected = true;
                    exists = true;
                    break;
                }
            }
            if (!exists)
                fields.Add(new Field(_name, _value));
        }

        /// <summary>
        /// Добавить поле
        /// </summary>
        public static void AddValue(string _name)
        {
            bool exists = false;

            foreach (var item in fields)
            {
                if (item.name == _name)
                {
                    item.value = "";
                    item.selected = false;
                    exists = true;
                    break;
                }
            }
            if (!exists)
                fields.Add(new Field(_name, ""));
        }

        /// <summary>
        /// Добавить поля из списка
        /// </summary>
        public static void AddFields(List<string> _fields)
        {
            for (int i = 0; i < _fields.Count; i++)
                AddValue(_fields[i], "");
        }

        /// <summary>
        /// Существование поля
        /// </summary>
        public static bool IsField(string _name)
        {
            for (int i = 0; i < fields.Count; i++)
                if (fields[i].name == _name)
                {
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Загрузить значение поля
        /// </summary>
        public static string LoadField(string _name)
        {
            for (int i = 0; i < fields.Count; i++)
                if (fields[i].name == _name)
                {
                    return fields[i].value;
                }
            return "";
        }

        /// <summary>
        /// Убрать отметку с поля
        /// </summary>
        public static void UnSelectValue(string _name)
        {
            foreach (var item in fields)
            {
                if (item.name == _name)
                {
                    item.selected = false;
                    break;
                }
            }
        }
        /// <summary>
        /// Очистить список полей
        /// </summary>
        public static void ClearList()
        {
            fields.Clear();
        }

        /// <summary>
        /// Получить все отмеченные поля
        /// </summary>
        public static Dictionary<string, string> GetSelected()
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();

            foreach (var item in fields)
            {
                if (item.selected)
                {
                    pairs.Add(item.name, item.value);
                }
            }
            return pairs;
        }

        public class Field
        {
            public string name { get; set; }
            public string value { get; set; }
            public bool selected { get; set; }

            public Field(string _name, string _value)
            {
                this.name = _name;
                this.value = _value;
                selected = false;
            }
        }
    }
}

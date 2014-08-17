using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBoleto.Utils
{
    public class CodigoBarras : FixedString
    {
        public CodigoBarras()
            : base (44)
        {
        }
    }

    public class LinhaDigitavel
    {
        public FixedString Campo1 { get; set; }
        public FixedString Campo2 { get; set; }
        public FixedString Campo3 { get; set; }
        public FixedString Campo4 { get; set; }
        public FixedString Campo5 { get; set; }

        public LinhaDigitavel()
        {
            Campo1 = new FixedString(10);
            Campo2 = new FixedString(11);
            Campo3 = new FixedString(11);
            Campo4 = new FixedString(1);
            Campo5 = new FixedString(14);
        }

        public override string ToString()
        {
            return Campo1.Substring(1, 5) + "." + Campo1.Substring(6, 10) + "  " +
                Campo2.Substring(1, 5) + "." + Campo2.Substring(6, 11) + "  " +
                Campo3.Substring(1, 5) + "." + Campo3.Substring(6, 11) + "  " +
                Campo4.ToString() + "  " +
                Campo5.ToString();
        }
    }

    // Classe para controle de string de tamanho fixas basedas em posição 1
    public class FixedString
    {
        public char[] Value;
        private int _index = 1;

        public FixedString(int size)
        {
            Value = new char[size];

            for (var i = 0; i < Value.Length; i++)
                Value[i] = '*';
        }

        public int Index
        {
            get { return _index; }
            set { _index = Math.Min(value, Value.Length); }
        }

        public FixedString Set(int start, int end, int value)
        {
            return Set(start, end, value.ToString());
        }

        public FixedString Set(int start, int end, string value)
        {
            var length = end - start + 1;
            var val = value.PadLeft(length, '0').Substring(0, length);

            for (var i = 0; i < length; i++)
            {
                Value[start + i - 1] = val[i];
            }
            return this;
        }

        public FixedString Add(int value)
        {
            return Add(value.ToString().Length, value.ToString());
        }

        public FixedString Add(string value)
        {
            return Add(value.Length, value);
        }
       
        public FixedString Add(int length, int value)
        {
            return Add(length, value.ToString());
        }

        public FixedString Add(int length, string value)
        {
            var val = (string.IsNullOrEmpty(value) ? "" : value).PadLeft(length, '0').Substring(0, length);

            for (var i = 0; i < length; i++)
            {
                Value[Index - 1] = val[i];
                Index++;
            }
            return this;
        }

        public string Substring(int start, int end)
        {
            var sb = new StringBuilder();
            for (var i = start; i <= end; i++)
            {
                sb.Append(Value[i - 1]);
            }
            return sb.ToString();
        }

        public string Substring(int start)
        {
            return Substring(start, start);
        }

        public override string ToString()
        {
            return string.Join("", Value);
        }
    }
}

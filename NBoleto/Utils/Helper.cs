using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NBoleto.Utils
{
    internal class Helper
    {
        public static string FatorVencimento(DateTime dtVencto)
        {
            var timespan = dtVencto.Subtract(new DateTime(1997, 10, 7));
            var fator = timespan.Days.ToString().PadLeft(4, '0');
            return fator.StartsWith("0") ? "0000" : fator;
        }

        public static string Left(string value, int length)
        {
            if (value.Length > length) return value.Substring(0, length);
            else return value;
        }

        public static void ValidateLength(string str, int length, string error)
        {
            if (string.IsNullOrEmpty(str) || str.Length != length)
                throw new ApplicationException(error);
        }

        public static void ValidateLength(string str, int minLength, int maxLength, string error)
        {
            var l = string.IsNullOrEmpty(str) ? 0 : str.Length;

            if (l < minLength || l > maxLength)
                throw new ApplicationException(error);
        }

        public static string Logotipo(string codbanco)
        {
            using (var stream = typeof(Helper).Assembly.GetManifestResourceStream("NBoleto.Logos." + codbanco + ".jpg"))
            {
                return "data:image/jpg;base64," + Convert.ToBase64String(ReadFully(stream));
            }
        }

        public static string CodigoBarras(string codigo)
        {
            var cb = new C2of5i(codigo, 1, 50, codigo.Length); // 50 => 1.3cm
            return "data:image/jpg;base64," + Convert.ToBase64String(cb.ToByte());
        }

        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[32768];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        return ms.ToArray();
                    ms.Write(buffer, 0, read);
                }
            }
        }

        public static string FormatCpfCnpj(string numero)
        {
            if (numero.Length == 14)
            {
                return numero.Insert(2, ".").Insert(6, ".").Insert(10, "/").Insert(15, "-");
            }
            else if (numero.Length == 11)
            {
                return numero.Insert(3, ".").Insert(7, ".").Insert(11, "-");
            }
            return "";
        }

        /// <summary>
        /// Obtém a data de vencimento em formato Juliano
        /// </summary>
        /// <param name="dtVencimento">Data de vencimento</param>
        /// <returns>Data em formato Juliano</returns>
        public static string FormataJuliano(DateTime dtVencimento)
        {
            return string.Concat(dtVencimento.DayOfYear.ToString("000"), dtVencimento.Year.ToString()[3]);
        }

        #region Calculo de Módulos

        public static int Mod7(string numero)
        {
            int soma = 0;
            int mult = 2;

            for (var idx = numero.Length - 1; idx >= 0; idx--)
            {
                soma += (Convert.ToByte(numero[idx]) - 48) * mult;
                if (mult == 7) mult = 2;
                else mult++;
            }

            soma = soma % 11;
            if (soma > 1) soma = 11 - soma;

            return soma;
        }

        public static int Mod10(string numero)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, r;

            for (int i = numero.Length; i > 0; i--)
            {
                r = (Convert.ToInt32(numero.Substring(i - 1, 1)) * p);

                if (r > 9)
                    r = (r / 10) + (r % 10);

                s += r;

                if (p == 2)
                    p = 1;
                else
                    p = p + 1;
            }
            d = ((10 - (s % 10)) % 10);
            return d;
        }

        public static int Mod11(string numero)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 9;

            for (int i = 0; i < numero.Length; i++)
            {
                s = s + (Convert.ToInt32(numero[i]) * p);
                if (p < b)
                    p = p + 1;
                else
                    p = 2;
            }

            d = 11 - (s % 11);
            if (d > 9)
                d = 0;
            return d;
        }

        public static int Mod11(string numero, int b)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2;


            for (int i = numero.Length; i > 0; i--)
            {
                s = s + (Convert.ToInt32(numero.Substring(i - 1, 1)) * p);
                if (p == b)
                    p = 2;
                else
                    p = p + 1;
            }

            d = 11 - (s % 11);


            if ((d > 9) || (d == 0) || (d == 1))
                d = 1;

            return d;
        }

        public static string Mod11Base7(string numero)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */
            string d = "0";

            int s = 0, p = 2, b = 7, r;

            for (int i = numero.Length - 1; i >= 0; i--)
            {
                s += Convert.ToInt32(numero.Substring(i, 1)) * p;
                p = (p == b) ? 2 : p + 1;
            }

            r = s % 11;

            if (r == 0)
                d = "0";
            else if (r == 1)
                d = "P";
            else
                d = (11 - r).ToString();

            return d;
        }

        public static int Mod11Base9(string seq)
        {
            /* Variáveis
             * -------------
             * d - Dígito
             * s - Soma
             * p - Peso
             * b - Base
             * r - Resto
             */

            int d, s = 0, p = 2, b = 9;


            for (int i = seq.Length - 1; i >= 0; i--)
            {
                string aux = Convert.ToString(seq[i]);
                s += (Convert.ToInt32(aux) * p);
                if (p >= b)
                    p = 2;
                else
                    p = p + 1;
            }

            if (s < 11)
            {
                d = 11 - s;
                return d;
            }
            else
            {
                d = 11 - (s % 11);
                if ((d > 9) || (d == 0))
                    d = 0;

                return d;
            }
        }

        /// <summary>
        /// Calcula o resto da divisão por 11 depois de aplicar os pesos de 2 a 'lim'
        /// </summary>
        /// <param name="seq">Sequência numérica</param>
        /// <param name="lim">Limite do peso</param>
        /// <param name="flag">
        /// Flag para soma:
        /// 1: Retorna o resto sem calcular nenhuma soma
        /// 2: Se o resto for 10, retorna 0
        /// 3: Se 11 menos o resto for 10, retorna 0
        /// 4: Se o resto for 10, retorna 0. Senão retorna 11 menos o resto
        /// </param>
        /// <param name="InvertePesos">Utiliza o 2 como limite final e o parâmetro lim como limite inicial</param>
        /// <returns>Resto da divisão por 11 depois de aplicar os pesos de 2 a 'lim'</returns>
        public static int Mod11(string seq, int lim, int flag, bool InvertePesos = false)
        {
            int lim1 = 2;
            int lim2 = lim;
            int mult = lim1;
            int total = 0;
            int ndig = 0;
            int nresto = 0;
            string num = string.Empty;

            if (InvertePesos)
            {
                //Inverte os pesos para fazer a multiplicação decrescente de 'lim' até 2
                mult = lim2;
                lim2 = lim1;
                lim1 = mult;
            }

            for (int i = seq.Length - 1; i >= 0; i--)
            {
                total += Convert.ToInt32(seq[i].ToString()) * mult;

                if (mult == lim2)
                    mult = InvertePesos ? lim : 2;
                else
                    mult += InvertePesos ? -1 : 1;
            }

            nresto = (total % 11);

            if (flag == 1)
                return nresto;
            else if (flag == 2)
            {
                //Se o resto for maior que 9, retorna 0
                if (nresto > 9)
                    nresto = 0;

                return nresto;
            }
            else if (flag == 3)
            {
                //Subtrai de 11
                nresto = 11 - nresto;

                //Se o resto for maior que 9, retorna 0
                if (nresto > 9)
                    nresto = 0;

                return nresto;
            }
            else if (flag == 4)
            {
                //Se o resto for maior que 9, retorna 0
                if (nresto > 9 || nresto == 0)
                    return 0;

                //Subtrai de 11
                nresto = 11 - nresto;

                return (nresto == 0 || nresto == 1 || nresto == 10) ? 1 : nresto;
            }
            else
            {
                if (nresto == 0 || nresto == 1 || nresto == 10)
                    ndig = 1;
                else
                    ndig = (11 - nresto);

                return ndig;
            }
        }

        public static int Mult10Mod11(string seq, int lim, int flag)
        {
            int mult = 0;
            int total = 0;
            int pos = 1;
            int ndig = 0;
            int nresto = 0;
            string num = string.Empty;

            mult = 1 + (seq.Length % (lim - 1));

            if (mult == 1)
                mult = lim;

            while (pos <= seq.Length)
            {
                num = seq.Substring(pos - 1, 1);
                total += Convert.ToInt32(num) * mult;

                mult -= 1;
                if (mult == 1)
                    mult = lim;

                pos += 1;
            }

            nresto = ((total * 10) % 11);

            if (flag == 1)
                return nresto;
            else
            {
                if (nresto == 0 || nresto == 1 || nresto == 10)
                    ndig = 1;
                else
                    ndig = nresto;

                return ndig;
            }
        }

        /// <summary>
        /// Encontra multiplo de 10 igual ou superior a soma e subtrai multiplo da soma devolvendo o resultado
        /// </summary>
        /// <param name="soma"></param>
        /// <returns></returns>
        public static int Multiplo10(int soma)
        {
            //Variaveis
            int result = 0;
            //Encontrando multiplo de 10
            while (result < soma)
            {
                result = result + 10;
            }
            //Subtraindo
            result = result - soma;
            //Retornando
            return result;
        }

        #endregion

    }
}

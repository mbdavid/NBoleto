using NBoleto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NBoleto.Bancos
{
    public class Santander : Boleto
    {
        public Santander()
        {
            this.Banco = "033-7";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.Agencia, 5, "Código da agencia deve ter 5 dígitos");
            Helper.ValidateLength(cedente.CodCedente, 7, "Código do cedente deve ter 7 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 13, "Nosso número deve ser de 1 a 13 dígitos");

            //102- Cobrança simples – SEM Registro
            if (cedente.Carteira != "CSR")
                throw new ApplicationException(string.Format("Carteira {0} não implementada. Apenas carteiras CSR", cedente.Carteira));
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            var n = nossoNumero.PadLeft(12, '0');

            var dv1 = Helper.Mod11(n,9,0).ToString();
            return n  + "-" + dv1;
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return cedente.Agencia + "/" + cedente.CodCedente;
        }

        /// <summary>
        ///	O código de barra para cobrança contém 44 posições dispostas da seguinte forma:
        ///    01 a 03 -  3 - 033 fixo - Código do banco
        ///    04 a 04 -  1 - 9 fixo - Código da moeda (R$)
        ///    05 a 05 –  1 - Dígito verificador do código de barras
        ///    06 a 09 -  4 - Fator de vencimento
        ///    10 a 19 - 10 - Valor
        ///    20 a 20 –  1 - Fixo 9
        ///    21 a 27 -  7 - Código do cedente padrão satander
        ///    28 a 40 - 13 - Nosso número
        ///    41 - 41 - 1 -  IOS  - Seguradoras(Se 7% informar 7. Limitado  a 9%) Demais clientes usar 0 
        ///    42 - 44 - 3 - Tipo de modalidade da carteira 101, 102, 201
        /// </summary>
        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "033") // Código do Banco na Câmara de Compensação = "033"
                .Set(4, 4, "9") // Código da Moeda = '9'
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 20, "9")  //20 a 20 –  1 - Fixo 9
                .Set(21, 27, cedente.CodCedente) // 7 - Código do cedente padrão satander
                .Set(28, 40, NossoNumero.Replace("-","")) // 13 - Nosso número
                .Set(41, 41, "0") //  IOS  - Seguradoras(Se 7% informar 7. Limitado  a 9%) Demais clientes usar 0 
                .Set(42, 44, "102")//3 - Tipo de modalidade da carteira 101, 102, 201
                .Set(5, 5, Helper.Mod11(barras.Substring(1, 4) + barras.Substring(6, 44), 9)); // DAC

            return barras;
        }

        /// <summary>
        ///	A Linha Digitavel para cobrança contém 44 posições dispostas da seguinte forma:
        ///   1º Grupo - 
        ///    01 a 03 -  3 - 033 fixo - Código do banco
        ///    04 a 04 -  1 - 9 fixo - Código da moeda (R$) outra moedas 8
        ///    05 a 05 –  1 - Fixo 9
        ///    06 a 09 -  4 - Código cedente padrão santander
        ///    10 a 10 -  1 - Código DV do primeiro grupo
        ///   2º Grupo -
        ///    11 a 13 –  3 - Restante do código cedente
        ///    14 a 20 -  7 - 7 primeiros campos do nosso número
        ///    21 a 21 -  1 - Código DV do segundo grupo
        ///   3º Grupo -  
        ///    22 - 27 - 6 -  Restante do nosso número
        ///    28 - 28 - 1 - IOS  - Seguradoras(Se 7% informar 7. Limitado  a 9%) Demais clientes usar 0 
        ///    29 - 31 - 3 - Tipo de carteira
        ///    32 - 32 - 1 - Código DV do terceiro grupo
        ///   4º Grupo -
        ///    33 - 33 - 1 - Composto pelo DV do código de barras
        ///   5º Grupo -
        ///    34 - 36 - 4 - Fator de vencimento
        ///    37 - 47 - 10 - Valor do título
        /// </summary>
        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            linha.Campo1
                .Add(barras.Substring(1, 3)) // Código do Banco na Câmara de Compensação "033"
                .Add(barras.Substring(4)) // Código da moeda "9" (*) 
                .Add(barras.Substring(4)) //1 - Fixo 9
                .Add(barras.Substring(21,24)) //4 - Código cedente padrão santander
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9))); // DV

            linha.Campo2
                .Add(barras.Substring(25, 27)) // 11-13 3 - Restante do código cedente 
                .Add(barras.Substring(28, 34))// 14-20  7 primeiros campos do N/N
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV 21-21 1 9 (01) Dígito verificador do segundo grupo

            linha.Campo3
                .Add(barras.Substring(35, 40)) //22 - 27 - 6 - Restante do nosso número
                .Add(barras.Substring(41, 41)) //IOS – somente para Seguradoras (Se 7% informar 7, limitado a 9%) Demais clientes usar 0 (zero)
                .Add(barras.Substring(42, 44)) //29-31 3 Tipo de Modalidade Carteira 101-Cobrança Simples Rápida COM Registro  102- Cobrança simples SEM Registro 201- Penhor
                .Add(Helper.Mod10(linha.Campo3.Substring(1, 10))); // DV

            linha.Campo4
                .Add(barras.Substring(5)); // DAC ou Dígito Verificador (posição 05 do Código de Barras)

            linha.Campo5
                .Add(barras.Substring(6, 9)) //  34 - 36 - 4 - Fator de vencimento
                .Add(barras.Substring(10, 19)); // 37 - 47 - 10 - Valor do título

            return linha;
        }
    }
}

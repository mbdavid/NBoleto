using NBoleto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBoleto.Bancos
{
    public class HSBC : Boleto
    {
        public HSBC()
        {
            this.Banco = "399-9";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.Agencia, 4, "Código da agencia deve ter 4 dígitos");
            Helper.ValidateLength(cedente.CodCedente, 7, "Código do cedente deve ter 7 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 13, "Nosso número deve ser de 1 a 13 dígitos");

            if (cedente.Carteira != "CNR")
                throw new ApplicationException(string.Format("Carteira {0} não implementada. Apenas carteiras CNR", cedente.Carteira));
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            //Primeiro dígito verificador (Aplica peso de 9 a 2 da direita para esquerda)
            int dv1 = Helper.Mod11(nossoNumero, 9, 2, true);

            //Concatena o primeiro digito verificador e o tipo verificador (4)            
            nossoNumero = string.Concat(nossoNumero, dv1, "4");

            //Segundo dígito verificador (Aplica peso de 9 a 2 da direita para esquerda)
            int dv2 = Helper.Mod11((Convert.ToInt64(nossoNumero) +
                                    Convert.ToInt32(cedente.CodCedente) +
                                    Convert.ToInt32(dtVencto.ToString("ddMMyy"))).ToString(), 9, 2, true);

            return string.Concat(nossoNumero.PadLeft(13,'0'), dv2);
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return cedente.CodCedente;
        }

        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "399") // Código do Banco na Câmara de Compensação = "399"
                .Set(4, 4, "9") // Código da Moeda = '9'
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 26, cedente.CodCedente)  //Código do Beneficiário
                .Set(27, 39, nossoNumero) //Nosso Número
                .Set(40, 43, Helper.FormataJuliano(dtVencto)) // Data de Vencimento no Formato Juliano
                .Set(44, 44, "2") // Código do Produto CNR
                .Set(5, 5, Helper.Mod11(string.Concat(barras.Substring(1, 4),
                                                        barras.Substring(6, 44)), 9, 0)); //Calcula DAC (Remove o char do DAC)

            return barras;
        }

        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            linha.Campo1
                .Add(barras.Substring(1, 3)) // Código do Banco na Câmara de Compensação "399"
                .Add(barras.Substring(4)) // Código da moeda "9" (*) 
                .Add(barras.Substring(20, 24)) // Início do código de beneficiário
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9)).ToString()); // DV

            linha.Campo2
                .Add(barras.Substring(25, 26)) // Final do código de beneficiário
                .Add(barras.Substring(27, 34)) // Início do Nosso Número
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV

            linha.Campo3
                .Add(barras.Substring(35, 39)) // Final do Nosso Número (Sem os dígitos verificadores)
                .Add(barras.Substring(40, 43)) // Data de vencimento no formato Juliano
                .Add("2") // Código do produto
                .Add(Helper.Mod10(linha.Campo3.Substring(1, 10))); // DV

            linha.Campo4
                .Add(barras.Substring(5)); // DAC ou Dígito Verificador (posição 05 do Código de Barras)

            linha.Campo5
                .Add(barras.Substring(6, 9)) // Fator de Vencimento (posição 06 a 09 do código de barras)
                .Add(barras.Substring(10, 19)); // Valor nominal (posição 10 a 19 do código de barras com zeros entre o fator de vencimento e o valor).

            return linha;
        }
    }
}

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
    public class Bradesco : Boleto
    {
        public Bradesco()
        {
            this.Banco = "237-2";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.CodCedente, 8, "Código do cedente deve ter 8 dígitos");
            Helper.ValidateLength(cedente.Agencia, 5, "Código da agencia deve ter 5 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 11, "Nosso número deve ser de 1 a 11 dígitos");
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            var n = cedente.Carteira.PadLeft(2,'0') + nossoNumero.PadLeft(11, '0');
            var dv = Helper.Mod11Base7(n);

            return cedente.Carteira.PadLeft(2, '0') + " / " + nossoNumero.PadLeft(11, '0') + "-" + dv;
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return cedente.Agencia.Substring(0, 4) + "-" + cedente.Agencia.Substring(4,1) + " / " + cedente.CodCedente.Substring(0, 7) + "-" + cedente.CodCedente.Substring(7, 1);
        }

        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "237") // Código do Banco "041"
                .Set(4, 4, "9") // Moeda de Emissão (9 = REAL)
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 23, cedente.Agencia.Substring(0, cedente.Agencia.Length - 1).PadLeft(4, '0')) //agencia cedente sem dv (tam 4 - pos 20 a 23)
                .Set(24, 25, cedente.Carteira.PadLeft(2, '0')) //carteira (tam 2 - pos 24 a 25)
                .Set(26, 36, nossoNumero.PadLeft(11, '0')) //nosso numero sem dv (tam 11 - pos 26 a 36)
                .Set(37, 43, cedente.CodCedente.Substring(0, cedente.CodCedente.Length -1).PadLeft(7, '0')) //conta cedente sem dv (tam 7 - pos 37 a 43)
                .Set(44, 44, "0") // 0
                .Set(5, 5, Helper.Mod11(barras.Substring(1, 4) + barras.Substring(6, 44), 9)); // DAC

            return barras;
        }

        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            //campo livre = barras de 20 a 44

            //Composto pelo código de Banco, código da moeda, as cinco primeiras posições do campo livre e o dígito verificador deste campo; 
            linha.Campo1
                .Add(barras.Substring(1, 3)) // Constante, Código do Banco junto a Câmara de Compensação (posição 01 a 03 do Código de Barras).
                .Add(barras.Substring(4)) // Moeda (posição 04 a 04 do Código de Barras)
                .Add(barras.Substring(20, 24)) // cinco primeiras posições do campo livre
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9))); // DV

            //Composto pelas posições 6ª a 15ª do campo livre e o dígito verificador deste campo; 
            linha.Campo2
                .Add(barras.Substring(25, 34))
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV

            //Composto pelas posições 16ª a 25ª do campo livre e o dígito verificador deste campo; 
            linha.Campo3
                .Add(barras.Substring(35, 44)) // posições 16ª a 25ª do campo livre
                .Add(Helper.Mod10(linha.Campo3.Substring(1, 10))); // DV
            
            //Composto pelo dígito verificador do código de barras, ou seja, a 5ª posição do código de barras; 
            linha.Campo4
                .Add(barras.Substring(5, 5)); // DAC ou Dígito Verificador (posição 05 do Código de Barras)

            //Composto pelo fator de vencimento com 4(quatro) caracteres e o valor do documento com 10(dez) caracteres, sem separadores e sem edição.
            linha.Campo5
                .Add(barras.Substring(6, 9)) // Fator de Vencimento (posição 06 a 09 do código de barras)
                .Add(barras.Substring(10, 19)); // Valor nominal (posição 10 a 19 do código de barras com zeros entre o fator de vencimento e o valor).

            return linha;
        }
    }
}
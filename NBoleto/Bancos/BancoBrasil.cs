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
    /// <summary>
    /// Documentação em: http://www.bb.com.br/docs/pub/emp/empl/dwn/Doc5175Bloqueto.pdf  (pagina 11)
    /// </summary>
    public class BancoBrasil : Boleto
    {
        public BancoBrasil()
        {
            this.Banco = "001-9";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.Agencia, 4, "Código da agencia deve ter 4 dígitos");
            Helper.ValidateLength(cedente.CodCedente, 8, "Código do cedente deve ter 8 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 17, "Nosso número deve ser de 1 a 17 dígitos");
            Helper.ValidateLength(cedente.Convenio, 6, "Código do convênio deve ter 6 digitos");

            if (cedente.Carteira != "16")
                throw new ApplicationException(string.Format("Carteira {0} não implementada. Apenas carteiras 16", cedente.Carteira));
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            return nossoNumero.PadLeft(17, '0');
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return cedente.Agencia + "/" + cedente.CodCedente;
        }

        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "001") // Código do Banco na Câmara de Compensação = "001"
                .Set(4, 4, "9") // Código da Moeda = '9'
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 25, cedente.Convenio) // Número do Convênio de Seis Posições
                .Set(26, 42, NossoNumero) // Nosso-Número Livre do cliente
                .Set(43, 44, "21") // "21" Tipo de Modalidade de Cobrança
                .Set(5, 5, Helper.Mod11(barras.Substring(1, 4) + barras.Substring(6, 44), 9)); // DAC

            return barras;
        }

        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            linha.Campo1
                .Add(barras.Substring(1, 3)) // Código do Banco na Câmara de Compensação "001"
                .Add(barras.Substring(4)) // Código da moeda "9" (*) 
                .Add(barras.Substring(20, 24)) // Posição 20 a 24 do código de barras
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9))); // DV

            linha.Campo2
                .Add(barras.Substring(25, 34)) // Posição 25 a 34 do código de barras 
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV

            linha.Campo3
                .Add(barras.Substring(35, 44)) // Posição 35 a 44 do código de barras
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
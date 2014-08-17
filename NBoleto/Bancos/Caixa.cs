using NBoleto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NBoleto.Bancos
{
    public class Caixa : Boleto
    {
        public Caixa()
        {
            this.Banco = "104-0";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.Agencia, 4, "Código da agencia deve ter 4 dígitos");
            Helper.ValidateLength(cedente.CodCedente, 6, "Código do cedente deve ter 6 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 15, "Nosso número deve ser de 1 a 15 dígitos");

            if (cedente.Carteira != "SR")
                throw new ApplicationException(string.Format("Carteira {0} não implementada. Apenas carteiras SR", cedente.Carteira));
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            string n = string.Concat("24", nossoNumero.PadLeft(15, '0'));

            return string.Concat(n, " - ", Helper.Mod11(n, 9, 3).ToString());
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return string.Concat(cedente.Agencia, "/", cedente.CodCedente, "-", Helper.Mod11(cedente.CodCedente, 9, 0));
        }

        protected override Utils.CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            nossoNumero = nossoNumero.PadLeft(17, '0').Insert(2, "/");

            barras
                .Set(1, 3, "104") // Código do Banco na Câmara de Compensação = "104"
                .Set(4, 4, "9") // Código da Moeda = '9'
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 25, cedente.CodCedente)  //Código do Beneficiário
                .Set(26, 26, Helper.Mod11(barras.Substring(20, 25), 9, 3)) //DV Código do Beneficiário
                .Set(27, 29, nossoNumero.Substring(3, 3)) //Nosso número sequência 1
                .Set(30, 30, "2") //Tipo de Cobrança (1 - Registrada / 2 - Sem Registro)
                .Set(31, 33, nossoNumero.Substring(6, 3)) //Nosso número sequência 2
                .Set(34, 34, "4") //Identificador de Emissão de Boleto (4 - Beneficiário)
                .Set(35, 43, nossoNumero.Substring(9, 9)) //Nosso número sequência 2
                .Set(44, 44, Helper.Mod11(barras.Substring(20, 43), 9, 3)) //DV Campo Livre
                .Set(5, 5, Helper.Mod11(string.Concat(barras.Substring(1, 4),
                                                        barras.Substring(6, 44)), 9, 0)); //DAC

            return barras;
        }

        protected override Utils.LinhaDigitavel GerarLinhaDigitavel(Utils.CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            linha.Campo1
                .Add(barras.Substring(1, 3)) // Código do Banco na Câmara de Compensação "399"
                .Add(barras.Substring(4)) // Código da moeda "9" (*) 
                .Add(barras.Substring(20, 24)) // Início do código de beneficiário
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9)).ToString()); // DV

            linha.Campo2
                .Add(barras.Substring(25, 25)) // Final do código de beneficiário
                .Add(barras.Substring(26, 26)) //DV Código do Beneficiário
                .Add(barras.Substring(27, 29)) //Nosso número sequência 1
                .Add(barras.Substring(30, 30)) //Tipo de Cobrança (1 - Registrada / 2 - Sem Registro)
                .Add(barras.Substring(31, 33)) //Nosso número sequência 2
                .Add(barras.Substring(34, 34)) //Identificador de Emissão de Boleto (4 - Beneficiário)
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV

            linha.Campo3
                .Add(barras.Substring(35, 43)) //Nosso número sequência 2
                .Add(barras.Substring(44, 44)) // Data de vencimento no formato Juliano
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

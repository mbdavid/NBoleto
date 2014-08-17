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
    /// Documentação em: https://www.banrisul.com.br/bob/data/CobrancaEletronicaBanrisul_layout_pdr_Febraban400_vrs26052010_ed02.pdf
    /// </summary>
    public class Banrisul : Boleto
    {
        public Banrisul()
        {
            this.Banco = "041-8";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.CodCedente, 9, "Código do cedente deve ter 9 dígitos (incluindo digito verificador)");
            Helper.ValidateLength(cedente.Agencia, 4, "Código da agencia deve ter 4 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 8, "Nosso número deve ser de 1 a 8 dígitos");
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            var n = nossoNumero.PadLeft(8, '0');
            var dv1 = Helper.Mod10(n).ToString();
            var dv2 = Helper.Mod11(n + dv1, 7, 4).ToString();

            //Segundo dígito igual a 1 é inválido
            if (dv2 == "1")
            {
                //Adiciona 1 no primeiro e refaz o cálculo
                dv1 = dv1 == "9" ? "0" : (Convert.ToInt16(dv1) + 1).ToString();
                dv2 = Helper.Mod11(n + dv1, 7, 4).ToString();
            }


            return n + "-" + dv1 + dv2;
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            var n = cedente.Agencia.PadLeft(4, '0');
            var dv1 = Helper.Mod10(cedente.Agencia).ToString();
            var dv2 = Helper.Mod11(n + dv1, 7, 4).ToString();

            //Segundo dígito igual a 1 é inválido
            if (dv2 == "1")
            {
                //Adiciona 1 no primeiro e refaz o cálculo
                dv1 = dv1 == "9" ? "0" : (Convert.ToInt16(dv1) + 1).ToString();
                dv2 = Helper.Mod11(n + dv1, 7, 4).ToString();
            }

            return cedente.Agencia + "." + dv1 + dv2 + "/" + cedente.CodCedente.Substring(0, 7) + "." + cedente.CodCedente.Substring(7, 2);
        }

        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "041") // Constante "041"
                .Set(4, 4, "9") // Moeda de Emissão (9 = REAL)
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor Nominal (zeros se for Moeda variável)
                .Set(20, 20, "2") // Produto: "2" Cobrança Direta, Fichário emitido pelo CLIENTE
                .Set(21, 21, "1") // Constante "1"
                .Set(22, 25, cedente.Agencia) // Código da Agência, com quatro dígitos, sem o Número de Controle.
                .Set(26, 32, cedente.CodCedente) // Código do Cedente sem Número de Controle.
                .Set(33, 40, nossoNumero) // Nosso Número sem Número de Controle
                .Set(41, 42, "40")  // Constante "40" (significa agencia com 4 digitos)
                .Set(43, 43, Helper.Mod10(barras.Substring(20, 42))) // DV1 - Mod10 
                .Set(44, 44, Helper.Mod11(barras.Substring(20, 43), 7)) // DV2 - Mod11
                .Set(5, 5, Helper.Mod11(barras.Substring(1, 4) + barras.Substring(6, 44), 9)); // DAC

            return barras;
        }

        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            linha.Campo1
                .Add(barras.Substring(1, 3)) // Constante, Código do Banco junto a Câmara de Compensação (posição 01 a 03 do Código de Barras).
                .Add(barras.Substring(4)) // Moeda (posição 04 a 04 do Código de Barras)
                .Add(barras.Substring(20)) // Constante, identifica o Produto (posição 20 do Código de Barras)
                .Add(barras.Substring(21)) // Constante, identifica o Sistema BDL - Carteira de Letras (posição 21 do Código de Barras).
                .Add(barras.Substring(22, 24)) // Agência, sem o NC, quatro primeiros dígitos (posição 22 a 25 do Código de Barras)
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9))); // DV

            linha.Campo2
                .Add(barras.Substring(25)) // Agência, sem o NC, quatro primeiros dígitos (posição 22 a 25 do Código de Barras)
                .Add(barras.Substring(26, 32)) // Código do Cedente, sem o NC, sete primeiros dígitos (posição 26 a 32 Do Código de Barras).
                .Add(barras.Substring(33, 34)) // Nosso Número, sem o NC, oito primeiros dígitos. (posição 33 a 40 do Código de Barras).
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10))); // DV

            linha.Campo3
                .Add(barras.Substring(35, 40)) // Nosso Número, sem o NC, oito primeiros dígitos. (posição 33 a 40 do Código de Barras).
                .Add(barras.Substring(41, 42)) // Constante. Indica agência com 4 Dígitos.
                .Add(barras.Substring(43, 44)) //Número de Controle, cálculo através dos módulos 10 e 11. (posição 43 a 44 do Código de Barras).
                .Add(Helper.Mod10(linha.Campo3.Substring(1, 10))); // DV

            linha.Campo4
                .Add(barras.Substring(5, 5)); // DAC ou Dígito Verificador (posição 05 do Código de Barras)

            linha.Campo5
                .Add(barras.Substring(6, 9)) // Fator de Vencimento (posição 06 a 09 do código de barras)
                .Add(barras.Substring(10, 19)); // Valor nominal (posição 10 a 19 do código de barras com zeros entre o fator de vencimento e o valor).

            return linha;
        }
    }
}
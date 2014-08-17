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
    public class Itau : Boleto
    {
        public Itau()
        {
            this.Banco = "341-7";
        }

        protected override void ValidaDados(Cedente cedente, string nossoNumero)
        {
            Helper.ValidateLength(cedente.CodCedente, 5, "Código do cedente deve ter 5 dígitos");
            Helper.ValidateLength(cedente.Agencia, 4, "Código da agencia deve ter 4 dígitos");
            Helper.ValidateLength(nossoNumero, 1, 8, "Nosso número deve ser de 1 a 8 dígitos");
        }

        protected override string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto)
        {
            var n = cedente.Agencia.PadLeft(4,'0') + cedente.CodCedente.PadLeft(5,'0') + cedente.Carteira.PadLeft(3,'0') + nossoNumero.PadLeft(8, '0');
            var dv = Helper.Mod10(n);

            return cedente.Carteira + "/" + nossoNumero.PadLeft(8, '0') + "-" + dv;
        }

        protected override string FormataAgCodCedente(Cedente cedente)
        {
            return cedente.Agencia + "/" + cedente.CodCedente + "-" + Helper.Mod10(cedente.Agencia + cedente.CodCedente);
        }

        protected override CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            var barras = new CodigoBarras();

            barras
                .Set(1, 3, "341") // Código do Banco na Câmara de Compensação = '341'
                .Set(4, 4, "9") // Código da Moeda = '9'
                .Set(6, 9, Helper.FatorVencimento(dtVencto)) // Fator de Vencimento (Anexo 6)
                .Set(10, 19, Convert.ToInt64(vrBoleto * 100).ToString("0000000000")) // Valor
                .Set(20, 22, cedente.Carteira.PadLeft(3, '0')) // Carteira
                .Set(23, 31, FormataNossoNumero(nossoNumero, cedente, dtVencto).Replace("-","").Replace("/","").Substring(3,9)) //Nosso Número + DAC [Agência /Conta/Carteira/Nosso Número] (Anexo 4)
                .Set(32, 35, cedente.Agencia.PadLeft(4, '0')) //N.º da Agência cedente
                .Set(36, 40, cedente.CodCedente.PadLeft(5, '0')) //N.º da Conta Corrente
                .Set(41, 41, Helper.Mod10(cedente.Agencia + cedente.CodCedente)) // DAC [Agência/Conta Corrente] (Anexo 3)
                .Set(42, 44, "000") // Zeros
                .Set(5, 5, Helper.Mod11(barras.Substring(1, 4) + barras.Substring(6, 44), 9)); //DAC  código de Barras (Anexo 2)

            return barras;
        }

        protected override LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras)
        {
            var linha = new LinhaDigitavel();

            ////campo livre = barras de 20 a 44

            //Campo 1 (AAABC.CCDDX)
            linha.Campo1
                .Add(barras.Substring(1, 3))                          //AAA =	Código do Banco na Câmara de Compensação ( Itaú=341)
                .Add(barras.Substring(4))                             //B =	Código da moeda = "9" (*)
                .Add(barras.Substring(20, 22))                        //CCC =	Código da  carteira de cobrança
                .Add(barras.Substring(23, 24))                        //DD =	Dois primeiros dígitos do Nosso Número
                .Add(Helper.Mod10(linha.Campo1.Substring(1, 9)));     //X =	DAC que amarra o campo 1 (Anexo3)

            //Campo 2 (DDDDD.DEFFFY)
            linha.Campo2
                .Add(barras.Substring(25, 30))                        //DDDDDD=	Restante do Nosso Número
                .Add(barras.Substring(31, 31))                        //E = 	DAC do campo [ Agência/Conta/Carteira/ Nosso Número ]
                .Add(barras.Substring(32, 34))                        //FFF =	Três primeiros números que identificam a Agência
                .Add(Helper.Mod10(linha.Campo2.Substring(1, 10)));    // Y =	DAC que amarra o campo 2  (Anexo 3)

            //Campo 3 (FGGGG.GGHHHZ) 
            linha.Campo3
                .Add(barras.Substring(35, 35))                      // F =	Restante do número que identifica a agência
                .Add(barras.Substring(36, 41))                      // GGGGGG =	Número da conta corrente +  DAC
                .Add(barras.Substring(42, 44))                      // HHH =	Zeros ( Não utilizado )
                .Add(Helper.Mod10(linha.Campo3.Substring(1, 10)));  // Z =	DAC que amarra o campo 3 (Anexo 3)

            //Campo 4 (K)
            linha.Campo4
                .Add(barras.Substring(5, 5));       // K =	DAC do Código de Barras (Anexo 2 )

            //Campo 5 (UUUUVVVVVVVVVV)
            linha.Campo5
                .Add(barras.Substring(6, 9))        // UUUU=	Fator de vencimento
                .Add(barras.Substring(10, 19));     // VVVVVVVVVV=	valor do Título (*)

            return linha;
        }
    }
}
using NBoleto.Bancos;
using NBoleto.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace NBoleto
{
    public abstract class Boleto
    {
        internal const string DATE_FORMAT = "dd/MM/yyyy";
        internal const string MONEY_FORMAT = "#,0.00";

        protected abstract void ValidaDados(Cedente cedente, string nossoNumero);
        protected abstract string FormataNossoNumero(string nossoNumero, Cedente cedente, DateTime dtVencto);
        protected abstract string FormataAgCodCedente(Cedente cedente);
        protected abstract CodigoBarras GerarCodigoBarras(Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto);
        protected abstract LinhaDigitavel GerarLinhaDigitavel(CodigoBarras barras);
        
        public static Boleto Gerar(int codBanco, Cedente cedente, string nossoNumero, DateTime dtVencto, decimal vrBoleto)
        {
            Boleto b = null;

            switch (codBanco)
            {
                case 1: b = new BancoBrasil(); break;
                case 104: b = new Caixa(); break;
                case 41: b = new Banrisul(); break;
                case 237: b = new Bradesco(); break;
                case 341: b = new Itau(); break;
                case 33: b = new Santander(); break;
                case 399: b = new HSBC(); break;
                default: throw new NotImplementedException("Código de banco não implementado");
            }

            b.ValidaDados(cedente, nossoNumero);
            b.NossoNumero = b.FormataNossoNumero(nossoNumero, cedente, dtVencto);
            b.AgCodCedente = b.FormataAgCodCedente(cedente);

            var barras = b.GerarCodigoBarras(cedente, nossoNumero, dtVencto, vrBoleto);
            var linhadig = b.GerarLinhaDigitavel(barras);

            b.CodigoBarras = barras.ToString();
            b.LinhaDigitavel = linhadig.ToString();

            b.Cedente = cedente.Nome + " (CNPJ: " + Helper.FormatCpfCnpj(cedente.CNPJ) + ")";
            b.Carteira = cedente.Carteira;
            b.NumeroDocumento = nossoNumero;
            b.DataProcessamento = DateTime.Now.ToString(DATE_FORMAT);
            b.DataDocumento = DateTime.Now.ToString(DATE_FORMAT);
            b.LocalPagamento = "QUALQUER AGÊNCIA BANCÁRIA ATÉ A DATA DO VENCIMENTO";
            b.DataVencimento = dtVencto.ToString(DATE_FORMAT);
            b.ValorDocumento = vrBoleto.ToString(MONEY_FORMAT);
            b.Instrucoes = "ATENÇÃO SENHOR CAIXA: NÃO RECEBER APÓS VENCIMENTO";

#if !DEBUG
            // Quando release, gera o Base64 do logotipo e codigo de barras
            b.Logotipo = Helper.Logotipo(b.Banco.Substring(0, 3));
            b.CodigoBarras = Helper.CodigoBarras(b.CodigoBarras);
#endif

            return b;
        }

        #region Propriedades publicas do boleto

        public string Descricao { get; set; }

        public string Banco { get; protected set; }
        public string Logotipo { get; protected set; }
        public string LinhaDigitavel { get; protected set; }
        public string CodigoBarras { get; protected set; }

        public string DataVencimento { get; protected set; }
        public string Cedente { get; protected set; }
        public string AgCodCedente { get; protected set; }
        public string DataDocumento { get; protected set; }
        public string NumeroDocumento { get; protected set; }
        public string DataProcessamento { get; protected set; }
        public string NossoNumero { get; protected set; }
        public string Carteira { get; protected set; }
        public string ValorDocumento { get; protected set; }

        public string LocalPagamento { get; set; }
        public string Instrucoes { get; set; }
        public string Sacado { get; set; }
        public string SacadoEndereco { get; set; }

        #endregion
    }

    #region Definição de Cedente

    public class Cedente
    {
        public Cedente(string nome, string cnpj, string carteira, string agencia, string codCedente)
        {
            this.CNPJ = cnpj;
            this.Nome = nome;
            this.Carteira = carteira;
            this.Agencia = agencia;
            this.CodCedente = codCedente;
        }

        public string Carteira { get; set; }
        public string Agencia { get; set; }
        public string CodCedente { get; set; }

        public string Nome { get; set; }
        public string CNPJ { get; set; }

        // BB
        public string Convenio { get; set; }
    }

    #endregion
}
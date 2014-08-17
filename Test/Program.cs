using NBoleto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var b = BB();
            //var b = Banrisul();
            //var b = Bradesco();
            //var b = Caixa();
            //var b = HSBC();
            //var b = Itau();
            //var b = Santander();

            // Em modo DEBUG, o CodigoBarras/Logotipo são apenas os numeros.
            // Apenas em RELEASE é que é gerado a Base64

            Console.WriteLine("Banco = " + b.Banco);
            Console.WriteLine("Nosso Numero = " + b.NossoNumero);
            Console.WriteLine("Linha Digitavel = " + b.LinhaDigitavel);
            Console.WriteLine("Codigo de Barras = " + b.CodigoBarras);

            Console.ReadKey();
        }

        static Boleto BB()
        {
            var cedente = new Cedente("PONTO SERVICE", "10538495000160", "16", "0367", "00046197");
            cedente.Convenio = "000000";

            return Boleto.Gerar(1,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto Banrisul()
        {
            var cedente = new Cedente("TESTE INFORMATICA", "55351958000125", "1", "0075", "825138409");

            return Boleto.Gerar(41,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto Bradesco()
        {
            var cedente = new Cedente("SOMBRIO COMÉRCIO E SERVIÇOS DE INFORMÁTICA LTDA ", "", "09", "03255", "01406221");

            return Boleto.Gerar(237,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto Itau()
        {
            var cedente = new Cedente("BANCO ITAUCARD SA", "", "175", "2525", "04516");
            
            return Boleto.Gerar(341,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto Santander()
        {
            var cedente = new Cedente("SIND. EMPRESAS INFORMÁTICA RS", "92954957000195", "CSR", "10011", "1615700");

            return Boleto.Gerar(033,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto HSBC()
        {
            var cedente = new Cedente("SIND. EMPRESAS INFORMÁTICA RS", "92954957000195", "CNR", "0111", "3498506");

            return Boleto.Gerar(399,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }

        static Boleto Caixa()
        {
            var cedente = new Cedente("SIND. EMPRESAS INFORMÁTICA RS", "64297823000103", "SR", "0120", "373220");

            return Boleto.Gerar(104,
                cedente,
                "12345",
                new DateTime(2013, 3, 18),
                70.98m);
        }
    }
}

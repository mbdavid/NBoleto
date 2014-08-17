> NBoleto is a library for Brazil only payments, so all documents are in portuguese.

---
# NBoleto - Boleto banc�rio para .NET

O NBoleto � uma DLL para calculo de boletos banc�rios **n�o registrado** (a forma mais simples e mais utilizada para web).
Atualmente o NBoleto est� implementado para os seguintes bancos:

- 001 - Banco do Brasil
- 041 - Banrisul
- 237 - Bradesco
- 104 - Caixa
- 033 - Santander
- 341 - Itau
- 399 - HSBC

O NBoleto n�o imprime o boleto, n�o gera o HTML nem a imagem do boleto, apenas faz os calculos: Nosso N�mero, Linha digitavel e C�digo de Barras.
� a aplica��o que usa o NBoleto a responsavel por criar o design do boleto e usar a classe `Boleto` para preencher todos os campos.

```
var cedente = new Cedente(nome, nrCNPJ, nrCarteira, codAgencia, codCedente);

var boleto = Boleto.Gerar(041, cedente, nossoNumero, dtVencto, vrBoleto); 

// A classe Boleto contem todas as informa��es para montar um boleto
// Todas as propriedades s�o string e j� est�o formatadas
// Abaixo as propriedades que s�o calculadas pelo NBoleto

boleto.Banco           // C�digo do banco
boleto.Logotipo        // Imagem do logotipo do banco, em formato Base64 (<img src="...")
boleto.LinhaDigitavel  // Linha digitavel calculada e formatada conforme regra do banco
boleto.CodigoBarras    // Imagem do codigo de barras, em formato Base64
boleto.NossoNumero     // Nosso numero formatado conforme regras do banco

// Demais propriedades que s�o usadas para emiss�o de um boleto
boleto.LocalPagamento
boleto.DataVencimento
boleto.Cedente
boleto.AgCodCedente
boleto.DataDocumento
boleto.NumeroDocumento
boleto.DataProcessamento
boleto.Carteira
boleto.ValorDocumento
boleto.Instrucoes
boleto.Sacado
boleto.SacadoEndereco

```

### Homologa��o nos bancos

Esta biblioteca foi montada a partir dos documentos disponibilizados pelos bancos em seus sites.
Todos os documentos utilizados est�o no diretorio `\Docs` e foram feitos apenas testes internos.

**ATEN��O: ** Os boletos n�o foram homologados junto aos bancos. Eu at� que tentei, mas a burocracia era tanta que n�o tive paciencia! Ou seja, use a seu risco! 


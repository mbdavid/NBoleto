> NBoleto is a library for Brazil only payments, so all documents are in portuguese.

---
# NBoleto - Boleto bancário para .NET

O NBoleto é uma DLL para calculo de boletos bancários **não registrado** (a forma mais simples e mais utilizada para web).
Atualmente o NBoleto está implementado para os seguintes bancos:

- 001 - Banco do Brasil
- 041 - Banrisul
- 237 - Bradesco
- 104 - Caixa
- 033 - Santander
- 341 - Itau
- 399 - HSBC

O NBoleto não imprime o boleto, não gera o HTML nem a imagem do boleto, apenas faz os calculos: Nosso Número, Linha digitavel e Código de Barras.
É a aplicação que usa o NBoleto a responsavel por criar o design do boleto e usar a classe `Boleto` para preencher todos os campos.

```
var cedente = new Cedente(nome, nrCNPJ, nrCarteira, codAgencia, codCedente);

var boleto = Boleto.Gerar(041, cedente, nossoNumero, dtVencto, vrBoleto); 

// A classe Boleto contem todas as informações para montar um boleto
// Todas as propriedades são string e já estão formatadas
// Abaixo as propriedades que são calculadas pelo NBoleto

boleto.Banco           // Código do banco
boleto.Logotipo        // Imagem do logotipo do banco, em formato Base64 (<img src="...")
boleto.LinhaDigitavel  // Linha digitavel calculada e formatada conforme regra do banco
boleto.CodigoBarras    // Imagem do codigo de barras, em formato Base64
boleto.NossoNumero     // Nosso numero formatado conforme regras do banco

// Demais propriedades que são usadas para emissão de um boleto
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

### Homologação nos bancos

Esta biblioteca foi montada a partir dos documentos disponibilizados pelos bancos em seus sites.
Todos os documentos utilizados estão no diretorio `\Docs` e foram feitos apenas testes internos.

**ATENÇÃO: ** Os boletos não foram homologados junto aos bancos. Eu até que tentei, mas a burocracia era tanta que não tive paciencia! Ou seja, use a seu risco! 


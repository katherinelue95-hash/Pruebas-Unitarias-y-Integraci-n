using Xunit;
using CAIB.Cadenas;

namespace CAIB.Cadenas.UnitTestAppxUnit;

public class ManipuladorCadenasTests
{
    private readonly ManipuladorCadenas _manipulador = new ManipuladorCadenas();

    [Fact]
    public void Invertir_TextoNormal_RetornaTextoInvertido()
    {
        // Arrange
        string texto = "hola";
        string esperado = "aloh";

        // Act
        string actual = _manipulador.Invertir(texto);

        // Assert
        Assert.Equal(esperado, actual);
    }

    [Theory]
    [InlineData("hola", 2)]
    [InlineData("murcielago", 5)]
    [InlineData("xyz", 0)]
    public void ContarVocales_VariosTextos_RetornaCantidadCorrecta(string texto, int esperado)
    {
        // Act
        int actual = _manipulador.ContarVocales(texto);

        // Assert
        Assert.Equal(esperado, actual);
    }
}
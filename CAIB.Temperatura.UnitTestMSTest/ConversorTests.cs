using Microsoft.VisualStudio.TestTools.UnitTesting;
using CAIB.Temperatura;

namespace CAIB.Temperatura.UnitTestMSTest;

[TestClass]
public class ConversorTests
{
    private readonly Conversor _conversor = new Conversor();

    [TestMethod]
    public void CelsiusAFahrenheit_CeroGrados_Retorna32()
    {
        // Arrange
        double celsius = 0;
        double resultadoEsperado = 32;

        // Act
        double resultadoActual = _conversor.CelsiusAFahrenheit(celsius);

        // Assert
        Assert.AreEqual(resultadoEsperado, resultadoActual);
    }

    [TestMethod]
    public void FahrenheitACelsius_32Grados_RetornaCero()
    {
        // Arrange
        double fahrenheit = 32;
        double resultadoEsperado = 0;

        // Act
        double resultadoActual = _conversor.FahrenheitACelsius(fahrenheit);

        // Assert
        Assert.AreEqual(resultadoEsperado, resultadoActual);
    }
}
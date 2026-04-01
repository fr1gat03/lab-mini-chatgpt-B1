using Lib.Corpus;
using Lib.Corpus.Configuration;
using Lib.Tokenization.Application;
using Integration.DataPipeline.Mocks;

namespace Integration.DataPipeline.Tests;

[TestFixture]
public class CorpusCharTokenizerIntegrationTests
{
    [Test]
    public void CorpusAndCharTokenizer_EncodeDecodeRoundTrip()
    {
        var originalText = "Привіт, світе! Це інтеграційний тест CharTokenizer: 123.";
        var mockFs = new MockFileSystem();
        mockFs.AddFile("char_test.txt", originalText);

        var loader = new CorpusLoader(mockFs);

        var options = new CorpusLoadOptions(Lowercase: false, ValidationFraction: 0.0);
        var factory = new CharTokenizerFactory();

        var corpus = loader.Load("char_test.txt", options);
        var tokenizer = factory.BuildFromText(corpus.TrainText);

        var tokens = tokenizer.Encode(corpus.TrainText);
        var decodedText = tokenizer.Decode(tokens);

        Assert.That(corpus.TrainText, Is.EqualTo(originalText),
            "Корпус має повністю складатися з оригінального тексту (ValidationFraction = 0.0, Lowercase = false).");

        Assert.That(tokens, Is.Not.Empty,
            "Масив токенів не повинен бути порожнім.");

        Assert.That(decodedText, Is.EqualTo(corpus.TrainText),
            "Декодований текст має ідеально збігатися з оригінальним текстом корпусу (повний round-trip).");
    }
}
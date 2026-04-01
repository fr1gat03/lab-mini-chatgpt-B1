using Lib.Corpus;
using Lib.Corpus.Configuration;
using Lib.Tokenization.Application;
using Integration.DataPipeline.Mocks;

namespace Integration.DataPipeline.Tests;

[TestFixture]
public class PipelineIntegrationTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void LoadCorpus_WithDifferentLowercaseOptions_WordTokenizerProducesConsistentVocab(bool useCorpusLowercase)
    {
        var rawText = "Привіт Світе ПРИВІТ";
        var mockFs = new MockFileSystem();
        mockFs.AddFile("dummy.txt", rawText);

        var loader = new CorpusLoader(mockFs);
        var options = new CorpusLoadOptions(Lowercase: useCorpusLowercase, ValidationFraction: 0.0);
        var factory = new WordTokenizerFactory();

        var dataset = loader.Load("dummy.txt", options);

        var tokenizer = factory.BuildFromText(dataset.TrainText);

        var tokens = tokenizer.Encode(dataset.TrainText);
        var decodedText = tokenizer.Decode(tokens);

        Assert.That(tokenizer.VocabSize, Is.EqualTo(3), "Розмір словника має бути ідентичним незалежно від налаштувань.");

        Assert.That(decodedText, Is.EqualTo("привіт світе привіт"));
    }

    [Test]
    public void LoadCorpus_WithMissingFile_TokenizerBuildsFromFallbackText()
    {
        var fallbackText = "це резервний текст";
        var mockFs = new MockFileSystem();

        var loader = new CorpusLoader(mockFs);
        var options = new CorpusLoadOptions(Lowercase: true, ValidationFraction: 0.0, FallbackText: fallbackText);
        var factory = new WordTokenizerFactory();

        var dataset = loader.Load("missing.txt", options);
        var tokenizer = factory.BuildFromText(dataset.TrainText);

        var tokens = tokenizer.Encode(dataset.TrainText);
        var decodedText = tokenizer.Decode(tokens);

        Assert.That(tokenizer.VocabSize, Is.EqualTo(4), "Словник має містити токени з FallbackText.");
        Assert.That(decodedText, Is.EqualTo(fallbackText), "Декодований текст має збігатися з FallbackText.");
    }

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
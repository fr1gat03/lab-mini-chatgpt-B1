using Lib.Corpus;
using Lib.Corpus.Configuration;
using Lib.Tokenization.Application;
using Integration.DataPipeline.Mocks;

namespace Integration.DataPipeline.Tests;

[TestFixture]
public class AdvancedEdgeCasesTests
{
    [TestCase("апельсин")]
    [TestCase("чіпси")]
    [TestCase("123456789")]
    [TestCase("Supraisthebestcarintheworld")]
    public void WordTokenizer_EncodeUnknownWord_AssignsUnkTokenIdZero(string unknownWord)
    {
        var mockFs = new MockFileSystem();
        mockFs.AddFile("train.txt", "яблуко сливи банани");
        var loader = new CorpusLoader(mockFs);
        var dataset = loader.Load("train.txt");
        
        var factory = new WordTokenizerFactory();
        var tokenizer = factory.BuildFromText(dataset.TrainText);

        var tokens = tokenizer.Encode($"яблуко {unknownWord}");

        Assert.That(tokens.Length, Is.EqualTo(2), "Має бути закодовано рівно 2 токени.");
        Assert.That(tokens[0], Is.Not.EqualTo(0), "Слово 'яблуко' має отримати нормальний ID.");
        Assert.That(tokens[1], Is.EqualTo(0), $"Слово '{unknownWord}' має отримати ID = 0 (<UNK>).");
    }

    [TestCase("Слово1 Слово2 Слово3")]
    [TestCase("Слово1,   \t\t \n\n Слово2. \r\n Слово3!")]
    [TestCase("\n\tСлово1 Слово2 Слово3\t\n")]
    [TestCase("Слово1.,..       Слово2   .,.    Слово3")]
    public void WordTokenizer_TextWithVariousSpaces_DoesNotCreateEmptyTokens(string dirtyText)
    {
        var mockFs = new MockFileSystem();
        mockFs.AddFile("dirty.txt", dirtyText);
        
        var loader = new CorpusLoader(mockFs);
        var dataset = loader.Load("dirty.txt");
        
        var factory = new WordTokenizerFactory();
        var tokenizer = factory.BuildFromText(dataset.TrainText);

        Assert.That(tokenizer.VocabSize, Is.EqualTo(4), 
            "Токенізатор має ігнорувати зайві пробіли, символи та переноси рядків і не створювати порожніх токенів.");
    }

    [Test]
    public void CorpusLoader_ValidationFractionOne_LeavesTrainTextEmpty()
    {
        var mockFs = new MockFileSystem();
        string fullText = "Цей текст має повністю піти у валідацію.";
        mockFs.AddFile("extreme.txt", fullText);
        
        var loader = new CorpusLoader(mockFs);
        var options = new CorpusLoadOptions(ValidationFraction: 1.0); 

        var dataset = loader.Load("extreme.txt", options);
        var factory = new WordTokenizerFactory();
        var tokenizer = factory.BuildFromText(dataset.TrainText);

        Assert.That(dataset.TrainText, Is.Empty, "TrainText має бути порожнім.");
        Assert.That(dataset.ValText, Is.EqualTo(fullText), "ValText має містити весь текст.");
        Assert.That(tokenizer.VocabSize, Is.EqualTo(1), "Словник для порожнього TrainText має розмір 1 (<UNK>).");
    }

    [Test]
    public void CharTokenizer_WithUnseenCharacters_DecodesToUnkRepresentation()
    {
        var mockFs = new MockFileSystem();
        mockFs.AddFile("english.txt", "abc"); 
        var loader = new CorpusLoader(mockFs);
        var dataset = loader.Load("english.txt");
        
        var factory = new CharTokenizerFactory();
        var tokenizer = factory.BuildFromText(dataset.TrainText);

        var tokens = tokenizer.Encode("aї7");
        var decoded = tokenizer.Decode(tokens);

        Assert.That(tokens.Length, Is.EqualTo(3), "Всього 3 токени.");
        Assert.That(tokens[0], Is.Not.EqualTo(0), "'a' має нормальний ID.");
        Assert.That(tokens[1], Is.EqualTo(0), "'ї' має стати <UNK>.");
        Assert.That(tokens[2], Is.EqualTo(0), "'7' стає <UNK>.");
        
        Assert.That(decoded, Is.EqualTo("a\0\0"), "Невідомі символи мають декодуватися в NUL.");
    }
}
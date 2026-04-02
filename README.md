# B1: Data Pipeline — How to integrate

Тут описується, як командам з Етапу 3 підключити та використовувати підсистему підготовки даних. Наша підсистема відповідає за завантаження тексту, нормалізацію, розбиття на тренувальну/валідаційну вибірки та побудову токенізаторів.

Всі наші токенізатори реалізують спільний контракт `MiniChatGPT.Contracts.ITokenizer`.

---

## 1. Як отримати ITokenizer з корпусу (шлях до файлу)

Процес отримання готового токенізатора складається з трьох кроків: ініціалізація завантажувача з доступом до файлової системи, завантаження корпусу та побудова токенізатора через фабрику.

**Важливо (Migration Note):** Словник токенізатора завжди будується **тільки на основі тренувальної вибірки `TrainText`**, щоб уникнути витоку даних з валідаційної вибірки.

```csharp
// 1 Створюємо файлову систему та завантажувач
IFileSystem fileSystem = new DefaultFileSystem();
ICorpusLoader loader = new CorpusLoader(fileSystem);

// 2 Завантажуємо корпус
Corpus dataset = loader.Load("data/input.txt");

// 3 Створюємо фабрику та будуємо токенізатор лише на TrainText
ITokenizerFactory factory = new WordTokenizerFactory();
ITokenizer tokenizer = factory.BuildFromText(dataset.TrainText);
```

## 2. Як вибрати Word vs Char

Вибір між посимвольною та послівною токенізацією відбувається на етапі створення фабрики.

* **Для створення CharTokenizer:** Використовуйте `CharTokenizerFactory()`. Розмір словника буде малим (близько 50-100 токенів), але контекст моделі заповнюватиметься швидше.
* **Для створення WordTokenizer:** Використовуйте `WordTokenizerFactory()`. Він розбиває текст на слова, ігноруючи пунктуацію. Розмір словника буде великим, але кожен токен нестиме більше змістового навантаження.

```csharp
// Для символьної моделі:
ITokenizerFactory charFactory = new CharTokenizerFactory();
ITokenizer charTokenizer = charFactory.BuildFromText(dataset.TrainText);

// Для словесної моделі:
ITokenizerFactory wordFactory = new WordTokenizerFactory();
ITokenizer wordTokenizer = wordFactory.BuildFromText(dataset.TrainText);
```

---

## 3. Опції CorpusLoadOptions

Клас `CorpusLoadOptions` дозволяє гнучко налаштувати процес завантаження тексту. Ви можете передати його другим параметром у метод `loader.Load()`.

**Доступні параметри:**

* **`Lowercase`** (Тип: `bool`, За замовчуванням: `false`):
  Якщо `true`, весь текст буде переведено в нижній регістр перед обробкою. **Рекомендовано встановлювати `true` при використанні `WordTokenizer`**, щоб зменшити розмір словника ("Слово" і "слово" мали однаковий ID).
* **`ValidationFraction`** (Тип: `double`, За замовчуванням: `0.1`):
  Відсоток тексту від 0.0 до 1.0, який буде відрізано в кінець файлу і поміщено в `ValText`. Наприклад, `0.2` означає, що 20% тексту піде на валідацію, а 80% — у `TrainText`.
* **`FallbackText`** (Тип: `string`, За замовчуванням: `""`):
  Резервний текст, який буде використано, якщо файл за вказаним шляхом не знайдено (запобігає падінню програми).

**Приклад використання опцій:**

```csharp
var options = new CorpusLoadOptions(
    Lowercase: true, 
    ValidationFraction: 0.15, 
    FallbackText: "Резервний датасет"
);
Corpus dataset = loader.Load("data/input.txt", options);
```

---

## 4. Приклади коду для команд Етапу 3

### Приклад для команди Trainer

Команді, що займається тренуванням, потрібно отримати токенізатор.

```csharp
// 1 Ініціалізація Pipeline
var loader = new CorpusLoader(new DefaultFileSystem());
var options = new CorpusLoadOptions(Lowercase: true, ValidationFraction: 0.1);
Corpus dataset = loader.Load("data/showcase.txt", options);

// 2 Побудова токенізатора
var factory = new WordTokenizerFactory();
ITokenizer tokenizer = factory.BuildFromText(dataset.TrainText);

// 3 Кодування даних для тренування
int[] trainTokens = tokenizer.Encode(dataset.TrainText);
int[] valTokens = tokenizer.Encode(dataset.ValText);

// 4 Передача у вашу підсистему (псевдокод)
var provider = new TokenBatchProvider(trainTokens);
model.Train(provider);
```

### Приклад для команди Chat

Команді чату корпус потрібен для того, щоб побудувати або відновити з чекпоінту токенізатор. Далі вони працюють лише з методами **`Encode` для вводу користувача та** `Decode` для виводу моделі.

```csharp
// Припускаємо, що токенізатор вже побудовано або відновлений з JSON
string userPrompt = "Привіт, як справи?";

// 1. Кодуємо запит користувача
int[] promptTokens = tokenizer.Encode(userPrompt);

// 2. Згодовуємо токени генератору (псевдокод)
int[] generatedTokens = TextGenerator.Run(promptTokens);

// 3. Декодуємо результат назад у текст
string finalAnswer = tokenizer.Decode(generatedTokens);
```
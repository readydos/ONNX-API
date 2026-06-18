using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntimeGenAI;
using OnnxChatApi.Options;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace OnnxChatApi.Services;

public sealed class ONNXChatService : IChatService, IDisposable {
    private readonly ONNXGenAIOptions _options;
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;
    private readonly SemaphoreSlim _generationLock = new(1, 1);

    public ONNXChatService(IOptions<ONNXGenAIOptions> options) {
        _options = options.Value;

        if (!Directory.Exists(_options.ModelPath)) {
            throw new DirectoryNotFoundException(
                $"ONNX GenAI model folder not found: {_options.ModelPath}");
        }

        _model = new Model(_options.ModelPath);
        _tokenizer = new Tokenizer(_model);
    }

    public async Task<string> ChatAsync(string userMessage, CancellationToken cancellationToken) {
        await _generationLock.WaitAsync(cancellationToken);

        try {
            var prompt = BuildPrompt(userMessage);

            using var sequences = _tokenizer.Encode(prompt);
            using var generatorParams = new GeneratorParams(_model);

            generatorParams.SetSearchOption("max_length", _options.MaxLength);
            generatorParams.SetSearchOption("temperature", _options.Temperature);
            generatorParams.SetSearchOption("top_p", _options.TopP);
            generatorParams.SetSearchOption("do_sample", true);

            using var generator = new Generator(_model, generatorParams);

            generator.AppendTokenSequences(sequences);

            using TokenizerStream ts = _tokenizer.CreateStream();

            StringBuilder sb = new();

            while (!generator.IsDone()) {
                cancellationToken.ThrowIfCancellationRequested();
                generator.GenerateNextToken();

                string piece = ts.Decode(generator.GetSequence(0)[^1]);
                //if (piece == _thinkStart) {
                //    continue;
                //}
                sb.Append($"{piece} ");
            }

            var outputTokens = generator.GetSequence(0);
            var fullText = _tokenizer.Decode(outputTokens);

            return TrimPromptEcho(fullText, prompt);
        } finally {
            _generationLock.Release();
        }
    }

    private string BuildPrompt(string userMessage) {
        return $"""
        <|system|>
        {_options.SystemMessage}
        <|user|>
        {userMessage}
        <|assistant|>
        """;
    }

    private static string TrimPromptEcho(string generatedText, string prompt) {
        return generatedText.StartsWith(prompt, StringComparison.Ordinal)
            ? generatedText[prompt.Length..].Trim()
            : generatedText.Trim();
    }

    public void Dispose() {
        _tokenizer.Dispose();
        _model.Dispose();
        _generationLock.Dispose();
    }
}
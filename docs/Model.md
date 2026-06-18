# Model Configuration

## Model Directory

Example:

```text
models/
└── phi/
    ├── model.onnx
    ├── tokenizer.json
    ├── genai_config.json
    └── ...
```

## Configuration

```json
{
  "OnnxGenAI": {
    "ModelPath": "./models/phi"
  }
}
```

## Supported Models

Any model supported by ONNX Runtime GenAI.

Examples:

- Phi-3 Mini
- Phi-4
- Llama variants converted for GenAI
- Gemma variants converted for GenAI

Refer to ONNX Runtime GenAI documentation for supported model layouts.

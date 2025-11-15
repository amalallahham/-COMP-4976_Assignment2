# AI Tribute Rewriter Feature - Implementation Summary

## Overview
Implemented a full AI-powered obituary rewrite feature that allows users to transform rough bullet-point notes into polished, formal obituary paragraphs using Ollama (local AI models).

## What Was Implemented

### Backend (Server)
1. **New Controller: `AIController.cs`**
   - Endpoint: `POST /api/ai/rewrite`
   - Accepts JSON body: `{ "text": "string" }`
   - Connects to Ollama API running locally
   - Includes intelligent text cleaning to remove AI-generated prefixes and quotes
   - Returns only the cleaned obituary text

2. **Configuration Updates**
   - Added Ollama settings to `appsettings.json` and `appsettings.Development.json`
   - Configurable Ollama URL (defaults to `http://localhost:11434`)
   - Configurable model name (defaults to `llama3:latest`)

3. **HttpClient Registration**
   - Added `AddHttpClient()` to `Program.cs` for external API calls

### Frontend (Blazor Client)
1. **Updated Pages**
   - `CreateObituary.razor` - Added "Rewrite with AI" button
   - `EditObituary.razor` - Added "Rewrite with AI" button
   - Both pages include error handling and loading states

2. **Authentication Improvements**
   - Updated `Program.cs` to use `JwtAuthorizationMessageHandler`
   - Automatically adds JWT tokens to all API requests
   - Fixed 401 Unauthorized errors when creating/editing obituaries

3. **User Experience**
   - Button appears below the Biography textarea
   - Shows "Rewriting..." state while processing
   - Displays error messages if the rewrite fails
   - Automatically replaces the biography text with the AI-rewritten version

## How It Works

1. User enters rough notes in the Biography field (e.g., "born 1952, loved gardening and teaching")
2. User clicks "Rewrite with AI" button
3. Client sends POST request to `/api/ai/rewrite` with the text
4. Server forwards request to Ollama API with a carefully crafted prompt
5. Ollama generates a formal obituary paragraph
6. Server cleans the response (removes prefixes, quotes, etc.)
7. Client receives and displays the cleaned text in the biography field

## AI Prompt Engineering

The system uses a carefully crafted prompt that:
- Instructs the AI to write in past tense (obituary format)
- Explicitly forbids adding information not in the original text
- Prevents hallucinations (no made-up dates, family members, etc.)
- Ensures only formal tone rewriting, not content addition
- Outputs only the obituary text (no introductory phrases)

## Setup Instructions for Team Members

### Prerequisites
- .NET 9.0 SDK installed
- Ollama installed and running

### Step 1: Install Ollama

**macOS:**
```bash
# Download and install from https://ollama.com
# Or use Homebrew:
brew install ollama
```

**Windows:**
- Download installer from https://ollama.com/download
- Run the installer

**Linux:**
```bash
curl -fsSL https://ollama.com/install.sh | sh
```

### Step 2: Start Ollama

**macOS/Linux:**
```bash
# Start Ollama service (usually runs automatically after installation)
ollama serve
```

**Windows:**
- Ollama should start automatically as a service
- Or run from command prompt: `ollama serve`

### Step 3: Download a Model

You need at least one language model. Recommended models:

```bash
# Download llama3:latest (recommended, ~4.7GB)
ollama pull llama3:latest

# Or download other available models:
ollama pull llama3.1:latest
ollama pull mistral-nemo:latest
ollama pull phi3:medium
```

**Note:** The first download may take several minutes depending on your internet speed.

### Step 4: Verify Ollama is Running

Test that Ollama is accessible:
```bash
curl http://localhost:11434/api/tags
```

You should see a JSON response with your installed models.

### Step 5: Configure the Application

1. **Check `appsettings.Development.json`:**
   ```json
   {
     "Ollama": {
       "BaseUrl": "http://localhost:11434",
       "Model": "llama3:latest"
     }
   }
   ```

2. **Change the model if needed:**
   - If you downloaded a different model, update the `Model` value
   - Available models: `llama3:latest`, `llama3.1:latest`, `mistral-nemo:latest`, etc.

### Step 6: Run the Application

1. **Start the server:**
   ```bash
   cd assignment.Server
   dotnet watch
   ```

2. **Start the client (in a new terminal):**
   ```bash
   cd assignment.Client
   dotnet watch
   ```

3. **Access the application:**
   - Client: http://localhost:5038
   - Server API: http://localhost:5141

### Step 7: Test the Feature

1. Log in to the application
2. Navigate to "Create Obituary" or "Edit Obituary"
3. Enter some bullet points in the Biography field, e.g.:
   ```
   born 1952, loved gardening and teaching, worked at Vancouver High School
   ```
4. Click "Rewrite with AI"
5. The text should be transformed into a formal obituary paragraph

## Troubleshooting

### "Connection Refused" Error
- **Problem:** Ollama is not running
- **Solution:** Start Ollama with `ollama serve` or ensure the service is running

### "Model not found" Error
- **Problem:** The specified model isn't downloaded
- **Solution:** Run `ollama pull llama3:latest` (or your chosen model)

### 401 Unauthorized Error
- **Problem:** Not logged in or token expired
- **Solution:** Log in at `/login` page

### AI Adds Extra Information
- **Problem:** Model is hallucinating
- **Solution:** The cleaning function should handle this, but if it persists, the prompt may need adjustment

### Slow Response Times
- **Problem:** Large model or slow hardware
- **Solution:** Try a smaller model like `llama3.2` or `qwen2.5-coder:3b`

## Configuration Options

### Environment Variables
You can override Ollama settings with environment variables:
```bash
export OLLAMA_BASE_URL="http://localhost:11434"
export OLLAMA_MODEL="llama3:latest"
```

### Different Ollama Instance
If Ollama is running on a different machine or port:
```json
{
  "Ollama": {
    "BaseUrl": "http://your-ollama-server:11434",
    "Model": "llama3:latest"
  }
}
```

## Files Modified/Created

### New Files
- `assignment.Server/Controllers/AIController.cs`

### Modified Files
- `assignment.Server/Program.cs` - Added HttpClient registration
- `assignment.Server/appsettings.json` - Added Ollama configuration
- `assignment.Server/appsettings.Development.json` - Added Ollama configuration
- `assignment.Client/Program.cs` - Added JWT authorization handler
- `assignment.Client/Pages/CreateObituary.razor` - Added AI rewrite button
- `assignment.Client/Pages/EditObituary.razor` - Added AI rewrite button

## Technical Details

### API Endpoint
- **URL:** `POST /api/ai/rewrite`
- **Request Body:** `{ "text": "string" }`
- **Response:** Plain text obituary paragraph
- **Authentication:** Not required (public endpoint)

### Ollama API Integration
- **Endpoint:** `http://localhost:11434/api/chat`
- **Method:** POST
- **Request Format:** OpenAI-compatible chat completion format
- **Response Format:** `{ "message": { "content": "..." } }`

### Text Cleaning
The `CleanObituaryText` method removes:
- Introductory phrases ("Here is...", "The rewritten version...")
- Quotes (single, double, backticks)
- Empty lines
- Phrases like "formal and respectful tone"

## Notes

- Ollama runs locally, so no API keys are needed
- Models are downloaded once and stored locally
- First request may be slower as the model loads into memory
- Subsequent requests are faster
- Works offline once models are downloaded


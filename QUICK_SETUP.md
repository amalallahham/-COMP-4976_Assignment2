# Quick Setup Guide - AI Rewrite Feature

## For Team Members: Get Ollama Working in 5 Minutes

### 1. Install Ollama
- **macOS:** `brew install ollama` or download from https://ollama.com
- **Windows:** Download installer from https://ollama.com/download
- **Linux:** `curl -fsSL https://ollama.com/install.sh | sh`

### 2. Download a Model
```bash
ollama pull llama3:latest
```
*(This downloads ~4.7GB, takes a few minutes)*

### 3. Verify It's Working
```bash
curl http://localhost:11434/api/tags
```
You should see JSON with your models listed.

### 4. Run the Application
```bash
# Terminal 1 - Server
cd assignment.Server
dotnet watch

# Terminal 2 - Client  
cd assignment.Client
dotnet watch
```

### 5. Test It
1. Go to http://localhost:5038
2. Log in
3. Create/Edit an obituary
4. Enter text like: "born 1952, loved gardening"
5. Click "Rewrite with AI"
6. See the magic! ✨

If you get errors:
- **"Connection refused"** → Ollama isn't running. Start it with `ollama serve`
- **"Model not found"** → Run `ollama pull llama3:latest`
- **401 Unauthorized** → Make sure you're logged in

For detailed information, see `AI_REWRITE_FEATURE.md`


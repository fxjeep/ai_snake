# RAG Demo Application - Implementation Plan

## Overview
This document outlines the steps to build a Retrieval-Augmented Generation (RAG) application using Groq (LLM), ChromaDB (Vector Database), and Hugging Face (Embeddings).

---

## Step 1: Setup Environment

### 1.1 Install Dependencies
```bash
pip install python-dotenv chromadb groq sentence-transformers langchain langchain-community pypdf
```

### 1.2 Configure Environment Variables (.env)
```
GROQ_API_KEY=your_groq_api_key_here
HF_TOKEN=your_huggingface_token_here
```

### 1.3 Verify Connections
```bash
python src/connection_tester.py
```

---

## Step 2: Document Ingestion Pipeline

### 2.1 Load PDF Documents
```python
from pypdf import PdfReader
from pathlib import Path

def load_pdfs(folder_path):
    all_text = []
    for pdf_file in Path(folder_path).glob("*.pdf"):
        reader = PdfReader(str(pdf_file))
        text = ""
        for page in reader.pages:
            text += page.extract_text() or ""
        all_text.append({"filename": pdf_file.name, "content": text})
    return all_text
```

### 2.2 Text Chunking
```python
from langchain.text_splitter import RecursiveCharacterTextSplitter

def chunk_text(documents, chunk_size=1000, chunk_overlap=200):
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap
    )
    chunks = []
    for doc in documents:
        splits = splitter.split_text(doc["content"])
        for i, split in enumerate(splits):
            chunks.append({
                "content": split,
                "source": doc["filename"],
                "chunk_id": i
            })
    return chunks
```

### 2.3 Generate Embeddings
```python
from sentence_transformers import SentenceTransformer

def get_embeddings(texts, hf_token=None):
    model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2", token=hf_token)
    return model.encode(texts)
```

### 2.2 Text Chunking
```python
from langchain.text_splitter import RecursiveCharacterTextSplitter

splitter = RecursiveCharacterTextSplitter(
    chunk_size=1000,
    chunk_overlap=200
)
chunks = splitter.split_documents(documents)
```

### 2.3 Generate Embeddings
```python
from sentence_transformers import SentenceTransformer

embedding_model = SentenceTransformer("all-MiniLM-L6-v2")

def get_embeddings(texts):
    return embedding_model.encode(texts)
```

---

## Step 3: Vector Database Setup

### 3.1 Initialize ChromaDB
```python
import chromadb

CHROMA_PATH = "./chroma_data"
COLLECTION_NAME = "pdf_documents"

client = chromadb.PersistentClient(path=CHROMA_PATH)
collection = client.get_or_create_collection(name=COLLECTION_NAME)
```

### 3.2 Store Embeddings
```python
# Add to collection
collection.add(
    ids=[f"chunk_{chunk['source']}_{chunk['chunk_id']}" for chunk in chunks],
    documents=[chunk["content"] for chunk in chunks],
    embeddings=embeddings.tolist(),
    metadatas=[{"source": chunk["source"], "chunk_id": chunk["chunk_id"]} for chunk in chunks]
)
```

### 3.3 Clear ChromaDB (Optional)
```python
# To reset and redo ingestion
client.delete_collection(name=COLLECTION_NAME)
```

---

## Step 4: Query and Retrieval

### 4.1 Query Embedding
```python
query = "What is the main topic?"
query_embedding = get_embeddings([query])
```

### 4.2 Similarity Search
```python
results = collection.query(
    query_embeddings=query_embedding.tolist(),
    n_results=3  # Top 3 relevant chunks
)

retrieved_docs = results["documents"][0]
```

---

## Step 5: LLM Integration (Groq)

### 5.1 Initialize Groq Client
```python
from groq import Groq
import os
from dotenv import load_dotenv

load_dotenv()
client = Groq(api_key=os.getenv("GROQ_API_KEY"))
```

### 5.2 Create Context-Aware Prompt
```python
context = "\n\n".join(retrieved_docs)

prompt = f"""Based on the following context, answer the question.

Context:
{context}

Question: {query}

Answer:"""
```

### 5.3 Generate Response
```python
response = client.chat.completions.create(
    model="llama-3.1-8b-instant",  # Use available model
    messages=[{"role": "user", "content": prompt}],
    max_tokens=500
)

answer = response.choices[0].message.content
```

---

## Step 6: Build RAG Chain

### 6.1 Complete RAG Function
```python
def rag_query(user_query):
    # 1. Embed the query
    query_embedding = get_embeddings([user_query])
    
    # 2. Retrieve relevant documents
    results = collection.query(
        query_embeddings=query_embedding.tolist(),
        n_results=3
    )
    retrieved_docs = results["documents"][0]
    
    # 3. Build prompt with context
    context = "\n\n".join(retrieved_docs)
    prompt = f"""Based on the following context, answer the question.
    
Context:
{context}

Question: {user_query}

Answer:"""
    
    # 4. Generate response
    response = client.chat.completions.create(
        model="llama-3.1-8b-instant",
        messages=[{"role": "user", "content": prompt}],
        max_tokens=500
    )
    
    return response.choices[0].message.content
```

---

## Step 7: Build User Interface

### 7.1 Simple CLI Interface
```python
def main():
    print("RAG Demo - Ask me anything about your documents!")
    while True:
        query = input("\nYou: ")
        if query.lower() == "exit":
            break
        answer = rag_query(query)
        print(f"\nAssistant: {answer}")
```

### 7.2 Streamlit Web Interface (Optional)
```bash
pip install streamlit
streamlit run app.py
```

---

## Step 8: Testing and Optimization

### 8.1 Test with Sample Queries
```bash
python src/connection_tester.py
```

### 8.2 Optimize Parameters
- **Chunk size**: Adjust based on document structure
- **Number of retrieved chunks**: Balance between context and relevance
- **Embedding model**: Try different models for better accuracy

---

## Architecture Diagram

```
┌─────────────┐     ┌─────────────────┐     ┌──────────────┐
│  Documents  │────▶│  Text Chunker   │────▶│   ChromaDB   │
└─────────────┘     └─────────────────┘     └──────────────┘
                                                   │
                                                   ▼
┌─────────────┐     ┌─────────────────┐     ┌──────────────┐
│   User      │────▶│   Embed Query   │────▶│   Similarity │
│   Query     │     └─────────────────┘     │    Search    │
└─────────────┘            │                └──────────────┘
                           │                        │
                           ▼                        ▼
                    ┌─────────────────┐     ┌──────────────┐
                    │  Groq API LLM   │◀────│   Retrieved  │
                    │   (Context)     │     │    Chunks    │
                    └─────────────────┘     └──────────────┘
```

---

## File Structure
```
RAG_Groq_Chroma/
├── .env                    # API keys
├── chroma_data/            # ChromaDB persistence
├── PDFFiles/               # Source PDF documents (add your PDFs here)
├── src/
│   ├── connection_tester.py
│   ├── ingest.py           # Document ingestion (Steps 2 & 3)
│   └── rag_chain.py        # RAG pipeline (Steps 4-6)
├── RAG_steps.md            # This file
└── requirements.txt
```

---

## Available Groq Models
| Model | ID | Best For |
|-------|-----|----------|
| Llama 3.1 8B | `llama-3.1-8b-instant` | Fast, cheap responses |
| Llama 3.3 70B | `llama-3.3-70b-versatile` | Complex reasoning |
| GPT OSS 120B | `openai/gpt-oss-120b` | High quality |

---

## Next Steps
1. Add sample documents to `documents/` folder
2. Run `python src/ingest.py` to populate the vector database
3. Start the application and test with queries

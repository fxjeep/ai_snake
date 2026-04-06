import chromadb
from sentence_transformers import SentenceTransformer

CHROMA_PATH = "./chroma_data"
COLLECTION_NAME = "demo"

# Step 1: Text chunks (e.g., after splitting a PDF)
text_chunks = [
    "Machine learning is a subset of artificial intelligence.",
    "Deep learning uses neural networks with multiple layers.",
    "Natural language processing deals with text and speech.",
]

# Step 2: Load embedding model
model = SentenceTransformer("sentence-transformers/all-MiniLM-L6-v2")

# Step 3: Generate embeddings
embeddings = model.encode(text_chunks)

print("=" * 50)
print("Example: String Chunk → Embedding → ChromaDB")
print("=" * 50)

print(f"\n[1] Text Chunks ({len(text_chunks)} chunks):")
for i, chunk in enumerate(text_chunks):
    print(f'    Chunk {i}: "{chunk}"')

print(f"\n[2] Embeddings shape: {embeddings.shape}")
print(f"    Each embedding has {embeddings.shape[1]} dimensions")
print(f"    First embedding (first 5 values): {embeddings[0][:5].tolist()}")

# Step 4: Store in ChromaDB
client = chromadb.PersistentClient(path=CHROMA_PATH)
collection = client.get_or_create_collection(name=COLLECTION_NAME)

ids = [f"chunk_{i}" for i in range(len(text_chunks))]
metadatas = [{"index": i} for i in range(len(text_chunks))]

collection.add(
    ids=ids,
    documents=text_chunks,
    embeddings=embeddings.tolist(),
    metadatas=metadatas,
)

print(f"\n[3] Stored in ChromaDB:")
print(f"    Collection: {COLLECTION_NAME}")
print(f"    Total documents: {collection.count()}")

# Step 5: Query ChromaDB
query = "What is deep learning?"
query_embedding = model.encode([query])

results = collection.query(query_embeddings=query_embedding.tolist(), n_results=2)

print(f'\n[4] Query: "{query}"')
print(f"\n[5] Top 2 Results:")
for i, (doc, distance) in enumerate(
    zip(results["documents"][0], results["distances"][0])
):
    similarity = 1 - distance  # Convert distance to similarity
    print(f'    Result {i + 1}: "{doc}"')
    print(f"            Similarity: {similarity:.4f}")

# Cleanup
client.delete_collection(name=COLLECTION_NAME)
print(f"\n[OK] Demo collection cleaned up")

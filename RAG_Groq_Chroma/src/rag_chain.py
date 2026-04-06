import os
import re
from dotenv import load_dotenv

load_dotenv()

CHROMA_BASE_PATH = "./chroma_data"
GROQ_MODEL = "llama-3.1-8b-instant"
RERANK_MODEL = "cross-encoder/ms-marco-MiniLM-L-6-v2"
INITIAL_TOP_K = 20
RERANK_TOP_K = 3


def normalize_name(name):
    """Convert folder name to valid collection name."""
    name = name.replace("\\", "/").split("/")[-1]
    name = re.sub(r"[^a-zA-Z0-9_-]", "_", name)
    return name


def get_chroma_path(collection_name):
    """Get ChromaDB path for a specific collection."""
    return os.path.join(CHROMA_BASE_PATH, collection_name)


def get_embedding_model():
    from sentence_transformers import SentenceTransformer

    hf_token = os.getenv("HF_TOKEN")
    model = SentenceTransformer(
        "sentence-transformers/all-MiniLM-L6-v2", token=hf_token
    )
    return model


def get_reranker():
    from sentence_transformers import CrossEncoder

    hf_token = os.getenv("HF_TOKEN")
    model = CrossEncoder(RERANK_MODEL, max_length=512, token=hf_token)
    return model


def expand_query(query, num_expansions=3):
    """Generate query variations using LLM."""
    client = get_groq_client()

    prompt = f"""Generate {num_expansions} different search queries that represent different angles 
of the following question. Return ONLY the queries, one per line, no numbering, no quotes.

Original question: {query}

Queries:"""

    response = client.chat.completions.create(
        model=GROQ_MODEL,
        messages=[{"role": "user", "content": prompt}],
        temperature=0.8,
        max_tokens=200,
    )

    expanded = response.choices[0].message.content.strip().split("\n")
    expanded = [q.strip() for q in expanded if q.strip()]
    return [query] + expanded


def _vector_search(query, n_results, collection_name):
    """Internal vector search for a single query."""
    import chromadb

    embedding_model = get_embedding_model()
    query_embedding = embedding_model.encode([query])

    chroma_path = get_chroma_path(collection_name)
    client = chromadb.PersistentClient(path=chroma_path)
    collection = client.get_or_create_collection(name=collection_name)

    results = collection.query(
        query_embeddings=query_embedding.tolist(), n_results=n_results
    )

    client.close()

    documents = results["documents"][0] if results["documents"] else []
    distances = results["distances"][0] if results["distances"] else []
    metadatas = results["metadatas"][0] if results["metadatas"] else []

    return documents, distances, metadatas


def _reciprocal_rank_fusion(results_list, k=60):
    """Merge results from multiple queries using RRF."""
    doc_scores = {}

    for query_results in results_list:
        for rank, (doc, distance, meta) in enumerate(zip(*query_results)):
            if doc not in doc_scores:
                doc_scores[doc] = {"score": 0, "distance": distance, "metadata": meta}
            doc_scores[doc]["score"] += 1 / (k + rank + 1)

    sorted_docs = sorted(doc_scores.items(), key=lambda x: x[1]["score"], reverse=True)

    documents = [doc for doc, _ in sorted_docs]
    distances = [data["distance"] for _, data in sorted_docs]
    metadatas = [data["metadata"] for _, data in sorted_docs]

    return documents, distances, metadatas


def get_groq_client():
    from groq import Groq

    api_key = os.getenv("GROQ_API_KEY")
    if not api_key:
        raise ValueError("GROQ_API_KEY not set in .env")
    return Groq(api_key=api_key)


def retrieve_documents(
    query,
    n_results=3,
    use_reranker=True,
    use_expansion=False,
    collection_name="PDFFiles",
):
    if use_expansion:
        print(f"[EXPAND] Generating query variations...")
        expanded_queries = expand_query(query)
        print(f"[EXPAND] Expanded to {len(expanded_queries)} queries")

        results_list = []
        for eq in expanded_queries:
            result = _vector_search(eq, INITIAL_TOP_K, collection_name)
            results_list.append(result)

        documents, distances, metadatas = _reciprocal_rank_fusion(results_list)
    else:
        documents, distances, metadatas = _vector_search(
            query, INITIAL_TOP_K, collection_name
        )

    if not documents or not use_reranker:
        return documents[:n_results], distances[:n_results], metadatas[:n_results]

    reranker = get_reranker()
    query_doc_pairs = [[query, doc] for doc in documents]
    rerank_scores = reranker.predict(query_doc_pairs)

    reranked_indices = sorted(
        range(len(rerank_scores)), key=lambda i: rerank_scores[i], reverse=True
    )

    reranked_documents = [documents[i] for i in reranked_indices[:RERANK_TOP_K]]
    reranked_scores = [rerank_scores[i] for i in reranked_indices[:RERANK_TOP_K]]
    reranked_metadatas = [metadatas[i] for i in reranked_indices[:RERANK_TOP_K]]

    return reranked_documents, reranked_scores, reranked_metadatas


def build_prompt(query, documents, metadatas):
    if not documents:
        return f"Question: {query}\n\nAnswer: I don't have enough information to answer this question."

    context_parts = []
    for i, (doc, meta) in enumerate(zip(documents, metadatas)):
        source = meta.get("source", "unknown") if meta else "unknown"
        page_start = meta.get("page_start", "?")
        page_end = meta.get("page_end", "?")
        context_parts.append(
            f"[Document {i + 1}] (Source: {source}, Pages: {page_start}-{page_end})\n{doc}"
        )

    context = "\n\n".join(context_parts)

    prompt = f"""Based on the following context, answer the question. If the context doesn't contain relevant information, say so. Include citations in your answer by referencing the document numbers.

Context:
{context}

Question: {query}

Answer:"""

    return prompt


def generate_response(prompt):
    client = get_groq_client()

    response = client.chat.completions.create(
        model=GROQ_MODEL,
        messages=[{"role": "user", "content": prompt}],
        temperature=0.7,
        max_tokens=500,
    )

    return response.choices[0].message.content


def rag_query(
    user_query,
    n_results=3,
    use_reranker=True,
    use_expansion=False,
    collection_name="PDFFiles",
):
    print(f"\n[QUERY] {user_query}")
    print(f"[INFO] Using collection: {collection_name}")

    documents, scores, metadatas = retrieve_documents(
        user_query, n_results, use_reranker, use_expansion, collection_name
    )

    if not documents:
        return "I couldn't find any relevant documents in the database. Please ingest some PDFs first."

    print(f"[RETRIEVED] {len(documents)} documents (after reranking)")
    for i, (doc, score, meta) in enumerate(zip(documents, scores, metadatas)):
        rerank_score = round(score, 2)
        source = meta.get("source", "unknown") if meta else "unknown"
        page_start = meta.get("page_start", "?")
        page_end = meta.get("page_end", "?")
        print(
            f"  [{i + 1}] {source} pages {page_start}-{page_end} (rerank score: {rerank_score})"
        )

    prompt = build_prompt(user_query, documents, metadatas)

    print("[GENERATING] Calling Groq LLM...")
    answer = generate_response(prompt)

    return answer


def main():
    import argparse

    parser = argparse.ArgumentParser(description="RAG Query Interface")
    parser.add_argument(
        "--folder",
        type=str,
        default="PDFFiles",
        help="PDF folder name (matches collection name, e.g., 'PDFFiles', 'Books')",
    )
    parser.add_argument(
        "--no-rerank",
        action="store_true",
        help="Disable reranking (faster but less accurate)",
    )
    parser.add_argument(
        "--expand",
        action="store_true",
        help="Enable query expansion for better retrieval",
    )
    args = parser.parse_args()

    use_reranker = not args.no_rerank
    use_expansion = args.expand
    collection_name = normalize_name(args.folder)

    print("=" * 50)
    print("RAG Query Interface")
    print("=" * 50)
    print(f"\n  Collection: {collection_name}")
    print(f"  Model: {GROQ_MODEL}")
    print(f"  Reranking: {'Enabled' if use_reranker else 'Disabled'}")
    print(f"  Query Expansion: {'Enabled' if use_expansion else 'Disabled'}")
    print("\nAsk questions about your PDFs. Type 'exit' to quit.")

    while True:
        query = input("\nYou: ").strip()

        if query.lower() in ["exit", "quit", "q"]:
            print("Goodbye!")
            break

        if not query:
            continue

        answer = rag_query(
            query,
            use_reranker=use_reranker,
            use_expansion=use_expansion,
            collection_name=collection_name,
        )
        print(f"\nAssistant: {answer}")


if __name__ == "__main__":
    main()

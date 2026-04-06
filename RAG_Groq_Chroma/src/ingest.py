import os
import sys
import re
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()

CHROMA_BASE_PATH = "./chroma_data"
DEFAULT_CHUNK_SIZE = 1000
DEFAULT_CHUNK_OVERLAP = 200
DEFAULT_SEMANTIC_THRESHOLD = 0.7
DEFAULT_SEMANTIC_MIN_SENTENCES = 3
DEFAULT_SEMANTIC_MAX_SENTENCES = 10


def normalize_name(name):
    """Convert folder name to valid collection name."""
    name = name.replace("\\", "/").split("/")[-1]
    name = re.sub(r"[^a-zA-Z0-9_-]", "_", name)
    return name


def get_chroma_path(collection_name):
    """Get ChromaDB path for a specific collection."""
    return os.path.join(CHROMA_BASE_PATH, collection_name)


def clear_chroma_db(pdf_folder):
    import chromadb

    collection_name = normalize_name(pdf_folder)
    chroma_path = get_chroma_path(collection_name)

    if not Path(chroma_path).exists():
        print("[OK] ChromaDB collection does not exist")
        return

    client = chromadb.PersistentClient(path=chroma_path)
    try:
        client.delete_collection(name=collection_name)
        print(f"[OK] ChromaDB collection '{collection_name}' deleted successfully")
    except Exception:
        print(f"[OK] ChromaDB collection '{collection_name}' already empty")
    client.close()


def get_embedding_model():
    from sentence_transformers import SentenceTransformer

    hf_token = os.getenv("HF_TOKEN")
    print("  Loading embedding model (all-MiniLM-L6-v2)...")
    model = SentenceTransformer(
        "sentence-transformers/all-MiniLM-L6-v2", token=hf_token
    )
    return model


def split_into_sentences(text):
    """Split text into sentences using simple regex."""
    import re

    sentence_endings = re.compile(r"(?<=[.!?])\s+")
    sentences = sentence_endings.split(text)
    sentences = [s.strip() for s in sentences if s.strip()]
    return sentences


def semantic_chunk(
    text,
    embedding_model,
    threshold=DEFAULT_SEMANTIC_THRESHOLD,
    min_sentences=DEFAULT_SEMANTIC_MIN_SENTENCES,
    max_sentences=DEFAULT_SEMANTIC_MAX_SENTENCES,
    max_tokens=DEFAULT_CHUNK_SIZE,
):
    """
    Split text into semantically coherent chunks based on sentence similarity.

    Algorithm:
    1. Split text into sentences
    2. Group sentences together based on semantic similarity
    3. When similarity drops below threshold OR chunk exceeds max_tokens, start new chunk
    """
    sentences = split_into_sentences(text)
    if not sentences:
        return []

    if len(sentences) <= min_sentences:
        return [" ".join(sentences)] if sentences else []

    chunks = []
    current_chunk = [sentences[0]]
    current_tokens = len(sentences[0].split())

    embeddings = embedding_model.encode(sentences)

    for i in range(1, len(sentences)):
        sentence = sentences[i]
        sentence_tokens = len(sentence.split())

        similarity = 0
        if current_chunk:
            current_embedding = embeddings[i - 1]
            next_embedding = embeddings[i]
            similarity = float(
                (current_embedding * next_embedding).sum()
                / (
                    (current_embedding**2).sum() ** 0.5
                    * (next_embedding**2).sum() ** 0.5
                    + 1e-8
                )
            )

        should_split = False
        if similarity < threshold and len(current_chunk) >= min_sentences:
            should_split = True
        elif (
            current_tokens + sentence_tokens > max_tokens
            and len(current_chunk) >= min_sentences
        ):
            should_split = True
        elif len(current_chunk) >= max_sentences:
            should_split = True

        if should_split:
            chunks.append(" ".join(current_chunk))
            current_chunk = [sentence]
            current_tokens = sentence_tokens
        else:
            current_chunk.append(sentence)
            current_tokens += sentence_tokens

    if current_chunk:
        chunks.append(" ".join(current_chunk))

    return chunks


def process_single_pdf(
    pdf_file,
    embedding_model,
    collection,
    pages_per_batch=5,
    use_semantic_chunking=False,
    semantic_threshold=DEFAULT_SEMANTIC_THRESHOLD,
):
    from pypdf import PdfReader
    from langchain_text_splitters import RecursiveCharacterTextSplitter

    chunking_method = "semantic" if use_semantic_chunking else "recursive"
    print(f"  Processing: {pdf_file.name} (chunking: {chunking_method})")
    reader = PdfReader(str(pdf_file))
    total_pages = len(reader.pages)
    print(f"    Total pages: {total_pages}")

    if use_semantic_chunking:
        splitter = None
    else:
        splitter = RecursiveCharacterTextSplitter(
            chunk_size=DEFAULT_CHUNK_SIZE, chunk_overlap=DEFAULT_CHUNK_OVERLAP
        )

    total_chunks = 0
    batch_count = 0

    for batch_start in range(0, total_pages, pages_per_batch):
        batch_end = min(batch_start + pages_per_batch, total_pages)
        batch_text = ""

        for page_num in range(batch_start, batch_end):
            page_text = reader.pages[page_num].extract_text() or ""
            batch_text += page_text

        if batch_text.strip():
            batch_count += 1

            if use_semantic_chunking:
                page_chunks = semantic_chunk(
                    batch_text, embedding_model, threshold=semantic_threshold
                )
            else:
                page_chunks = splitter.split_text(batch_text)

            if page_chunks:
                batch_embeddings = embedding_model.encode(page_chunks)

                ids = [
                    f"chunk_{pdf_file.name}_{total_chunks + j}"
                    for j in range(len(page_chunks))
                ]
                metadatas = [
                    {
                        "source": pdf_file.name,
                        "batch": batch_count,
                        "page_start": batch_start + 1,
                        "page_end": batch_end,
                        "chunk_id": j,
                        "chunking_method": chunking_method,
                    }
                    for j in range(len(page_chunks))
                ]

                collection.add(
                    ids=ids,
                    documents=page_chunks,
                    embeddings=batch_embeddings.tolist(),
                    metadatas=metadatas,
                )

                total_chunks += len(page_chunks)
                print(
                    f"    Batch {batch_count}: pages {batch_start + 1}-{batch_end}, {len(page_chunks)} chunks"
                )

    if total_chunks == 0:
        print(f"    [WARN] No text extracted from {pdf_file.name}")
        return 0

    return total_chunks


def process_pdfs_one_by_one(
    folder_path,
    embedding_model,
    collection_name,
    chroma_path,
    pages_per_batch=5,
    use_semantic_chunking=False,
    semantic_threshold=DEFAULT_SEMANTIC_THRESHOLD,
):
    import chromadb

    pdf_files = list(Path(folder_path).glob("*.pdf"))

    if not pdf_files:
        print(f"[WARN] No PDF files found in {folder_path}")
        return 0

    print(f"  Found {len(pdf_files)} PDF file(s)")

    print(f"  Connecting to ChromaDB (collection: {collection_name})...")
    client = chromadb.PersistentClient(path=chroma_path)
    collection = client.get_or_create_collection(name=collection_name)

    total_chunks = 0

    for i, pdf_file in enumerate(pdf_files, 1):
        print(f"\n  [{i}/{len(pdf_files)}] Processing: {pdf_file.name}")
        chunk_count = process_single_pdf(
            pdf_file,
            embedding_model,
            collection,
            pages_per_batch,
            use_semantic_chunking,
            semantic_threshold,
        )
        total_chunks += chunk_count

    client.close()
    return total_chunks


def ingest(
    pdf_folder="./PDFFiles",
    clear_db=False,
    pages_per_batch=5,
    use_semantic_chunking=False,
    semantic_threshold=DEFAULT_SEMANTIC_THRESHOLD,
):
    collection_name = normalize_name(pdf_folder)
    chroma_path = get_chroma_path(collection_name)

    chunking_method = "semantic" if use_semantic_chunking else "recursive"

    print("\n" + "=" * 50)
    print("PDF Document Ingestion Pipeline")
    print("=" * 50)
    print(f"\n  Collection: {collection_name}")
    print(f"  Folder: {pdf_folder}")
    print(f"  Chunking: {chunking_method}")
    if use_semantic_chunking:
        print(f"  Semantic threshold: {semantic_threshold}")

    if clear_db:
        print("\n[CLEAR] Clearing existing ChromaDB...")
        clear_chroma_db(pdf_folder)

    if not Path(pdf_folder).exists():
        print(f"\n[ERROR] PDF folder '{pdf_folder}' not found")
        print(f"        Create the folder and add PDF files")
        return False

    print(f"\n[LOAD] Loading embedding model...")
    embedding_model = get_embedding_model()

    print(f"\n[PROCESS] Processing PDFs (pages_per_batch={pages_per_batch})...")
    count = process_pdfs_one_by_one(
        pdf_folder,
        embedding_model,
        collection_name,
        chroma_path,
        pages_per_batch,
        use_semantic_chunking,
        semantic_threshold,
    )

    if count == 0:
        print("\n[ERROR] No documents to process")
        return False

    print("\n" + "=" * 50)
    print(f"[DONE] Ingestion complete! {count} chunks stored in ChromaDB")
    print(f"       Collection: {collection_name}")
    print("=" * 50)
    return True


def main():
    import argparse

    parser = argparse.ArgumentParser(description="Ingest PDF documents into ChromaDB")
    parser.add_argument(
        "--folder",
        type=str,
        default="./PDFFiles",
        help="Path to PDF folder (collection name derived from folder name)",
    )
    parser.add_argument(
        "--clear", action="store_true", help="Clear existing ChromaDB before ingestion"
    )
    parser.add_argument(
        "--pages",
        type=int,
        default=5,
        help="Number of pages to process per batch (default: 5)",
    )
    parser.add_argument(
        "--semantic",
        action="store_true",
        help="Use semantic chunking instead of recursive character splitting",
    )
    parser.add_argument(
        "--threshold",
        type=float,
        default=DEFAULT_SEMANTIC_THRESHOLD,
        help=f"Semantic similarity threshold (0.0-1.0, default: {DEFAULT_SEMANTIC_THRESHOLD})",
    )
    args = parser.parse_args()

    success = ingest(
        pdf_folder=args.folder,
        clear_db=args.clear,
        pages_per_batch=args.pages,
        use_semantic_chunking=args.semantic,
        semantic_threshold=args.threshold,
    )
    sys.exit(0 if success else 1)


if __name__ == "__main__":
    main()

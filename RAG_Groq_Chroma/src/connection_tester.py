import os
import sys
from pathlib import Path
from dotenv import load_dotenv

load_dotenv()


def test_groq():
    print("\n" + "=" * 50)
    print("Testing Groq API Connection...")
    print("=" * 50)

    api_key = os.getenv("GROQ_API_KEY")
    if not api_key or api_key == "your_groq_api_key_here":
        print("[FAIL] GROQ_API_KEY not set or is placeholder")
        return False

    try:
        from groq import Groq

        client = Groq(api_key=api_key)

        response = client.chat.completions.create(
            model="llama-3.1-8b-instant",
            messages=[{"role": "user", "content": "Hi, reply with 'OK' only"}],
            max_tokens=10,
        )

        if response.choices[0].message.content.strip() == "OK":
            print("[PASS] Groq API connection successful!")
            print(f"       Model: llama-3.1-8b-instant")
            return True
        else:
            print("[FAIL] Unexpected response from Groq")
            return False

    except Exception as e:
        print(f"[FAIL] Groq API error: {e}")
        return False


def test_huggingface():
    print("\n" + "=" * 50)
    print("Testing Hugging Face Model (all-MiniLM-L6-v2)...")
    print("=" * 50)

    hf_token = os.getenv("HF_TOKEN")

    try:
        from sentence_transformers import SentenceTransformer

        print("  Loading model...")
        model = SentenceTransformer(
            "sentence-transformers/all-MiniLM-L6-v2", token=hf_token
        )

        print("  Generating test embedding...")
        test_text = "This is a test sentence"
        embedding = model.encode(test_text)

        if len(embedding) == 384:
            print("[PASS] Hugging Face model loaded successfully!")
            print(f"       Embedding dimension: {len(embedding)}")
            return True
        else:
            print(f"[FAIL] Unexpected embedding size: {len(embedding)}")
            return False

    except Exception as e:
        print(f"[FAIL] Hugging Face error: {e}")
        return False


def test_chroma():
    print("\n" + "=" * 50)
    print("Testing Chroma DB Connection...")
    print("=" * 50)

    try:
        import chromadb
        from chromadb.config import Settings

        print("  Creating local Chroma client...")
        client = chromadb.PersistentClient(path="./chroma_data")

        print("  Creating test collection...")
        test_collection = client.get_or_create_collection(name="test_connection")

        print("  Adding test document...")
        test_collection.add(documents=["Test document"], ids=["test1"])

        result = test_collection.get(ids=["test1"])
        if result["documents"] and result["documents"][0] == "Test document":
            print("[PASS] Chroma DB connection successful!")

            print("  Cleaning up test collection...")
            client.delete_collection(name="test_connection")
            return True
        else:
            print("[FAIL] Chroma returned unexpected data")
            return False

    except Exception as e:
        print(f"[FAIL] Chroma DB error: {e}")
        return False


def main():
    print("\n" + "#" * 50)
    print("# CATBOT - Connection Tester")
    print("#" * 50)

    results = {
        "Groq API": test_groq(),
        "Hugging Face Model": test_huggingface(),
        "Chroma DB": test_chroma(),
    }

    print("\n" + "#" * 50)
    print("# SUMMARY")
    print("#" * 50)

    all_passed = True
    for service, passed in results.items():
        status = "[PASS]" if passed else "[FAIL]"
        print(f"  {status} {service}")
        if not passed:
            all_passed = False

    print("\n" + "=" * 50)
    if all_passed:
        print("All connections verified successfully!")
        print("You can proceed to Step 2.")
    else:
        print("Some connections failed. Please check your API keys.")
        print("Edit the .env file with correct credentials.")
    print("=" * 50 + "\n")

    return 0 if all_passed else 1


if __name__ == "__main__":
    sys.exit(main())

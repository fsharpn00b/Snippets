import argparse
import os
from typing import Dict, List, Tuple
import yaml

from langchain_chroma import Chroma

from langchain_core.documents import Document

from langchain_ollama import OllamaEmbeddings

import tiktoken

# Constants

RETRIEVAL_TEMPLATE_TEXT: str = """You are an expert writer of {genre} stories.

# Guidelines
When you write, please do the following:
{guidelines}

# Outline
Here is the outline for the story you are writing:
{outline}

# Characters
Here is a reference for the characters in the story:
{characters}

# Request
{request}
"""

# Functions

def extract_metadata_and_text(file_path: str) -> Tuple[Dict[str, str], str]:
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    if content.startswith('---'):
        parts = content.split('---', 2)
        if len(parts) >= 3:
            metadata_yaml = parts[1]
            story_text = parts[2].strip()
            metadata = yaml.safe_load(metadata_yaml)
            return metadata, story_text

    return {}, content

def chunk_document_by_tokens(token_encoding: tiktoken.Encoding, text: str, metadata: Dict[str, str], chunk_token_size: int, chunk_overlap_tokens: int) -> List[Document]:
    tokens = token_encoding.encode(text)
    chunks = []

    start = 0
    while start < len(tokens):
        end = start + chunk_token_size
        chunk_tokens = tokens[start:end]
        chunk_text = token_encoding.decode(chunk_tokens)

        chunks.append(Document(page_content=chunk_text, metadata=metadata))

        start += chunk_token_size - chunk_overlap_tokens  # slide window with overlap

    return chunks

def batch_process(rag_database: Chroma, documents: List[Document], batch_size: int) -> None:
    number_of_documents = len(documents)
    for i in range(0, number_of_documents, batch_size):
        print(f"Adding document chunks {i}-{min([number_of_documents, i + batch_size])}/{number_of_documents} to RAG database...")
        batch = documents[i:i + batch_size]
        rag_database.add_documents(documents=batch)

def get_rag_database(token_encoding: tiktoken.Encoding, args: argparse.Namespace) -> Chroma:
    embedding_model = OllamaEmbeddings(model=args.model_name)

    if not os.path.isdir(args.rag_database_folder) :
        os.makedirs(args.rag_database_folder, exist_ok=True)
        rag_database = Chroma(
            embedding_function=embedding_model,
            persist_directory=args.rag_database_folder
        )
    else:
        all_documents: List[Document] = []

        for root, dirs, files in os.walk(args.rag_input_folder):
            for filename in files:
                if filename.endswith(".txt"):
                    file_path = os.path.join(root, filename)
                    metadata, text = extract_metadata_and_text(file_path)
                    documents = chunk_document_by_tokens(token_encoding=token_encoding, text=text, metadata=metadata, chunk_token_size=args.chunk_size, chunk_overlap_tokens=args.chunk_overlap)
                    all_documents.extend(documents)

        rag_database = Chroma(
            embedding_function=embedding_model,
            persist_directory=args.rag_database_folder
        )

        batch_process(rag_database=rag_database, documents=all_documents, batch_size=args.rag_database_batch_size)
        print("Done adding documents to RAG database.")

    return rag_database

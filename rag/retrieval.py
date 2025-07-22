import argparse
import os

# Functions

def set_up_output_folders(args : argparse.Namespace) -> None :
    os.makedirs(args.log_folder, exist_ok=True)

def log_and_print_request(log_file_path: str, request: str) -> None :
    output = f"""

Request:
{request}

"""
    with open(log_file_path, mode="a", encoding="utf-8") as log_file:
        log_file.write(output)
    print("Thinking...")

def log_and_print_context(log_file_path:str, context:str) -> None :
    output = f"""

Context:
{context}

"""
    with open(log_file_path, mode="a", encoding="utf-8") as log_file:
        log_file.write(output)
    print(output, end='')

def log_and_print_response(log_file_path: str, response: str) -> None :
    output = f"""

Response:
{response}

"""
    with open(log_file_path, mode="a", encoding="utf-8") as log_file:
        log_file.write(output)
    print(output, end='')

def log_exit_info(log_file_path: str, chat_history_file_path: str, rag_database_folder: str) -> None :
    output = f"""

Log file path: {log_file_path}
Chat history file path: {chat_history_file_path}
RAG database folder: {rag_database_folder}

"""
    with open(log_file_path, mode="a", encoding="utf-8") as log_file:
        log_file.write(output)
    print(output, end='')

def log_check_prompt_size(
    log_file_path: str,
    context_window_size: int,
    fixed_prompt_size: int,
    non_fixed_prompt_size: int,
    remaining: int,
    initial_chat_history_messages_count: int,
    initial_total_chat_history_length: int,
    final_chat_history_messages_count: int,
    final_total_chat_history_length: int
    ) -> None :
    output = f"""

Context window size: {context_window_size}
Fixed prompt size (template, genre, guidelines, characters): {fixed_prompt_size}
Non-fixed prompt size (outline, context, request): {non_fixed_prompt_size}
Remaining tokens: {remaining}
Initial number of messages in chat history: {initial_chat_history_messages_count}
Initial tokens in chat history: {initial_total_chat_history_length}
Final number of messages in chat history: {final_chat_history_messages_count}
Final tokens in chat history: {final_total_chat_history_length}

"""
    with open(log_file_path, mode="a", encoding="utf-8") as log_file:
        log_file.write(output)
    print(output, end='')

import argparse
from enum import Enum
import os
from typing import Any, List, Tuple

from langchain_community.chat_message_histories import FileChatMessageHistory

from langchain_core.prompts import PromptTemplate
from langchain_core.runnables.base import RunnableSerializable

from langchain_ollama import OllamaLLM

import tiktoken

from log import log_check_prompt_size

# Constants

PROMPT_TEMPLATE_TEXT: str = """You are an expert writer of {genre} stories.

# Guidelines
When you write, please do the following:
{guidelines}

# Outline
Here is the outline for the story you are writing:
{outline}

# Characters
Here is a reference for the characters in the story:
{characters}

# Reference stories
Here are reference stories to guide your writing style:
{context}

# Conversation history
Also please refer to the conversation history:
{chat_history}

# Request
{request}

First, explain your thinking process between <think> tags. Then, provide your response after the thinking process.

Response:
"""

USER_PROMPT: str = ">>> ('/end' to submit, '/cancel' to start over, '/ret' to test document retrieval, '/bye' to quit): "

# Enums

class UserCommand(Enum):
    CONTINUE = 0
    QUIT = 1
    TEST_DOCUMENT_RETRIEVAL = 2

# Functions - prompt information

def get_guidelines_characters(args: argparse.Namespace) -> Tuple[str, str] :
    with open(args.guidelines_text_file_path, mode="r", encoding="utf-8") as f:
        guidelines = "\n".join(f.readlines())
    with open(args.character_reference_text_file_path, mode="r", encoding="utf-8") as f:
        characters = "\n".join(f.readlines())
    return guidelines, characters

# Functions - chain

def get_chain(args: argparse.Namespace) -> RunnableSerializable[dict[Any, Any], str] :
    llm = OllamaLLM(
        model=args.model_name,
        num_ctx=args.context_window_size,
# Use default max output length.
        num_predict=-2,
        temperature=args.temperature
    )
    prompt_template: PromptTemplate = PromptTemplate.from_template(PROMPT_TEMPLATE_TEXT)
    return prompt_template | llm

# Functions - chat history 

def get_chat_history(args: argparse.Namespace, timestamp: str) -> FileChatMessageHistory :
    if args.chat_history_file_path is None :
        chat_history_file_path = os.path.join(args.log_folder, f"chat_history_{timestamp}.txt")
    else :
        chat_history_file_path = args.chat_history_file_path
    return FileChatMessageHistory(file_path=chat_history_file_path)

def count_tokens(token_encoding: tiktoken.Encoding, text: str) -> int:
    return len(token_encoding.encode(text))

def check_prompt_size(
    token_encoding: tiktoken.Encoding,
    context_window_size: int,
    fixed_prompt_size: int,
    outline: str,
    context: str,
    request: str,
    chat_history_messages: List[Any],
    log_file_path: str
    ) -> List[Any] :
    non_fixed_prompt_size = (
        count_tokens(token_encoding=token_encoding, text=outline) +
        count_tokens(token_encoding=token_encoding, text=context) +
        count_tokens(token_encoding=token_encoding, text=request)
    )
    remaining = context_window_size - fixed_prompt_size - non_fixed_prompt_size

    chat_history_message_lengths = list(map(lambda message : count_tokens(token_encoding=token_encoding, text=message.content), chat_history_messages))
    initial_chat_history_messages_count = len(chat_history_messages)
    initial_total_chat_history_length = sum(chat_history_message_lengths)
    final_total_chat_history_length = initial_total_chat_history_length
    while final_total_chat_history_length > remaining:
        final_total_chat_history_length -= chat_history_message_lengths[0]
        del chat_history_messages[0]
        del chat_history_message_lengths[0]
    final_chat_history_messages_count = len(chat_history_messages)

    log_check_prompt_size(
        log_file_path=log_file_path,
        context_window_size=context_window_size,
        fixed_prompt_size=fixed_prompt_size,
        non_fixed_prompt_size=non_fixed_prompt_size,
        remaining=remaining,
        initial_chat_history_messages_count=initial_chat_history_messages_count,
        initial_total_chat_history_length=initial_total_chat_history_length,
        final_chat_history_messages_count=final_chat_history_messages_count,
        final_total_chat_history_length=final_total_chat_history_length
    )

    return chat_history_messages

def format_chat_history(messages: List[Any]) -> str:
    return "\n".join([f"{msg.role}: {msg.content}" for msg in messages])

# Functions - interface

def collect_lines(lines: List[str]) -> Tuple[List[str], UserCommand]:
    line = input()
    lower_line = line.lower()
    if lower_line == "/end":
        return lines, UserCommand.CONTINUE
    elif lower_line == "/bye":
        return lines, UserCommand.QUIT
    elif lower_line == "/cancel":
        print("Input lines cleared.")
        return collect_lines([])
    elif lower_line == "/ret":
        print("Testing document retrieval.")
        return lines, UserCommand.TEST_DOCUMENT_RETRIEVAL
    else:
        lines.append(line)
        return collect_lines(lines)

def read_request_lines() -> Tuple[List[str], UserCommand]:
    print(USER_PROMPT, end="")
    return collect_lines([])

import argparse
from datetime import datetime
import os
import sys
from typing import Any

from langchain.vectorstores.base import VectorStoreRetriever

from langchain_community.chat_message_histories import FileChatMessageHistory

from langchain_core.runnables.base import RunnableSerializable

import tiktoken

from log import log_and_print_context, log_and_print_request, log_and_print_response, log_exit_info, set_up_output_folders
from rag_helpers import check_prompt_size, count_tokens, format_chat_history, get_chain, get_guidelines_characters, get_chat_history, PROMPT_TEMPLATE_TEXT, read_request_lines, UserCommand
from retrieval import get_rag_database, RETRIEVAL_TEMPLATE_TEXT

# Main functions

def main_loop(
    context_window_size: int,
    genre: str,
    guidelines: str,
    outline_text_file_path: str,
    characters: str,
    fixed_prompt_size: int,
    log_file_path: str,
    chat_history: FileChatMessageHistory,
    chain: RunnableSerializable[dict[Any, Any], str],
    retriever: VectorStoreRetriever,
    token_encoding: tiktoken.Encoding,
    rag_database_folder: str
) -> None:
    while True:
        request_lines, user_command = read_request_lines()
        if user_command is UserCommand.QUIT :
            log_exit_info(log_file_path=log_file_path, chat_history_file_path=str(chat_history.file_path), rag_database_folder=rag_database_folder)
            break
        else :
# 20250719 We load the outline for every prompt in case the user changes it as the story progresses.
            with open(outline_text_file_path, mode="r", encoding="utf-8") as f:
                outline = "\n".join(f.readlines())
            request = '\n'.join(request_lines)
            log_and_print_request(log_file_path=log_file_path, request=request)

            retrieval_request = RETRIEVAL_TEMPLATE_TEXT.format(genre=genre, guidelines=guidelines, outline=outline, characters=characters, request=request)
            retrieved_docs = retriever.invoke(retrieval_request)
            context = "\n\n".join([doc.page_content for doc in retrieved_docs])
            log_and_print_context(log_file_path=log_file_path, context=context)

            if user_command is UserCommand.CONTINUE :
                chat_history_messages = check_prompt_size (
                    token_encoding=token_encoding,
                    context_window_size=context_window_size,
                    fixed_prompt_size=fixed_prompt_size,
                    outline=outline,
                    context=context,
                    request=request,
                    chat_history_messages=chat_history.messages.copy(),
                    log_file_path=log_file_path
                )

                response: str = chain.invoke({
                    "genre": genre,
                    "guidelines": guidelines,
                    "outline": outline,
                    "characters": characters,
                    "context": context,
                    "chat_history": format_chat_history(chat_history_messages),
                    "request": request
                })
                chat_history.add_user_message(request)
                chat_history.add_ai_message(response)
                log_and_print_response(log_file_path=log_file_path, response=response)

def main(parser: argparse.ArgumentParser, args: argparse.Namespace) -> None:
    if args.rag_input_folder is None and not os.path.isdir(args.rag_database_folder):
        print("Error: Either RAG_INPUT_FOLDER must be specified, or RAG_DATABASE_FOLDER must be an existing RAG database folder.")
        print()
        parser.print_help()
        sys.exit(1)

    guidelines, characters = get_guidelines_characters(args)
    set_up_output_folders(args)
    timestamp = datetime.today().strftime("%Y%m%d_%H%M%S")
    log_file_path = os.path.join(args.log_folder, f"log_{timestamp}.txt")
    chat_history = get_chat_history(args=args, timestamp=timestamp)
    token_encoding = tiktoken.get_encoding(args.tiktoken_encoding)
    rag_database = get_rag_database(token_encoding=token_encoding, args=args)
    retriever: VectorStoreRetriever = rag_database.as_retriever()
    chain = get_chain(args=args)

    fixed_prompt_size = (
        count_tokens(token_encoding=token_encoding, text=PROMPT_TEMPLATE_TEXT) +
        count_tokens(token_encoding=token_encoding, text=args.genre) +
        count_tokens(token_encoding=token_encoding, text=guidelines) +
        count_tokens(token_encoding=token_encoding, text=characters)
    )

    main_loop(context_window_size=args.context_window_size, genre=args.genre, guidelines=guidelines, outline_text_file_path=args.outline_text_file_path, characters=characters, fixed_prompt_size=fixed_prompt_size, log_file_path=log_file_path, chat_history=chat_history, chain=chain, retriever=retriever, token_encoding=token_encoding, rag_database_folder=args.rag_database_folder)

def parse_args(parser: argparse.ArgumentParser) -> argparse.Namespace:
    parser.add_argument('--character_reference_text_file_path', type=str, required=True, help="(Required) The path to a text file that contains character reference information.")
# TODO1 Verify chat memory works with no initial file (done), then loads existing file as expected.
    parser.add_argument('--chat_history_file_path', type=str, default=None, help="The path to a file that contains the chat history from a previous session. If not specified, a new chat history file is created in LOG_FOLDER. (Default: None)")
    parser.add_argument('--chunk_overlap', type=int, default=100, help="The chunk overlap, in tokens, to populate the RAG database. (Default: %(default)d)")
    parser.add_argument('--chunk_size', type=int, default=1000, help="The chunk size, in tokens, to populate the RAG database. (Default: %(default)d)")
    parser.add_argument('--context_window_size', type=int, default=131072, help="Sets the LLM context window size. (Default: %(default)d)")
    parser.add_argument('--genre', type=str, default="fiction", help="The genre of the stories you want to create. (Default: %(default)s)")
    parser.add_argument('--guidelines_text_file_path', type=str, required=True, help="(Required) The path to a text file that contains writing guidelines.")
    parser.add_argument('--rag_input_folder', type=str, default=None, help="The path to the reference stories to add to the RAG database. Not required if RAG_DATABASE_FOLDER is set to an existing RAG database folder. (Default: None)")
    parser.add_argument('--log_folder', type=str, required=True, help="(Required) The path to the log output folder.")
    parser.add_argument('--model_name', type=str, default="huihui_ai/deepseek-r1-abliterated:70b", help="The LLM with which to create stories. (Default: %(default)s)")
    parser.add_argument('--outline_text_file_path', type=str, required=True, help="(Required) The path to a text file that contains a story outline.")
    parser.add_argument('--rag_database_batch_size', type=int, default=500, help="The batch size to use when adding document chunks to the RAG database. (Default: %(default)d)")
    parser.add_argument('--rag_database_folder', type=str, required=True, help="The path to a RAG database folder from a previous section. If the folder does not exist, (1) a new RAG database folder is created at the specified path, and (2) RAG_INPUT_FOLDER is required.")
# TODO1 Experiment to find best temp.
    parser.add_argument('--temperature', type=float, default=0.7, help="Sets the LLM temperature. A lower value requires the output to conform more closely to the prompt. A higher value allows more flexibility in the output. (Default: %(default)f)")
    parser.add_argument('--tiktoken_encoding', type=str, default="cl100k_base", help="Sets the token encoding for the RAG database. This value should match the token encoding used by the LLM. (Default: %(default)s)")

    return parser.parse_args()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Create stories with retrieval-augmented generation (RAG).")
    main(parser=parser, args=parse_args(parser))

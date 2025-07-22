# rag.py

I wrote this script to learn how to use retrieval-augmented generation (RAG).

Usage:
```
usage: rag.py [-h] --character_reference_text_file_path CHARACTER_REFERENCE_TEXT_FILE_PATH [--chat_history_file_path CHAT_HISTORY_FILE_PATH]
               [--chunk_overlap CHUNK_OVERLAP] [--chunk_size CHUNK_SIZE] [--context_window_size CONTEXT_WINDOW_SIZE] [--genre GENRE] --guidelines_text_file_path
               GUIDELINES_TEXT_FILE_PATH [--rag_input_folder RAG_INPUT_FOLDER] --log_folder LOG_FOLDER [--model_name MODEL_NAME] --outline_text_file_path
               OUTLINE_TEXT_FILE_PATH [--rag_database_batch_size RAG_DATABASE_BATCH_SIZE] --rag_database_folder RAG_DATABASE_FOLDER [--temperature TEMPERATURE]
               [--tiktoken_encoding TIKTOKEN_ENCODING]

Create stories with retrieval-augmented generation (RAG).

optional arguments:
  -h, --help            show this help message and exit
  --character_reference_text_file_path CHARACTER_REFERENCE_TEXT_FILE_PATH
                        (Required) The path to a text file that contains character reference information.
  --chat_history_file_path CHAT_HISTORY_FILE_PATH
                        The path to a file that contains the chat history from a previous session. If not specified, a new chat history file is created in LOG_FOLDER.
                        (Default: None)
  --chunk_overlap CHUNK_OVERLAP
                        The chunk overlap, in tokens, to populate the RAG database. (Default: 100)
  --chunk_size CHUNK_SIZE
                        The chunk size, in tokens, to populate the RAG database. (Default: 1000)
  --context_window_size CONTEXT_WINDOW_SIZE
                        Sets the LLM context window size. (Default: 131072)
  --genre GENRE         The genre of the stories you want to create. (Default: fiction)
  --guidelines_text_file_path GUIDELINES_TEXT_FILE_PATH
                        (Required) The path to a text file that contains writing guidelines.
  --rag_input_folder RAG_INPUT_FOLDER
                        The path to the reference stories to add to the RAG database. Not required if RAG_DATABASE_FOLDER is set to an existing RAG database folder.
                        (Default: None)
  --log_folder LOG_FOLDER
                        (Required) The path to the log output folder.
  --model_name MODEL_NAME
                        The LLM with which to create stories. (Default: huihui_ai/deepseek-r1-abliterated:70b)
  --outline_text_file_path OUTLINE_TEXT_FILE_PATH
                        (Required) The path to a text file that contains a story outline.
  --rag_database_batch_size RAG_DATABASE_BATCH_SIZE
                        The batch size to use when adding document chunks to the RAG database. (Default: 500)
  --rag_database_folder RAG_DATABASE_FOLDER
                        The path to a RAG database folder from a previous section. If the folder does not exist, (1) a new RAG database folder is created at the specified
                        path, and (2) RAG_INPUT_FOLDER is required.
  --temperature TEMPERATURE
                        Sets the LLM temperature. A lower value requires the output to conform more closely to the prompt. A higher value allows more flexibility in the
                        output. (Default: 0.700000)
  --tiktoken_encoding TIKTOKEN_ENCODING
                        Sets the token encoding for the RAG database. This value should match the token encoding used by the LLM. (Default: cl100k_base)

```

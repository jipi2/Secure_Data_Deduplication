from dotenv import load_dotenv
from azure.storage.blob import BlobServiceClient, BlobClient
import os

def download_blob(blob_name:str, filePath:str):  #aici trebuie umblat, ia prea mult
    load_dotenv()
    try:

        conn_str = os.environ.get('azure_connection_string')
        container_name = os.environ.get('azure_container_name')
        
        blob_service_client = BlobServiceClient.from_connection_string(conn_str)
        container_client = blob_service_client.get_container_client(container= container_name)
        chunk_size = 1048576
        with open(filePath, "wb") as download_file:
            download_stream = container_client.download_blob(blob_name)
            while True:
                data = download_stream.read(chunk_size)
                if not data:
                    break
                download_file.write(data)
        print(f"Blob downloaded to {filePath} successfully.")
    except Exception as e:
        print(str(e))
        raise e